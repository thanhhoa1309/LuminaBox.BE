namespace LuminaBox.Domain.Entities
{
    public class OrderBoxSelection : BaseEntity
    {
        public Guid OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; } = null!;

        public Guid? ProductItemId { get; set; }
        public ProductItem? ProductItem { get; set; }

        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public Guid CategoryId { get; set; }
        public CakeCategory Category { get; set; } = null!;

        public Guid FillingId { get; set; }
        public Filling Filling { get; set; } = null!;

        // Sum of quantities in one box line must equal BoxItem.Capacity
        public int Quantity { get; set; }
    }
}
