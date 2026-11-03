namespace LuminaBox.Domain.Enums
{
    // Role of the recipient in the event, not the account role (an organizer can also be a customer)
    public enum NotificationRecipientRole
    {
        User,
        Organizer,
        Manager
    }
}
