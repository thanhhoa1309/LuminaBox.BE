using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Notifications
{
    public sealed record NotificationTemplate(
        string Title,
        string Content,
        IReadOnlyList<NotificationChannel> Channels);

    public sealed class NotificationRoleTemplates
    {
        private readonly Dictionary<NotificationRecipientRole, NotificationTemplate> _templates = new();

        private NotificationRoleTemplates()
        {
        }

        public IReadOnlyDictionary<NotificationRecipientRole, NotificationTemplate> Templates => _templates;

        public static NotificationRoleTemplates For(
            NotificationRecipientRole role,
            string title,
            string content,
            params NotificationChannel[] channels)
            => new NotificationRoleTemplates().And(role, title, content, channels);

        public NotificationRoleTemplates And(
            NotificationRecipientRole role,
            string title,
            string content,
            params NotificationChannel[] channels)
        {
            _templates[role] = new NotificationTemplate(title, content, channels.Distinct().ToList());
            return this;
        }

        public NotificationTemplate? Get(NotificationRecipientRole role)
            => _templates.TryGetValue(role, out var template) ? template : null;
    }
}
