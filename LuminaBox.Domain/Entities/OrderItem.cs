using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class OrderItem : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public OrderItemType ItemType { get; set; }

        // Exactly one of ProductId / BoxItemId is set, depending on ItemType
        public Guid? ProductId { get; set; }
        public Product? Product { get; set; }

        // The exact variant sold, needed to return stock; null for Box lines
        public Guid? ProductItemId { get; set; }
        public ProductItem? ProductItem { get; set; }

        public Guid? BoxItemId { get; set; }
        public BoxItem? BoxItem { get; set; }

        // Required for Product lines; null for Box lines
        public Guid? CategoryId { get; set; }
        public CakeCategory? Category { get; set; }

        public Guid? FillingId { get; set; }
        public Filling? Filling { get; set; }

        public int Quantity { get; set; }

        // Price snapshot at order time
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }

        public ICollection<OrderBoxSelection> BoxSelections { get; set; } = new List<OrderBoxSelection>();
        public Feedback? Feedback { get; set; }
    }
}
