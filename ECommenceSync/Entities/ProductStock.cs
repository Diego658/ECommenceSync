using ECommenceSync.Interfaces;

namespace ECommenceSync.Entities
{
    public class ProductStock<TKey> : IEntity<TKey>
        where TKey : struct
    {
        public TKey Id { get; set; }
        public string Name { get; set; }
        public bool Updated { get; set; }
        public decimal Existencia { get; set; }
        public TKey ProductId { get; set; }
        public int RetryCount { get; set; }

    }
}
