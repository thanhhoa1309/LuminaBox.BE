namespace LuminaBox.Domain.Enums
{
    public enum RegistrationStatus
    {
        Pending,

        // Online only: Manager approved with a commission rate, waiting for the user to accept
        AwaitingConfirmation,

        Approved,
        Rejected,
        Cancelled
    }
}
