namespace LuminaBox.Domain.Entities
{
    public class VoucherUsage : BaseEntity
    {
        public Guid VoucherId { get; set; }
        public Voucher Voucher { get; set; } = null!;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public decimal DiscountAmount { get; set; }
        public DateTime UsedAt { get; set; }
    }
}
