using ECommenceSync.Interfaces;
using System;

namespace ECommenceSync
{
    internal class PosponedEntity<TValue, TKey>
        where TKey: struct
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
