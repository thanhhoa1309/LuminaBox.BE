namespace LuminaBox.Application.Dtos.OrganizerRegistration
{
    public class ApproveOrganizerRegistrationRequest
    {
        // Required for Online, must be omitted for Stall
        public decimal? CommissionRate { get; set; }
    }
}
