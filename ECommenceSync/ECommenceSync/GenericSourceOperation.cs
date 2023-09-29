using ECommenceSync.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ECommenceSync
{
    public abstract class GenericSourceOperation<TKey, TValue> : ISourceOperation<TKey, TValue>
        where TKey : struct
        where TValue : IEntity<TKey>
    {
        public abstract Operations Operation { get; }
        public abstract OperationModes Mode { get; }
        public abstract OperationDirections Direction { get; }
        public abstract Guid Identifier { get; }

        private readonly List<IProcessorOperation<TKey, TValue>> _processorOperations = new List<IProcessorOperation<TKey, TValue>>();
        Task _taskProcessor;
        public CancellationTokenSource CancelTokenSource { get; private set; }
        internal CancellationTokenSource TaskCancelTokenSource;

        public IReadOnlyList<IProcessorOperation<TKey, TValue>> Processors { get => _processorOperations.AsReadOnly(); }

        public SyncTimeInfo SyncTimeInfo { get; set; }
        public abstract OperationStatus Status { get; }

        public void AddChangeProcessor(IProcessorOperation<TKey, TValue> processor)
        {
            _processorOperations.Add(processor);
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
            CancelTokenSource.Cancel();
            var end = _taskProcessor.Wait(timeOut);
            if (!end)
            {
                try
                {
                    TaskCancelTokenSource.Cancel();
                }
                catch (OperationCanceledException)
                {
                }
            }
            _taskProcessor.Dispose();
        }

        public async Task Sleep(int timeToSleep)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!CancelTokenSource.IsCancellationRequested && stopwatch.ElapsedMilliseconds < timeToSleep)
            {
                await Task.Delay(250);
            }
            stopwatch.Stop();
        }


        public abstract Task BeginSync();

        public abstract Task EndSync();

        public abstract Task Work();
        public abstract Task<List<TValue>> GetUpdated();

        public abstract Task<List<TValue>> ResolveEntities(List<TKey> keys);

        public abstract Dictionary<TKey, TKey> GetHierarchy();
        public abstract Task<TValue> ResolveEntity(TKey key);
    }
}
