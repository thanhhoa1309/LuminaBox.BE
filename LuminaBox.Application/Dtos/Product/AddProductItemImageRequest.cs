namespace LuminaBox.Application.Dtos.Product
{
    public class AddProductItemImageRequest
    {
        public string Url { get; set; } = string.Empty;
        public int? SortOrder { get; set; }
    }
}
