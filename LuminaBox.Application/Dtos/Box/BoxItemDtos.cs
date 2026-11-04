using LuminaBox.Application.Dtos.Product;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Box
{
    public class BoxItemResponse
    {
        public Guid Id { get; set; }
        public Guid OrganizerId { get; set; }
        public string OrganizerBrandName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Capacity { get; set; }
        public int Weight { get; set; }
        public decimal LengthCm { get; set; }
        public decimal WidthCm { get; set; }
        public decimal HeightCm { get; set; }
        public decimal Price { get; set; }
        public ProductStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ProductImageResponse> Images { get; set; } = new();
        public List<PriceTierResponse> PriceTiers { get; set; } = new();
        public List<StockResponse> Stocks { get; set; } = new();
    }

    public class BoxAvailableProductResponse
    {
        public Guid ProductItemId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string FillingName { get; set; } = string.Empty;
        public int EggCount { get; set; }
        public int Weight { get; set; }
        public string? ThumbnailUrl { get; set; }
        public int StockQuantity { get; set; }
    }

    public class CreateBoxItemRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Capacity { get; set; }
        public int Weight { get; set; }
        public decimal LengthCm { get; set; }
        public decimal WidthCm { get; set; }
        public decimal HeightCm { get; set; }
        public decimal Price { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }

    public class UpdateBoxItemRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? Capacity { get; set; }
        public int? Weight { get; set; }
        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? Price { get; set; }
    }
}
