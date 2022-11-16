using Dapper;
using ECommenceSync.Helper;
using ECommenceSync.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ECommenceSync
{
    public class GenericChangeProcessor<TConn, TSourceKey, TDesKey, TValue> : IProcessorOperation<TSourceKey, TValue>
        where TSourceKey : struct
        where TDesKey : struct
        where TValue : IEntity<TSourceKey>
        where TConn : DbConnection
    {
        private readonly string SqlRecuperaCambios;
        private readonly string SqlInsertaCambios;
        private readonly string SqlEliminaCambio;
        private readonly string SqlActualizaError ;

        readonly IDatabaseHelper<TConn, TDesKey> _databaseHelper;
        IDestinationOperation<TSourceKey, TValue> _destination;
        readonly Action<TValue, SyncResult, Exception> _onSynchronized;
        CancellationTokenSource _taskCancelTokenSource;
        CancellationTokenSource _cancelTokenSource;
        Task _taskProcessor;
        ConcurrentQueue<TValue> _workQueue;
        ConcurrentQueue<PosponedEntity<TValue, TSourceKey>> _posponedQueue;
        ConcurrentDictionary<TSourceKey, TDesKey> _productLinks;
        ConcurrentDictionary<TSourceKey, TValue> _pendingChanges;
        readonly ISourceOperation<TSourceKey, TValue> _sourceOperation;
        readonly TDesKey tempId;
        ConcurrentDictionary<TSourceKey, TDesKey> _customersLinks;
        
        public IDestinationOperation<TSourceKey, TValue> Destination
        {
            get
            {
                return _destination;
            }
            set
            {
                _destination = value;
                _destination.OnSynchronized = _onSynchronized ;
                //LoadPendingChanges();
            }
        }



        public GenericChangeProcessor(IDatabaseHelper<TConn, TDesKey> databaseHelper,
            ISourceOperation<TSourceKey, TValue> sourceOperation,
            TDesKey tempId,
            Action<TValue, SyncResult, Exception> onSynchronized = null, 
            string changesTable = "CambiosPendientes")
        {
            this.tempId = tempId;
            _sourceOperation = sourceOperation;
            _onSynchronized = onSynchronized ;
            if (_onSynchronized == null)
            {
                _onSynchronized = (t, r, e) => __OnSynchronized(t, r, e);
            }
            _databaseHelper = databaseHelper;
            SqlRecuperaCambios = $"SELECT ClaveExterna FROM {changesTable} WHERE Destino = @Destino ";
            SqlInsertaCambios = $"INSERT INTO {changesTable}(Destino, ClaveExterna) VALUES(@Destino, @ClaveExterna)";
            SqlEliminaCambio = $"DELETE FROM {changesTable} WHERE Destino = @Destino AND ClaveExterna = @ClaveExterna";
            SqlActualizaError = $"UPDATE {changesTable} SET TieneError = 1, Error = @Error, StackTrace = @StackTrace WHERE Destino = @Destino AND ClaveExterna = @ClaveExterna";
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
            conex.Open();
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
            _workQueue = new ConcurrentQueue<TValue>();
            _posponedQueue = new ConcurrentQueue<PosponedEntity<TValue, TSourceKey>>();
            if (Destination.Operation == ECommenceSync.Operations.Products)
            {
                _productLinks = await _databaseHelper.GetProductsLinks<TSourceKey>();
                _customersLinks = await _databaseHelper.GetCustomersLinks<TSourceKey>();
            }

            await LoadPendingChanges();
            var listWork = new List<TValue>();
            while (!_cancelTokenSource.IsCancellationRequested)
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
            while (_posponedQueue.TryPeek(out var entity))
            {
                if (DateTime.Now.Subtract(entity.PostponedDate).TotalMilliseconds > _databaseHelper.TimeToRetryPosponed)
                {
                    if (_posponedQueue.TryDequeue(out entity)) _workQueue.Enqueue(entity.Entity);
                }

            }

        }

        private async Task LoadPendingChanges()
        {
            _pendingChanges = new ConcurrentDictionary<TSourceKey, TValue>(2, 100);
            using var conex = _databaseHelper.GetConnection();
            using var cmd = conex.CreateCommand();
            cmd.CommandText = SqlRecuperaCambios;
            var param = cmd.CreateParameter();
            param.ParameterName = "Destino";
            param.Value = Destination.Operation.ToString();
            var keys = new List<TSourceKey>();// = await conex.QueryAsync<TKey>(SqlRecuperaCambios, new { Destino = Destination.Operation.ToString() });
            if (keys.Count() == 0) return;
            var entities = await _sourceOperation.ResolveEntities(keys.ToList());
            foreach (var entity in entities)
            {
                _pendingChanges.TryAdd(entity.Id, entity);
                _workQueue.Enqueue(entity);
            }
        }

        private void __OnSynchronized(TValue entity, SyncResult status, Exception error)
        {
            using var conex = _databaseHelper.GetConnection();
            switch (status)
            {
                case SyncResult.Created:
                case SyncResult.Updated:
                case SyncResult.NotSynchronized:
                    if (status == SyncResult.NotSynchronized && Destination.Operation == ECommenceSync.Operations.Products)
                    {
                        _productLinks.TryAdd(entity.Id, tempId);
                    }
                    else
                    {
                        conex.Execute(SqlEliminaCambio, new { Destino = Destination.Operation.ToString(), ClaveExterna = entity.Id });
                    }
                    _pendingChanges.TryRemove(entity.Id, out entity);
                    break;
                case SyncResult.NotSynchronizedPostponed:
                    _posponedQueue.Enqueue(new PosponedEntity<TValue, TSourceKey>(entity));
                    break;
                case SyncResult.Error:
                    conex.Execute(SqlActualizaError, new
                    {
                        Destino = Destination.Operation.ToString(),
                        Error = error.Message.GetTruncated(4000),
                        StackTrace = error.StackTrace.GetTruncated(4000),
                        ClaveExterna = entity.Id
                    });
                    break;
                default:
                    break;
            }

        }

    }
}
