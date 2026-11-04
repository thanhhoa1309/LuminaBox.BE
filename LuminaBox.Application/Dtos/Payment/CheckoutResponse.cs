namespace LuminaBox.Application.Dtos.Payment
{
    public class CheckoutResponse
    {
        public Guid PaymentId { get; set; }
        public Guid OrderId { get; set; }
        public decimal Amount { get; set; }
        public string CheckoutUrl { get; set; } = string.Empty;
    }
}