using ECommenceSync.Interfaces;

namespace ECommenceSync.Entities
{
    public class Tag<TKey> : IEntity<TKey>
        where TKey : struct
    {
        public TKey Id { get; set; }
        public string Name { get; set; }
        public bool Updated { get; set; }
        public int RetryCount { get; set; }
    }
}
