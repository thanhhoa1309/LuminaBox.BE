namespace LuminaBox.Domain.Entities
{
    public class ProductImage : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string ImageUrl { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsPrimary { get; set; }
    }
}
