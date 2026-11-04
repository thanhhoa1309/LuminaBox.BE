using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    // The combo box itself: an empty box with a fixed price; the buyer picks the cakes, stock lives in StallBoxItem
    public class BoxItem : BaseEntity
    {
        public Guid OrganizerId { get; set; }
        public Organizer Organizer { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Capacity { get; set; }
        public int Weight { get; set; }
        public decimal LengthCm { get; set; }
        public decimal WidthCm { get; set; }
        public decimal HeightCm { get; set; }
        public decimal Price { get; set; }
        public ProductStatus Status { get; set; } = ProductStatus.Draft;

        public ICollection<BoxItemImage> Images { get; set; } = new List<BoxItemImage>();
        public ICollection<PriceTier> PriceTiers { get; set; } = new List<PriceTier>();
        public ICollection<StallBoxItem> Stocks { get; set; } = new List<StallBoxItem>();
    }
}
