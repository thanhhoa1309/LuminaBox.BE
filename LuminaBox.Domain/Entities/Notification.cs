using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }

        // Null when the notification is raised by the system (background jobs)
        public Guid? ActorUserId { get; set; }
        public User? ActorUser { get; set; }

        public string? EntityType { get; set; }
        public Guid? EntityId { get; set; }

        public NotificationType Type { get; set; }
        public NotificationChannel Channel { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public bool IsRead { get; set; }

        public SendStatus SendStatus { get; set; } = SendStatus.Pending;
        public DateTime? SentAt { get; set; }
    }
}
