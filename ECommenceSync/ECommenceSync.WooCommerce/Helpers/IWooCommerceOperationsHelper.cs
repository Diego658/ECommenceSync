using Microsoft.Extensions.Configuration;

namespace ECommenceSync.WooCommerce.Helpers
{
    public interface IWooCommerceOperationsHelper
    {
        IConfigurationSection Configuration { get; }
        int GetMaxRetryCount(ECommenceSync.Operations operation);
        int GetSearchTime(ECommenceSync.Operations operation);
        int HoursToAdjust { get; }
        int OperationsWorkQueueWaitTime { get; }
        string TaxClassIva0 { get; }
        string TaxClassIva12 { get; }
    }
}
