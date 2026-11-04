namespace LuminaBox.Application.Dtos.Catalog
{
    public class LookupResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class LookupRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
