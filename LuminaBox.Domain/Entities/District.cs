namespace LuminaBox.Domain.Entities
{
    public class District : BaseEntity
    {
        public Guid ProvinceId { get; set; }
        public Province Province { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int? GhnDistrictId { get; set; }

        public ICollection<Ward> Wards { get; set; } = new List<Ward>();
    }
}
