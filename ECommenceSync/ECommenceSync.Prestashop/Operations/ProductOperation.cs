using Bukimedia.PrestaSharp.Factories;
using Dapper;
using ECommenceSync.Entities;
using ECommenceSync.Interfaces;
using ECommenceSync.Prestashop.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ECommenceSync.Prestashop.Operations
{
    public class ProductOperation<TExternalKey> : IDestinationOperation<TExternalKey, Product<TExternalKey>>
         where TExternalKey : struct
    {
        private const string SqlAgregarLink = "INSERT INTO StoreSync_Productos_Prestashop(ItemID, PrestashopID) VALUES (@ItemID, @PrestashopID)";

        private const string SqlAddOrUpdateProductLink = @" MERGE StoreSync_Productos_Links AS tgt  
    USING (SELECT @ItemID, @LinkWeb) as src (ItemId, LinkWeb)  
    ON (tgt.ItemId = src.ItemId)  
    WHEN MATCHED THEN
        UPDATE SET LinkWeb = src.LinkWeb  
    WHEN NOT MATCHED THEN  
        INSERT (ItemId, LinkWeb)  
        VALUES (src.ItemId, src.LinkWeb);";

        readonly ConcurrentQueue<Product<TExternalKey>> _workQueue;
        readonly IPrestashopDatabaseHelper _databaseHelper;
        readonly ProductFactory _productFactory;
        private CancellationTokenSource TaskCancelTokenSource;
        private CancellationTokenSource CancelTokenSource;
        private Task _taskProcessor;
        ConcurrentDictionary<TExternalKey, long> _links;
        ConcurrentDictionary<TExternalKey, long> _categoryLinks;
        ConcurrentDictionary<TExternalKey, long> _brandsLinks;
        readonly int _syncLanguage;
        readonly int _ivaTaxRuleGroup;
        readonly bool _addAllParentCategoriesToProduct = true;
        readonly Dictionary<TExternalKey, TExternalKey> _categoriesHierarchy;
        ConcurrentDictionary<TExternalKey, long> _tagLinks;

        public Action<Product<TExternalKey>, SyncResult, Exception> OnSynchronized { get; set; }

        public ECommenceSync.Operations Operation => ECommenceSync.Operations.Products;

        public OperationModes Mode => OperationModes.Automatic;

        public OperationDirections Direction => OperationDirections.ErpToStore;

        public Guid Identifier => Guid.NewGuid();

        public OperationStatus Status { get; set; }

        public ProductOperation(IPrestashopDatabaseHelper databaseHelper, Dictionary<TExternalKey, TExternalKey> categoriesHierarchy)
        {
            _categoriesHierarchy = categoriesHierarchy;
            Status = OperationStatus.Created;
            _workQueue = new ConcurrentQueue<Product<TExternalKey>>();
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            _productFactory = new ProductFactory(databaseHelper.ApiUrl, databaseHelper.ApiSecret, "");
            _syncLanguage = databaseHelper.SyncLanguage;
            _ivaTaxRuleGroup = databaseHelper.TaxRuleGroup;
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
                await Task.Delay(1000);
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
            try
            {
                var idPrestashop = _links.ContainsKey(product.Id) ? _links[product.Id] : 0;
                if (idPrestashop ==0 && ( product.Weight <= 0 || product.StockAvailable <= 0 || product.Price <= 0))
                {
                    return Tuple.Create(SyncResult.NotSynchronized, default(Exception));
                }

                var idCategoriaPrestashop = _categoryLinks.ContainsKey(product.ParentId) ? _categoryLinks[product.ParentId] : 0;
                if (idCategoriaPrestashop == 0)
                {
                    product.RetryCount++;
                    return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception));
                }

                product.RetryCount = 0;

                long idMarcaPrestashop;
                if (product.BrandId is null)
                {
                    idMarcaPrestashop = 0;
                }
                else if (_brandsLinks.ContainsKey(product.BrandId.Value))
                {
                    idMarcaPrestashop = _brandsLinks[product.BrandId.Value];
                }
                else
                {
                    product.RetryCount++;
                    return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception));
                }

                product.TagIds ??=  "";

                var tags =  product.TagIds.Split(',').Where(s=> s.Length>0) .Select(id => Convert.ToInt32(id)).Cast<TExternalKey>().All(id => _tagLinks.ContainsKey(id));

                if (!tags)
                {
                    product.RetryCount++;
                    return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception));
                }


                
                Tuple<SyncResult, Exception> resultado;
                if (idPrestashop == 0)
                {
                    resultado = await AddProduct(product, idCategoriaPrestashop, idMarcaPrestashop);
                }
                else
                {
                    resultado = await UpdateProduct(product, idPrestashop, idCategoriaPrestashop, idMarcaPrestashop);
                    await AddOrUpdateLinkWeb(product.Id, product.LinkRewrite);
                }
                
                return resultado;
            }
            catch (Exception ex)
            {
                return Tuple.Create(SyncResult.Error, ex);
            }
        }

        private List<long> GetProductCategories(Product<TExternalKey> product)
        {
            var listado = new List<long>();
            if (_addAllParentCategoriesToProduct)
            {
                var idPrestashop = _categoryLinks[product.ParentId];
                var idExteno = product.ParentId;

                while (true)
                {
                    listado.Add(idPrestashop);
                    if (_categoriesHierarchy.ContainsKey(idExteno))
                    {
                        idExteno = _categoriesHierarchy[idExteno];
                        if (!_categoryLinks.ContainsKey(idExteno)) //La categoria padre no se migra a prestashop por lo tanto no existe en las categorias de prestashop
                        {
                            break;
                        }
                        idPrestashop = _categoryLinks[idExteno];
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                listado.Add(_categoryLinks[product.ParentId]);
            }
            return listado;
        }

        async Task<Tuple<SyncResult, Exception>> AddProduct(Product<TExternalKey> product, long idCategoriaPrestashop, long idMarcaPrestashop)
        {
            if (string.IsNullOrEmpty(product.LinkRewrite)) product.LinkRewrite = (product.Reference + " " + product.Name).GetStringForLinkRewrite();
            var prestashopProduct = new Bukimedia.PrestaSharp.Entities.product
            {
                additional_shipping_cost = 0,
                active = 1,
                advanced_stock_management = 0,
                available_for_order = 1,
                condition = product.Condition,
                show_condition = Convert.ToInt32(product.ShowCondition),
                price = product.Price,
                customizable = 0,
                depth = product.Depth,
                ean13 = product.EAN13,
                upc = product.UPC,
                ecotax = 0,
                height = product.Height,
                weight = product.Weight,
                id_category_default = idCategoriaPrestashop,
                supplier_reference = product.SupplierReference,
                isbn = product.ISBN,
                is_virtual = Convert.ToInt32(product.IsVirtual),
                minimal_quantity = 1,
                online_only = 0,
                width = product.Width,
                on_sale = 1,
                reference = product.Reference,
                state = 1,
                show_price = 1,
                wholesale_price = 0,
                visibility = "both",
                id_shop_default = 1,
                location = product.Location,
                id_manufacturer = idMarcaPrestashop,

            };

            prestashopProduct.AddName(new Bukimedia.PrestaSharp.Entities.AuxEntities.language { id = _syncLanguage, Value = product.Name.GetCleanStringForName() });
            prestashopProduct.description.Add(new Bukimedia.PrestaSharp.Entities.AuxEntities.language { id = _syncLanguage, Value = product.Description });
            prestashopProduct.description_short.Add(new Bukimedia.PrestaSharp.Entities.AuxEntities.language { id = _syncLanguage, Value = product.DescriptionShort });
            prestashopProduct.AddLinkRewrite(new Bukimedia.PrestaSharp.Entities.AuxEntities.language { id = _syncLanguage, Value = product.LinkRewrite });
            prestashopProduct.associations.categories.AddRange(GetProductCategories(product).Select(k => new Bukimedia.PrestaSharp.Entities.AuxEntities.category { id = k }));

            prestashopProduct.associations.tags.AddRange(
                    product.TagIds.Split(',')
                    .Where(s => s.Trim().Length>0)
                    .Select(id => Convert.ToInt32(id))
                    .Cast<TExternalKey>()
                    .Select(id => new Bukimedia.PrestaSharp.Entities.AuxEntities.tag { id = _tagLinks[id] }));


            if (product.HasTaxes)
            {
                prestashopProduct.id_tax_rules_group = _ivaTaxRuleGroup;
            }

            var (newPrestashopProduct, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                var tmp = await _productFactory.AddAsync(prestashopProduct);
                return tmp;
            }, MethodHelper.NotRetryOnBadrequest);

            if (error is null)
            {
                await AddLink(product.Id, newPrestashopProduct.id.Value);
                await AddOrUpdateLinkWeb(product.Id, product.LinkRewrite);
                return Tuple.Create(SyncResult.Created, error);
            }
            else
            {
                return Tuple.Create(SyncResult.Error, error);
            }


        }
        async Task<Tuple<SyncResult, Exception>> UpdateProduct(Product<TExternalKey> product, long idPrestashop, long idCategoriaPrestashop, long idMarcaPrestashop)
        {
            if (string.IsNullOrEmpty(product.LinkRewrite)) product.LinkRewrite = (product.Reference + " " + product.Name).GetStringForLinkRewrite();

            var (prestashopProduct, error) = await MethodHelper.ExecuteMethodAsync(async () => await _productFactory.GetAsync(idPrestashop), MethodHelper.NotRetryOnBadrequest);

            if (error is null)
            {
                prestashopProduct.name.Clear();
                prestashopProduct.description.Clear();
                prestashopProduct.description_short.Clear();
                prestashopProduct.link_rewrite.Clear();
                prestashopProduct.associations.categories.Clear();
                prestashopProduct.associations.tags.Clear();
                
                prestashopProduct.active = Convert.ToInt32(product.Active);
                prestashopProduct.id_manufacturer = idMarcaPrestashop;
                prestashopProduct.height = product.Height;
                prestashopProduct.weight = product.Weight;
                prestashopProduct.depth = product.Depth;
                prestashopProduct.width = product.Width;
                prestashopProduct.reference = product.Reference;
                prestashopProduct.price = product.Price;
                prestashopProduct.condition = product.Condition;
                prestashopProduct.show_condition = product.ShowCondition? 1:0;
                //prestashopProduct.position_in_category = product.PositionInCategory;
                if (prestashopProduct.price <=0)
                {
                    prestashopProduct.active = 0;
                }

                prestashopProduct.AddName(new Bukimedia.PrestaSharp.Entities.AuxEntities.language { id = _syncLanguage, Value = product.Name.GetCleanStringForName() });
                prestashopProduct.description.Add(new Bukimedia.PrestaSharp.Entities.AuxEntities.language { id = _syncLanguage, Value = product.Description });
                prestashopProduct.description_short.Add(new Bukimedia.PrestaSharp.Entities.AuxEntities.language { id = _syncLanguage, Value = product.DescriptionShort });
                prestashopProduct.AddLinkRewrite(new Bukimedia.PrestaSharp.Entities.AuxEntities.language { id = _syncLanguage, Value = product.LinkRewrite });
                prestashopProduct.associations.categories.AddRange(GetProductCategories(product).Select(k => new Bukimedia.PrestaSharp.Entities.AuxEntities.category { id = k }));
                
                
                prestashopProduct.associations.tags.AddRange(
                    product.TagIds.Split(',')
                    .Where(s => s.Trim().Length > 0)
                    .Select(id => Convert.ToInt32(id)).Cast<TExternalKey>()
                    .Select(id => new Bukimedia.PrestaSharp.Entities.AuxEntities.tag { id = _tagLinks[id] }));

                bool isOk;
                (isOk, error) = await MethodHelper.ExecuteMethodAsync(async () =>
                {
                    await _productFactory.UpdateAsync(prestashopProduct);
                    return true;
                }, MethodHelper.NotRetryOnBadrequest);

                return Tuple.Create(isOk ? SyncResult.Updated : SyncResult.Error, error);

            }
            else
            {
                return Tuple.Create(SyncResult.Error, error);
            }

        }

        private async Task AddLink(TExternalKey externalKey, long key)
        {
            using var conex = _databaseHelper.GetConnection();
            await conex.ExecuteAsync(SqlAgregarLink, new { ItemID = externalKey, PrestashopID = key });
            _links.AddOrUpdate(externalKey, key, (k, v) => v);
        }

        private async Task AddOrUpdateLinkWeb(TExternalKey id, string link)
        {
            using var conex = _databaseHelper.GetConnection();
            await conex.ExecuteAsync(SqlAddOrUpdateProductLink, new { ItemID = id, LinkWeb = link });
        }

    }
}