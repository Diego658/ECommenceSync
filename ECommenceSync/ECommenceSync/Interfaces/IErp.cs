using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommenceSync.Interfaces
{
    public interface IErp
    {
        public string Name { get;  }
        public IReadOnlyCollection<IOperation> Operations { get; set; }
        public Task Start();
        public Task Stop();
        public void ConfigureOperations<TStoreKey>(IStore store) where TStoreKey : struct;

    }
}
