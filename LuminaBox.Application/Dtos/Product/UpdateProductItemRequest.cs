namespace LuminaBox.Application.Dtos.Product
{
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
}
