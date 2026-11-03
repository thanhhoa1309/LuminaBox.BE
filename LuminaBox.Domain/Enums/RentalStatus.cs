namespace LuminaBox.Domain.Enums
{
    public enum RentalStatus
    {
        // Approved by the Manager, waiting for StartDate to take effect
        Confirmed,
        Active,
        Expired,
        Terminated
    }
}
