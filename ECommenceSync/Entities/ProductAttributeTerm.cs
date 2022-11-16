using ECommenceSync.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommenceSync.Entities
{
    public class ProductAttributeTerm<TKey> : IEntity<TKey>
        where TKey : struct
    {
        public TKey Id { get; set; }
        public TKey AttributeId { get; set; }
        public string Name { get; set; }
        public bool Updated { get; set; }
        public int RetryCount { get; set; }
    }
}
