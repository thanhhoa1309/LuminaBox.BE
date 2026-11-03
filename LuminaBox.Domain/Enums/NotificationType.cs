namespace LuminaBox.Domain.Enums
{
    public enum NotificationType
    {
        OrderCreated,
        OrderConfirmed,
        OrderDelivering,
        OrderDelivered,
        OrderCancelled,
        OrganizerApproved,
        OrganizerRejected,
        RegistrationCreated,
        RegistrationAwaitingConfirmation,
        StallRentalExpired,
        StallRentalActivated,
        StallRentalTerminated,
        RegistrationCancelled
    }
}
