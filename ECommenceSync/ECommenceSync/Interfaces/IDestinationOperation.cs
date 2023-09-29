using System;
using System.Collections.Generic;

namespace ECommenceSync.Interfaces
{
    public interface IDestinationOperation<TExternalKey, TValue> : IOperation
        where TExternalKey:struct
        where TValue : IEntity<TExternalKey>
    {
        Action<TValue, SyncResult, Exception> OnSynchronized { get; set; }
        void AddWork(List<TValue> values);
    }
}
