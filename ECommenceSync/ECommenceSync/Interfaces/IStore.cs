using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommenceSync.Interfaces
{
    public interface IStore
    {
        public string Name { get;  }
        public IReadOnlyCollection<IOperation> Operations { get;  }
        public IReadOnlyCollection<IProcessor> Processors { get;  }
        public void ConfigureOperations<TErpKey>() where TErpKey:struct;
        public Task Start();
        public Task Stop();

    }
}
