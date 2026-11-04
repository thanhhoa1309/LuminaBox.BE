namespace LuminaBox.Application.Dtos.Box
{
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
}
