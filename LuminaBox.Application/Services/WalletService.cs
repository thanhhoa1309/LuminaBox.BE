using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentTime _currentTime;

        public WalletService(IUnitOfWork unitOfWork, ICurrentTime currentTime)
        {
            _unitOfWork = unitOfWork;
            _currentTime = currentTime;
        }

        public async Task SettleOrderEarningAsync(Guid orderId)
        {
            var existingCommission = await _unitOfWork.Commissions.FirstOrDefaultAsync(c => c.OrderId == orderId);
            if (existingCommission is not null && existingCommission.Status == CommissionStatus.Settled)
                return;

            var order = await _unitOfWork.Orders.GetByIdAsync(orderId)
                ?? throw ErrorHelper.NotFound("Order not found.");

            var payment = await _unitOfWork.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId)
                ?? throw ErrorHelper.Conflict("Order has no payment record.");

            if (payment.Method != PaymentMethod.Stripe)
                return;

            if (payment.Status != PaymentStatus.Success)
                throw ErrorHelper.Conflict("Only successfully paid Stripe orders can settle to wallet.");

            var organizer = await _unitOfWork.Organizers.GetByIdAsync(order.OrganizerId)
                ?? throw ErrorHelper.NotFound("Organizer not found.");

            var rate = await GetApprovedOnlineCommissionRateAsync(order.OrganizerId);
            var orderAmount = order.SubtotalAmount - order.OrganizerDiscount;
            if (orderAmount < 0)
                orderAmount = 0;

            var commissionAmount = Math.Round(orderAmount * rate / 100m, 2, MidpointRounding.AwayFromZero);
            if (commissionAmount > orderAmount)
                commissionAmount = orderAmount;

            var organizerCredit = orderAmount - commissionAmount;
            var now = _currentTime.GetCurrentTime();

            Commission commission;
            if (existingCommission is null)
            {
                commission = new Commission
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    OrganizerId = order.OrganizerId,
                    OrderAmount = orderAmount,
                    Rate = rate,
                    CommissionAmount = commissionAmount,
                    Status = CommissionStatus.Settled,
                    SettledAt = now
                };
                await _unitOfWork.Commissions.AddAsync(commission);
            }
            else
            {
                commission = existingCommission;
                commission.OrderAmount = orderAmount;
                commission.Rate = rate;
                commission.CommissionAmount = commissionAmount;
                commission.Status = CommissionStatus.Settled;
                commission.SettledAt = now;
                await _unitOfWork.Commissions.Update(commission);
            }

            var alreadyEarned = await _unitOfWork.WalletTransactions.GetQueryable()
                .AnyAsync(t =>
                    t.OrderId == order.Id &&
                    t.Type == WalletTransactionType.OrderEarning);

            if (alreadyEarned)
            {
                await _unitOfWork.SaveChangesAsync();
                return;
            }

            var wallet = await GetOrCreateWalletAsync(organizer.UserId);
            wallet.AvailableBalance += organizerCredit;

            await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
            {
                WalletId = wallet.Id,
                Type = WalletTransactionType.OrderEarning,
                Amount = organizerCredit,
                BalanceAfter = wallet.AvailableBalance,
                OrderId = order.Id,
                CommissionId = commission.Id,
                Note = $"Order earning for {order.OrderCode}"
            });

            await _unitOfWork.Wallets.Update(wallet);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task ReverseOrderEarningAsync(Guid orderId)
        {
            var commission = await _unitOfWork.Commissions.FirstOrDefaultAsync(c => c.OrderId == orderId);
            if (commission is null || commission.Status != CommissionStatus.Settled)
                return;

            var alreadyReversed = await _unitOfWork.WalletTransactions.GetQueryable()
                .AnyAsync(t =>
                    t.OrderId == orderId &&
                    t.Type == WalletTransactionType.OrderReversal);

            if (alreadyReversed)
                return;

            var earning = await _unitOfWork.WalletTransactions.GetQueryable()
                .FirstOrDefaultAsync(t =>
                    t.OrderId == orderId &&
                    t.Type == WalletTransactionType.OrderEarning);

            if (earning is null)
                return;

            var wallet = await _unitOfWork.Wallets.GetByIdAsync(earning.WalletId)
                ?? throw ErrorHelper.NotFound("Wallet not found.");

            var reverseAmount = earning.Amount;
            if (wallet.AvailableBalance < reverseAmount)
            {
                throw ErrorHelper.Conflict(
                    "Wallet balance is insufficient to reverse this order earning.");
            }

            wallet.AvailableBalance -= reverseAmount;

            var order = await _unitOfWork.Orders.GetByIdAsync(orderId);
            var orderCode = order?.OrderCode ?? orderId.ToString();

            await _unitOfWork.WalletTransactions.AddAsync(new WalletTransaction
            {
                WalletId = wallet.Id,
                Type = WalletTransactionType.OrderReversal,
                Amount = -reverseAmount,
                BalanceAfter = wallet.AvailableBalance,
                OrderId = orderId,
                CommissionId = commission.Id,
                Note = $"Order reversal for {orderCode}"
            });

            await _unitOfWork.Wallets.Update(wallet);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<decimal> GetApprovedOnlineCommissionRateAsync(Guid organizerId)
        {
            var rate = await _unitOfWork.OrganizerRegistrations.GetQueryable()
                .AsNoTracking()
                .Where(r =>
                    r.OrganizerId == organizerId &&
                    r.Type == RegistrationType.Online &&
                    r.Status == RegistrationStatus.Approved &&
                    r.CommissionRate != null)
                .OrderByDescending(r => r.ReviewedAt ?? r.CreatedAt)
                .Select(r => r.CommissionRate)
                .FirstOrDefaultAsync();

            if (!rate.HasValue)
            {
                throw ErrorHelper.Conflict(
                    "No approved online commission rate found for this organizer. Cannot settle wallet.");
            }

            return rate.Value;
        }

        private async Task<Wallet> GetOrCreateWalletAsync(Guid userId)
        {
            var wallet = await _unitOfWork.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet is not null)
                return wallet;

            wallet = new Wallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                AvailableBalance = 0,
                Currency = "VND"
            };
            await _unitOfWork.Wallets.AddAsync(wallet);
            await _unitOfWork.SaveChangesAsync();
            return wallet;
        }
    }
}
