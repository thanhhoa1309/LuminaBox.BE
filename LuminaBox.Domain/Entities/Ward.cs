namespace LuminaBox.Domain.Entities
{
    public class Ward : BaseEntity
    {
        public Guid DistrictId { get; set; }
        public District District { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? GhnWardCode { get; set; }
    }
}
