using ECommenceSync.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommenceSync.AutomatizerSQL
{
    public class AutomatizerSqlChangeProcessor<TKey, TValue> : IProcessorOperation<TKey, TValue>
        where TKey : struct
        where TValue : IEntity<TKey>
    {
        public IDestinationOperation<TKey, TValue> Destination => throw new NotImplementedException();

        public Task ProcessChanges(List<TValue> changes)
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
