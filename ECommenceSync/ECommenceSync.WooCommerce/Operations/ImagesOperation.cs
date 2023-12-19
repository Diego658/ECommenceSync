using Dapper;
using ECommenceSync.Entities;
using ECommenceSync.Interfaces;
using ECommenceSync.WooCommerce.Helpers;
using ECommenceSync.WooCommerce.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using WordPressPCL;


namespace ECommenceSync.WooCommerce.Operations
{
    public class ImagesOperation<TExternalKey> : IDestinationOperation<TExternalKey, EntityImage<TExternalKey>>
        where TExternalKey : struct
    {
        ConcurrentDictionary<TExternalKey, long> _productLinks;
        ConcurrentDictionary<TExternalKey, long> _categoryLinks;
        ConcurrentDictionary<TExternalKey, long> _brandLinks;
        private ConcurrentDictionary<TExternalKey, WooProductVariationVsVariants<TExternalKey>> _productVariationsVsVariants;
        ConcurrentDictionary<TExternalKey, long> _producImagesLinks;
        private readonly WCObject _wc;
        private readonly  WordPressClient _wp;
        private  CancellationTokenSource _taskCancelTokenSource;
        private CancellationTokenSource _cancelTokenSource;
        private Task _taskProcessor;
        readonly ConcurrentQueue<EntityImage<TExternalKey>> _workQueue;
        private readonly IWooCommerceDatabaseHelper _databaseHelper;

        public Action<EntityImage<TExternalKey>, SyncResult, Exception> OnSynchronized { get; set; }

        public ECommenceSync.Operations Operation => ECommenceSync.Operations.ProductImages;

        public OperationModes Mode => OperationModes.Automatic;

        public OperationDirections Direction => OperationDirections.ErpToStore;

        public Guid Identifier => Guid.NewGuid();

        public OperationStatus Status { get; set; }
        public ImagesOperation(IWooCommerceDatabaseHelper databaseHelper)
        {
            Status = OperationStatus.Created;
            _workQueue = new ConcurrentQueue<EntityImage<TExternalKey>>();
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            var rest = new RestAPI($"{databaseHelper.ApiUrl}/v2/", databaseHelper.ApiUser, databaseHelper.ApiPassword);
            _wc = new WCObject(rest);
            _wp = new WordPressClient(databaseHelper.ApiUrlWordpress);
            _wp.Auth.UseBasicAuth(databaseHelper.ApiWpAppUser, databaseHelper.ApiWpAppPwd);

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
            _taskCancelTokenSource = new CancellationTokenSource();
            _cancelTokenSource = new CancellationTokenSource();
            _taskProcessor = new Task(async () => await Work(), _taskCancelTokenSource.Token, TaskCreationOptions.LongRunning);
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
            while (!_cancelTokenSource.IsCancellationRequested)
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
            _productVariationsVsVariants = await _databaseHelper.GetProductVariationsVsVariants<TExternalKey>();
            _producImagesLinks =  new ConcurrentDictionary<TExternalKey, long>();
            using var sqlConex = (SqlConnection)_databaseHelper.GetConnection();
            using var cmd = sqlConex.CreateCommand();
            cmd.CommandText = "SELECT ImageID, WooCommerceID FROM StoreSync_ImagenesItems_WooCommerce";
            await sqlConex.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                _producImagesLinks.TryAdd(reader.GetFieldValue<TExternalKey>(0), reader.GetInt64(1));
            }

            return true;
        }

        async Task AddLink(TExternalKey externalId, long wcId)
        {
            using var conex = _databaseHelper.GetConnection();
            await conex.ExecuteAsync("INSERT INTO StoreSync_ImagenesItems_WooCommerce(ImageID, WooCommerceID) VALUES (@ImageID, @WooCommerceID)",
                new { ImageID = externalId, WooCommerceID = wcId });
            _producImagesLinks.TryAdd(externalId, wcId);
        }

