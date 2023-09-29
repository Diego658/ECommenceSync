using System.Collections.Concurrent;
using System.Data.Common;
using System.Threading.Tasks;

namespace ECommenceSync.Interfaces
{
    public interface IDatabaseHelper<TConn, TKey>
        where TKey:struct
        where TConn : DbConnection
    {
        public TConn GetConnection();
        Task<ConcurrentDictionary<TExternalKey, TKey>> GetCategoryLinks<TExternalKey>() where TExternalKey : struct;
        Task<ConcurrentDictionary<TExternalKey, TKey>> GetProductsLinks<TExternalKey>() where TExternalKey : struct;
        Task<ConcurrentDictionary<TExternalKey, TKey>> GetBrandsLinks<TExternalKey>() where TExternalKey : struct;
        Task<ConcurrentDictionary<TExternalKey, TKey>> GetTagsLinks<TExternalKey>() where TExternalKey : struct;
        Task<ConcurrentDictionary<TExternalKey, TKey>> GetCustomersLinks<TExternalKey>() where TExternalKey : struct;
        int TimeToRetryPosponed { get; }

    }
}
