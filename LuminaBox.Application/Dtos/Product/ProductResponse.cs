using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Product
{
    public class ProductResponse
    {
        public Guid Id { get; set; }
        public Guid OrganizerId { get; set; }
        public string OrganizerBrandName { get; set; } = string.Empty;
        public string? OrganizerBusinessInfo { get; set; }
        public string? OrganizerContactPhone { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ProductStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ProductItemResponse> Items { get; set; } = new();
    }
}
