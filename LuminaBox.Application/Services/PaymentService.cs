using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using LuminaBox.Application.Dtos.Payment;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class PaymentService : IPaymentService
    {
        public const int UnpaidExpireMinutes = 10;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly ICurrentTime _currentTime;
        private readonly IStripePaymentService _stripe;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PaymentService> _logger;
        private readonly IWalletService _walletService;
        private readonly IOrderStockService _orderStockService;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IClaimsService claimsService,
            ICurrentTime currentTime,
            IStripePaymentService stripe,
            IConfiguration configuration,
            ILogger<PaymentService> logger,
            IWalletService walletService,
            IOrderStockService orderStockService)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _currentTime = currentTime;
            _stripe = stripe;
            _configuration = configuration;
            _logger = logger;
            _walletService = walletService;
            _orderStockService = orderStockService;
        }

        public async Task<CheckoutResponse> CreateCheckoutAsync(Guid orderId)
        {
            var userId = _claimsService.GetRequiredUserId();

            var order = await _unitOfWork.Orders.GetByIdAsync(orderId)
                ?? throw ErrorHelper.NotFound("Order not found.");

            if (order.UserId != userId)
                throw ErrorHelper.Forbidden("You can only checkout your own orders.");

            if (order.Status != OrderStatus.Pending)
                throw ErrorHelper.Conflict("Only pending orders can be checked out.");

            var payment = await _unitOfWork.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId)
                ?? throw ErrorHelper.NotFound("Payment not found for this order.");

            if (payment.Method != PaymentMethod.Stripe)
                throw ErrorHelper.BadRequest("Cash on delivery checkout is not supported yet.");

            if (payment.Status == PaymentStatus.Success)
                throw ErrorHelper.Conflict("This order has already been paid.");

            if (payment.Status is PaymentStatus.Cancelled or PaymentStatus.Failed or PaymentStatus.Refunded)
                throw ErrorHelper.Conflict($"Payment is {payment.Status} and cannot be checked out.");

            var expireAt = payment.CreatedAt.AddMinutes(UnpaidExpireMinutes);
            if (_currentTime.GetCurrentTime() >= expireAt)
                throw ErrorHelper.Conflict("Payment window has expired. Please create a new order.");

            // No webhook required: Stripe redirects browser to API after pay.
            var apiUrl = (_configuration["APP_PUBLIC_API_URL"]
                ?? "http://localhost:5000").TrimEnd('/');
            var successUrl =
                $"{apiUrl}/api/payments/stripe-return?session_id={{CHECKOUT_SESSION_ID}}";
            var cancelUrl =
                $"{apiUrl}/api/payments/stripe-cancel?paymentId={payment.Id}";

            var (checkoutUrl, sessionId) = await _stripe.CreateCheckoutSessionAsync(
                payment,
                order.OrderCode,
                successUrl,
                cancelUrl);

            payment.CheckoutSessionId = sessionId;
            await _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "[CreateCheckout] User {UserId} opened Stripe checkout for order {OrderId}. Payment={PaymentId}",
                userId, orderId, payment.Id);

            return new CheckoutResponse
            {
                PaymentId = payment.Id,
                OrderId = order.Id,
                Amount = payment.Amount,
                CheckoutUrl = checkoutUrl
            };
        }

        public async Task HandleStripeWebhookAsync(string json, string signature)
        {
            var (eventType, sessionId, transactionId) = await _stripe.ParseWebhookEventAsync(json, signature);

            switch (eventType)
            {
                case "checkout.session.completed":
                {
                    _logger.LogInformation(
                        "[StripeWebhook] Processing checkout.session.completed for session {SessionId}",
                        sessionId);

                    var payment = await _unitOfWork.Payments.FirstOrDefaultAsync(
                        p => p.CheckoutSessionId == sessionId)
                        ?? throw ErrorHelper.NotFound($"Payment not found for Stripe session '{sessionId}'.");

                    await HandlePaymentSuccessAsync(payment, transactionId ?? sessionId);
                    break;
                }

                case "checkout.session.expired":
                {
                    _logger.LogInformation("[StripeWebhook] Session expired: {SessionId}", sessionId);

                    var payment = await _unitOfWork.Payments.FirstOrDefaultAsync(
                        p => p.CheckoutSessionId == sessionId);

                    if (payment is not null)
                        await AbandonUnpaidPaymentAsync(payment, PaymentStatus.Failed, "Stripe checkout session expired.");
                    else
                        _logger.LogWarning("[StripeWebhook] No payment found for expired session {SessionId}", sessionId);

                    break;
                }

                default:
                    _logger.LogInformation("[StripeWebhook] Ignoring event type: {Type}", eventType);
                    break;
            }
        }

        public async Task CancelPaymentAsync(Guid paymentId)
        {
            var userId = _claimsService.GetRequiredUserId();
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId)
                ?? throw ErrorHelper.NotFound("Payment not found.");

            if (payment.UserId != userId)
                throw ErrorHelper.Forbidden("You can only cancel your own payments.");

            if (payment.Status != PaymentStatus.Pending)
            {
                _logger.LogInformation(
                    "[CancelPayment] Payment {Id} is already {Status}. Skipping.",
                    payment.Id, payment.Status);
                return;
            }

            await AbandonUnpaidPaymentAsync(payment, PaymentStatus.Cancelled, "Buyer cancelled Stripe checkout.");
        }

        public async Task CancelPaymentFromReturnAsync(Guid paymentId)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId)
                ?? throw ErrorHelper.NotFound("Payment not found.");

            if (payment.Status != PaymentStatus.Pending)
            {
                _logger.LogInformation(
                    "[CancelPaymentFromReturn] Payment {Id} is already {Status}. Skipping.",
                    payment.Id, payment.Status);
                return;
            }

            await AbandonUnpaidPaymentAsync(payment, PaymentStatus.Cancelled, "Buyer cancelled Stripe checkout.");
        }

        public async Task<PaymentResponse> GetPaymentByIdAsync(Guid id)
        {
            var userId = _claimsService.GetRequiredUserId();
            var payment = await _unitOfWork.Payments.GetByIdAsync(id)
                ?? throw ErrorHelper.NotFound("Payment not found.");

            if (payment.UserId != userId)
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(payment.OrderId)
                    ?? throw ErrorHelper.NotFound("Order not found.");

                var organizer = await _unitOfWork.Organizers.FirstOrDefaultAsync(o => o.UserId == userId);
                if (organizer is null || order.OrganizerId != organizer.Id)
                    throw ErrorHelper.Forbidden("You do not have permission to view this payment.");
            }

            return ToPaymentResponse(payment);
        }

        public async Task<PaymentResponse> SyncCheckoutSessionAsync(Guid paymentId)
        {
            var userId = _claimsService.GetRequiredUserId();
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId)
                ?? throw ErrorHelper.NotFound("Payment not found.");

            if (payment.UserId != userId)
                throw ErrorHelper.Forbidden("You can only sync your own payments.");

            if (payment.Status == PaymentStatus.Success)
                return ToPaymentResponse(payment);

            if (payment.Method != PaymentMethod.Stripe)
                throw ErrorHelper.BadRequest("Only Stripe payments can be synced from checkout.");

            if (payment.Status != PaymentStatus.Pending)
                throw ErrorHelper.Conflict($"Payment is {payment.Status} and cannot be synced.");

            if (string.IsNullOrWhiteSpace(payment.CheckoutSessionId))
                throw ErrorHelper.Conflict("Checkout session has not been created yet.");

            var (paymentStatus, paymentIntentId) = await _stripe.GetCheckoutSessionAsync(payment.CheckoutSessionId);

            _logger.LogInformation(
                "[SyncCheckout] Payment {PaymentId} Stripe status={Status}",
                payment.Id, paymentStatus);

            if (!string.Equals(paymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
                throw ErrorHelper.Conflict($"Stripe checkout is not paid yet (status: {paymentStatus}).");

            await HandlePaymentSuccessAsync(payment, paymentIntentId ?? payment.CheckoutSessionId);

            var updated = await _unitOfWork.Payments.GetByIdAsync(paymentId)
                ?? throw ErrorHelper.NotFound("Payment not found.");
            return ToPaymentResponse(updated);
        }

        public async Task<PaymentResponse> CompleteCheckoutBySessionAsync(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw ErrorHelper.BadRequest("Stripe session id is required.");

            var payment = await _unitOfWork.Payments.FirstOrDefaultAsync(
                p => p.CheckoutSessionId == sessionId)
                ?? throw ErrorHelper.NotFound($"Payment not found for Stripe session '{sessionId}'.");

            if (payment.Status == PaymentStatus.Success)
                return ToPaymentResponse(payment);

            if (payment.Method != PaymentMethod.Stripe)
                throw ErrorHelper.BadRequest("Only Stripe payments can be completed from checkout.");

            if (payment.Status != PaymentStatus.Pending)
                throw ErrorHelper.Conflict($"Payment is {payment.Status} and cannot be completed.");

            var (paymentStatus, paymentIntentId) = await _stripe.GetCheckoutSessionAsync(sessionId);

            _logger.LogInformation(
                "[CompleteCheckout] Payment {PaymentId} Stripe status={Status}",
                payment.Id, paymentStatus);

            if (!string.Equals(paymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
                throw ErrorHelper.Conflict($"Stripe checkout is not paid yet (status: {paymentStatus}).");

            await HandlePaymentSuccessAsync(payment, paymentIntentId ?? sessionId);

            var updated = await _unitOfWork.Payments.GetByIdAsync(payment.Id)
                ?? throw ErrorHelper.NotFound("Payment not found.");
            return ToPaymentResponse(updated);
        }

        public async Task<int> ExpireUnpaidPaymentsAsync()
        {
            var now = _currentTime.GetCurrentTime();
            var cutoff = now.AddMinutes(-UnpaidExpireMinutes);

            var expiredPayments = await _unitOfWork.Payments.GetQueryable()
                .Where(p =>
                    p.Method == PaymentMethod.Stripe &&
                    p.Status == PaymentStatus.Pending &&
                    p.CreatedAt < cutoff)
                .ToListAsync();

            var count = 0;
            foreach (var payment in expiredPayments)
            {
                await AbandonUnpaidPaymentAsync(
                    payment,
                    PaymentStatus.Cancelled,
                    "Payment was not completed within 10 minutes.");
                count++;
            }

            return count;
        }

        public async Task RefundOrderPaymentAsync(Guid orderId)
        {
            var payment = await _unitOfWork.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId)
                ?? throw ErrorHelper.NotFound("Payment not found for this order.");

            if (payment.Status == PaymentStatus.Refunded)
                return;

            if (payment.Status != PaymentStatus.Success)
                throw ErrorHelper.Conflict("Only successful payments can be refunded.");

            if (payment.Method == PaymentMethod.Stripe)
                await _walletService.ReverseOrderEarningAsync(orderId);

            if (payment.Method == PaymentMethod.Stripe &&
                !string.IsNullOrWhiteSpace(payment.TransactionId))
            {
                await _stripe.RefundPaymentAsync(payment.TransactionId);
            }

            payment.Status = PaymentStatus.Refunded;
            payment.RefundedAt = _currentTime.GetCurrentTime();
            await _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task TryRefundOrderPaymentAsync(Guid orderId)
        {
            try
            {
                await RefundOrderPaymentAsync(orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Refund] Refund failed for order {OrderId}. It will be retried.", orderId);
            }
        }

        public async Task<int> RetryPendingRefundsAsync()
        {
            var orderIds = await _unitOfWork.Orders.GetQueryable()
                .AsNoTracking()
                .Where(o => o.Status == OrderStatus.Cancelled
                    && o.Payment != null
                    && o.Payment.Method == PaymentMethod.Stripe
                    && o.Payment.Status == PaymentStatus.Success)
                .Select(o => o.Id)
                .ToListAsync();

            foreach (var orderId in orderIds)
                await TryRefundOrderPaymentAsync(orderId);

            return orderIds.Count;
        }

        private async Task HandlePaymentSuccessAsync(Payment payment, string transactionId)
        {
            if (payment.Status == PaymentStatus.Success)
            {
                _logger.LogInformation(
                    "[HandlePaymentSuccess] Payment {PaymentId} already Success. Skipping.",
                    payment.Id);
                return;
            }

            if (payment.Status != PaymentStatus.Pending)
            {
                _logger.LogWarning(
                    "[HandlePaymentSuccess] Payment {PaymentId} is {Status}; refunding unexpected Stripe payment.",
                    payment.Id, payment.Status);

                if (!string.IsNullOrWhiteSpace(transactionId))
                    await _stripe.RefundPaymentAsync(transactionId);

                return;
            }

            var order = await _unitOfWork.Orders.GetByIdAsync(payment.OrderId);
            if (order is null || order.Status == OrderStatus.Cancelled)
            {
                _logger.LogWarning(
                    "[HandlePaymentSuccess] Order for payment {PaymentId} is missing or cancelled; refunding.",
                    payment.Id);

                if (!string.IsNullOrWhiteSpace(transactionId))
                    await _stripe.RefundPaymentAsync(transactionId);

                payment.Status = PaymentStatus.Refunded;
                payment.TransactionId = transactionId;
                payment.RefundedAt = _currentTime.GetCurrentTime();
                await _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync();
                return;
            }

            payment.Status = PaymentStatus.Success;
            payment.TransactionId = transactionId;
            payment.PaidAt = _currentTime.GetCurrentTime();
            await _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "[HandlePaymentSuccess] Payment {PaymentId} confirmed for order {OrderId}.",
                payment.Id, payment.OrderId);
        }

        private async Task AbandonUnpaidPaymentAsync(
            Payment payment,
            PaymentStatus paymentStatus,
            string orderCancelReason)
        {
            if (payment.Status != PaymentStatus.Pending)
                return;

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                // Re-load inside transaction
                var current = await _unitOfWork.Payments.GetByIdAsync(payment.Id);
                if (current is null || current.Status != PaymentStatus.Pending)
                    return true;

                current.Status = paymentStatus;
                await _unitOfWork.Payments.Update(current);

                var order = await _unitOfWork.Orders.GetByIdAsync(current.OrderId);

                if (order is not null && order.Status == OrderStatus.Pending)
                {
                    var now = _currentTime.GetCurrentTime();
                    var fromStatus = order.Status;
                    order.Status = OrderStatus.Cancelled;
                    order.CancelledAt = now;
                    order.CancelReason = orderCancelReason;

                    await RestoreVouchersForOrderAsync(order.Id, now);
                    await _orderStockService.RestoreAsync(order.Id);
                    await _unitOfWork.Orders.Update(order);
                    await _unitOfWork.OrderStatusHistories.AddAsync(new OrderStatusHistory
                    {
                        OrderId = order.Id,
                        FromStatus = fromStatus,
                        ToStatus = OrderStatus.Cancelled,
                        ChangedBy = null,
                        ChangedAt = now
                    });
                }

                await _unitOfWork.SaveChangesAsync();
                return true;
            });

            _logger.LogInformation(
                "[AbandonUnpaidPayment] Payment {PaymentId} marked {Status}.",
                payment.Id, paymentStatus);
        }

        private async Task RestoreVouchersForOrderAsync(Guid orderId, DateTime now)
        {
            var usages = await _unitOfWork.VoucherUsages.GetQueryable()
                .Where(u => u.OrderId == orderId)
                .ToListAsync();

            if (usages.Count == 0)
                return;

            foreach (var usage in usages)
            {
                var voucher = await _unitOfWork.Vouchers.GetByIdAsync(usage.VoucherId);
                if (voucher is not null)
                {
                    if (voucher.UsedCount > 0)
                        voucher.UsedCount -= 1;
                    await _unitOfWork.Vouchers.Update(voucher);
                }

                await _unitOfWork.VoucherUsages.SoftRemove(usage);

                var alreadyInWallet = await _unitOfWork.UserVouchers.GetQueryable()
                    .AnyAsync(uv => uv.UserId == usage.UserId && uv.VoucherId == usage.VoucherId);

                if (!alreadyInWallet)
                {
                    await _unitOfWork.UserVouchers.AddAsync(new UserVoucher
                    {
                        Id = Guid.NewGuid(),
                        UserId = usage.UserId,
                        VoucherId = usage.VoucherId,
                        ClaimedAt = now
                    });
                }
            }
        }

        private static PaymentResponse ToPaymentResponse(Payment p) => new()
        {
            Id = p.Id,
            Code = p.Code,
            UserId = p.UserId,
            OrderId = p.OrderId,
            Amount = p.Amount,
            Currency = p.Currency,
            Method = p.Method,
            Status = p.Status,
            CheckoutSessionId = p.CheckoutSessionId,
            TransactionId = p.TransactionId,
            PaidAt = p.PaidAt,
            RefundedAt = p.RefundedAt,
            CreatedAt = p.CreatedAt
        };
    }
}
