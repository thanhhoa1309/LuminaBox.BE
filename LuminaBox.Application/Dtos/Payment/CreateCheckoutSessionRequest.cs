namespace LuminaBox.Application.Dtos.Payment
{
    public class CreateCheckoutSessionRequest
    {
        public Guid? PaymentId { get; set; }
        public Guid? OrderId { get; set; }
        public string? SuccessUrl { get; set; }
        public string? CancelUrl { get; set; }
    }
}

