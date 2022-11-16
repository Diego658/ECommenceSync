using Bukimedia.PrestaSharp.Entities;
using Bukimedia.PrestaSharp.Factories;
using Dapper;
using ECommenceSync.Entities;
using ECommenceSync.Interfaces;
using ECommenceSync.Prestashop.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ECommenceSync.Prestashop.Operations
{
    public class ProductSpecificPricesOperation<TExternalKey> : IDestinationOperation<TExternalKey, ProductPrice<TExternalKey>>
        where TExternalKey : struct
    {
        CancellationTokenSource TaskCancelTokenSource { get; set; }
        CancellationTokenSource CancelTokenSource { get; set; }
        Task _taskProcessor;
        readonly ConcurrentQueue<ProductPrice<TExternalKey>> _workQueue;
        readonly IPrestashopDatabaseHelper _databaseHelper;
        readonly SpecificPriceFactory _specificPricesFactory;
        readonly int _syncLanguage;
        ConcurrentDictionary<TExternalKey, long> _links;
        ConcurrentDictionary<TExternalKey, long> _productLinks;
        readonly Dictionary<string, TExternalKey> _erpClientGroups;
        readonly Dictionary<TExternalKey, string> _erpClientGroupsReverse;
        readonly IPrestashopOperationsHelper _operationsHelper;
        Dictionary<string, long> _pricesToSync;
        Dictionary<long, string> _pricesToSyncReverse;
        readonly int _maxRetryCount;


        public Action<ProductPrice<TExternalKey>, SyncResult, Exception> OnSynchronized { get; set; }

        public ECommenceSync.Operations Operation => ECommenceSync.Operations.ProductPrices;

        public OperationModes Mode => OperationModes.Automatic;

        public OperationDirections Direction => OperationDirections.ErpToStore;

        public Guid Identifier => Guid.NewGuid();

        public OperationStatus Status { get; set; }


        public ProductSpecificPricesOperation(IPrestashopDatabaseHelper databaseHelper, IPrestashopOperationsHelper operationsHelper, Dictionary<string, TExternalKey> erpClientGroups)
        {
            _operationsHelper = operationsHelper;
            _erpClientGroups = erpClientGroups;
            _erpClientGroupsReverse = new Dictionary<TExternalKey, string>(erpClientGroups.Select(kv => new KeyValuePair<TExternalKey, string>(kv.Value, kv.Key)));
            Status = OperationStatus.Created;
            _workQueue = new ConcurrentQueue<ProductPrice<TExternalKey>>();
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            _specificPricesFactory = new SpecificPriceFactory(databaseHelper.ApiUrl, databaseHelper.ApiSecret, "");
            _syncLanguage = databaseHelper.SyncLanguage;
            _maxRetryCount = operationsHelper.GetMaxRetryCount(Operation);
            LoadPricesToSync();
        }


        void LoadPricesToSync()
        {

            _pricesToSync = new Dictionary<string, long>();
            var specificPriccesSection = _operationsHelper.Configuration.GetSection("SpecificPrices");

            foreach (var priceSection in specificPriccesSection.GetChildren())
            {
                _pricesToSync.Add(priceSection.Key, long.Parse(priceSection.GetSection("IdGrupo").Value));
            }

            _pricesToSyncReverse = new Dictionary<long, string>(_pricesToSync.Select(kv => new KeyValuePair<long, string>(kv.Value, kv.Key)));


        }


        public void AddWork(List<ProductPrice<TExternalKey>> values)
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
            if (_erpClientGroups is null || _erpClientGroups.Count == 0)
            {
                Status = OperationStatus.Stopped;
                return;
            }
            if (!await LoadLinks())
            {
                return;
            }

            Status = OperationStatus.Working;
            while (!CancelTokenSource.IsCancellationRequested)
            {
                while (_workQueue.TryDequeue(out var tag))
                {
                    var (result, ex) = await SyncPrice(tag);
                    OnSynchronized(tag, result, ex);
                    await Task.Delay(10);
                }
                await Task.Delay(1000);
            }
            Status = OperationStatus.Stopped;
        }


        private async Task<bool> LoadLinks()
        {
            _links = new ConcurrentDictionary<TExternalKey, long>();
            using var sqlConex = _databaseHelper.GetConnection();
            sqlConex.Open();
            using var command = (SqlCommand)sqlConex.CreateCommand();
            command.CommandText = "SELECT PrecioID, PrestashopID FROM StoreSync_Productos_Precios_Prestashop";
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                _links.TryAdd(reader.GetFieldValue<TExternalKey>(0), reader.GetInt64(1));
            }
            _productLinks = await _databaseHelper.GetProductsLinks<TExternalKey>();
            return true;
        }
        async Task<Tuple<SyncResult, Exception>> SyncPrice(ProductPrice<TExternalKey> price)
        {
            if (!_erpClientGroupsReverse.ContainsKey(price.ClientGroupId))
            {
                return Tuple.Create(SyncResult.NotSynchronized, default(Exception));
            }

            var erpGroupName = _erpClientGroupsReverse[price.ClientGroupId];
            //var erpGroupId = _erpClientGroups[erpGroupName];

            if (!_pricesToSync.ContainsKey(erpGroupName))
            {
                return Tuple.Create(SyncResult.Error, new Exception($"El erp define el grupo de sincronización {erpGroupName} que no se encuentra en la configuracion de la tienda, corrija la configuracion y reintente todos los pendientes"));
            }


            if (!_productLinks.ContainsKey(price.ParentId))
            {
                if (price.RetryCount > _maxRetryCount)
                {
                    return Tuple.Create(SyncResult.NotSynchronized, default(Exception));
                }
                else
                {
                    price.RetryCount++;
                    return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception));
                }
                
            }
            var idItemPrestashop = _productLinks[price.ParentId];
            if (idItemPrestashop == long.MinValue)//Productos que por algun motivo(Sin existencia, sin peso, sin precio, etc) no seran migrados a la tienda
            {
                return Tuple.Create(SyncResult.NotSynchronized, default(Exception));
            }


            var idPrestashop = _links.ContainsKey(price.Id) ? _links[price.Id] : 0;
            var idGrupoPrestashop = _pricesToSync[erpGroupName];

            try 
            { 
                if (idPrestashop == 0)
                {
                    return await AddPrice(price, idItemPrestashop, idGrupoPrestashop);
                }
                else
                {
                    return await UpdatePrice(price, idPrestashop, idItemPrestashop, idGrupoPrestashop);
                }
            }
            catch (Exception ex)
            {
                return Tuple.Create(SyncResult.Error, ex);
            }

        }

        private async Task AddLink(TExternalKey externalKey, long key)
        {
            using var conex = _databaseHelper.GetConnection();
            await conex.ExecuteAsync("INSERT INTO StoreSync_Productos_Precios_Prestashop(PrecioID, PrestashopID) VALUES(@PrecioID, @PrestashopID)", 
                new { PrecioID = externalKey, PrestashopID = key });
            _links.TryAdd(externalKey, key);
        }

        async Task<Tuple<SyncResult, Exception>> AddPrice(ProductPrice<TExternalKey> price, long idItemPrestashop, long idGrupoPrestashop)
        {
            var prestashoPrice = new specific_price
            {
                id = 0,
                id_shop_group = 0,
                id_shop = 1,
                id_cart = 0,
                id_product = idItemPrestashop,
                id_product_attribute = 0,
                id_currency = 0,
                id_country = 0,
                id_group = idGrupoPrestashop,
                id_customer = 0,
                id_specific_price_rule = 0,
                price = price.Price,
                from_quantity = 1,
                reduction = 0,
                reduction_tax = 1,
                reduction_type = "amount"
            };

            Exception error;
             (prestashoPrice, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                return await _specificPricesFactory.AddAsync(prestashoPrice);
            }, MethodHelper.NotRetryOnBadrequest);

            if (error is null)
            {
                await AddLink(price.Id, prestashoPrice.id.Value);
                return (Tuple.Create(SyncResult.Created, error));
            }
            else
            {
                return (Tuple.Create(SyncResult.Error, error));
            }

            


        }
        async Task<Tuple<SyncResult, Exception>> UpdatePrice(ProductPrice<TExternalKey> price, long idPrestashop, long idItemPrestashop, long idGrupoPrestashop)
        {
            var prestashoPrice = new specific_price
            {
                id = idPrestashop,
                id_shop_group = 0,
                id_shop = 1,
                id_cart = 0,
                id_product = idItemPrestashop,
                id_product_attribute = 0,
                id_currency = 0,
                id_country = 0,
                id_group = idGrupoPrestashop,
                id_customer = 0,
                id_specific_price_rule = 0,
                price = price.Price,
                from_quantity = 1,
                reduction = 0,
                reduction_tax = 1,
                reduction_type = "amount"
            };

            Exception error;
            bool isOk;
            (isOk, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                await _specificPricesFactory.UpdateAsync(prestashoPrice);
                return true;
            }, MethodHelper.NotRetryOnBadrequest);

            return (Tuple.Create(isOk? SyncResult.Updated: SyncResult.Error, error));

        }






    }
}
