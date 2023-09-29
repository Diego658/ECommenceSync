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
    public class ImagesOperation<TExternalKey> : IDestinationOperation<TExternalKey, EntityImage<TExternalKey>>
        where TExternalKey : struct
    {
        private CancellationTokenSource TaskCancelTokenSource;
        private CancellationTokenSource CancelTokenSource;
        private Task _taskProcessor;
        readonly ConcurrentQueue<EntityImage<TExternalKey>> _workQueue;
        readonly IPrestashopDatabaseHelper _databaseHelper;
        readonly ImageFactory _imagesFactory;
        readonly ProductFactory _productFactory;
        readonly CategoryFactory _categoryfactory;
        ConcurrentDictionary<TExternalKey, long> _productLinks;
        ConcurrentDictionary<TExternalKey, long> _categoryLinks;
        ConcurrentDictionary<TExternalKey, long> _brandLinks;
        ConcurrentDictionary<TExternalKey, long> _producImagesLinks;
        public Action<EntityImage<TExternalKey>, SyncResult, Exception> OnSynchronized { get; set; }

        public ECommenceSync.Operations Operation => ECommenceSync.Operations.ProductImages;

        public OperationModes Mode => OperationModes.Automatic;

        public OperationDirections Direction => OperationDirections.ErpToStore;

        public Guid Identifier => Guid.NewGuid();

        public OperationStatus Status { get; set; }

        public ImagesOperation(IPrestashopDatabaseHelper databaseHelper)
        {
            Status = OperationStatus.Created;
            _workQueue = new ConcurrentQueue<EntityImage<TExternalKey>>();
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            _productFactory = new ProductFactory(databaseHelper.ApiUrl, databaseHelper.ApiSecret, "");
            _imagesFactory = new ImageFactory(databaseHelper.ApiUrl, databaseHelper.ApiSecret, "");
            _categoryfactory = new CategoryFactory(databaseHelper.ApiUrl, databaseHelper.ApiSecret, "");
        }

        public void AddWork(List<EntityImage<TExternalKey>> values)
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
                    var (result, ex) = await SyncImage(brand);
                    OnSynchronized(brand, result, ex);
                    await Task.Delay(10);
                }
                await Task.Delay(1000);
            }
            Status = OperationStatus.Stopped;
        }

        private async Task<bool> LoadLinks()
        {
            _productLinks = await _databaseHelper.GetProductsLinks<TExternalKey>();
            _categoryLinks = await _databaseHelper.GetCategoryLinks<TExternalKey>();
            _brandLinks = await _databaseHelper.GetBrandsLinks<TExternalKey>();
            _producImagesLinks = new ConcurrentDictionary<TExternalKey, long>();
            using var sqlConex = (SqlConnection)_databaseHelper.GetConnection();
            using var cmd = sqlConex.CreateCommand();
            cmd.CommandText = "SELECT ImageID, PrestashopID FROM StoreSync_ImagenesItems_Prestashop";
            await sqlConex.OpenAsync();
            var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                _producImagesLinks.TryAdd(reader.GetFieldValue<TExternalKey>(0), reader.GetInt64(1));
            }

            return true;
        }

        async Task<Tuple<SyncResult, Exception>> SyncImage(EntityImage<TExternalKey> image)
        {
            try
            {
                return image.ImageType switch
                {
                    ImageTypes.Producto => await SyncProductImage(image),
                    ImageTypes.Categoria => await SyncCategoryImage(image),
                    _ => throw new ArgumentOutOfRangeException(nameof(image.ImageType)),
                };
            }
            catch (Exception ex)
            {
                return (Tuple.Create(SyncResult.Error, ex));
            }
            
        }
        async Task<Tuple<SyncResult, Exception>> SyncProductImage(EntityImage<TExternalKey> image)
        {
            var idPrestashop = _producImagesLinks.ContainsKey(image.Id) ? _producImagesLinks[image.Id] : 0;
            if (!_productLinks.ContainsKey(image.ParentId))
            {
                return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception));
            }
            if (_productLinks[image.ParentId] == long.MinValue)//Productos que por algun motivo(Sin existencia, sin peso, sin precio, etc) no seran migrados a la tienda
            {
                return Tuple.Create(SyncResult.NotSynchronized, default(Exception));
            }

            if (idPrestashop == 0)
            {
                if (image.Blob is null)
                {
                    return Tuple.Create(SyncResult.Error, new Exception($"La imagen {image.Id} no tiene acceso al blob!!!"));
                }


                var (id, error) = await MethodHelper.ExecuteMethodAsync(async () =>
                {
                    using var stream = await image.Blob.GetStream();
                    var length = Convert.ToInt32(stream.Length);
                    var buffer = new byte[length];
                    stream.Read(buffer, 0, length);
                    var id = await _imagesFactory.AddProductImageAsync(_productLinks[image.ParentId], buffer);
                    buffer = null;
                    return id;
                }, MethodHelper.NotRetryOnBadrequest);

                if (error is null)
                {
                    await AddLink(image.Id, id);
                    return Tuple.Create(SyncResult.Created, error);
                }
                else
                {
                    return Tuple.Create(SyncResult.Error, error);
                }


            }
            else
            {
                if (image.IsDefault)
                {
                    var result = await MethodHelper.ExecuteMethodAsync(async () =>
                    {
                        var product = await _productFactory.GetAsync(_productLinks[image.ParentId]);
                        product.id_default_image = idPrestashop;
                        await _productFactory.UpdateAsync(product);
                        return true;
                    }, MethodHelper.NotRetryOnBadrequest);
                    return Tuple.Create(result.Item1 ? SyncResult.Updated : SyncResult.Error, result.Item2);
                }
                else if (image.Operation == ImageOperations.Delete)
                {
                    await MethodHelper.ExecuteMethodAsync(async () =>
                    {
                        await _imagesFactory.DeleteProductImageAsync(_productLinks[image.ParentId], idPrestashop);
                        return true;
                    }, MethodHelper.NotRetryOnBadrequest);
                    return Tuple.Create(SyncResult.Updated, default(Exception));
                }
                else
                {
                    return Tuple.Create(SyncResult.Updated, default(Exception));
                }
            }

        }
        async Task AddLink(TExternalKey externalId, long psId)
        {
            using var conex = _databaseHelper.GetConnection();
            await conex.ExecuteAsync("INSERT INTO StoreSync_ImagenesItems_Prestashop(ImageID, PrestashopID) VALUES (@ImageID, @PrestashopID)",
                new { ImageID = externalId, PrestashopID = psId });
            _producImagesLinks.TryAdd(externalId, psId);
        }





        async Task<Tuple<SyncResult, Exception>> SyncCategoryImage(EntityImage<TExternalKey> image)
        {
            var idCategoriaPrestashop = _categoryLinks.ContainsKey(image.ParentId) ? _categoryLinks[image.ParentId] : 0;

            if (idCategoriaPrestashop ==0)
            {
                return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception));
            }

            await MethodHelper.ExecuteMethodAsync(async () => 
            {
                await _imagesFactory.DeleteCategoryImageAsync(idCategoriaPrestashop);
                return true;
            }, MethodHelper.NotRetryOnBadrequest);

            if (image.Operation == ImageOperations.Add)
            {
                var (isOk, error) = await MethodHelper.ExecuteMethodAsync(async () =>
                 {
                     using var stream = await image.Blob.GetStream();
                     var length = Convert.ToInt32(stream.Length);
                     var buffer = new byte[length];
                     stream.Read(buffer, 0, length);
                     await _imagesFactory.AddCategoryImageAsync(idCategoriaPrestashop, buffer);
                     buffer = null;
                     return true;
                 }, MethodHelper.NotRetryOnBadrequest);

                return Tuple.Create(isOk ? SyncResult.Updated : SyncResult.Error, error);
            }
            else if (image.Operation == ImageOperations.Delete)
            {
                await MethodHelper.ExecuteMethodAsync(async () =>
                {
                    await _imagesFactory.DeleteCategoryImageAsync(idCategoriaPrestashop);
                    return true;
                }, MethodHelper.NotRetryOnBadrequest);
                return Tuple.Create(SyncResult.Updated, default(Exception));
            }
            else
            {
                return Tuple.Create(SyncResult.Updated, default(Exception));
            }

        }

    }
}
