using ECommenceSync.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommenceSync
{
    public abstract class DestinationOperation<TExternalKey, TValue> : IDestinationOperation<TExternalKey, TValue>
        where TExternalKey : struct
        where TValue : IEntity<TExternalKey>
    {
        public Action<TValue, SyncResult, Exception> OnSynchronized { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Operations Operation => throw new NotImplementedException();

        public OperationModes Mode => throw new NotImplementedException();

        public OperationDirections Direction => throw new NotImplementedException();

        public Guid Identifier => throw new NotImplementedException();

        public abstract OperationStatus Status { get; }

        public void AddWork(List<TValue> values)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop(TimeSpan timeOut)
        {
            throw new NotImplementedException();
        }

        public Task Work()
        {
            throw new NotImplementedException();
        }
    }
}
