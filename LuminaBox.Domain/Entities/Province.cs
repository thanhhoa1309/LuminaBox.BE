namespace LuminaBox.Domain.Entities
{
    public class Province : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public int? GhnProvinceId { get; set; }

        public ICollection<District> Districts { get; set; } = new List<District>();
    }
}
