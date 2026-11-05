using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.OrganizerRegistration
{
    public class CreateOrganizerRegistrationRequest
    {
        public RegistrationType? Type { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public string? BusinessInfo { get; set; }

        // Stall only, must be omitted for Online
        public Guid? StallId { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }
}
