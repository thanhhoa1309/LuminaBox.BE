namespace LuminaBox.Application.Dtos.Geography
{
    public class ProvinceResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int? GhnProvinceId { get; set; }
    }
}