        async Task<Tuple<SyncResult, Exception>> SyncImage(EntityImage<TExternalKey> image)
        {
            try
            {
                return image.ImageType switch
                {
                    ImageTypes.Producto => await SyncProductImage(image),
                    ImageTypes.Categoria => await SyncCategoryImage(image),
                    ImageTypes.ProductoVariable => await SyncProductVariantImage(image),
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
            var idWoo = _producImagesLinks.ContainsKey(image.Id) ? _producImagesLinks[image.Id] : 0;
            if (!_productLinks.ContainsKey(image.ParentId))
            {
                return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception));
            }
            if(_productLinks[image.ParentId] == long.MinValue)//Productos que por algun motivo(Sin existencia, sin peso, sin precio, etc) no seran migrados a la tienda
            {
                return Tuple.Create(SyncResult.NotSynchronized, default(Exception));
            }
            var productIdWoo = Convert.ToUInt64( _productLinks[image.ParentId]);
            if (idWoo == 0)
            {
                if (image.Blob is null)
                {
                    return Tuple.Create(SyncResult.Error, new Exception($"La imagen {image.Id} no tiene acceso al blob!!!"));
                }
                var (media, error) = await MethodHelper.ExecuteMethodAsync(async () =>
                {
                    var product = await _wc.Product.Get(productIdWoo);
                    if(product is null)
                    {
                        throw new Exception("No se pudo recuperar el producto para la imagen");
                    }
                    using var stream = await image.Blob.GetStream();
                    //using var stream =  System.IO.File.OpenRead(@"C:\Users\diego\OneDrive\Escritorio\banner-02-1-1024x410.jpg");
                    //var media = await  _wp.Media.Create(@"C:\Users\diego\OneDrive\Escritorio\banner-02-1-1024x410.jpg",$"{image.ParentId}-{image.Id}-{product.slug}.jpg");
                    var media = await _wp.Media.CreateAsync(stream, $"{image.ParentId}-{image.Id}-{product.slug}.png");
                    product.images.Add(new() 
                    {
                        name = media.Slug,
                        src = media.SourceUrl,
                        position = image.IsDefault? 1: product.images.Count+1
                    });
                    await _wc.Product.Update(productIdWoo, product);
                    return media;
                }, 5, MethodHelper.TryAgainOnBadRequest, MethodHelper.StopsOnTermExist);
                if (error is null)
                {
                    await AddLink(image.Id, media.Id);
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
                    //Buscar la manera de cambiar la imagen principal de un producto en wc
                    return Tuple.Create(SyncResult.Updated, default(Exception));
                }
                else if (image.Operation == ImageOperations.Delete)
                {
                    //buscar manera de quitar las imagenes de un producto de la MediaLibrary de Wp y Del Producto de Wc
                    //await MethodHelper.ExecuteMethodAsync(async () =>
                    //{
                    //    await _imagesFactory.DeleteProductImageAsync(_productLinks[image.ParentId], idPrestashop);
                    //    return true;
                    //}, MethodHelper.NotRetryOnBadRequest);
                    return Tuple.Create(SyncResult.Updated, default(Exception));
                }
                else
                {
                    return Tuple.Create(SyncResult.Updated, default(Exception));
                }
            }

        }



         Task<Tuple<SyncResult, Exception>> SyncCategoryImage(EntityImage<TExternalKey> image)
        {
            throw new NotImplementedException();
        }


        async Task<Tuple<SyncResult, Exception>> SyncProductVariantImage(EntityImage<TExternalKey> image)
        {
            var productId = image.ParentId;
            var info = _productVariationsVsVariants.ContainsKey(productId)? _productVariationsVsVariants[productId] : null;
            if(info == null) //No se ha sincronizado la variante.
            {
                return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception));
            }

            var idWoo = Convert.ToUInt64( _producImagesLinks.ContainsKey(image.Id) ? _producImagesLinks[image.Id] : 0);
            var productIdWoo = Convert.ToUInt64( info.WooProductId);
            var variantIdWoo = Convert.ToUInt64(info.WooVariationId);

            if (idWoo == 0)
            {
                if (image.Blob is null)
                {
                    return Tuple.Create(SyncResult.Error, new Exception($"La imagen {image.Id} no tiene acceso al blob!!!"));
                }
                var (media, error) = await MethodHelper.ExecuteMethodAsync(async () =>
                {
                    var product = await _wc.Product.Get(productIdWoo);
                    if (product is null)
                    {
                        throw new Exception("No se pudo recuperar el producto para la imagen");
                    }
                    using var stream = await image.Blob.GetStream();
                    //using var stream =  System.IO.File.OpenRead(@"C:\Users\diego\OneDrive\Escritorio\banner-02-1-1024x410.jpg");
                    //var media = await  _wp.Media.Create(@"C:\Users\diego\OneDrive\Escritorio\banner-02-1-1024x410.jpg",$"{image.ParentId}-{image.Id}-{product.slug}.jpg");
                    var media = await _wp.Media.CreateAsync(stream, $"{image.ParentId}-{image.Id}-{product.slug}.png");
                    product.images.Add(new()
                    {
                        name = media.Slug,
                        src = media.SourceUrl,
                        position = image.IsDefault ? 1 : product.images.Count + 1
                    });
                    await _wc.Product.Update(productIdWoo, product);
                    return media;
                }, 5, MethodHelper.TryAgainOnBadRequest, MethodHelper.StopsOnTermExist);
                if (error is null)
                {
                    await AddLink(image.Id, media.Id);

                    var variant = await _wc.Product.Variations.Get(variantIdWoo, productIdWoo);

                    variant.image = new() 
                    {
                        name = media.Slug,
                        src = media.SourceUrl,
                    };

                   variant =  await _wc.Product.Variations.Update(variantIdWoo, variant, productIdWoo);

                    return Tuple.Create(SyncResult.Created, error);
                }
                else
                {
                    return Tuple.Create(SyncResult.Error, error);
                }
            }

            throw new NotImplementedException();
        }

    }
}
