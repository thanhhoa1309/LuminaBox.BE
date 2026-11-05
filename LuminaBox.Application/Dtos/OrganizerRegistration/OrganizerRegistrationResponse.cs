using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.OrganizerRegistration
{
    public class OrganizerRegistrationResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public RegistrationType Type { get; set; }
        public Guid? StallId { get; set; }
        public string? StallCode { get; set; }
        public string? StallName { get; set; }
        public Guid? OrganizerId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string? ContactPhone { get; set; }
        public string? BusinessInfo { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public decimal? CommissionRate { get; set; }
        public RegistrationStatus Status { get; set; }
        public Guid? ReviewedBy { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? RejectReason { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
