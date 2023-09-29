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
    public class TagsOperation<TExternalKey> : IDestinationOperation<TExternalKey, Tag<TExternalKey>>
        where TExternalKey : struct
    {
        private const string SqlAddLink = "INSERT INTO StoreSync_Tags_Prestashop(TagId, PrestashopId) VALUES (@TagId, @PrestashopId)";
        readonly ConcurrentQueue<Tag<TExternalKey>> _workQueue;
        readonly IPrestashopDatabaseHelper _databaseHelper;
        readonly TagFactory _tagFactory;
        readonly int _syncLanguage;
        Task _taskProcessor;
        ConcurrentDictionary<TExternalKey, long> _links;
        public Action<Tag<TExternalKey>, SyncResult, Exception> OnSynchronized { get; set; }

        public ECommenceSync.Operations Operation => ECommenceSync.Operations.Tags;

        public OperationModes Mode => OperationModes.Automatic;

        public OperationDirections Direction => OperationDirections.ErpToStore;

        public Guid Identifier => Guid.NewGuid();

        public OperationStatus Status { get; set; }
        public CancellationTokenSource TaskCancelTokenSource { get; set; }
        public CancellationTokenSource CancelTokenSource { get; set; }

        public TagsOperation(IPrestashopDatabaseHelper databaseHelper)
        {
            Status = OperationStatus.Created;
            _workQueue = new ConcurrentQueue<Tag<TExternalKey>>();
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            _tagFactory = new TagFactory(databaseHelper.ApiUrl, databaseHelper.ApiSecret, "");
            _syncLanguage = databaseHelper.SyncLanguage;
        }

        public void AddWork(List<Tag<TExternalKey>> values)
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
                while (_workQueue.TryDequeue(out var tag))
                {
                    var (result, ex) = await SyncTag(tag);
                    OnSynchronized(tag, result, ex);
                }
                await Task.Delay(1000);
            }
            Status = OperationStatus.Stopped;
        }

        private async Task<bool> LoadLinks()
        {
            _links = await _databaseHelper.GetTagsLinks<TExternalKey>();
            return true;
        }


        private async Task<Tuple<SyncResult, Exception>> SyncTag(Tag<TExternalKey> tag)
        {
            var idPrestashop = _links.ContainsKey(tag.Id) ? _links[tag.Id] : 0;
            Tuple<SyncResult, Exception> resultado;
            if (idPrestashop == 0)
            {
                resultado = await AddTag(tag);
            }
            else
            {
                resultado = await UpdateTag(tag, idPrestashop);
            }
            return resultado;

        }

        private async Task AddLink(TExternalKey externalKey, long key)
        {
            using var conex = _databaseHelper.GetConnection();
            await conex.ExecuteAsync(SqlAddLink, new { TagId = externalKey, PrestashopId = key });
            _links.TryAdd(externalKey, key);
        }

        async Task<Tuple<SyncResult, Exception>> AddTag(Tag<TExternalKey> erpTag)
        {
            var prestashopTag = new tag
            {
                id_lang = _syncLanguage,
                name = erpTag.Name,
            };
            Exception error;
            (prestashopTag, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                var tmp = await _tagFactory.AddAsync(prestashopTag);
                return tmp;
            }, MethodHelper.NotRetryOnBadrequest);

            if (error is null)
            {
                await AddLink(erpTag.Id, prestashopTag.id.Value);
                return Tuple.Create(SyncResult.Created, default(Exception));
            }
            else
            {
                return Tuple.Create(SyncResult.Error, error);
            }

        }
        async Task<Tuple<SyncResult, Exception>> UpdateTag(Tag<TExternalKey> erpTag, long idPrestashop)
        {
            var prestashopTag = new tag
            {
                id_lang = _syncLanguage,
                name = erpTag.Name,
                id = idPrestashop
            };
            Exception error;
            bool isOk;
            (isOk, error) = await MethodHelper.ExecuteMethodAsync(async () =>
            {
                await _tagFactory.UpdateAsync(prestashopTag);
                return true;
            }, MethodHelper.NotRetryOnBadrequest);

            if (isOk)
            {
                return Tuple.Create(SyncResult.Updated, default(Exception));
            }
            else
            {
                return Tuple.Create(SyncResult.Error, error);
            }
        }



    }
}
