using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class Commission : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public Guid OrganizerId { get; set; }
        public Organizer Organizer { get; set; } = null!;

        // SubtotalAmount - OrganizerDiscount
        public decimal OrderAmount { get; set; }

        // Copied from the organizer's approved Online registration
        public decimal Rate { get; set; }

        // Amount owed to the platform
        public decimal CommissionAmount { get; set; }

        public CommissionStatus Status { get; set; } = CommissionStatus.Pending;
        public DateTime? SettledAt { get; set; }
    }
}
