using System.ComponentModel.DataAnnotations;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class Payment : BaseEntity
    {
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public decimal Amount { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; } = "VND";

        public PaymentMethod Method { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        /// <summary>Stripe Checkout Session id (or other gateway session id).</summary>
        [MaxLength(255)]
        public string? CheckoutSessionId { get; set; }

        /// <summary>Gateway transaction id (Stripe PaymentIntent id after success).</summary>
        [MaxLength(255)]
        public string? TransactionId { get; set; }

        public DateTime? PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }
    }
}
