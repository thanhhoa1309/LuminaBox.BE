using LuminaBox.Application.Dtos.Payment;

namespace LuminaBox.Application.Interface
{
    public interface IPaymentService
    {
        Task<CheckoutResponse> CreateCheckoutAsync(Guid orderId);
        Task HandleStripeWebhookAsync(string json, string signature);
        Task CancelPaymentAsync(Guid paymentId);
        /// <summary>Stripe cancel redirect (anonymous). Cancels pending payment + unpaid order.</summary>
        Task CancelPaymentFromReturnAsync(Guid paymentId);
        Task<PaymentResponse> GetPaymentByIdAsync(Guid id);
        /// <summary>Manual/FE re-check: ask Stripe session and mark Success if paid.</summary>
        Task<PaymentResponse> SyncCheckoutSessionAsync(Guid paymentId);
        /// <summary>
        /// Stripe redirects here after Checkout (no webhook). Marks payment Success when session is paid.
        /// </summary>
        Task<PaymentResponse> CompleteCheckoutBySessionAsync(string sessionId);
        Task<int> ExpireUnpaidPaymentsAsync();
        Task RefundOrderPaymentAsync(Guid orderId);
        /// <summary>Refunds without failing the caller; the expire worker retries what is left.</summary>
        Task TryRefundOrderPaymentAsync(Guid orderId);
        /// <summary>Refunds cancelled orders whose Stripe payment is still marked Success.</summary>
        Task<int> RetryPendingRefundsAsync();
    }
}
