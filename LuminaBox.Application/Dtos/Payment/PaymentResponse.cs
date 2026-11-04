using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Payment
{
    public class PaymentResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public Guid OrderId { get; set; }
<<<<<<< HEAD
        public string OrderCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
=======
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
>>>>>>> main
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public string? CheckoutSessionId { get; set; }
        public string? TransactionId { get; set; }
<<<<<<< HEAD
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }
    }
}

=======
        public DateTime? PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
>>>>>>> main
