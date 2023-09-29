using System;

namespace ECommenceSync.Interfaces
{
    public interface IEcommerceSyncStoresBuilder
    {
        public void AddStore(IStore store); 
    }

    public class EcommerceSyncStoresBuilder: IEcommerceSyncStoresBuilder
    {
        private readonly IStoresCollection storesCollection;

        public IErp Erp { get; }

        public EcommerceSyncStoresBuilder(IStoresCollection storesCollection, IErp erp)
        {
            this.storesCollection = storesCollection ?? throw new ArgumentNullException(nameof(storesCollection));
            Erp = erp ?? throw new ArgumentNullException(nameof(erp));
        }

        public void AddStore(IStore store)
        {
            storesCollection.AddStore(store);
        }
    }

}
