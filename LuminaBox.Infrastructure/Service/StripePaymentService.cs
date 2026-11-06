using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using Stripe;
using Stripe.Checkout;

namespace LuminaBox.Infrastructure.Service
{
    public class StripePaymentService : IStripePaymentService
    {
        private readonly StripeSettings _settings;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(IOptions<StripeSettings> settings, ILogger<StripePaymentService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            StripeConfiguration.ApiKey = _settings.SecretKey;
        }

        public async Task<(string url, string sessionId)> CreateCheckoutSessionAsync(
            Payment payment,
            string orderCode,
            string successUrl,
            string cancelUrl)
        {
            _logger.LogInformation(
                "[Stripe] Creating checkout session for payment {PaymentId}, order {OrderCode}",
                payment.Id, orderCode);

            var options = new SessionCreateOptions
            {
                Mode = "payment",
                // VND: pin card to avoid automatic PMs that break Checkout.
                PaymentMethodTypes = ["card"],
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = payment.Currency.ToLowerInvariant(),
                            // VND has zero decimal places in Stripe
                            UnitAmount = (long)payment.Amount,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"LuminaBox order {orderCode}",
                                Description = $"Payment for order {orderCode}"
                            }
                        }
                    }
                ],
                Metadata = new Dictionary<string, string>
                {
                    { "paymentId", payment.Id.ToString() },
                    { "orderId", payment.OrderId.ToString() }
                },
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            _logger.LogInformation("[Stripe] Session created: {SessionId}", session.Id);
            return (session.Url, session.Id);
        }

        public Task<(string eventType, string sessionId, string? transactionId)> ParseWebhookEventAsync(
            string json,
            string stripeSignature)
        {
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    _settings.WebhookSecret);

                _logger.LogInformation("[Stripe] Webhook event type: {Type}", stripeEvent.Type);

                if (stripeEvent.Data.Object is Session session)
                {
                    return Task.FromResult((stripeEvent.Type, session.Id, (string?)session.PaymentIntentId));
                }

                return Task.FromResult((stripeEvent.Type, string.Empty, (string?)null));
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "[Stripe] Webhook signature validation failed.");
                throw ErrorHelper.BadRequest($"Invalid Stripe webhook signature: {ex.Message}");
            }
        }

        public async Task<(string paymentStatus, string? paymentIntentId)> GetCheckoutSessionAsync(string sessionId)
        {
            var service = new SessionService();
            var session = await service.GetAsync(sessionId);
            return (session.PaymentStatus, session.PaymentIntentId);
        }

        public async Task RefundPaymentAsync(string paymentIntentId)
        {
            _logger.LogInformation("[Stripe] Refunding PaymentIntent {PaymentIntentId}", paymentIntentId);

            var service = new RefundService();
            await service.CreateAsync(
                new RefundCreateOptions { PaymentIntent = paymentIntentId },
                new RequestOptions { IdempotencyKey = $"refund-{paymentIntentId}" });
        }
    }
}
