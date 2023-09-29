using System.Collections.Generic;

namespace ECommenceSync.Interfaces
{
    public interface IStoresCollection
    {
        public void AddStore(IStore store);
        public IReadOnlyCollection<IStore> Stores { get;  }
    }

    public class StoresCollection:IStoresCollection
    {
        private readonly List<IStore> _stores;

        public IReadOnlyCollection<IStore> Stores { get=> _stores;  }

        public StoresCollection()
        {
            _stores = new List<IStore>();
        }

        public void AddStore(IStore store)
        {
            _stores.Add(store);
        }
    }

}
