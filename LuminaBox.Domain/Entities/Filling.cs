namespace LuminaBox.Domain.Entities
{
    public class Filling : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public ICollection<ProductItem> ProductItems { get; set; } = new List<ProductItem>();
    }
}
