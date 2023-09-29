using Bukimedia.PrestaSharp.Factories;
using Dapper;
using ECommenceSync.Entities;
using ECommenceSync.Interfaces;
using ECommenceSync.Prestashop.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace ECommenceSync.Prestashop.Operations
{
    public class ProductCategoryOperation<TExternalKey> : IDestinationOperation<TExternalKey, ProductCategory<TExternalKey>>
        where TExternalKey : struct
    {
        const long RootCategoryPrestashop = 2;
        const string SqlAddLink = "INSERT INTO StoreSync_Categorias_Prestashop(CategoriaID, PrestashopID) VALUES (@ExternalKey, @Key)";
        readonly int _syncLanguage;
        readonly ConcurrentQueue<ProductCategory<TExternalKey>> _workQueue;
        readonly IPrestashopDatabaseHelper _databaseHelper;
        readonly CategoryFactory _categoryFactory;
        Task _taskProcessor;
        ConcurrentDictionary<TExternalKey, long> _links;
        readonly Dictionary<char, char> _descriptioncharsRewrite = new Dictionary<char, char>();
        public Action<ProductCategory<TExternalKey>, SyncResult, Exception> OnSynchronized { get; set; }

        public ECommenceSync.Operations Operation => ECommenceSync.Operations.ProductsCategories;

        public OperationModes Mode => OperationModes.Automatic;

        public OperationDirections Direction => OperationDirections.ErpToStore;

        public Guid Identifier => Guid.NewGuid();

        public OperationStatus Status { get; set; }

        public CancellationTokenSource TaskCancelTokenSource { get; set; }
        public CancellationTokenSource CancelTokenSource { get; set; }

        public ProductCategoryOperation(IPrestashopDatabaseHelper databaseHelper)
        {
            Status = OperationStatus.Created;
            _workQueue = new ConcurrentQueue<ProductCategory<TExternalKey>>();
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            _categoryFactory = new CategoryFactory(databaseHelper.ApiUrl, databaseHelper.ApiSecret, "");
            _syncLanguage = databaseHelper.SyncLanguage;
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
                await Task.Delay(1000);
            }
            Status = OperationStatus.Stopped;
        }
        async Task<Tuple<SyncResult, Exception>> SyncCategory(ProductCategory<TExternalKey> productCategory)
        {
            var idPrestashop = _links.ContainsKey(productCategory.Id) ? _links[productCategory.Id] : 0;
            var idPrestashopParent = productCategory.ParentId is null ? RootCategoryPrestashop : _links.ContainsKey(productCategory.ParentId.Value) ? _links[productCategory.ParentId.Value] : 0;


            if (idPrestashopParent == 0)
            {
                //La categoria padre no ha sido sincronizada, intentar luego.
                return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception));
            }

            Tuple<SyncResult, Exception> resultado;

            if (idPrestashop == 0)
            {
                resultado = await AddCategory(productCategory, idPrestashopParent);
            }
            else
            {
                resultado = await UpdateCategory(productCategory, idPrestashop, idPrestashopParent);
            }

            return resultado;

        }
        async Task AddLink(TExternalKey externalKey, long key)
        {
            using var conex = _databaseHelper.GetConnection();
            await conex.ExecuteAsync(SqlAddLink, new { ExternalKey = externalKey, Key = key });
            _links.TryAdd(externalKey, key);
        }
        async Task<Tuple<SyncResult, Exception>> AddCategory(ProductCategory<TExternalKey> productCategory, long idPrestashopParent)
        {
            var prestashopCategory = new Bukimedia.PrestaSharp.Entities.category
            {
                id_parent = idPrestashopParent,
                active = Convert.ToInt32(productCategory.Active),
                id_shop_default = 1,
                is_root_category = 0,
                position = 0
            };


            prestashopCategory.
                AddName(new Bukimedia.PrestaSharp.Entities.AuxEntities.language { id = _syncLanguage, Value = productCategory.Name.GetCleanStringForName() });

            prestashopCategory.
                AddDescription(new Bukimedia.PrestaSharp.Entities.AuxEntities.language { id = _syncLanguage, Value = productCategory.Description.GetCleanString(_descriptioncharsRewrite) });

            prestashopCategory.AddLinkRewrite(new Bukimedia.PrestaSharp.Entities.AuxEntities.language { id = _syncLanguage, Value = productCategory.Name.GetStringForLinkRewrite() });

            prestashopCategory.associations.categories.Add(new Bukimedia.PrestaSharp.Entities.AuxEntities.category { id = idPrestashopParent });

            Exception error;

            (prestashopCategory, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                var tmp = await _categoryFactory.AddAsync(prestashopCategory);
                return tmp;
            }, MethodHelper.NotRetryOnBadrequest);

            if (error is null)
            {
                await AddLink(productCategory.Id, prestashopCategory.id.Value);
                return Tuple.Create(SyncResult.Created, default(Exception));
            }
            else
            {
                return Tuple.Create(SyncResult.Error, error);
            }

        }
        async Task<Tuple<SyncResult, Exception>> UpdateCategory(ProductCategory<TExternalKey> productCategory, long idPrestashop, long idPrestashopParent)
        {
            var (prestashopCategory, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                var tmp = await _categoryFactory.GetAsync(idPrestashop);
                return tmp;
            }, MethodHelper.NotRetryOnBadrequest);

            if (error is null)
            {
                prestashopCategory.name.Clear();
                prestashopCategory.description.Clear();
                prestashopCategory.link_rewrite.Clear();
                prestashopCategory.associations.categories.Clear();

                prestashopCategory.
                AddName(new Bukimedia.PrestaSharp.Entities.AuxEntities.language { id = _syncLanguage, Value = productCategory.Name.GetCleanStringForName() });

                prestashopCategory.
                    AddDescription(new Bukimedia.PrestaSharp.Entities.AuxEntities.language { id = _syncLanguage, Value = productCategory.Description.GetCleanString(_descriptioncharsRewrite) });

                prestashopCategory.AddLinkRewrite(new Bukimedia.PrestaSharp.Entities.AuxEntities.language { id = _syncLanguage, Value = productCategory.Name.GetStringForLinkRewrite() });

                prestashopCategory.associations.categories.Add(new Bukimedia.PrestaSharp.Entities.AuxEntities.category { id = idPrestashopParent });

                

                bool isOk;
                (isOk, error) = await MethodHelper.ExecuteMethodAsync(async () =>
                {
                    await _categoryFactory.UpdateAsync(prestashopCategory);
                    return true;
                }, MethodHelper.NotRetryOnBadrequest);


                return Tuple.Create(isOk ? SyncResult.Updated : SyncResult.Error, isOk ? default : error);

            }
            else
            {
                return Tuple.Create(SyncResult.Error, error);
            }


        }


    }
}
