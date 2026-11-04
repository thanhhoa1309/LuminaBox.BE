namespace LuminaBox.Application.Dtos.Product
{
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
}
