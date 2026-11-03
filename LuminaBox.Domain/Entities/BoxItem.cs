using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class BoxItem : BaseEntity
    {
        public Guid OrganizerId { get; set; }
        public Organizer Organizer { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Number of cakes that fit in one box
        public int Capacity { get; set; }

        // Fixed price for the whole box
        public decimal Price { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Draft;

        public ICollection<Stall> Stalls { get; set; } = new List<Stall>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<BoxImage> Images { get; set; } = new List<BoxImage>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
