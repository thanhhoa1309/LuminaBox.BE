using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Product
{
    public class ProductImageResponse
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }

    public class PriceTierResponse
    {
        public int MinQuantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class StockResponse
    {
        public Guid Id { get; set; }
        public Guid? StallId { get; set; }
        public string? StallName { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
    }

    public class ProductItemResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public Guid FillingId { get; set; }
        public string FillingName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int EggCount { get; set; }
        public int Weight { get; set; }
        public decimal LengthCm { get; set; }
        public decimal WidthCm { get; set; }
        public decimal HeightCm { get; set; }
        public decimal Price { get; set; }
        public List<ProductImageResponse> Images { get; set; } = new();
        public List<PriceTierResponse> PriceTiers { get; set; } = new();
        public List<StockResponse> Stocks { get; set; } = new();
    }

    public class ProductResponse
    {
        public Guid Id { get; set; }
        public Guid OrganizerId { get; set; }
        public string OrganizerBrandName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ProductStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ProductItemResponse> Items { get; set; } = new();
    }

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

    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateProductRequest
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }

    public class UpdateProductStatusRequest
    {
        public ProductStatus Status { get; set; }
    }

    public class CreateProductItemRequest
    {
        public Guid CategoryId { get; set; }
        public Guid FillingId { get; set; }
        public string? Name { get; set; }
        public int EggCount { get; set; }
        public int Weight { get; set; }
        public decimal LengthCm { get; set; }
        public decimal WidthCm { get; set; }
        public decimal HeightCm { get; set; }
        public decimal Price { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }

    public class UpdateProductItemRequest
    {
        public Guid? CategoryId { get; set; }
        public Guid? FillingId { get; set; }
        public string? Name { get; set; }
        public int? EggCount { get; set; }
        public int? Weight { get; set; }
        public decimal? LengthCm { get; set; }
        public decimal? WidthCm { get; set; }
        public decimal? HeightCm { get; set; }
        public decimal? Price { get; set; }
    }

    public class AddProductItemImageRequest
    {
        public string Url { get; set; } = string.Empty;
        public int? SortOrder { get; set; }
    }

    public class PriceTierRequest
    {
        public int MinQuantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class StockRequest
    {
        public Guid? StallId { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
