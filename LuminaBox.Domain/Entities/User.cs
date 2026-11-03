using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
        public UserStatus Status { get; set; } = UserStatus.Active;
        public bool IsEmailVerified { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public Organizer? Organizer { get; set; }
        public Wallet? Wallet { get; set; }
        public Cart? Cart { get; set; }
        public ICollection<OrganizerRegistration> OrganizerRegistrations { get; set; } = new List<OrganizerRegistration>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
        public ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
        public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<EmailOtp> EmailOtps { get; set; } = new List<EmailOtp>();
    }
}
