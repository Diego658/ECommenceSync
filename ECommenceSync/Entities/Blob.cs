using ECommenceSync.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace ECommenceSync.Entities
{
    public abstract class Blob<TKey> : IEntity<TKey>
        where TKey : struct
    {
        public TKey Id { get; set; }
        public string Name { get; set; }
        public bool Updated { get; set; }
        public abstract Task<Stream> GetStream();
        public int RetryCount { get; set; }
    }
}
