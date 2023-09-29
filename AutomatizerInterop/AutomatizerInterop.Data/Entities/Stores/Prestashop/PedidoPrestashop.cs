using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AutomatizerInterop.Data.Entities.Stores.Prestashop
{
    [Table("StoreSync_PrestashopOrders")]
    public class PedidoPrestashop
    {
        [Key]
        public long Id { get; set; }

        public int CurrentState { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        public string Payment { get; set; }

        public string Cliente { get; set; }

        public string Email { get; set; }

        public string Status { get; set; }

        public bool Valid { get; set; }

        public Decimal TotalPaid { get; set; }

        public Decimal TotalProducts { get; set; }

        public Decimal TotalProductsWT { get; set; }

        public Decimal TotalShipping { get; set; }

        public Decimal TotalShippingTaxIncl { get; set; }

        public Decimal TotalShippingTaxExcl { get; set; }

        public Decimal TotalWrapping { get; set; }

        public Decimal TotalWrappingTaxIncl { get; set; }

        public Decimal TotalWrappingTaxExcl { get; set; }

        public string Reference { get; set; }

        public Decimal TotalDiscounts { get; set; }

        public string CarrierName { get; set; }

        public Decimal? Weight { get; set; }

        public string TrackingNumber { get; set; }

        public string Color { get; set; }
    }
}
