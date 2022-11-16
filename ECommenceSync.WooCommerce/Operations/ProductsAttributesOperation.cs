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

namespace ECommenceSync.WooCommerce.Operations
{
    public class ProductsAttributesOperation<TExternalKey> : IDestinationOperation<TExternalKey, ProductAttribute<TExternalKey>>
        where TExternalKey : struct
    {
        private const string SqlAgregarLink = @"INSERT INTO [dbo].[StoreSync_ProductsAttributes_WooCommerce]([AttributeId],[WooCommerceID])
        VALUES(@AttributeId,@WooCommerceID)";
        public Action<ProductAttribute<TExternalKey>, SyncResult, Exception> OnSynchronized { get; set; }

        public ECommenceSync.Operations Operation => ECommenceSync.Operations.Attributes;

        public OperationModes Mode => OperationModes.Automatic;

        public OperationDirections Direction => OperationDirections.ErpToStore;

        public Guid Identifier => Guid.NewGuid();

        private OperationStatus _status;
        private CancellationTokenSource _taskCancelTokenSource;
        private CancellationTokenSource _cancelTokenSource;
        private Task _taskProcessor;
        private ConcurrentDictionary<TExternalKey, long> _attributesLinks;
        private readonly ConcurrentQueue<ProductAttribute<TExternalKey>> _workQueue;
        private readonly IWooCommerceDatabaseHelper _databaseHelper;
        private readonly WCObject _wc;
        private readonly WordPressClient _wp;

        public OperationStatus Status => _status;


        public ProductsAttributesOperation(IWooCommerceDatabaseHelper databaseHelper)
        {
            _status = OperationStatus.Created;
            _workQueue = new ConcurrentQueue<ProductAttribute<TExternalKey>>();
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            var rest = new RestAPI($"{databaseHelper.ApiUrl}/v3/", databaseHelper.ApiUser, databaseHelper.ApiPassword);
            _wc = new WCObject(rest);
            _wp = new WordPressClient(databaseHelper.ApiUrlWordpress);
            _wp.AuthMethod = WordPressPCL.Models.AuthMethod.ApplicationPassword;
            _wp.UserName = databaseHelper.ApiWpAppUser;
            _wp.SetApplicationPassword(databaseHelper.ApiWpAppPwd);
        }

        public void AddWork(List<ProductAttribute<TExternalKey>> values)
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

            _status = OperationStatus.Working;
            while (!_cancelTokenSource.IsCancellationRequested)
            {
                while (_workQueue.TryDequeue(out var attribute))
                {
                    var (result, ex) = await SyncAttribute(attribute);
                    OnSynchronized(attribute, result, ex);
                    await Task.Delay(10);
                }
                await Task.Delay(1000);
            }
            _status = OperationStatus.Stopped;
        }

        private async Task AddLink(TExternalKey externalKey, long key)
        {
            using var conex = _databaseHelper.GetConnection();
            await conex.ExecuteAsync(SqlAgregarLink, new { AttributeId = externalKey, WooCommerceID = key });
            _attributesLinks.AddOrUpdate(externalKey, key, (k, v) => v);
        }

        private async Task<Tuple<SyncResult, Exception>> SyncAttribute(ProductAttribute<TExternalKey> attribute)
        {
            var idWoo = _attributesLinks.ContainsKey(attribute.Id) ? _attributesLinks[attribute.Id] : 0;
            if(idWoo == 0) //nuevo
            {
                var wooAtrib = new ProductAttribute() 
                {
                    name = attribute.Name,
                    slug =attribute.Name.GetStringForLinkRewrite(),
                    type = "select"
                };
                Exception error;
                (wooAtrib, error) = await MethodHelper.ExecuteMethodAsync(async () =>
                {
                    var tmp = await _wc.Attribute.Add(wooAtrib);
                    //_wc.Product.Variations.Add(new Variation)
                    return tmp;
                }, 5, MethodHelper.TryAgainOnBadRequest, MethodHelper.StopsOnTermExist);
                if (error is null)
                {
                    await AddLink(attribute.Id, wooAtrib.id.Value);
                    return Tuple.Create(SyncResult.Created, error);
                }
                else
                {
                    return Tuple.Create(SyncResult.Error, error);
                }
            }
            else
            {
                var wooAtrib = await _wc.Attribute.Get(Convert.ToInt32( idWoo));
                wooAtrib.name = attribute.Name;
                wooAtrib.slug = attribute.Name.GetStringForLinkRewrite();
                Exception error;
                (wooAtrib, error) = await MethodHelper.ExecuteMethodAsync(async () =>
                {
                    var tmp = await _wc.Attribute.Update(Convert.ToInt32(idWoo), wooAtrib);
                    return tmp;
                }, 5,  MethodHelper.TryAgainOnBadRequest, MethodHelper.StopsOnTermExist);
                return error is null ? Tuple.Create(SyncResult.Created, error) : Tuple.Create(SyncResult.Error, error);
            }
        }

        private async Task<bool> LoadLinks()
        {
            _attributesLinks = await _databaseHelper.GetAttributesLinks<TExternalKey>();
            //using var sqlConex = (SqlConnection)_databaseHelper.GetConnection();
            //using var cmd = sqlConex.CreateCommand();
            //cmd.CommandText = "SELECT AttributeId, WooCommerceID FROM StoreSync_ProductsAttributes_WooCommerce";
            //await sqlConex.OpenAsync();
            //using var reader = await cmd.ExecuteReaderAsync();
            //_attributesLinks = new ConcurrentDictionary<TExternalKey, long>();
            //while (await reader.ReadAsync())
            //{
            //    _attributesLinks.TryAdd(reader.GetFieldValue<TExternalKey>(0), reader.GetInt64(1));
            //}

            return true;
        }
    }
}
