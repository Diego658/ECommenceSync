using Dapper;
using ECommenceSync.Helper;
using ECommenceSync.Interfaces;
using ECommenceSync.WooCommerce.Helpers;
using ECommenceSync.WooCommerce.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ECommenceSync.WooCommerce
{
    public class WooCommerceChangeProcessor<TKey, TValue> : IProcessorOperation<TKey, TValue>
        where TKey : struct
        where TValue : IEntity<TKey>
    {
        private const string SqlRecuperaCambios = "SELECT ClaveExterna FROM StoreSync_WooCommerceCambiosPendientes WHERE Destino = @Destino ";
        private const string SqlEliminaCambio = "DELETE FROM StoreSync_WooCommerceCambiosPendientes WHERE Destino = @Destino AND ClaveExterna = @ClaveExterna";
        private const string SqlInsertaCambios = "DELETE FROM StoreSync_WooCommerceCambiosPendientes WHERE Destino = @Destino AND ClaveExterna = @ClaveExterna; INSERT INTO StoreSync_WooCommerceCambiosPendientes(Destino, ClaveExterna) VALUES(@Destino, @ClaveExterna)";
        private const string SqlActualizaError = "UPDATE StoreSync_WooCommerceCambiosPendientes SET TieneError = 1, Error = @Error, StackTrace = @StackTrace WHERE Destino = @Destino AND ClaveExterna = @ClaveExterna";

        private readonly IWooCommerceDatabaseHelper _databaseHelper;
        private readonly ISourceOperation<TKey, TValue> _sourceOperation;
        private IDestinationOperation<TKey, TValue> _destination;
        private ConcurrentDictionary<TKey, TValue> _pendingChanges;
        private CancellationTokenSource TaskCancelTokenSource;
        private CancellationTokenSource CancelTokenSource;
        private Task _taskProcessor;
        private ConcurrentQueue<TValue> _workQueue; 
        ConcurrentQueue<PosponedEntity<TKey,TValue>> _posponedQueue;
        ConcurrentDictionary<TKey, long> _productLinks;
        readonly bool _retryWithErrorOnRestart;

        public IDestinationOperation<TKey, TValue> Destination
        {
            get
            {
                return _destination;
            }
            set
            {
                _destination = value;
                _destination.OnSynchronized = (t, r, e) => OnSynchronized(t, r, e);
                //LoadPendingChanges();
            }
        }


        public WooCommerceChangeProcessor(IWooCommerceDatabaseHelper databaseHelper, 
            ISourceOperation<TKey, TValue> sourceOperation, 
            IConfiguration configuration)
        {
            _databaseHelper = databaseHelper ?? throw new ArgumentNullException(nameof(databaseHelper));
            _sourceOperation = sourceOperation ?? throw new ArgumentNullException(nameof(sourceOperation));
            var section = configuration.
                GetSection("RetryWithErrorOnRestart");
            _retryWithErrorOnRestart = section.Exists() ? bool.Parse(section.Value) : true;
        }


        public async Task ProcessChanges(List<TValue> changes)
        {
            await SavePendingChanges(changes);
            foreach (var item in changes)
            {
                _workQueue.Enqueue(item);
            }

        }

        private async Task SavePendingChanges(List<TValue> changes)
        {
            using var conex = _databaseHelper.GetConnection();
            await conex.OpenAsync();
            using var tran = conex.BeginTransaction();
            foreach (var item in changes)
            {
                if (_pendingChanges.TryAdd(item.Id, item))
                {
                    await conex.ExecuteAsync(SqlInsertaCambios, new { Destino = Destination.Operation.ToString(), ClaveExterna = item.Id }, tran);
                }
            }
            tran.Commit();
            conex.Close();
        }


        private async Task LoadPendingChanges()
        {
            _pendingChanges = new ConcurrentDictionary<TKey, TValue>(2, 100);
            using var conex = _databaseHelper.GetConnection();
            var keys = await conex.QueryAsync<TKey>(SqlRecuperaCambios + (_retryWithErrorOnRestart ? "" : " AND TieneError = 0 "), new { Destino = Destination.Operation.ToString() });
            if (keys.Count() == 0) return;
            var entities = await _sourceOperation.ResolveEntities(keys.ToList());
            foreach (var entity in entities)
            {
                _pendingChanges.TryAdd(entity.Id, entity);
                _workQueue.Enqueue(entity);
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
            _workQueue = new ConcurrentQueue<TValue>();
            _posponedQueue = new ConcurrentQueue<PosponedEntity<TKey,TValue>>();
            if (Destination.Operation == ECommenceSync.Operations.Products)
            {
                _productLinks = await _databaseHelper.GetProductsLinks<TKey>();
            }

            await LoadPendingChanges();
            var listWork = new List<TValue>();
            while (!CancelTokenSource.IsCancellationRequested)
            {
                if (_workQueue.TryDequeue(out var entity))
                {
                    listWork.Add(entity);
                }
                else if (listWork.Count > 0)
                {
                    Destination.AddWork(listWork);
                    listWork.Clear();
                }
                else
                {
                    await Task.Delay(1000);
                    ProccPostponed();
                    await Task.Delay(1000);
                }
            }
        }


        public void ProccPostponed()
        {
            if (_posponedQueue.TryPeek(out _))
            {
                while (_posponedQueue.TryPeek(out PosponedEntity<TKey, TValue> entity))
                {
                    if (DateTime.Now.Subtract(entity.PostponedDate).TotalMilliseconds > _databaseHelper.TimeToRetryPosponed)
                    {
                        if (_posponedQueue.TryDequeue(out entity)) _workQueue.Enqueue(entity.Entity);
                    }
                    else
                    {
                        break;
                    }

                }
            }


        }


        private void OnSynchronized(TValue entity, SyncResult status, Exception error)
        {
            using var conex = _databaseHelper.GetConnection();
            switch (status)
            {
                case SyncResult.Created:
                case SyncResult.Updated:
                case SyncResult.NotSynchronized:
                    if (status == SyncResult.NotSynchronized && Destination.Operation == ECommenceSync.Operations.Products)
                    {
                        _productLinks.TryAdd(entity.Id, long.MinValue);

                    }
                    
                    conex.Execute(SqlEliminaCambio, new { Destino = Destination.Operation.ToString(), ClaveExterna = entity.Id });
                    _pendingChanges.TryRemove(entity.Id, out entity);
                    break;
                case SyncResult.NotSynchronizedPostponed:
                    _posponedQueue.Enqueue(new PosponedEntity<TKey,TValue>(entity));
                    break;
                case SyncResult.Error:
                    var indexOfErrorPs = error.Message.LastIndexOf("ws_key:");
                    if (indexOfErrorPs > 0)
                    {
                        conex.Execute(SqlActualizaError, new
                        {
                            Destino = Destination.Operation.ToString(),
                            Error = error.Message.Substring(indexOfErrorPs + 44),
                            StackTrace = error.StackTrace.GetTruncated(4000),
                            ClaveExterna = entity.Id
                        });
                    }
                    else
                    {
                        conex.Execute(SqlActualizaError, new
                        {
                            Destino = Destination.Operation.ToString(),
                            Error = error.Message.GetTruncated(4000),
                            StackTrace = error.StackTrace.GetTruncated(4000),
                            ClaveExterna = entity.Id
                        });
                    }

                    break;
                default:
                    break;
            }

        }

    }
}
