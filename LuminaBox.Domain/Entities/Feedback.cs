using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class Feedback : BaseEntity
    {
        public Guid OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid OrganizerId { get; set; }
        public Organizer Organizer { get; set; } = null!;

        // 1 to 5
        public int Rating { get; set; }
        public string? Comment { get; set; }

        public string? ReplyContent { get; set; }
        public DateTime? RepliedAt { get; set; }

        public FeedbackStatus Status { get; set; } = FeedbackStatus.Visible;

        public ICollection<FeedbackImage> Images { get; set; } = new List<FeedbackImage>();
    }
}
