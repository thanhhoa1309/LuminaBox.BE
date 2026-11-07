using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Order
{
    public class OrderPaymentSummaryResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public string? CheckoutSessionId { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }
    }
}

