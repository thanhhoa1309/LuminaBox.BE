using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class Event : BaseEntity
    {
        /// <summary>Null = platform event; set = organizer-owned event.</summary>
        public Guid? OrganizerId { get; set; }
        public Organizer? Organizer { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public EventStatus Status { get; set; } = EventStatus.Draft;

        public ICollection<EventImage> Images { get; set; } = new List<EventImage>();
    }
}
