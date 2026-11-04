using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class Product : BaseEntity
    {
        public Guid OrganizerId { get; set; }
        public Organizer Organizer { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Draft;

        public ICollection<ProductItem> Items { get; set; } = new List<ProductItem>();
    }
}
