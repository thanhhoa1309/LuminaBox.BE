using LuminaBox.Application.Dtos.Payment;

namespace LuminaBox.Application.Interface
{
    public interface IPaymentService
    {
        Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request);
        Task<PaymentResponse> GetPaymentByDetail(Guid id);
        Task<PaymentResponse> SyncPaymentAsync(Guid id);
        Task<PaymentResponse> CancelPaymentAsync(Guid id);
        Task<PaymentResponse> HandleStripeReturnAsync(string? sessionId, Guid? paymentId);
        Task<PaymentResponse> HandleStripeCancelAsync(string? sessionId, Guid? paymentId);
        Task HandleStripeWebhookAsync(string payload, string stripeSignature);
    }
}

