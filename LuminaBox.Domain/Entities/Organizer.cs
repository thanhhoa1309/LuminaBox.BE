using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class Organizer : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string BrandName { get; set; } = string.Empty;
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? BusinessInfo { get; set; }
        public OrganizerStatus Status { get; set; } = OrganizerStatus.Active;

        public Guid? ApprovedBy { get; set; }
        public User? Approver { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // Filled while Status = Suspended, cleared on reactivation
        public string? SuspendReason { get; set; }
        public DateTime? SuspendedAt { get; set; }

        public ICollection<OrganizerRegistration> Registrations { get; set; } = new List<OrganizerRegistration>();
        public ICollection<StallRental> StallRentals { get; set; } = new List<StallRental>();
        public ICollection<CakeCategory> CakeCategories { get; set; } = new List<CakeCategory>();
        public ICollection<Filling> Fillings { get; set; } = new List<Filling>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<BoxItem> BoxItems { get; set; } = new List<BoxItem>();
        public ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public ICollection<Commission> Commissions { get; set; } = new List<Commission>();
        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
