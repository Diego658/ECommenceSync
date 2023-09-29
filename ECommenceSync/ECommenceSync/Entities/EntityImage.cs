using ECommenceSync.Interfaces;
using System;

namespace ECommenceSync.Entities
{
    public class EntityImage<TKey> : IEntity<TKey>
        where TKey : struct
    {
        public TKey Id { get; set; }
        public TKey ParentId { get; set; }
        public ImageTypes ImageType { get; set; }
        public string Type { get; set; }
        public ImageOperations Operation { get; set; }
        public bool IsDefault { get; set; }
        public string Name { get; set; }
        public DateTimeOffset DateUpdated { get; set; }
        public bool Updated { get; set; }
        public Blob<TKey> Blob { get; set; }
        public int RetryCount { get; set; }
    }
}
