using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Notifications;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Jobs.Payment
{
    public class UnpaidPaymentExpireJob
    {
        private static readonly TimeSpan ExpiryWindow = TimeSpan.FromMinutes(10);

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentTime _currentTime;
        private readonly INotificationService _notificationService;

        public UnpaidPaymentExpireJob(
            IUnitOfWork unitOfWork,
            ICurrentTime currentTime,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _currentTime = currentTime;
            _notificationService = notificationService;
        }

        public async Task<int> ExecuteAsync()
        {
            var now = _currentTime.GetCurrentTime();
            var cutoff = now.Subtract(ExpiryWindow);

            var expiredPayments = await _unitOfWork.Payments.GetQueryable()
                .Include(p => p.Order)
                    .ThenInclude(o => o.Organizer)
                .Include(p => p.Order)
                    .ThenInclude(o => o.VoucherUsages)
                        .ThenInclude(vu => vu.Voucher)
                .Where(p => p.Method == PaymentMethod.Stripe
                         && p.Status == PaymentStatus.Pending
                         && p.CreatedAt <= cutoff)
                .ToListAsync();

            if (expiredPayments.Count == 0)
                return 0;

            var cancelledOrders = new List<(Guid BuyerId, Guid OrganizerUserId, Guid OrderId, string OrderCode, string BrandName)>();

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                foreach (var payment in expiredPayments)
                {
                    payment.Status = PaymentStatus.Cancelled;
                    await _unitOfWork.Payments.Update(payment);

                    var order = payment.Order;
                    if (order.Status == OrderStatus.Pending)
                    {
                        var previousStatus = order.Status;
                        order.Status = OrderStatus.Cancelled;
                        order.CancelledAt = now;
                        order.CancelReason = "Stripe payment expired after 10 minutes.";
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
                            ChangedBy = null,
                            ChangedAt = now
                        });

                        cancelledOrders.Add((
                            order.UserId,
                            order.Organizer.UserId,
                            order.Id,
                            order.OrderCode,
                            order.Organizer.BrandName));
                    }
                }

                await _unitOfWork.SaveChangesAsync();
                return true;
            });

            foreach (var item in cancelledOrders)
            {
                await _notificationService.SendAsync(
                    NotificationCatalog.OrderCancelled(
                        item.BuyerId,
                        item.OrganizerUserId,
                        item.OrderId,
                        item.OrderCode,
                        item.BrandName,
                        cancelledByOrganizer: true,
                        reason: "Stripe payment expired after 10 minutes."));
            }

            return expiredPayments.Count;
        }
    }
}

