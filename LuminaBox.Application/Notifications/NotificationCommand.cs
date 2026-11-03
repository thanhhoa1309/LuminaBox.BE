using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Notifications
{
    // Each recipient gets the template of its role; one Notification row is created per channel
    public sealed record NotificationCommand(
        NotificationType Type,
        NotificationAudience Audience,
        NotificationRoleTemplates Templates,
        IReadOnlyDictionary<string, string> Tokens,
        string? EntityType = null,
        Guid? EntityId = null,
        Guid? OrderId = null);
}
