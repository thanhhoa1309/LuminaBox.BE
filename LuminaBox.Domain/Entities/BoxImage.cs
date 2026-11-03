namespace LuminaBox.Domain.Entities
{
    public class BoxImage : BaseEntity
    {
        public Guid BoxItemId { get; set; }
        public BoxItem BoxItem { get; set; } = null!;

        public string ImageUrl { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsPrimary { get; set; }
    }
}
