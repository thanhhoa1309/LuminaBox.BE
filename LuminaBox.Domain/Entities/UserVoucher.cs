namespace LuminaBox.Domain.Entities
{
    /// <summary>Voucher claimed into a user's wallet.</summary>
    public class UserVoucher : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid VoucherId { get; set; }
        public Voucher Voucher { get; set; } = null!;

        public DateTime ClaimedAt { get; set; }
    }
}
