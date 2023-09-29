using ECommenceSync.Interfaces;

namespace ECommenceSync.Entities
{
    public class ProductPrice<TKey> : IEntity<TKey>
        where TKey : struct
    {
        public TKey Id { get; set; }
        public string Name { get; set; }
        public bool Updated { get; set; }
        public TKey ParentId { get; set; }
        public TKey ClientGroupId { get; set; }
        public decimal Price { get; set; }
        public int RetryCount { get; set; }
    }
}
