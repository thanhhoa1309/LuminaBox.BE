using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class Voucher : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public VoucherScope Scope { get; set; }

        // Only set when Scope = Organizer
        public Guid? OrganizerId { get; set; }
        public Organizer? Organizer { get; set; }

        // Mapped to BaseEntity.CreatedBy (manager or organizer account)
        public User? Creator { get; set; }

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DiscountType DiscountType { get; set; }

        // Percent or fixed amount depending on DiscountType
        public decimal DiscountValue { get; set; }

        // Cap applied when DiscountType = Percent
        public decimal? MaxDiscountAmount { get; set; }
        public decimal MinOrderAmount { get; set; }

        // null = unlimited
        public int? UsageLimit { get; set; }
        public int UsageLimitPerUser { get; set; } = 1;
        public int UsedCount { get; set; }

        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public VoucherStatus Status { get; set; } = VoucherStatus.Active;

        public ICollection<VoucherUsage> Usages { get; set; } = new List<VoucherUsage>();
        public ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
    }
}
