using System.Collections.Concurrent;
using System.Data.Common;
using System.Threading.Tasks;

namespace ECommenceSync.Prestashop.Helpers
{
    public interface IPrestashopDatabaseHelper
    {
        DbConnection GetConnection();
        public string ApiUrl { get; set; }
        public string ApiSecret { get; set; }
        public int SyncLanguage { get; }
        int TaxRuleGroup { get; }
        int TimeToRetryPosponed { get; }
        Task<ConcurrentDictionary<TExternalKey, long>> GetCategoryLinks<TExternalKey>() where TExternalKey:struct;
        Task<ConcurrentDictionary<TExternalKey, long>> GetProductsLinks<TExternalKey>() where TExternalKey : struct;
        Task<ConcurrentDictionary<TExternalKey, long>> GetBrandsLinks<TExternalKey>() where TExternalKey : struct;
        Task<ConcurrentDictionary<TExternalKey, long>> GetTagsLinks<TExternalKey>() where TExternalKey : struct;

    }
}
