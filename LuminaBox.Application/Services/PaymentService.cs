using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Dtos.Payment;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Notifications;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private static readonly TimeSpan CheckoutWindow = TimeSpan.FromMinutes(10);

        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly ICurrentTime _currentTime;
        private readonly IStripePaymentService _stripePaymentService;
        private readonly INotificationService _notificationService;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IClaimsService claimsService,
            ICurrentTime currentTime,
            IStripePaymentService stripePaymentService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _currentTime = currentTime;
            _stripePaymentService = stripePaymentService;
            _notificationService = notificationService;
        }

        public async Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();
            var now = _currentTime.GetCurrentTime();

            var payment = await _unitOfWork.Payments.GetQueryable()
                .Include(p => p.Order)
                .FirstOrDefaultAsync(p =>
                    (request.PaymentId.HasValue && p.Id == request.PaymentId.Value) ||
                    (request.OrderId.HasValue && p.OrderId == request.OrderId.Value))
                ?? throw ErrorHelper.NotFound("Payment not found.");

            if (payment.UserId != userId)
                throw ErrorHelper.Forbidden("You do not own this payment.");

            if (payment.Method != PaymentMethod.Stripe)
                throw ErrorHelper.BadRequest("CashOnDelivery checkout is not supported yet. Only Stripe payments are supported.");

            if (payment.Status != PaymentStatus.Pending || payment.Order.Status != OrderStatus.Pending)
                throw ErrorHelper.Conflict("Payment or order is no longer pending.");

            var expiresAt = payment.CreatedAt.Add(CheckoutWindow);
            if (now >= expiresAt)
                throw ErrorHelper.Gone("Stripe checkout window (10 minutes) has expired.");

            var successUrl = !string.IsNullOrWhiteSpace(request.SuccessUrl)
                ? request.SuccessUrl
                : $"http://localhost:5173/payments/{payment.Id}/checkout?status=success&session_id={{CHECKOUT_SESSION_ID}}";

            var cancelUrl = !string.IsNullOrWhiteSpace(request.CancelUrl)
                ? request.CancelUrl
                : $"http://localhost:5173/payments/{payment.Id}/checkout?status=cancel&session_id={{CHECKOUT_SESSION_ID}}";

            var (url, sessionId) = await _stripePaymentService.CreateCheckoutSessionAsync(
                payment,
                payment.Order.OrderCode,
                successUrl,
                cancelUrl);

            payment.CheckoutSessionId = sessionId;
            await _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            return new CheckoutSessionResponse
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                OrderCode = payment.Order.OrderCode,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = payment.Status,
                SessionId = sessionId,
                CheckoutUrl = url,
                ExpiresAt = expiresAt
            };
        }

        public async Task<PaymentResponse> GetPaymentByDetail(Guid id)
        {
            var userId = _claimsService.GetRequiredUserId();

            var item = await _unitOfWork.Payments.GetQueryable()
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.UserId,
                    OrganizerUserId = p.Order.Organizer.UserId,
                    Response = new PaymentResponse
                    {
                        Id = p.Id,
                        Code = p.Code,
                        UserId = p.UserId,
                        OrderId = p.OrderId,
                        OrderCode = p.Order.OrderCode,
                        Amount = p.Amount,
                        Currency = p.Currency,
                        Method = p.Method,
                        Status = p.Status,
                        CheckoutSessionId = p.CheckoutSessionId,
                        TransactionId = p.TransactionId,
                        CreatedAt = p.CreatedAt,
                        PaidAt = p.PaidAt,
                        RefundedAt = p.RefundedAt
                    }
                })
                .FirstOrDefaultAsync()
                ?? throw ErrorHelper.NotFound("Payment not found.");

            var isManager = await _unitOfWork.Users.GetQueryable()
                .AsNoTracking()
                .AnyAsync(u => u.Id == userId && u.Role == UserRole.Manager);

            if (item.UserId != userId && item.OrganizerUserId != userId && !isManager)
                throw ErrorHelper.Forbidden("You do not have permission to view this payment.");

            return item.Response;
        }

        public async Task<PaymentResponse> SyncPaymentAsync(Guid id)
        {
            var userId = _claimsService.GetRequiredUserId();
            var now = _currentTime.GetCurrentTime();

            var payment = await _unitOfWork.Payments.GetQueryable()
                .Include(p => p.Order)
                    .ThenInclude(o => o.Organizer)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw ErrorHelper.NotFound("Payment not found.");

            if (payment.UserId != userId && payment.Order.Organizer.UserId != userId)
                throw ErrorHelper.Forbidden("You do not have permission to sync this payment.");

            if (payment.Method != PaymentMethod.Stripe)
                throw ErrorHelper.BadRequest("Only Stripe payments can be synced.");

            if (payment.Status == PaymentStatus.Success)
                return await GetPaymentByDetail(payment.Id);

            if (string.IsNullOrWhiteSpace(payment.CheckoutSessionId))
                throw ErrorHelper.BadRequest("Payment does not have a Stripe checkout session yet.");

            var (paymentStatus, paymentIntentId) = await _stripePaymentService.GetCheckoutSessionAsync(payment.CheckoutSessionId);

            if (string.Equals(paymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
            {
                payment.Status = PaymentStatus.Success;
                payment.TransactionId = paymentIntentId ?? payment.TransactionId;
                payment.PaidAt = now;
                await _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync();
            }

            return await GetPaymentByDetail(payment.Id);
        }

        public async Task<PaymentResponse> CancelPaymentAsync(Guid id)
        {
            var userId = _claimsService.GetRequiredUserId();
            var now = _currentTime.GetCurrentTime();

            var payment = await _unitOfWork.Payments.GetQueryable()
                .Include(p => p.Order)
                    .ThenInclude(o => o.Organizer)
                .Include(p => p.Order)
                    .ThenInclude(o => o.VoucherUsages)
                        .ThenInclude(vu => vu.Voucher)
                .FirstOrDefaultAsync(p => p.Id == id)
                ?? throw ErrorHelper.NotFound("Payment not found.");

            if (payment.UserId != userId)
                throw ErrorHelper.Forbidden("You do not own this payment.");

            if (payment.Status != PaymentStatus.Pending)
                throw ErrorHelper.Conflict("Only pending payments can be cancelled.");

            await CancelPendingPaymentAndOrderAsync(payment, userId, "Payment cancelled by buyer", now);

            return await GetPaymentByDetail(payment.Id);
        }

        public async Task<PaymentResponse> HandleStripeReturnAsync(string? sessionId, Guid? paymentId)
        {
            var now = _currentTime.GetCurrentTime();

            var payment = await FindPaymentBySessionOrIdAsync(sessionId, paymentId)
                ?? throw ErrorHelper.NotFound("Payment not found.");

            if (payment.Status == PaymentStatus.Pending && !string.IsNullOrWhiteSpace(payment.CheckoutSessionId))
            {
                var (paymentStatus, paymentIntentId) = await _stripePaymentService.GetCheckoutSessionAsync(payment.CheckoutSessionId);
                if (string.Equals(paymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
                {
                    payment.Status = PaymentStatus.Success;
                    payment.TransactionId = paymentIntentId ?? payment.TransactionId;
                    payment.PaidAt = now;
                    await _unitOfWork.Payments.Update(payment);
                    await _unitOfWork.SaveChangesAsync();
                }
            }

            return await ProjectPaymentByIdAsync(payment.Id);
        }

        public async Task<PaymentResponse> HandleStripeCancelAsync(string? sessionId, Guid? paymentId)
        {
            var now = _currentTime.GetCurrentTime();

            var payment = await FindPaymentBySessionOrIdAsync(sessionId, paymentId)
                ?? throw ErrorHelper.NotFound("Payment not found.");

            if (payment.Status == PaymentStatus.Pending)
            {
                await CancelPendingPaymentAndOrderAsync(payment, payment.UserId, "Stripe checkout cancelled by buyer", now);
            }

            return await ProjectPaymentByIdAsync(payment.Id);
        }

        public async Task HandleStripeWebhookAsync(string payload, string stripeSignature)
        {
            var now = _currentTime.GetCurrentTime();
            var (eventType, sessionId, transactionId) = await _stripePaymentService.ParseWebhookEventAsync(payload, stripeSignature);

            if (string.IsNullOrWhiteSpace(sessionId))
                return;

            var payment = await FindPaymentBySessionOrIdAsync(sessionId, null);
            if (payment is null)
                return;

            if (string.Equals(eventType, "checkout.session.completed", StringComparison.OrdinalIgnoreCase) &&
                payment.Status == PaymentStatus.Pending)
            {
                payment.Status = PaymentStatus.Success;
                payment.TransactionId = transactionId ?? payment.TransactionId;
                payment.PaidAt = now;
                await _unitOfWork.Payments.Update(payment);
                await _unitOfWork.SaveChangesAsync();
            }
            else if (string.Equals(eventType, "checkout.session.expired", StringComparison.OrdinalIgnoreCase) &&
                     payment.Status == PaymentStatus.Pending)
            {
                await CancelPendingPaymentAndOrderAsync(
                    payment,
                    null,
                    "Stripe checkout session expired",
                    now,
                    markFailed: true);
            }
        }

        private async Task<Payment?> FindPaymentBySessionOrIdAsync(string? sessionId, Guid? paymentId)
        {
            var query = _unitOfWork.Payments.GetQueryable()
                .Include(p => p.Order)
                    .ThenInclude(o => o.Organizer)
                .Include(p => p.Order)
                    .ThenInclude(o => o.VoucherUsages)
                        .ThenInclude(vu => vu.Voucher);

            if (paymentId.HasValue && paymentId.Value != Guid.Empty)
            {
                return await query.FirstOrDefaultAsync(p => p.Id == paymentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                return await query.FirstOrDefaultAsync(p => p.CheckoutSessionId == sessionId);
            }

            return null;
        }

        private async Task CancelPendingPaymentAndOrderAsync(
            Payment payment,
            Guid? actorUserId,
            string reason,
            DateTime now,
            bool markFailed = false)
        {
            var orderCancelled = false;

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                payment.Status = markFailed ? PaymentStatus.Failed : PaymentStatus.Cancelled;
                await _unitOfWork.Payments.Update(payment);

                var order = payment.Order;
                if (order.Status == OrderStatus.Pending)
                {
                    var previousStatus = order.Status;
                    order.Status = OrderStatus.Cancelled;
                    order.CancelledAt = now;
                    order.CancelReason = reason;
                    await _unitOfWork.Orders.Update(order);

                    foreach (var usage in order.VoucherUsages.ToList())
                    {
                        var voucher = usage.Voucher;
                        if (voucher.UsedCount > 0)
                        {
                            voucher.UsedCount -= 1;
                            await _unitOfWork.Vouchers.Update(voucher);
                        }

                        var alreadyClaimed = await _unitOfWork.UserVouchers.GetQueryable()
                            .AnyAsync(uv => uv.UserId == order.UserId && uv.VoucherId == usage.VoucherId);

                        if (!alreadyClaimed)
                        {
                            await _unitOfWork.UserVouchers.AddAsync(new UserVoucher
                            {
                                UserId = order.UserId,
                                VoucherId = usage.VoucherId,
                                ClaimedAt = now
                            });
                        }

                        await _unitOfWork.VoucherUsages.SoftRemove(usage);
                    }

                    await _unitOfWork.OrderStatusHistories.AddAsync(new OrderStatusHistory
                    {
                        OrderId = order.Id,
                        FromStatus = previousStatus,
                        ToStatus = OrderStatus.Cancelled,
                        ChangedBy = actorUserId,
                        ChangedAt = now
                    });

                    orderCancelled = true;
                }

                await _unitOfWork.SaveChangesAsync();
                return true;
            });

            if (orderCancelled)
            {
                await _notificationService.SendAsync(
                    NotificationCatalog.OrderCancelled(
                        payment.Order.UserId,
                        payment.Order.Organizer.UserId,
                        payment.Order.Id,
                        payment.Order.OrderCode,
                        payment.Order.Organizer.BrandName,
                        cancelledByOrganizer: false,
                        reason: reason));
            }
        }

        private async Task<PaymentResponse> ProjectPaymentByIdAsync(Guid id)
        {
            return await _unitOfWork.Payments.GetQueryable()
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new PaymentResponse
                {
                    Id = p.Id,
                    Code = p.Code,
                    UserId = p.UserId,
                    OrderId = p.OrderId,
                    OrderCode = p.Order.OrderCode,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Method = p.Method,
                    Status = p.Status,
                    CheckoutSessionId = p.CheckoutSessionId,
                    TransactionId = p.TransactionId,
                    CreatedAt = p.CreatedAt,
                    PaidAt = p.PaidAt,
                    RefundedAt = p.RefundedAt
                })
                .FirstOrDefaultAsync()
                ?? throw ErrorHelper.NotFound("Payment not found.");
        }
    }
}

