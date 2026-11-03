using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Notifications
{
    public sealed record NotificationRecipient(Guid UserId, NotificationRecipientRole Role);

    public sealed record NotificationAudience
    {
        public IReadOnlyList<NotificationRecipient> Recipients { get; }

        // Resolved to every active Manager account when the notification is dispatched
        public bool IncludeManagers { get; }

        private NotificationAudience(IReadOnlyList<NotificationRecipient> recipients, bool includeManagers)
        {
            Recipients = recipients;
            IncludeManagers = includeManagers;
        }

        public static NotificationAudience ForUser(Guid userId, NotificationRecipientRole role)
            => new(new[] { new NotificationRecipient(userId, role) }, false);

        public static NotificationAudience ForManagers()
            => new(Array.Empty<NotificationRecipient>(), true);

        public static NotificationAudience ForOrderParties(Guid customerUserId, Guid organizerUserId)
            => new(
                new[]
                {
                    new NotificationRecipient(customerUserId, NotificationRecipientRole.User),
                    new NotificationRecipient(organizerUserId, NotificationRecipientRole.Organizer)
                },
                false);
    }
}
