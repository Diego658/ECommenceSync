using ECommenceSync.Interfaces;

namespace ECommenceSync.Entities
{
    public class Brand<TKey> : IEntity<TKey>
        where TKey : struct
    {
        public TKey Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string DescriptionShort { get; set; }
        public bool Updated { get; set; }
        public Blob<TKey> ImageBlob { get; set; }
        public int RetryCount { get; set; }
    }
}
