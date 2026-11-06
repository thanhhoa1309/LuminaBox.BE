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

        // Maps to the PostgreSQL xmin system column; used as the optimistic concurrency token
        public uint Version { get; set; }

        public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
    }
}
