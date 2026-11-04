namespace LuminaBox.Application.Dtos.Product
{
    public class ProductImageResponse
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
