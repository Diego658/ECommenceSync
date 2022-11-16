using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommenceSync.Interfaces
{

    public interface ISourceOperation<TKey, TValue> : IOperation
        where TKey : struct
        where TValue : IEntity<TKey>
    {
        IReadOnlyList<IProcessorOperation<TKey, TValue>> Processors {get; }
        void AddChangeProcessor(IProcessorOperation<TKey, TValue> processor);
        
        public Task BeginSync();
        public Task EndSync();
        public SyncTimeInfo SyncTimeInfo{ get; }
        public Task<List<TValue>> GetUpdated();
        public Task<List<TValue>> ResolveEntities(List<TKey> keys);
        public Dictionary<TKey, TKey> GetHierarchy();

    }
}
