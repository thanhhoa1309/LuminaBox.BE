using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class CartItem : BaseEntity
    {
        public Guid CartId { get; set; }
        public Cart Cart { get; set; } = null!;

        public OrderItemType ItemType { get; set; }

        // Exactly one of ProductItemId / BoxItemId is set, depending on ItemType
        public Guid? ProductItemId { get; set; }
        public ProductItem? ProductItem { get; set; }

        public Guid? BoxItemId { get; set; }
        public BoxItem? BoxItem { get; set; }

        public int Quantity { get; set; }

        // Snapshot: Product → ProductItem.Price; Box → BoxItem.Price
        public decimal UnitPrice { get; set; }

        // Snapshot of selected stall; null for online-only items
        public Guid? StallId { get; set; }
        public Stall? Stall { get; set; }
    }
}
