using Dapper;
using ECommenceSync.Entities;
using ECommenceSync.Interfaces;
using ECommenceSync.WooCommerce.Helpers;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using WordPressPCL;
using WordPressPCL.Models;

namespace ECommenceSync.WooCommerce.Operations
{
    public class BrandsOperation<TExternalKey> : IDestinationOperation<TExternalKey, Brand<TExternalKey>>
        where TExternalKey : struct
    {
        const string SqlAddLink = "INSERT INTO StoreSync_Marcas_WooCommerce(MarcaId, WooCommerceID) VALUES (@ExternalKey, @Key)";

        ConcurrentDictionary<TExternalKey, long> _brandLinks;
        private readonly WCObject _wc;
        private readonly WordPressClient _wp;
        private CancellationTokenSource _taskCancelTokenSource;
        private CancellationTokenSource _cancelTokenSource;
        private Task _taskProcessor;

        readonly ConcurrentQueue<Brand<TExternalKey>> _workQueue;
        private readonly IWooCommerceDatabaseHelper _databaseHelper;
        private readonly RestAPI _rest;

        public Action<Brand<TExternalKey>, SyncResult, Exception> OnSynchronized { get; set; }


        public ECommenceSync.Operations Operation => ECommenceSync.Operations.Brands;

        public OperationModes Mode => OperationModes.Automatic;

        public OperationDirections Direction => OperationDirections.ErpToStore;

        public System.Guid Identifier => System.Guid.NewGuid();

        public OperationStatus Status { get; set; }

        public BrandsOperation(IWooCommerceDatabaseHelper databaseHelper)
        {
            Status = OperationStatus.Created;
            _workQueue = new ConcurrentQueue<Brand<TExternalKey>>();
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            _rest = new RestAPI($"{databaseHelper.ApiUrl}/v3/", databaseHelper.ApiUser, databaseHelper.ApiPassword);
            _wc = new WCObject(_rest);
            _wp = new WordPressClient(databaseHelper.ApiUrlWordpress);
            _wp.Auth.UseBasicAuth(databaseHelper.ApiWpAppUser, databaseHelper.ApiWpAppPwd);

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
                    var (result, ex) = await SyncBrand(brand);
                    OnSynchronized(brand, result, ex);
                    await Task.Delay(10);
                }
                await Task.Delay(1000);
            }
            Status = OperationStatus.Stopped;
        }

        private async Task<bool> LoadLinks()
        {
            _brandLinks = await _databaseHelper.GetBrandsLinks<TExternalKey>();
            return true;
        }

        async Task<Tuple<SyncResult, Exception>> SyncBrand(Brand<TExternalKey> brand)
        {
            try
            {
                var idWoo = Convert.ToUInt64(_brandLinks.ContainsKey(brand.Id) ? _brandLinks[brand.Id] : 0);
                Tuple<SyncResult, Exception> resultado;
                if (idWoo == 0)
                {
                    var wooCategory = new ProductCategory
                    {
                        parent = null,
                        name = brand.Name,
                        display = "default",
                        description = brand.Description,
                        slug = brand.Name.GetStringForLinkRewrite(),
                    };
                    Exception error;
                    (wooCategory, error) = await MethodHelper.ExecuteMethodAsync(async () =>
                    {
                        var tmp = await _wc.Product.API.SendHttpClientRequest("products/brands", RequestMethod.POST, wooCategory, null);
                        //var tmp = await _wc.Category.Add(wooCategory);
                        return _wc.Product.API.DeserializeJSon<ProductCategory>(tmp);
                    }, 5, MethodHelper.TryAgainOnBadRequest, MethodHelper.StopsOnTermExist);
                    if (error is null)
                    {
                        await AddLink(brand.Id, wooCategory.id.Value);
                        resultado = Tuple.Create(SyncResult.Created, default(Exception));
                    }
                    else
                    {
                        resultado = Tuple.Create(SyncResult.Error, error);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
                return resultado;
            }
            catch (Exception ex)
            {
                return (Tuple.Create(SyncResult.Error, ex));
            }

        }

        async Task AddLink(TExternalKey externalKey, ulong key)
        {
            var lKey = Convert.ToInt64(key);
            using var conex = _databaseHelper.GetConnection();
            await conex.ExecuteAsync(SqlAddLink, new { ExternalKey = externalKey, Key = lKey });
            _brandLinks.TryAdd(externalKey, lKey);
        }


        //async Task<Tuple<SyncResult, Exception>> AddBrand(Brand<TExternalKey> brand)
        //{

        //}

        //async Task<Tuple<SyncResult, Exception>> AddBrand(Brand<TExternalKey> brand, ulong idWoo)
        //{

        //}

    }
}
