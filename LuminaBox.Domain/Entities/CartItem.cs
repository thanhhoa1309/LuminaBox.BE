using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class CartItem : BaseEntity
    {
        public Guid CartId { get; set; }
        public Cart Cart { get; set; } = null!;

        public OrderItemType ItemType { get; set; }

        // Exactly one of ProductId / BoxItemId is set, depending on ItemType
        public Guid? ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid? BoxItemId { get; set; }
        public BoxItem? BoxItem { get; set; }

        // Required for Product lines; null for Box lines (box selections chosen at checkout)
        public Guid? CategoryId { get; set; }
        public CakeCategory? Category { get; set; }

        public Guid? FillingId { get; set; }
        public Filling? Filling { get; set; }

        public int Quantity { get; set; }

        // Snapshot: Product → ProductVariant.Price; Box → BoxItem.Price
        public decimal UnitPrice { get; set; }

        // Snapshot of selected stall; null for online-only items
        public Guid? StallId { get; set; }
        public Stall? Stall { get; set; }
    }
}
