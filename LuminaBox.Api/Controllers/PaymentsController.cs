using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Dtos.Payment;
using LuminaBox.Application.Exceptions;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public PaymentsController(IPaymentService paymentService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
        }

        private string FrontendUrl => (_configuration["APP_FRONTEND_URL"] ?? "http://localhost:5173").TrimEnd('/');

        [Authorize]
        [HttpPost("checkout")]
        [SwaggerOperation(
            Summary = "Create Stripe checkout session for an order",
            Description = "Creates a Stripe Checkout Session for a pending unpaid order and returns the hosted checkout URL. Requires authentication.")]
        public async Task<IActionResult> CreateCheckout([FromBody] CheckoutRequest request)
        {
            var result = await _paymentService.CreateCheckoutAsync(request.OrderId);
            return Ok(ApiResult<CheckoutResponse>.Success(result));
        }

        [Authorize]
        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get payment details by id",
            Description = "Gets payment details by id for the buyer or the order organizer. Requires authentication.")]
        public async Task<IActionResult> GetPaymentById(Guid id)
        {
            var result = await _paymentService.GetPaymentByIdAsync(id);
            return Ok(ApiResult<PaymentResponse>.Success(result));
        }

        [Authorize]
        [HttpPost("{id:guid}/sync")]
        [SwaggerOperation(
            Summary = "Sync Stripe checkout session status",
            Description = "Asks Stripe for the Checkout Session status and marks the payment Success when paid. Requires authentication.")]
        public async Task<IActionResult> SyncCheckoutSession(Guid id)
        {
            var result = await _paymentService.SyncCheckoutSessionAsync(id);
            return Ok(ApiResult<PaymentResponse>.Success(result));
        }

        [Authorize]
        [HttpPatch("{id:guid}/cancel")]
        [SwaggerOperation(
            Summary = "Cancel a pending payment",
            Description = "Marks a pending payment as cancelled and cancels the related unpaid order. Requires authentication.")]
        public async Task<IActionResult> CancelPayment(Guid id)
        {
            await _paymentService.CancelPaymentAsync(id);
            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("stripe-return")]
        [SwaggerOperation(
            Summary = "Stripe Checkout return (no webhook)",
            Description = "Stripe redirects here after successful payment. Verifies the session with Stripe and marks payment Success.")]
        public async Task<IActionResult> StripeReturn([FromQuery(Name = "session_id")] string sessionId)
        {
            try
            {
                var result = await _paymentService.CompleteCheckoutBySessionAsync(sessionId);
                return Redirect($"{FrontendUrl}/payment/result?status=success&orderId={result.OrderId}");
            }
            catch (AppException)
            {
                return Redirect($"{FrontendUrl}/payment/result?status=failed");
            }
        }

        [AllowAnonymous]
        [HttpGet("stripe-cancel")]
        [SwaggerOperation(
            Summary = "Stripe Checkout cancel return",
            Description = "Stripe redirects here when the buyer cancels Checkout. Marks pending payment Cancelled when possible.")]
        public async Task<IActionResult> StripeCancel([FromQuery] Guid paymentId)
        {
            if (paymentId != Guid.Empty)
            {
                try
                {
                    await _paymentService.CancelPaymentFromReturnAsync(paymentId);
                }
                catch
                {
                    // Idempotent UI
                }
            }

            return Redirect($"{FrontendUrl}/payment/result?status=cancelled");
        }

        [AllowAnonymous]
        [HttpPost("stripe-webhook")]
        [SwaggerOperation(
            Summary = "Stripe webhook (optional / production)",
            Description = "Optional. Not required for local Checkout return flow.")]
        public async Task<IActionResult> StripeWebhook()
        {
            string json;
            using (var reader = new StreamReader(HttpContext.Request.Body))
                json = await reader.ReadToEndAsync();

            var stripeSignature = Request.Headers["Stripe-Signature"].FirstOrDefault();
            if (string.IsNullOrEmpty(stripeSignature))
                return BadRequest("Missing Stripe-Signature header.");

            await _paymentService.HandleStripeWebhookAsync(json, stripeSignature);
            return Ok();
        }
    }
}
