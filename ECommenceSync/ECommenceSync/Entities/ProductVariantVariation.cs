using ECommenceSync.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommenceSync.Entities
{
    public class ProductVariantVariation<TKey> 
        where TKey : struct
    {
        public List<ProductAttributeTerm<TKey>> AttributeTerms { get; set; }
        public Product<TKey> Product { get; set; }

    }
}
