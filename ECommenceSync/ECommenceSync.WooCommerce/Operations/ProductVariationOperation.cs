using Dapper;
using ECommenceSync.Entities;
using ECommenceSync.Interfaces;
using ECommenceSync.WooCommerce.Helpers;
using ECommenceSync.WooCommerce.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace ECommenceSync.WooCommerce.Operations
{
    public class ProductVariationOperation<TExternalKey> : IDestinationOperation<TExternalKey, ProductVariant<TExternalKey>>
         where TExternalKey : struct
    {
        private const string SqlAgregarLink = "INSERT INTO StoreSync_Variations_WooCommerce(VariationID, WooCommerceID) VALUES (@VariationID, @WooCommerceID)";

        private const string SqlAgregarLinkProduct = "INSERT INTO StoreSync_Productos_WooCommerce(ItemID, WooCommerceID) VALUES (@ItemID, @WooCommerceID)";

        readonly ConcurrentQueue<ProductVariant<TExternalKey>> _workQueue;
        readonly IWooCommerceDatabaseHelper _databaseHelper;
        private readonly IWooCommerceOperationsHelper _operationsHelper;
        private Task _taskProcessor;
        private ConcurrentDictionary<TExternalKey, long> _links;
        ConcurrentDictionary<TExternalKey, long> _productsLinks;
        ConcurrentDictionary<TExternalKey, long> _categoryLinks;
        ConcurrentDictionary<TExternalKey, long> _tagLinks;
        ConcurrentDictionary<TExternalKey, long> _brandsLinks;
        ConcurrentDictionary<TExternalKey, long> _attributesLinks;
        ConcurrentDictionary<TExternalKey, long> _attributesTermsLinks;
        private ConcurrentDictionary<TExternalKey, WooProductVariationVsVariants<TExternalKey>> _productVariationsVsVariants;
        readonly bool _addAllParentCategoriesToProduct = true;
        readonly Dictionary<TExternalKey, TExternalKey> _categoriesHierarchy;
        private readonly GenericSourceOperation<TExternalKey, ProductAttributeTerm<TExternalKey>> _attributesOperation;
        private readonly WCObject _wc;
        public CancellationTokenSource TaskCancelTokenSource { get; set; }
        public CancellationTokenSource CancelTokenSource { get; set; }

        public Action<ProductVariant<TExternalKey>, SyncResult, Exception> OnSynchronized { get; set; }

        public ECommenceSync.Operations Operation => ECommenceSync.Operations.ProductsVariations;
        public OperationModes Mode => OperationModes.Automatic;
        public OperationDirections Direction => OperationDirections.ErpToStore;
        public Guid Identifier => Guid.NewGuid();
        public OperationStatus Status { get; set; }

        public ProductVariationOperation(IWooCommerceDatabaseHelper databaseHelper,
            IWooCommerceOperationsHelper operationsHelper,
            Dictionary<TExternalKey, TExternalKey> categoriesHierarchy,
            GenericSourceOperation<TExternalKey, ProductAttributeTerm<TExternalKey>> attributesOperation)
        {
            if (attributesOperation is null)
            {
                throw new ArgumentNullException(nameof(attributesOperation));
            }

            _categoriesHierarchy = categoriesHierarchy;
            _attributesOperation = attributesOperation;
            Status = OperationStatus.Created;
            _workQueue = new ConcurrentQueue<ProductVariant<TExternalKey>>();
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            _operationsHelper = operationsHelper;
            var rest = new RestAPI($"{databaseHelper.ApiUrl}/v3/", databaseHelper.ApiUser, databaseHelper.ApiPassword);
            _wc = new WCObject(rest);
        }

        public void AddWork(List<ProductVariant<TExternalKey>> values)
        {
            foreach (var item in values)
            {
                _workQueue.Enqueue(item);
            }
        }


        public void Start()
        {
            TaskCancelTokenSource = new CancellationTokenSource();
            CancelTokenSource = new CancellationTokenSource();
            _taskProcessor = new Task(async () => await Work(), TaskCancelTokenSource.Token, TaskCreationOptions.LongRunning);
            _taskProcessor.Start();
        }

        public void Stop(TimeSpan timeOut)
        {
            throw new NotImplementedException();
        }

        private async Task<bool> LoadLinks()
        {
            _links = await _databaseHelper.GetProductsVariationsLinks<TExternalKey>();
            _productsLinks = await _databaseHelper.GetProductsLinks<TExternalKey>();
            _categoryLinks = await _databaseHelper.GetCategoryLinks<TExternalKey>();
            _brandsLinks = await _databaseHelper.GetBrandsLinks<TExternalKey>();
            _tagLinks = await _databaseHelper.GetTagsLinks<TExternalKey>();
            _attributesLinks = await _databaseHelper.GetAttributesLinks<TExternalKey>();
            _attributesTermsLinks = await _databaseHelper.GetAttributesTermsLinks<TExternalKey>();
            _productVariationsVsVariants = await _databaseHelper.GetProductVariationsVsVariants<TExternalKey>();
            return true;
        }


        public async Task Work()
        {
            if (!await LoadLinks())
            {
                return;
            }

            Status = OperationStatus.Working;
            while (!CancelTokenSource.IsCancellationRequested)
            {
                while (_workQueue.TryDequeue(out var product))
                {
                    var (result, ex) = await SyncVariant(product);
                    OnSynchronized(product, result, ex);
                }
                await Task.Delay(_operationsHelper.OperationsWorkQueueWaitTime);
            }
            Status = OperationStatus.Stopped;
        }

        async Task<Tuple<SyncResult, Exception>> SyncVariant(ProductVariant<TExternalKey> variation)
        {
            
            var idWoo = Convert.ToUInt64( _links.ContainsKey(variation.Id) ? _links[variation.Id] : 0);

            var idCategoriaWoo = _categoryLinks.ContainsKey(variation.ParentId) ? _categoryLinks[variation.ParentId] : 0;
            if (idCategoriaWoo == 0)
            {
                variation.RetryCount++;
                return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception));
            }



            SyncResult result;
            Exception ex;
            Product woovariant;

            if (idWoo == 0)
            {
                (result, ex, woovariant) = await AddProductVariable(variation, idCategoriaWoo);
                if(woovariant != null) idWoo = woovariant.id.Value; 

            }
            else
            {
                (result, ex, woovariant) = await UpdateProductVariable(variation, idWoo, idCategoriaWoo);
                //idWoo = Convert.ToInt64(woovariant.id);
            }

            if (ex != null)
            {
                return Tuple.Create(SyncResult.Error, ex);
            }

            if (result == SyncResult.NotSynchronizedPostponed)
            {
                return Tuple.Create(result, ex);
            }
           
            foreach (var productVariant in variation.VariantVariations)
            {
                (result, ex) = await SyncVariation(productVariant, idWoo);
                if (ex != null)
                {
                    return Tuple.Create(result, ex);
                }
                if (result == SyncResult.NotSynchronizedPostponed)
                {
                    return Tuple.Create(result, ex);
                }
            }

            return Tuple.Create(SyncResult.Updated, default(Exception));
        }

        /// <summary>
        /// Crea el producto principal de tipo variante que es al que se le va a establecer las variaciones.
        /// </summary>
        /// <param name="variation"></param>
        /// <param name="idCategoriaWoo"></param>
        /// <returns></returns>
        private async Task<Tuple<SyncResult, Exception, Product>> AddProductVariable(ProductVariant<TExternalKey> variation, long idCategoriaWoo)
        {
            var firstProduct = variation.VariantVariations.OrderBy(x => x.Product.Id).FirstOrDefault();

            long idMarcaWoo;
            if (firstProduct.Product.BrandId is null)
            {
                idMarcaWoo = 0;
            }
            else if (_brandsLinks.ContainsKey(firstProduct.Product.BrandId.Value))
            {
                idMarcaWoo = _brandsLinks[firstProduct.Product.BrandId.Value];
            }
            else
            {
                variation.RetryCount++;
                return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception), default(Product));
            }

            firstProduct.Product.TagIds ??= "";

            var tags = firstProduct.Product
                .TagIds
                .Split(',')
                .Where(s => s.Length > 0)
                .Select(id => Convert.ToInt32(id))
                .Cast<TExternalKey>()
                .All(id => _tagLinks.ContainsKey(id));

            if (!tags)
            {
                variation.RetryCount++;
                return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception), default(Product));
            }

            Dictionary<TExternalKey, Dictionary<TExternalKey, ProductAttributeTerm<TExternalKey>>> attibutes = new();
            foreach (var vv in variation.VariantVariations)
            {
                foreach (var attributeTerm in vv.AttributeTerms)
                {
                    if(!_attributesLinks.ContainsKey( attributeTerm.AttributeId))
                    {
                        variation.RetryCount++;
                        return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception), default(Product));
                    }

                    if (!_attributesTermsLinks.ContainsKey(attributeTerm.Id))
                    {
                        variation.RetryCount++;
                        return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception), default(Product));
                    }

                    var termn = await _attributesOperation.ResolveEntity(attributeTerm.Id);
                    //attributeTerm.Name = termn.Name;
                    if (!attibutes.ContainsKey(attributeTerm.AttributeId))
                    {
                        attibutes.Add(attributeTerm.AttributeId, new());
                    }
                    if (!attibutes[attributeTerm.AttributeId].ContainsKey(attributeTerm.Id))
                    {
                        attibutes[attributeTerm.AttributeId].Add(attributeTerm.Id, termn);
                    }
                }
            }

            if(!variation.VariantVariations.Any(x=> CanSyncVariantVariation(x)))
            {
                return Tuple.Create(SyncResult.NotSynchronized, default(Exception), default(Product));
            }

            var wooVariant = new Product()
            {
                name = variation.Name,
                slug = $"{variation.Id}-{ variation.Name}".GetStringForLinkRewrite(),
                type = "variable",
                status = "publish",
                featured = false,
                catalog_visibility = "visible",
                description = firstProduct?.Product.Description ?? "",
                short_description = firstProduct?.Product.DescriptionShort ?? "",
                sku = "",
                price = 0,
                regular_price = 0,
                sale_price = 0,
                _virtual = false,
                downloadable = false,
                manage_stock = false,
                stock_quantity = 0,
                stock_status = "instock",
                backorders = "no",
                sold_individually = false,
                weight = 0,
                reviews_allowed = true,
                parent_id = Convert.ToUInt64(idCategoriaWoo),
                categories = new List<ProductCategoryLine>()
                {
                    new(){ id= Convert.ToUInt64( idCategoriaWoo) },
                },
                tags = new(),
                menu_order = null,
                tax_status = "taxable",
                attributes = attibutes.Select( x=> new ProductAttributeLine() 
                {
                    id = Convert.ToUInt64(_attributesLinks[x.Key]),
                    variation = true,
                    visible = true,
                    options = x.Value.Select(a => a.Value.Name).ToList()
                }).ToList(),
                tax_class = (firstProduct?.Product.HasTaxes ?? false) ? _operationsHelper.TaxClassIva12 : _operationsHelper.TaxClassIva0,
            };



            Exception error;

            (wooVariant, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                var tmp = await _wc.Product.Add(wooVariant);
                //_wc.Product.Variations.Add(new Variation)
                return tmp;
            }, 5, MethodHelper.TryAgainOnBadRequest, MethodHelper.StopsOnTermExist);

            if (error is null)
            {
                await AddLinkToProductVariable(variation.Id, wooVariant.id.Value);
                await SetBrand(idMarcaWoo, wooVariant);
                return Tuple.Create(SyncResult.Created, error, wooVariant);
            }
            else
            {
                return Tuple.Create(SyncResult.Error, error, default(Product));
            }
        }

        #region SetBrand

        

        [DataContract]
        private class BrandsInfo
        {
            [DataMember(EmitDefaultValue = false, Name = "brands")]
            public List<long> Brands { get; set; }
            public BrandsInfo(long brand)
            {
                Brands = new List<long> { brand };
            }
        }

        private async Task SetBrand(long idMarcaWoo, Product wooproduct)
        {
            //Establecer marca
            await MethodHelper.ExecuteMethodAsync(async () =>
            {

                //var tmp = await _wc.Product.API.PostRestful($"products/{wooproduct.id}", new BrandsInfo( idMarcaWoo));
                var tmp = await _wc.Product.API.SendHttpClientRequest($"products/{wooproduct.id}",
                   RequestMethod.PUT, new BrandsInfo(idMarcaWoo));
                return tmp;
            }, 2, MethodHelper.TryAgainOnBadRequest);
        }
        #endregion


        /// <summary>
        /// Actualiza el producto principal de tipo variante que es al que se le va a establecer las variaciones.
        /// </summary>
        /// <param name="variation"></param>
        /// <param name="idCategoriaWoo"></param>
        /// <returns></returns>
        private async Task<Tuple<SyncResult, Exception, Product>> UpdateProductVariable(ProductVariant<TExternalKey> variation, ulong idWoo, long idCategoriaWoo)
        {
            var firstProduct = variation.VariantVariations.OrderBy(x => x.Product.Id).FirstOrDefault();


            long idMarcaWoo= 0;
            if (firstProduct.Product.BrandId is null)
            {
                idMarcaWoo = 0;
            }
            else if (_brandsLinks.ContainsKey(firstProduct.Product.BrandId.Value))
            {
                idMarcaWoo = _brandsLinks[firstProduct.Product.BrandId.Value];
            }
            

            Dictionary<TExternalKey, Dictionary<TExternalKey, ProductAttributeTerm<TExternalKey>>> attibutes = new();
            foreach (var vv in variation.VariantVariations)
            {
                foreach (var attributeTerm in vv.AttributeTerms)
                {
                    if (!_attributesLinks.ContainsKey(attributeTerm.AttributeId))
                    {
                        variation.RetryCount++;
                        return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception), default(Product));
                    }

                    if (!_attributesTermsLinks.ContainsKey(attributeTerm.Id))
                    {
                        variation.RetryCount++;
                        return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception), default(Product));
                    }

                    var termn = await _attributesOperation.ResolveEntity(attributeTerm.Id);
                    //attributeTerm.Name = termn.Name;
                    if (!attibutes.ContainsKey(attributeTerm.AttributeId))
                    {
                        attibutes.Add(attributeTerm.AttributeId, new());
                    }
                    if (!attibutes[attributeTerm.AttributeId].ContainsKey(attributeTerm.Id))
                    {
                        attibutes[attributeTerm.AttributeId].Add(attributeTerm.Id, termn);
                    }
                }

            }

            var wooVariant = new Product()
            {
                name = variation.Name,
                slug = $"{variation.Id}-{ variation.Name}".GetStringForLinkRewrite(),
                type = "variable",
                status = "publish",
                featured = false,
                catalog_visibility = "visible",
                description = firstProduct?.Product.Description ?? "",
                short_description = firstProduct?.Product.DescriptionShort ?? "",
                sku = "",
                price = 0,
                regular_price = 0,
                sale_price = 0,
                _virtual = false,
                downloadable = false,
                manage_stock = false,
                stock_quantity = 0,
                stock_status = "instock",
                backorders = "no",
                sold_individually = false,
                weight = 0,
                reviews_allowed = true,
                parent_id = Convert.ToUInt64(idCategoriaWoo),
                categories = new List<ProductCategoryLine>()
                {
                    new(){ id= Convert.ToUInt64( idCategoriaWoo) },
                },
                tags = new(),
                menu_order = null,
                tax_status = "taxable",
                attributes = attibutes.Select(x => new ProductAttributeLine()
                {
                    id = Convert.ToUInt64(_attributesLinks[ x.Key]),
                    variation = true,
                    visible = true,
                    name = "",
                    options = x.Value.Select(a => a.Value.Name).ToList()
                }).ToList(),
                tax_class = (firstProduct?.Product.HasTaxes ?? false) ? _operationsHelper.TaxClassIva12 : _operationsHelper.TaxClassIva0,
            };
            Exception error;
            (wooVariant, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                var tmp = await _wc.Product.Update(idWoo, wooVariant);
                return tmp;
            }, 5, MethodHelper.TryAgainOnBadRequest, MethodHelper.StopsOnTermExist);

            if (error is null)
            {
                //await AddLink(variation.Id, wooVariant.id.Value);
                await SetBrand(idMarcaWoo, wooVariant);

                return Tuple.Create(SyncResult.Created, error, wooVariant);
            }
            else
            {
                return Tuple.Create(SyncResult.Error, error, default(Product));
            }
        }

        private bool CanSyncVariantVariation(ProductVariantVariation<TExternalKey> productVariantVariation)
        {
            var product = productVariantVariation.Product;

            var idWoo = _productsLinks.ContainsKey(product.Id) ? _productsLinks[product.Id] : 0;

            if (idWoo == 0 && (product.StockAvailable <= 0 || product.Price <= 0))
            {
                return false;
            }

            return true;
        }

        async Task<Tuple<SyncResult, Exception>> SyncVariation(ProductVariantVariation<TExternalKey> productVariantVariation, ulong idWhooParent)
        {

            var product = productVariantVariation.Product;

            var idWoo = Convert.ToUInt64( _productsLinks.ContainsKey(product.Id) ? _productsLinks[product.Id] : 0);

            if (idWoo == 0 && (product.StockAvailable <= 0 || product.Price <= 0))
            {
                return Tuple.Create(SyncResult.NotSynchronized, default(Exception));
            }



            Tuple<SyncResult, Exception> resultado;
            if (idWoo == 0)
            {
                resultado = await AddVariation(productVariantVariation, idWhooParent);
            }
            else
            {
                resultado = await UpdateVariation(productVariantVariation, idWoo, idWhooParent);
            }
            return resultado;

        }

        async Task<Tuple<SyncResult, Exception>> AddVariation(ProductVariantVariation<TExternalKey> productVariantVariation, ulong idParent)
        {
            var product = productVariantVariation.Product;
            var wooVariation = new Variation()
            {
                description = product.Description,
                backordered = false,
                status = "publish",
                backorders_allowed = false,
                sku = product.Reference,
                downloadable = false,
                on_sale = true,
                stock_status = "instock",
                stock_quantity = Convert.ToInt32(product.StockAvailable),
                weight = product.Weight,
                _virtual = product.IsVirtual,
                price = product.Price,
                regular_price = product.Price,
                sale_price = product.Price,
                tax_status = "taxable",
                attributes = new(),
                manage_stock = true,
                
                
            };

            foreach (var attribute in productVariantVariation.AttributeTerms)
            {
                var idWhooAttibute = _attributesLinks.ContainsKey(attribute.AttributeId) ? _attributesLinks[attribute.AttributeId] : 0;
                var sourceAttribute = await _attributesOperation.ResolveEntity(attribute.Id);
                wooVariation.attributes.Add(new() { id = Convert.ToUInt64(idWhooAttibute), option = sourceAttribute.Name });
            }

            Exception error;
            (wooVariation, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                var tmp = await _wc.Product.Variations.Add(wooVariation,  idParent);
                //_wc.Product.Variations.Add(new Variation)
                return tmp;
            }, 5, MethodHelper.TryAgainOnBadRequest, MethodHelper.StopsOnTermExist);


            if (error is null)
            {
                await AddLink(product.Id, Convert.ToInt64( wooVariation.id.Value));
                await AddProductVariationVsVariantLink(idParent, wooVariation.id.Value, product.Id);
                return Tuple.Create(SyncResult.Created, error);
            }
            else
            {
                return Tuple.Create(SyncResult.Error, error);
            }
        }


        async Task<Tuple<SyncResult, Exception>> UpdateVariation(ProductVariantVariation<TExternalKey> productVariantVariation, ulong idWoo, ulong idParent)
        {
            var product = productVariantVariation.Product;
            var wooVariation = new Variation()
            {
                description = product.Description,
                backordered = false,
                status = "publish",
                backorders_allowed = false,
                sku = product.Reference,
                downloadable = false,
                on_sale = true,
                stock_status = "instock",
                stock_quantity = Convert.ToInt32(product.StockAvailable),
                weight = product.Weight,
                _virtual = product.IsVirtual,
                price = product.Price,
                regular_price = product.Price,
                sale_price = product.Price,
                tax_status = "taxable",
                attributes = new(),
                manage_stock=true
            };


            foreach (var attribute in productVariantVariation.AttributeTerms)
            {
                var idWhooAttibute = _attributesLinks.ContainsKey(attribute.AttributeId) ? _attributesLinks[attribute.AttributeId] : 0;
                var sourceAttribute = await _attributesOperation.ResolveEntity(attribute.Id);
                wooVariation.attributes.Add(new VariationAttribute() { id = Convert.ToUInt64(idWhooAttibute), option = sourceAttribute.Name  });
            }

            Exception error;
            (wooVariation, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                var tmp = await _wc.Product.Variations.Update(Convert.ToUInt64(idWoo), wooVariation, idParent);
                return tmp;
            }, 5, MethodHelper.TryAgainOnBadRequest, MethodHelper.StopsOnTermExist);

            if (error is null)
            {
                return Tuple.Create(SyncResult.Updated, error);
            }
            else
            {
                return Tuple.Create(SyncResult.Error, error);
            }
        }

        /// <summary>
        /// Crea el enlace entre la variación y el id externo de woocomerce, al ser un producto solo con un campo que lo identifica
        /// como variable el enlace se guarda hacia productos.
        /// </summary>
        /// <param name="externalKey"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private async Task AddLinkToProductVariable(TExternalKey externalKey, ulong key)
        {
            var lKey = Convert.ToInt64(key);
            using var conex = _databaseHelper.GetConnection();
            await conex.ExecuteAsync(SqlAgregarLink, new { VariationID = externalKey, WooCommerceID = lKey });
            _links.AddOrUpdate(externalKey, lKey, (k, v) => v);
        }

        private async Task AddLink(TExternalKey externalKey, long key)
        {
            using var conex = _databaseHelper.GetConnection();
            await conex.ExecuteAsync(SqlAgregarLinkProduct, new { ItemID = externalKey, WooCommerceID = key });
            _productsLinks.AddOrUpdate(externalKey, key, (k, v) => v);
        }


        private async Task AddProductVariationVsVariantLink(ulong productId, ulong variantId, TExternalKey externalKey)
        {
            const string SqlAgregarLink = @"INSERT INTO [dbo].[StoreSync_VariationsVsVariants_WooCommerce]
           ([WooProductId], [WooVariationId] ,[ExternalId])  VALUES (@WooProductId, @WooVariationId, @ExternalId)";
            using var conex = _databaseHelper.GetConnection();
            await conex.ExecuteAsync(SqlAgregarLink, new { WooProductId = Convert.ToInt64(productId),
                WooVariationId = Convert.ToInt64( variantId), ExternalId = externalKey });
            var info = new WooProductVariationVsVariants<TExternalKey>() 
            {
                ExternalId = externalKey,
                WooProductId = Convert.ToInt64( productId),
                WooVariationId = Convert.ToInt64( variantId)
            };
            _productVariationsVsVariants.AddOrUpdate(externalKey, info, (k, v) => v);
            //_productsLinks.AddOrUpdate(externalKey, key, (k, v) => v);
        }

    }
}
