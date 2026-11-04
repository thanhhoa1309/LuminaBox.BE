namespace LuminaBox.Domain.Entities
{
    public class BoxItemImage : BaseEntity
    {
        public Guid BoxItemId { get; set; }
        public BoxItem BoxItem { get; set; } = null!;

        public string Url { get; set; } = string.Empty;
        public int SortOrder { get; set; }
    }
}
