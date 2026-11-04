using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Product
{
    public class ProductSummaryResponse
    {
        public Guid Id { get; set; }
        public Guid OrganizerId { get; set; }
        public string OrganizerBrandName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ProductStatus Status { get; set; }
        public int ItemCount { get; set; }
        public decimal? MinPrice { get; set; }
        public string? ThumbnailUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
