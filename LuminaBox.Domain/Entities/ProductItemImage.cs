namespace LuminaBox.Domain.Entities
{
    public class ProductItemImage : BaseEntity
    {
        public Guid ProductItemId { get; set; }
        public ProductItem ProductItem { get; set; } = null!;

        public string Url { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
