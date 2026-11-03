namespace LuminaBox.Application.Realtime
{
    // Client method names the frontend subscribes to with connection.on(...)
    public static class RealtimeEvents
    {
        public const string NotificationReceived = "notification.received";
        public const string NotificationUnreadCountChanged = "notification.unread-count-changed";
    }
}
