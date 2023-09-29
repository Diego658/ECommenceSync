using ECommenceSync.Interfaces;
using System;

namespace ECommenceSync.WooCommerce.Models
{
    public class PosponedEntity<TKey, TValue>
        where TKey : struct
        where TValue : IEntity<TKey>
    {
        public TValue Entity { get; }
        public DateTime PostponedDate { get; }

        public PosponedEntity(TValue value)
        {
            Entity = value;
            PostponedDate = DateTime.Now;
        }
    }
}
