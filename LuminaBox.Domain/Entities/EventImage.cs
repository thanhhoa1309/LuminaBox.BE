namespace LuminaBox.Domain.Entities
{
    public class EventImage : BaseEntity
    {
        public Guid EventId { get; set; }
        public Event Event { get; set; } = null!;

        public string ImageUrl { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsPrimary { get; set; }
    }
}
