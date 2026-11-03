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

        public ICollection<Stall> Stalls { get; set; } = new List<Stall>();
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

        public ICollection<BoxItem> BoxItems { get; set; } = new List<BoxItem>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<OrderBoxSelection> BoxSelections { get; set; } = new List<OrderBoxSelection>();
    }
}
