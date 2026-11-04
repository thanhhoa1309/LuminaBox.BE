namespace LuminaBox.Domain.Entities
{
    public class ProductItem : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public Guid CategoryId { get; set; }
        public CakeCategory Category { get; set; } = null!;

        public Guid FillingId { get; set; }
        public Filling Filling { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public int EggCount { get; set; }
        public int Weight { get; set; }
        public decimal LengthCm { get; set; }
        public decimal WidthCm { get; set; }
        public decimal HeightCm { get; set; }
        public decimal Price { get; set; }

        public ICollection<ProductItemImage> Images { get; set; } = new List<ProductItemImage>();
        public ICollection<PriceTier> PriceTiers { get; set; } = new List<PriceTier>();
        public ICollection<StallProductItem> Stocks { get; set; } = new List<StallProductItem>();
    }
}
