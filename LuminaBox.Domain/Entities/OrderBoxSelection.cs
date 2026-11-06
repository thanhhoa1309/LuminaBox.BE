namespace LuminaBox.Domain.Entities
{
    // Kept only so old orders stay readable; new orders sell boxes on their own and never write this
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

        public int Quantity { get; set; }
    }
}
