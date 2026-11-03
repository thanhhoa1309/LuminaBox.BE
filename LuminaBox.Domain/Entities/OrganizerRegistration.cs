using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class OrganizerRegistration : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public RegistrationType Type { get; set; } = RegistrationType.Stall;

        // Stall only, null for Online
        public Guid? StallId { get; set; }
        public Stall? Stall { get; set; }

        // Filled after the registration is activated
        public Guid? OrganizerId { get; set; }
        public Organizer? Organizer { get; set; }

        public string BrandName { get; set; } = string.Empty;
        public string? ContactPhone { get; set; }
        public string? BusinessInfo { get; set; }

        // Stall only: requested rental period, copied to StallRental on approval
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        // Online only: percentage of each order paid to the platform, set by Manager on approval.
        // Stall rentals are paid by RentalPrice only, so Stall registrations keep it null.
        // An Approved Online registration is the online selling contract.
        public decimal? CommissionRate { get; set; }

        public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;

        public Guid? ReviewedBy { get; set; }
        public User? Reviewer { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? RejectReason { get; set; }

        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }

        public StallRental? StallRental { get; set; }
    }
}
