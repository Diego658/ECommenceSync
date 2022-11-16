
using Dapper;
using ECommenceSync.Entities;
using ECommenceSync.Interfaces;
using ECommenceSync.WooCommerce.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;

namespace ECommenceSync.WooCommerce.Operations
{
    public class ProductOperation<TExternalKey> : IDestinationOperation<TExternalKey, Product<TExternalKey>>
         where TExternalKey : struct
    {
        private const string SqlAgregarLink = "INSERT INTO StoreSync_Productos_WooCommerce(ItemID, WooCommerceID) VALUES (@ItemID, @WooCommerceID)";
        readonly ConcurrentQueue<Product<TExternalKey>> _workQueue;
        readonly IWooCommerceDatabaseHelper _databaseHelper;
        private readonly IWooCommerceOperationsHelper _operationsHelper;
        private Task _taskProcessor;
        ConcurrentDictionary<TExternalKey, long> _links;
        ConcurrentDictionary<TExternalKey, long> _categoryLinks;
        ConcurrentDictionary<TExternalKey, long> _tagLinks;
        ConcurrentDictionary<TExternalKey, long> _brandsLinks;

        
        readonly bool _addAllParentCategoriesToProduct = true;
        readonly Dictionary<TExternalKey, TExternalKey> _categoriesHierarchy;
        private readonly WCObject _wc;

        public CancellationTokenSource TaskCancelTokenSource { get; set; }
        public CancellationTokenSource CancelTokenSource { get; set; }

        public Action<Product<TExternalKey>, SyncResult, Exception> OnSynchronized { get; set; }

        public ECommenceSync.Operations Operation => ECommenceSync.Operations.Products;

        public OperationModes Mode => OperationModes.Automatic;

        public OperationDirections Direction => OperationDirections.ErpToStore;

        public Guid Identifier => Guid.NewGuid();

        public OperationStatus Status { get; set; }

        public ProductOperation(IWooCommerceDatabaseHelper databaseHelper,
            IWooCommerceOperationsHelper operationsHelper,
            Dictionary<TExternalKey, TExternalKey> categoriesHierarchy)
        {
            _categoriesHierarchy = categoriesHierarchy;
            Status = OperationStatus.Created;
            _workQueue = new ConcurrentQueue<Product<TExternalKey>>();
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            _operationsHelper = operationsHelper;
            var rest = new RestAPI($"{databaseHelper.ApiUrl}/v3/", databaseHelper.ApiUser, databaseHelper.ApiPassword);
            _wc = new WCObject(rest);
        }

        public void AddWork(List<Product<TExternalKey>> values)
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
                    var (result, ex) = await SyncProduct(product);
                    OnSynchronized(product, result, ex);
                }
                await Task.Delay(_operationsHelper.OperationsWorkQueueWaitTime);
            }
            Status = OperationStatus.Stopped;
        }

        private async Task<bool> LoadLinks()
        {

            _links = await _databaseHelper.GetProductsLinks<TExternalKey>();
            _categoryLinks = await _databaseHelper.GetCategoryLinks<TExternalKey>();
            _brandsLinks = await _databaseHelper.GetBrandsLinks<TExternalKey>();
            _tagLinks = await _databaseHelper.GetTagsLinks<TExternalKey>();

            return true;
        }

        async Task<Tuple<SyncResult, Exception>> SyncProduct(Product<TExternalKey> product)
        {
            

            var idWoo = _links.ContainsKey(product.Id) ? _links[product.Id] : 0;

            if (idWoo == 0 && (product.StockAvailable <= 0 || product.Price <= 0) || product.HasVariants )
            {
                return Tuple.Create(SyncResult.NotSynchronized, default(Exception));
            }


            var idCategoriaWoo = _categoryLinks.ContainsKey(product.ParentId) ? _categoryLinks[product.ParentId] : 0;
            if (idCategoriaWoo == 0)
            {
                product.RetryCount++;
                return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception));
            }
            product.RetryCount = 0;
            long idMarcaWoo;
            if (product.BrandId is null)
            {
                idMarcaWoo = 0;
            }
            else if (_brandsLinks.ContainsKey(product.BrandId.Value))
            {
                idMarcaWoo = _brandsLinks[product.BrandId.Value];
            }
            else
            {
                product.RetryCount++;
                return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception));
            }
            product.TagIds ??= "";
            var tags = product
                .TagIds
                .Split(',')
                .Where(s => s.Length > 0)
                .Select(id => Convert.ToInt32(id))
                .Cast<TExternalKey>()
                .All(id => _tagLinks.ContainsKey(id));

            if (!tags)
            {
                product.RetryCount++;
                return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception));
            }

            Tuple<SyncResult, Exception> resultado;
            if (idWoo == 0)
            {
                resultado = await AddProduct(product, idCategoriaWoo, idMarcaWoo);
            }
            else
            {
                resultado = await UpdateProduct(product, idWoo, idCategoriaWoo, idMarcaWoo);
            }
            return resultado;

        }

        async Task<Tuple<SyncResult, Exception>> AddProduct(Product<TExternalKey> product, long idCategoriaWoo, long idMarcaPrestashop)
        {
            
            var wooproduct = new Product
            {
                name = product.Name,
                slug = product.Name.GetStringForLinkRewrite(),
                type = "simple",
                status = "publish",
                featured = false,
                catalog_visibility = "visible",
                description = product.Description,
                short_description = product.DescriptionShort,
                sku = product.Reference,
                price = product.Price,
                regular_price = product.Price,
                sale_price = product.Price,
                _virtual = product.IsVirtual,
                downloadable = product.IsVirtual,
                manage_stock = true,
                stock_quantity = Convert.ToInt32(product.StockAvailable),
                stock_status = "instock",
                backorders = "no",
                sold_individually = false,
                weight = product.Weight,
                reviews_allowed = true,
                parent_id = Convert.ToUInt32(idCategoriaWoo),
                categories = new List<ProductCategoryLine>()
                {
                    new(){ id= Convert.ToUInt32( idCategoriaWoo) },
                },
                tags = product.TagIds.Split(',')
                    .Where(s => s.Trim().Length > 0)
                    .Select(id => Convert.ToInt32(id))
                    .Cast<TExternalKey>()
                    .Select(id => new ProductTagLine() { id = Convert.ToUInt32(_tagLinks[id]) }).ToList(),
                menu_order = product.PositionInCategory,
                tax_status = "taxable",
                tax_class = product.HasTaxes ? _operationsHelper.TaxClassIva12 : _operationsHelper.TaxClassIva0,
            };

            //wooproduct.variations


            Exception error;
            (wooproduct, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                var tmp = await _wc.Product.Add(wooproduct);
                //_wc.Product.Variations.Add(new Variation)
                return tmp;
            },5, MethodHelper.TryAgainOnBadRequest);


            if (error is null)
            {
                await AddLink(product.Id, wooproduct.id.Value);
                return Tuple.Create(SyncResult.Created, error);
            }
            else
            {
                return Tuple.Create(SyncResult.Error, error);
            }
        }


        private async Task AddLink(TExternalKey externalKey, long key)
        {
            using var conex = _databaseHelper.GetConnection();
            await conex.ExecuteAsync(SqlAgregarLink, new { ItemID = externalKey, WooCommerceID = key });
            _links.AddOrUpdate(externalKey, key, (k, v) => v);
        }

        async Task<Tuple<SyncResult, Exception>> UpdateProduct(Product<TExternalKey> product, long idWoo, long idCategoriaWoo, long idMarcaWoo)
        {
            var wooproduct = new Product
            {
                id= Convert.ToUInt32(idWoo),
                name = product.Name,
                slug = product.Name.GetStringForLinkRewrite(),
                type = "simple",
                status = "publish",
                featured = false,
                catalog_visibility = "visible",
                description = product.Description,
                short_description = product.DescriptionShort,
                sku = product.Reference,
                price = product.Price,
                regular_price = product.Price,
                sale_price = product.Price,
                _virtual = product.IsVirtual,
                downloadable = product.IsVirtual,
                manage_stock = true,
                stock_quantity = Convert.ToInt32(product.StockAvailable),
                stock_status = "instock",
                backorders = "no",
                sold_individually = false,
                weight = product.Weight,
                reviews_allowed = true,
                parent_id = Convert.ToUInt32(idCategoriaWoo),
                categories = new List<ProductCategoryLine>()
                {
                    new(){ id= Convert.ToUInt32( idCategoriaWoo) },
                },
                tags = product.TagIds.Split(',')
                    .Where(s => s.Trim().Length > 0)
                    .Select(id => Convert.ToInt32(id))
                    .Cast<TExternalKey>()
                    .Select(id => new ProductTagLine() { id = Convert.ToUInt32(_tagLinks[id]) }).ToList(),
                menu_order = product.PositionInCategory,
                tax_status = "taxable",
                tax_class = product.HasTaxes ? _operationsHelper.TaxClassIva12 : _operationsHelper.TaxClassIva0,
            };

            Exception error;
            (wooproduct, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                var tmp = await _wc.Product.Update(Convert.ToInt32(idWoo), wooproduct);
                return tmp;
            }, 5,MethodHelper.TryAgainOnBadRequest, MethodHelper.StopsOnTermExist);

            if (error is null)
            {
                return Tuple.Create(SyncResult.Updated, error);
            }
            else
            {
                return Tuple.Create(SyncResult.Error, error);
            }
        }

    }
}