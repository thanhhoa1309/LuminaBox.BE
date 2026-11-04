using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Payment
{
    public class CheckoutSessionResponse
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public PaymentStatus Status { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string CheckoutUrl { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}

