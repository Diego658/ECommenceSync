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
using System.Threading;
using System.Threading.Tasks;

namespace ECommenceSync.Prestashop.Operations
{
    public class ProductStocksOperation<TExternalKey> : IDestinationOperation<TExternalKey, ProductStock<TExternalKey>>
        where TExternalKey : struct
    {
        //readonly OperationStatus _status;
        readonly ConcurrentQueue<ProductStock<TExternalKey>> _workQueue;
        readonly IPrestashopDatabaseHelper _databaseHelper;
        readonly StockAvailableFactory _stocksFactory;
        readonly int _syncLanguage;
        CancellationTokenSource TaskCancelTokenSource { get; set; }
        CancellationTokenSource CancelTokenSource { get; set; }
        Task _taskProcessor;
        ConcurrentDictionary<TExternalKey, long> _productsLinks;
        ConcurrentDictionary<TExternalKey, long> _links;
        readonly ProductFactory _productFactory;
        public Action<ProductStock<TExternalKey>, SyncResult, Exception> OnSynchronized { get; set; }

        public ECommenceSync.Operations Operation => ECommenceSync.Operations.ProductStocks;

        public OperationModes Mode => OperationModes.Automatic;

        public OperationDirections Direction => OperationDirections.ErpToStore;

        public Guid Identifier => Guid.NewGuid();

        public OperationStatus Status { get; set; }

        public ProductStocksOperation(IPrestashopDatabaseHelper databaseHelper)
        {
            Status = OperationStatus.Created;
            _workQueue = new ConcurrentQueue<ProductStock<TExternalKey>>();
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            _stocksFactory = new StockAvailableFactory(databaseHelper.ApiUrl, databaseHelper.ApiSecret, "");
            _productFactory = new ProductFactory(databaseHelper.ApiUrl, databaseHelper.ApiSecret, "");
            _syncLanguage = databaseHelper.SyncLanguage;
        }

        public void AddWork(List<ProductStock<TExternalKey>> values)
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
                while (_workQueue.TryDequeue(out var stock))
                {
                    var (result, ex) = await SyncStock(stock);
                    OnSynchronized(stock, result, ex);
                    await Task.Delay(10);
                }
                await Task.Delay(1000);
            }
            Status = OperationStatus.Stopped;
        }
        async Task<Tuple<SyncResult, Exception>> SyncStock(ProductStock<TExternalKey> stock)
        {
            if (!_productsLinks.ContainsKey(stock.ProductId))
            {
                return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception));
            }
            if (_productsLinks[stock.ProductId] == long.MinValue)//Productos que por algun motivo(Sin existencia, sin peso, sin precio, etc) no seran migrados a la tienda
            {
                return Tuple.Create(SyncResult.NotSynchronized, default(Exception));
            }

            try
            {
                var productPrestashopId = _productsLinks[stock.ProductId];
                var idPrestashop = _links.ContainsKey(stock.Id) ? _links[stock.Id] : 0;
                if (idPrestashop == 0)
                {
                    return await AddStock(stock, productPrestashopId);
                }
                else
                {
                    return await UpdateStock(stock, productPrestashopId, idPrestashop);

                }
            }
            catch (Exception ex)
            {
                return Tuple.Create(SyncResult.Error, ex);
            }
            
        }

        private async Task<Tuple<SyncResult, Exception>> UpdateStock(ProductStock<TExternalKey> stock, long productPrestashopId, long idPrestashop)
        {
            var (isOk, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                var stockAvailable = new stock_available
                {
                    id = idPrestashop,
                    id_product = productPrestashopId,
                    quantity = Convert.ToInt32(Math.Floor(stock.Existencia)),
                    id_product_attribute = 0,
                    id_shop = 1,
                    id_shop_group = 0,
                    depends_on_stock = 0,
                    out_of_stock = 2
                };
                await _stocksFactory.UpdateAsync(stockAvailable);
                return true;
            }, MethodHelper.NotRetryOnBadrequest);

            return Tuple.Create(isOk ? SyncResult.Updated : SyncResult.Error, error);
        }

        private async Task<Tuple<SyncResult, Exception>> AddStock(ProductStock<TExternalKey> stock, long productPrestashopId)
        {
            var (id, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                var productoPrestashop = await _productFactory.GetAsync(productPrestashopId);
                var stockAvailableId = productoPrestashop.associations.stock_availables[0].id;
                var stockAvailable = new stock_available
                {
                    id = stockAvailableId,
                    id_product = productPrestashopId,
                    quantity = Convert.ToInt32(Math.Floor(stock.Existencia)),
                    id_product_attribute = 0,
                    id_shop = 1,
                    id_shop_group = 0,
                    depends_on_stock = 0,
                    out_of_stock = 2
                };
                await _stocksFactory.UpdateAsync(stockAvailable);
                return stockAvailableId;
            }, MethodHelper.NotRetryOnBadrequest);

            if (error is null)
            {
                await AddLink(stock.Id, id);
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
            await conex.ExecuteAsync("INSERT INTO StoreSync_Productos_Existencias_Prestashop(ExistenciaID, PrestashopID) VALUES (@ExistenciaID, @PrestashopID)", 
                new { ExistenciaID = externalKey, PrestashopID = key });
            _links.TryAdd(externalKey, key);
        }

        private async Task<bool> LoadLinks()
        {
            _productsLinks = await _databaseHelper.GetProductsLinks<TExternalKey>();
            _links = new ConcurrentDictionary<TExternalKey, long>();

            using var sqlConex = (SqlConnection)_databaseHelper.GetConnection();
            using var cmd = sqlConex.CreateCommand();
            cmd.CommandText = "SELECT ExistenciaID, PrestashopID FROM StoreSync_Productos_Existencias_Prestashop";
            await sqlConex.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                _links.TryAdd(reader.GetFieldValue<TExternalKey>(0), reader.GetInt64(1));
            }


            return true;
        }

    }
}
