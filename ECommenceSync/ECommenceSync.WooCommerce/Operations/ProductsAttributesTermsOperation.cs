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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using WordPressPCL;

namespace ECommenceSync.WooCommerce.Operations
{
    public class ProductsAttributesTermsOperation<TExternalKey> : IDestinationOperation<TExternalKey, ProductAttributeTerm<TExternalKey>>
        where TExternalKey : struct
    {
        private const string SqlAgregarLink = @"INSERT INTO [dbo].[StoreSync_ProductsAttributesTerms_WooCommerce]([AttributeTermId],[WooCommerceID])
        VALUES(@AttributeTermId, @WooCommerceID)";

        public Action<ProductAttributeTerm<TExternalKey>, SyncResult, Exception> OnSynchronized { get; set; }
        public ECommenceSync.Operations Operation => ECommenceSync.Operations.AttributesTerms;
        public OperationModes Mode => OperationModes.Automatic;
        public OperationDirections Direction => OperationDirections.ErpToStore;
        public Guid Identifier => Guid.NewGuid();
        private OperationStatus _status;
        private CancellationTokenSource _taskCancelTokenSource;
        private CancellationTokenSource _cancelTokenSource;
        private Task _taskProcessor;
        private ConcurrentDictionary<TExternalKey, long> _attributesTermsLinks;
        private ConcurrentDictionary<TExternalKey, long> _attributesLinks;
        private readonly ConcurrentQueue<ProductAttributeTerm<TExternalKey>> _workQueue;
        private readonly IWooCommerceDatabaseHelper _databaseHelper;
        private readonly WCObject _wc;
        private readonly WordPressClient _wp;

        public OperationStatus Status => _status;

        public ProductsAttributesTermsOperation(IWooCommerceDatabaseHelper databaseHelper)
        {
            _status = OperationStatus.Created;
            _workQueue = new ConcurrentQueue<ProductAttributeTerm<TExternalKey>>();
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            var rest = new RestAPI($"{databaseHelper.ApiUrl}/v3/", databaseHelper.ApiUser, databaseHelper.ApiPassword);
            _wc = new WCObject(rest);
            _wp = new WordPressClient(databaseHelper.ApiUrlWordpress);
            _wp.Auth.UseBasicAuth(databaseHelper.ApiWpAppUser, databaseHelper.ApiWpAppPwd);
            
        }

        public void AddWork(List<ProductAttributeTerm<TExternalKey>> values)
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
                    var (result, ex) = await SyncAttributeTerm(attribute);
                    OnSynchronized(attribute, result, ex);
                    await Task.Delay(10);
                }
                await Task.Delay(1000);
            }
            _status = OperationStatus.Stopped;
        }
        private async Task AddLink(TExternalKey externalKey, ulong key)
        {
            var iKey = Convert.ToInt64(key);
            using var conex = _databaseHelper.GetConnection();
            await conex.ExecuteAsync(SqlAgregarLink, new { AttributeTermId = externalKey, WooCommerceID = iKey });
            _attributesTermsLinks.AddOrUpdate(externalKey, iKey, (k, v) => v);
        }

        private async Task<Tuple<SyncResult, Exception>> SyncAttributeTerm(ProductAttributeTerm<TExternalKey> attribute)
        {
            var idWooParent = GetParentId(attribute);
            if (idWooParent is null)
            {
                //El atributo padre no ha sido sincronizado, intentar luego.
                return Tuple.Create(SyncResult.NotSynchronizedPostponed, default(Exception));
            }
            var idWoo = Convert.ToUInt64( _attributesTermsLinks.ContainsKey(attribute.Id) ? _attributesTermsLinks[attribute.Id] : 0);
            if (idWoo == 0)
            {
                var wooAtribTerm = new ProductAttributeTerm() 
                {
                    name = attribute.Name,
                    slug = attribute.Name.GetStringForLinkRewrite(),
                    description = attribute.Name,
                };
                Exception error;
                (wooAtribTerm, error) = await MethodHelper.ExecuteMethodAsync(async () =>
                {
                    var tmp = await _wc.Attribute.Terms.Add( wooAtribTerm, idWooParent.Value);
                    //_wc.Product.Variations.Add(new Variation)
                    return tmp;
                }, 5, MethodHelper.TryAgainOnBadRequest, MethodHelper.StopsOnTermExist);
                if (error is null)
                {
                    await AddLink(attribute.Id,  wooAtribTerm.id.Value);
                    return Tuple.Create(SyncResult.Created, error);
                }
                else
                {
                    if (error.TryGetWooErrorInfo(out var wooErrorInfo))
                    {
                        //Dio error la request pero se guardo, intentar actualizar con el id ya guardado
                        if (wooErrorInfo.Code == "term_exists")  
                        {
                            await AddLink(attribute.Id, Convert.ToUInt64( wooErrorInfo.Data.Resource_Id));
                            return Tuple.Create(SyncResult.Created, default(Exception));
                        }
                    }

                    return Tuple.Create(SyncResult.Error, error);
                }
            }
            else
            {
                var wooAtribTerm = await _wc.Attribute.Terms.Get(idWoo, idWooParent.Value);
                wooAtribTerm.name = attribute.Name;
                wooAtribTerm.slug = attribute.Name.GetStringForLinkRewrite();
                wooAtribTerm.description = attribute.Name;
                Exception error;
                (wooAtribTerm, error) = await MethodHelper.ExecuteMethodAsync(async () =>
                {
                    var tmp = await _wc.Attribute.Terms.Update(idWoo, wooAtribTerm, idWooParent.Value);
                    //_wc.Product.Variations.Add(new Variation)
                    return tmp;
                }, 5, MethodHelper.TryAgainOnBadRequest, MethodHelper.StopsOnTermExist);
                return error is null ? Tuple.Create(SyncResult.Created, error) : Tuple.Create(SyncResult.Error, error);
            }
            
        }

        private ulong? GetParentId(ProductAttributeTerm<TExternalKey> term)
        {
            ulong? idWooParent;
            idWooParent = _attributesLinks.ContainsKey(term.AttributeId) ? Convert.ToUInt64( _attributesLinks[term.AttributeId]) : null;
            return idWooParent;
        }

        private async Task<bool> LoadLinks()
        {
            _attributesLinks = await _databaseHelper.GetAttributesLinks<TExternalKey>();
            _attributesTermsLinks = await _databaseHelper.GetAttributesTermsLinks<TExternalKey>();
            ////_databaseHelper.GetProductsLinks
            //_attributesLinks = await _databaseHelper.GetAttributesLinks<TExternalKey>();
            //_attributesTermsLinks = await _databaseHelper.GetAttributesTermsLinks<TExternalKey>();

            //using var sqlConex = (SqlConnection)_databaseHelper.GetConnection();
            //using var cmd = sqlConex.CreateCommand();
            //cmd.CommandText = "SELECT AttributeTermId, WooCommerceID FROM StoreSync_ProductsAttributesTerms_WooCommerce";
            //await sqlConex.OpenAsync();
            //using var reader = await cmd.ExecuteReaderAsync();
            //_attributesTermsLinks = new ConcurrentDictionary<TExternalKey, long>();
            ////_attributesLinks = new ConcurrentDictionary<TExternalKey, long>();
            //while (await reader.ReadAsync())
            //{
            //    _attributesTermsLinks.TryAdd(reader.GetFieldValue<TExternalKey>(0), reader.GetInt64(1));
            //}
            //await reader.CloseAsync();


            //using var cmd2 = sqlConex.CreateCommand();
            //cmd2.CommandText = "SELECT AttributeId, WooCommerceID FROM StoreSync_ProductsAttributes_WooCommerce";
            ////await sqlConex.OpenAsync();
            //using var reader2 = await cmd2.ExecuteReaderAsync();
            //_attributesLinks = new ConcurrentDictionary<TExternalKey, long>();
            //while (await reader2.ReadAsync())
            //{
            //    _attributesLinks.TryAdd(reader2.GetFieldValue<TExternalKey>(0), reader2.GetInt64(1));
            //}

            return true;
        }

    }
}
