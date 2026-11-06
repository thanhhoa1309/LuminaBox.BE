using LuminaBox.Domain.Entities;

namespace LuminaBox.Application.Interface
{
    public interface IStripePaymentService
    {
        Task<(string url, string sessionId)> CreateCheckoutSessionAsync(
            Payment payment,
            string orderCode,
            string successUrl,
            string cancelUrl);

        Task<(string eventType, string sessionId, string? transactionId)> ParseWebhookEventAsync(
            string json,
            string stripeSignature);

        /// <summary>
        /// Loads a Checkout Session from Stripe. Returns (paymentStatus, paymentIntentId).
        /// </summary>
        Task<(string paymentStatus, string? paymentIntentId)> GetCheckoutSessionAsync(string sessionId);

        /// <summary>Closes the checkout link. Returns the session state, which is still "paid" if the buyer got there first.</summary>
        Task<(string paymentStatus, string? paymentIntentId)> ExpireCheckoutSessionAsync(string sessionId);

        Task RefundPaymentAsync(string paymentIntentId);
    }
}
