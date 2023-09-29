using ECommenceSync.Interfaces;
using System;
using System.Linq;

namespace ECommenceSync
{
    public sealed class GenericSyncWorker : ISyncWorker
    {
        private readonly IErp erp;
        private readonly IStoresCollection stores;

        public GenericSyncWorker(IErp erp, IStoresCollection stores)
        {
            this.erp = erp ?? throw new ArgumentNullException(nameof(erp));
            this.stores = stores ?? throw new ArgumentNullException(nameof(stores));
        }

        public void Run()
        {
            erp.Operations.Where(op => op.Direction == OperationDirections.ErpToStore);
        }
    }
}
