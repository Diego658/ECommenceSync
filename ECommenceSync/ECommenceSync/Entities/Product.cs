using ECommenceSync.Interfaces;
using System;

namespace ECommenceSync.Entities
{
    public class Product<TKey> : IEntity<TKey>
        where TKey : struct
    {
        public TKey Id { get; set; }
        public TKey ParentId { get; set; }
        public TKey? Manufacturerid { get; set; }
        public TKey? SupplierId { get; set; }
        public TKey CombinationDefaultId { get; set; }

        public string Name { get; set; }
        public bool Updated { get; set; }
        public int PositionInCategory { get; set; }
        public string ManufacturerName { get; set; }
        public string Reference { get; set; }
        public string SupplierReference { get; set; }

        public string Location { get; set; }

        public decimal Width { get; set; }

        public decimal Height { get; set; }

        public decimal Depth { get; set; }

        public decimal Weight { get; set; }
        public string EAN13 { get; set; }
        public string ISBN { get; set; }
        public string UPC { get; set; }
        public bool IsVirtual { get; set; }
        public bool State { get; set; }
        public bool OnSale { get; set; }
        public bool OnlineOnly { get; set; }
        public int MinimalQuantity { get; set; }
        public int LowStockThreshold { get; set; }
        public int LowStockAlert { get; set; }
        public decimal Price { get; set; }
        public decimal WholesalePrice { get; set; }
        public decimal AdditionalShippingCost { get; set; }
        public bool Active { get; set; }
        public bool AvailableForOrder { get; set; }
        public DateTime? AvailableDate { get; set; }
        public bool ShowCondition { get; set; }
        public string Condition { get; set; }
        public bool ShowPrice { get; set; }
        public string LinkRewrite { get; set; }
        public string Description { get; set; }
        public string DescriptionShort { get; set; }
        public decimal StockAvailable { get; set; }
        public TKey? BrandId { get; set; }
        public bool HasTaxes { get; set; }
        public bool HasOptions { get; set; }
        public string TagIds { get; set; }
        public int RetryCount { get; set; }
        public bool HasVariants { get; set; }
    }
}
