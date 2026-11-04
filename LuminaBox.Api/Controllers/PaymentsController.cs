using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Dtos.Payment;
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

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [Authorize]
        [HttpPost("checkout")]
        [SwaggerOperation(
            Summary = "Create Stripe checkout session",
            Description = "Creates a Stripe Checkout Session for a pending payment. Requires authentication.")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
        {
            var result = await _paymentService.CreateCheckoutSessionAsync(request);
            return Ok(ApiResult<CheckoutSessionResponse>.Success(result));
        }

        [Authorize]
        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get payment details by id",
            Description = "Gets the details of a payment by id. Requires authentication.")]
        public async Task<IActionResult> GetPaymentByDetail(Guid id)
        {
            var result = await _paymentService.GetPaymentByDetail(id);
            return Ok(ApiResult<PaymentResponse>.Success(result));
        }

        [Authorize]
        [HttpPost("{id:guid}/sync")]
        [SwaggerOperation(
            Summary = "Sync Stripe payment status",
            Description = "Synchronizes the payment status from Stripe Checkout Session. Requires authentication.")]
        public async Task<IActionResult> SyncPayment(Guid id)
        {
            var result = await _paymentService.SyncPaymentAsync(id);
            return Ok(ApiResult<PaymentResponse>.Success(result));
        }

        [Authorize]
        [HttpPatch("{id:guid}/cancel")]
        [SwaggerOperation(
            Summary = "Cancel pending payment",
            Description = "Cancels a pending payment and its associated pending order. Requires authentication.")]
        public async Task<IActionResult> CancelPayment(Guid id)
        {
            var result = await _paymentService.CancelPaymentAsync(id);
            return Ok(ApiResult<PaymentResponse>.Success(result));
        }

        [HttpGet("stripe-return")]
        [SwaggerOperation(
            Summary = "Handle Stripe checkout return redirect",
            Description = "Synchronizes payment status when buyer returns from Stripe Checkout.")]
        public async Task<IActionResult> StripeReturn(
            [FromQuery(Name = "session_id")] string? sessionId,
            [FromQuery] Guid? paymentId)
        {
            var result = await _paymentService.HandleStripeReturnAsync(sessionId, paymentId);
            return Ok(ApiResult<PaymentResponse>.Success(result));
        }

        [HttpGet("stripe-cancel")]
        [SwaggerOperation(
            Summary = "Handle Stripe checkout cancel redirect",
            Description = "Cancels the pending payment when buyer cancels on Stripe Checkout.")]
        public async Task<IActionResult> StripeCancel(
            [FromQuery(Name = "session_id")] string? sessionId,
            [FromQuery] Guid? paymentId)
        {
            var result = await _paymentService.HandleStripeCancelAsync(sessionId, paymentId);
            return Ok(ApiResult<PaymentResponse>.Success(result));
        }

        [HttpPost("stripe-webhook")]
        [SwaggerOperation(
            Summary = "Receive Stripe webhook events",
            Description = "Processes Stripe webhook events to update payment status.")]
        public async Task<IActionResult> StripeWebhook()
        {
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync();
            var signature = Request.Headers["Stripe-Signature"].ToString();

            await _paymentService.HandleStripeWebhookAsync(payload, signature);
            return Ok(ApiResult.Success(message: "Webhook processed."));
        }
    }
}

