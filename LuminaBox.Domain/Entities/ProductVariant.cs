namespace LuminaBox.Domain.Entities
{
    public class ProductVariant : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public Guid CategoryId { get; set; }
        public CakeCategory Category { get; set; } = null!;

        public Guid FillingId { get; set; }
        public Filling Filling { get; set; } = null!;

        public decimal Price { get; set; }
    }
}
