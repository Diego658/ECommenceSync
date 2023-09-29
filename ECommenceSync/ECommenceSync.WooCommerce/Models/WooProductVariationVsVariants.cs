using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommenceSync.WooCommerce.Models
{
    /// <summary>
    /// Relación entre un producto y su producto variable en woocomerce  y sus variantes
    /// </summary>
    /// <typeparam name="TExternalKey"></typeparam>
    public class WooProductVariationVsVariants<TExternalKey>
        where TExternalKey: struct
    {
        public long WooProductId { get; set; }
        public long WooVariationId { get; set; }
        public TExternalKey ExternalId { get; set; }

    }
}
