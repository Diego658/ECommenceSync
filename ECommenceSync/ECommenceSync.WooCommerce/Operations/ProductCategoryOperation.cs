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
    public class ProductCategoryOperation<TExternalKey> : IDestinationOperation<TExternalKey, ProductCategory<TExternalKey>>
        where TExternalKey : struct
    {
        const string SqlAddLink = "INSERT INTO StoreSync_Categorias_WooCommerce(CategoriaID, WooCommerceID) VALUES (@ExternalKey, @Key)";
        readonly ConcurrentQueue<ProductCategory<TExternalKey>> _workQueue;
        readonly IWooCommerceDatabaseHelper _databaseHelper;
        private readonly long _rootWoocategory;
        private readonly IWooCommerceOperationsHelper _operationsHelper;
        private readonly WCObject _wc;
        Task _taskProcessor;
        ConcurrentDictionary<TExternalKey, long> _links;

        public Action<ProductCategory<TExternalKey>, SyncResult, Exception> OnSynchronized { get; set; }

        public ECommenceSync.Operations Operation => ECommenceSync.Operations.ProductsCategories;

        public OperationModes Mode => OperationModes.Automatic;

        public OperationDirections Direction => OperationDirections.ErpToStore;

        public Guid Identifier => Guid.NewGuid();

        public OperationStatus Status { get; set; }

        public CancellationTokenSource TaskCancelTokenSource { get; set; }
        public CancellationTokenSource CancelTokenSource { get; set; }

        public ProductCategoryOperation(IWooCommerceDatabaseHelper databaseHelper, IWooCommerceOperationsHelper operationsHelper)
        {
            Status = OperationStatus.Created;
            _workQueue = new ConcurrentQueue<ProductCategory<TExternalKey>>();
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            var rest = new RestAPI($"{databaseHelper.ApiUrl}/v3/", databaseHelper.ApiUser, databaseHelper.ApiPassword);
            _wc = new WCObject(rest);
            _rootWoocategory = 0;
            _operationsHelper = operationsHelper;
        }

        public void AddWork(List<ProductCategory<TExternalKey>> values)
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
            _links = await _databaseHelper.GetCategoryLinks<TExternalKey>();
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
                while (_workQueue.TryDequeue(out var tag))
                {
                    var (result, ex) = await SyncCategory(tag);
                    OnSynchronized(tag, result, ex);
                }
                await Task.Delay(_operationsHelper.OperationsWorkQueueWaitTime);
            }
            Status = OperationStatus.Stopped;
        }

        async Task<Tuple<SyncResult, Exception>> SyncCategory(ProductCategory<TExternalKey> productCategory)
        {
            var idWoo = Convert.ToUInt64( _links.ContainsKey(productCategory.Id) ? _links[productCategory.Id] : 0);
            //var idPrestashopParent = productCategory.ParentId is null ? RootCategoryPrestashop : _links.ContainsKey(productCategory.ParentId.Value) ? _links[productCategory.ParentId.Value] : 0;
            var idWooParent = GetParentId(productCategory);
            if (idWooParent is null)
            {
                //La categoria padre no ha sido sincronizada, intentar luego.
                return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception));
            }
            Tuple<SyncResult, Exception> resultado;
            if (idWoo == 0)
            {
                resultado = await AddCategory(productCategory, idWooParent.Value);
            }
            else
            {
                resultado = await UpdateCategory(productCategory, idWoo, idWooParent.Value);
            }

            return resultado;
        }



        private long? GetParentId(ProductCategory<TExternalKey> productCategory)
        {
            long? idWooParent;
            if (productCategory.ParentId is null)
            {
                idWooParent = _rootWoocategory;
            }
            else
            {
                idWooParent = _links.ContainsKey(productCategory.ParentId.Value) ? _links[productCategory.ParentId.Value] : null;
            }
            return idWooParent;
        }

        async Task AddLink(TExternalKey externalKey, ulong key)
        {
            var lKey = Convert.ToInt64(key);
            using var conex = _databaseHelper.GetConnection();
            await conex.ExecuteAsync(SqlAddLink, new { ExternalKey = externalKey, Key = lKey });
            _links.TryAdd(externalKey, lKey);
        }

        private async Task<Tuple<SyncResult, Exception>> AddCategory(ProductCategory<TExternalKey> productCategory, long idWooParent)
        {
            var wooCategory = new ProductCategory
            {
                parent = Convert.ToUInt32(idWooParent),
                name = productCategory.Name,
                display = "default",
                description = productCategory.Description,
                menu_order = productCategory.Position,
                slug = productCategory.Name.GetStringForLinkRewrite(),
            };
            Exception error;
            (wooCategory, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                var tmp = await _wc.Category.Add(wooCategory);
                return tmp;
            }, 5, MethodHelper.TryAgainOnBadRequest, MethodHelper.StopsOnTermExist);

            if (error is null)
            {
                await AddLink(productCategory.Id, wooCategory.id.Value);
                return Tuple.Create(SyncResult.Created, default(Exception));
            }
            else
            {
                return Tuple.Create(SyncResult.Error, error);
            }

        }

        private async Task<Tuple<SyncResult, Exception>> UpdateCategory(ProductCategory<TExternalKey> productCategory, ulong idWoo, long idWooParent)
        {
            try
            {
                var wooCategory = await _wc.Category.Get(idWoo);
                wooCategory.name = productCategory.Name;
                wooCategory.description = productCategory.Description;
                wooCategory.slug = productCategory.Name.GetStringForLinkRewrite();
                Exception error;
                (wooCategory, error) = await MethodHelper.ExecuteMethodAsync(async () =>
                {
                    var tmp = await _wc.Category.Update(idWoo, wooCategory);
                    return tmp;
                }, 0, MethodHelper.TryAgainOnBadRequest);
                if (error is null)
                {
                    await AddLink(productCategory.Id, wooCategory.id.Value);
                    return Tuple.Create(SyncResult.Created, default(Exception));
                }
                else
                {
                    return Tuple.Create(SyncResult.Error, error);
                }
            }
            catch (Exception ex)
            {
                return Tuple.Create(SyncResult.Error, ex);
            }


        }
    }
}
