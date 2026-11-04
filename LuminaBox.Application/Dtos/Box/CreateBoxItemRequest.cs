namespace LuminaBox.Application.Dtos.Box
{
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
}
