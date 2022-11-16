using ECommenceSync.WooCommerce.Models;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Threading.Tasks;

namespace ECommenceSync.WooCommerce.Helpers
{
    public interface IWooCommerceDatabaseHelper
    {
        DbConnection GetConnection();
        public string ApiUrl { get; set; }
        public string ApiUser { get; set; }
        public string ApiPassword { get; set; }
        int TimeToRetryPosponed { get; }
        string ApiUrlWordpress { get; }
        string ApiWpAppPwd { get; }
        string ApiWpAppUser { get; }
        Task<ConcurrentDictionary<TExternalKey, long>> GetCategoryLinks<TExternalKey>() where TExternalKey : struct;
        Task<ConcurrentDictionary<TExternalKey, long>> GetProductsLinks<TExternalKey>() where TExternalKey : struct;
        //Task<ConcurrentDictionary<long, TExternalKey>> GetInverseProductsLinks<TExternalKey>() where TExternalKey : struct;
        Task<ConcurrentDictionary<TExternalKey, long>> GetBrandsLinks<TExternalKey>() where TExternalKey : struct;
        Task<ConcurrentDictionary<TExternalKey, long>> GetTagsLinks<TExternalKey>() where TExternalKey : struct;
        Task<ConcurrentDictionary<TExternalKey, long>> GetAttributesLinks<TExternalKey>() where TExternalKey : struct;
        Task<ConcurrentDictionary<TExternalKey, long>> GetAttributesTermsLinks<TExternalKey>() where TExternalKey : struct;
        Task<ConcurrentDictionary<TExternalKey, long>> GetProductsVariationsLinks<TExternalKey>() where TExternalKey : struct;
        Task<ConcurrentDictionary<TExternalKey, WooProductVariationVsVariants<TExternalKey>>> GetProductVariationsVsVariants<TExternalKey>() where TExternalKey:struct;
    }
}
