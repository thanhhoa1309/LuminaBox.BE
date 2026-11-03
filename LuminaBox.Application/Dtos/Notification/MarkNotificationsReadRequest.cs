namespace LuminaBox.Application.Dtos.Notification
{
    public class MarkNotificationsReadRequest
    {
        // Ignored when MarkAll is true
        public List<Guid>? NotificationIds { get; set; }
        public bool MarkAll { get; set; }
    }
}
