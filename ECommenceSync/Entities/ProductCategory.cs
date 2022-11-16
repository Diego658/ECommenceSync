using ECommenceSync.Interfaces;

namespace ECommenceSync.Entities
{
    public class ProductCategory<TKey> : IEntity<TKey>
        where TKey : struct
    {
        public TKey Id { get; set; }
        public TKey? ParentId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public bool Active { get; set; }
        public bool IsRootCategory { get; set; }
        public int Position { get; set; }
        public string Description { get; set; }
        public string DescriptionShort { get; set; }
        public bool Selected { get; set; }
        public bool Updated { get; set; }
        public int RetryCount { get; set; } 

    }
}
