using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class EmailOtp : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public string CodeHash { get; set; } = string.Empty;
        public OtpPurpose Purpose { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public int FailedAttempts { get; set; }
    }
}
