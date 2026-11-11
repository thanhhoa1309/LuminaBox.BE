using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Organizer
{
    public class OrganizerProfileResponse
    {
        public Guid Id { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? BusinessInfo { get; set; }
        public OrganizerStatus Status { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? SuspendReason { get; set; }
        public DateTime? SuspendedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateOrganizerProfileRequest
    {
        public string? BrandName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? BusinessInfo { get; set; }
    }
}
