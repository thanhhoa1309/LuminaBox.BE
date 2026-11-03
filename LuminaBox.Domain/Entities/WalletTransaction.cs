using System.ComponentModel.DataAnnotations;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class WalletTransaction : BaseEntity
    {
        public Guid WalletId { get; set; }
        public Wallet Wallet { get; set; } = null!;

        public WalletTransactionType Type { get; set; }

        /// <summary>Signed amount: positive credit, negative debit.</summary>
        public decimal Amount { get; set; }

        public decimal BalanceAfter { get; set; }

        public Guid? OrderId { get; set; }
        public Order? Order { get; set; }

        public Guid? CommissionId { get; set; }
        public Commission? Commission { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
