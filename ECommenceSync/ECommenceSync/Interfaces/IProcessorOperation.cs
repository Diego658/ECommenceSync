using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommenceSync.Interfaces
{

    public interface IProcessor
    {
        void Start();
        void Stop(TimeSpan timeOut);
        Task Work();
    }

    public interface IProcessorOperation<TKey, TValue>: IProcessor
        where TKey : struct
        where TValue: IEntity<TKey>
    {
        Task ProcessChanges(List<TValue> changes);
        //void AddDestination(IDestinationOperation<TKey> destinationOperation);
        public IDestinationOperation<TKey, TValue> Destination { get;  }
        
    }
}
