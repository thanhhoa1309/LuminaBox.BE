using System.ComponentModel.DataAnnotations;

namespace LuminaBox.Domain.Entities
{
    public class Wallet : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public decimal AvailableBalance { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; } = "VND";

        public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
    }
}
