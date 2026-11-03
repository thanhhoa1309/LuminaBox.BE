using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Notification
{
    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? ActorUserId { get; set; }
        public string? ActorName { get; set; }
        public string? EntityType { get; set; }
        public Guid? EntityId { get; set; }
        public NotificationType Type { get; set; }
        public NotificationChannel Channel { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public bool IsRead { get; set; }
        public SendStatus SendStatus { get; set; }
        public DateTime? SentAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
