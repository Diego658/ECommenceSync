using Bukimedia.PrestaSharp.Entities;
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
    public class BrandsOperation<TExternalKey> : IDestinationOperation<TExternalKey, Brand<TExternalKey>>
        where TExternalKey : struct
    {
        private const string SqlAgregarLink = "INSERT INTO StoreSync_Marcas_Prestashop (MarcaId, PrestashopID) VALUES (@MarcaId, @PrestashopID)";
        private CancellationTokenSource TaskCancelTokenSource;
        private CancellationTokenSource CancelTokenSource;
        private Task _taskProcessor;

        public Action<Brand<TExternalKey>, SyncResult, Exception> OnSynchronized { get; set; }

        public ECommenceSync.Operations Operation => ECommenceSync.Operations.Brands;

        public OperationModes Mode => OperationModes.Automatic;

        public OperationDirections Direction => OperationDirections.ErpToStore;

        public Guid Identifier => Guid.NewGuid();

        public OperationStatus Status { get; private set; }

        readonly ConcurrentQueue<Brand<TExternalKey>> _workQueue;
        private ConcurrentDictionary<TExternalKey, long> _links;
        private readonly IPrestashopDatabaseHelper databaseHelper;
        private readonly ManufacturerFactory _manufacturerFactory;
        readonly ImageFactory _imagesFactory;
        public BrandsOperation(IPrestashopDatabaseHelper databaseHelper)
        {
            Status = OperationStatus.Created;
            _workQueue = new ConcurrentQueue<Brand<TExternalKey>>();
            this.databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            _manufacturerFactory = new ManufacturerFactory(databaseHelper.ApiUrl, databaseHelper.ApiSecret, "");
            _imagesFactory = new ImageFactory(databaseHelper.ApiUrl, databaseHelper.ApiSecret, "");
        }

        public void AddWork(List<Brand<TExternalKey>> values)
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
                while (_workQueue.TryDequeue(out var brand))
                {
                    var (result, ex) = await SyncBrand(brand);
                    OnSynchronized(brand, result, ex);
                }
                await Task.Delay(1000);
            }
            Status = OperationStatus.Stopped;
        }

        private async Task<bool> LoadLinks()
        {
            _links = await databaseHelper.GetBrandsLinks<TExternalKey>();
            return true;
        }


        private async Task<Tuple<SyncResult, Exception>> SyncBrand(Brand<TExternalKey> brand)
        {
            var idPrestashop = _links.ContainsKey(brand.Id) ? _links[brand.Id] : 0;
            Tuple<SyncResult, Exception> resultado;
            if (idPrestashop == 0)
            {
                resultado = await AddBrand(brand);
            }
            else
            {
                resultado = await UpdateBrand(brand, idPrestashop);
            }
            return resultado;
        }



        private async Task<Tuple<SyncResult, Exception>> AddBrand(Brand<TExternalKey> brand)
        {
            var manufacturer = new manufacturer
            {
                name = brand.Name.GetCleanStringForName(),
                active = 1,
            };
            var (newManufacturer, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                var tmp = await _manufacturerFactory.AddAsync(manufacturer);
                return tmp;
            }, MethodHelper.NotRetryOnBadrequest);

            if (error is null)
            {
                await AddLink(brand.Id, newManufacturer.id.Value);
                bool isOk;
                (isOk, error) = await UploadImage(brand, newManufacturer.id.Value, false);
                return Tuple.Create(isOk ? SyncResult.Created : SyncResult.Error, isOk ? default : error);
            }
            else
            {
                return Tuple.Create(SyncResult.Error, error);
            }
        }


        private async Task<Tuple<bool, Exception>> UploadImage(Brand<TExternalKey> brand, long idPrestashop, bool tryDelete = true)
        {
            if (tryDelete)
            {
                await MethodHelper.ExecuteMethodAsync(async () =>
                {
                    await _imagesFactory.DeleteManufacturerImageAsync(idPrestashop);
                    return true;
                }, MethodHelper.NotRetryOnBadrequest);
            }

            if (brand.ImageBlob is null)
            {
                return Tuple.Create(true, default(Exception));
            }

            var (isOk, error) = await MethodHelper.ExecuteMethodAsync(async () =>
             {
                 var stream = await brand.ImageBlob.GetStream();
                 var length = Convert.ToInt32(stream.Length);
                 var buffer = new byte[length];
                 stream.Read(buffer, 0, length);
                 await _imagesFactory.AddManufacturerImageAsync(idPrestashop, buffer);
                 return true;
             }, MethodHelper.NotRetryOnBadrequest);

            return Tuple.Create(isOk, error);
        }


        private async Task<Tuple<SyncResult, Exception>> UpdateBrand(Brand<TExternalKey> brand, long idPrestashop)
        {
            var manufacturer = new manufacturer
            {
                name = brand.Name.GetCleanStringForName(),
                active = 1,
                id = idPrestashop
            };
            var (isOk, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                await _manufacturerFactory.UpdateAsync(manufacturer);
                return true;
            }, MethodHelper.NotRetryOnBadrequest);

            if (isOk)
            {
                (isOk, error) = await UploadImage(brand, idPrestashop);
                return Tuple.Create(isOk ? SyncResult.Updated : SyncResult.Error, isOk ? default : error);
            }
            else
            {
                return Tuple.Create(SyncResult.Error, error);
            }

        }


        private async Task AddLink(TExternalKey externalKey, long key)
        {
            using var conex = databaseHelper.GetConnection();
            await conex.ExecuteAsync(SqlAgregarLink, new { MarcaId = externalKey, PrestashopID = key });
            _links.TryAdd(externalKey, key);
        }

    }
}
