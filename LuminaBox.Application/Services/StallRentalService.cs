using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.StallRental;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Notifications;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuminaBox.Application.Services
{
    public class StallRentalService : IStallRentalService
    {
        private const int VietnamUtcOffsetHours = 7;
        private const int MaxBatchSize = 100;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly ICurrentTime _currentTime;
        private readonly INotificationService _notificationService;

        public StallRentalService(
            IUnitOfWork unitOfWork,
            IClaimsService claimsService,
            ICurrentTime currentTime,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _currentTime = currentTime;
            _notificationService = notificationService;
        }

        public Task<Pagination<StallRentalResponse>> GetAllAsync(
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            RentalStatus? status = null,
            Guid? stallId = null,
            Guid? organizerId = null)
        {
            var query = _unitOfWork.StallRentals.GetQueryable().AsNoTracking();

            if (stallId.HasValue)
                query = query.Where(r => r.StallId == stallId.Value);

            if (organizerId.HasValue)
                query = query.Where(r => r.OrganizerId == organizerId.Value);

            return GetPageAsync(query, search, sortBy, isDescending, page, pageSize, status);
        }

        public Task<Pagination<StallRentalResponse>> GetMineAsync(
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            RentalStatus? status = null)
        {
            var userId = _claimsService.GetRequiredUserId();
            var query = _unitOfWork.StallRentals.GetQueryable()
                .AsNoTracking()
                .Where(r => r.Organizer.UserId == userId);

            return GetPageAsync(query, search, sortBy, isDescending, page, pageSize, status);
        }

        public async Task<Pagination<StallRentalResponse>> GetByStallAsync(
            Guid stallId,
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            RentalStatus? status = null)
        {
            if (!await _unitOfWork.Stalls.GetQueryable().AnyAsync(s => s.Id == stallId))
                throw ErrorHelper.NotFound("Stall not found.", "E4.4");

            var query = _unitOfWork.StallRentals.GetQueryable()
                .AsNoTracking()
                .Where(r => r.StallId == stallId);

            return await GetPageAsync(query, search, sortBy, isDescending, page, pageSize, status);
        }

        public async Task<StallRentalResponse> GetByIdAsync(Guid id)
        {
            return await ProjectToResponse(_unitOfWork.StallRentals.GetQueryable()
                    .AsNoTracking()
                    .Where(r => r.Id == id))
                .FirstOrDefaultAsync()
                ?? throw ErrorHelper.NotFound("Stall rental not found.");
        }

        public async Task<StallRentalResponse> TerminateAsync(Guid id)
        {
            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var rental = await _unitOfWork.StallRentals.GetByIdAsync(
                    id, r => r.Stall, r => r.Organizer)
                    ?? throw ErrorHelper.NotFound("Stall rental not found.");

                if (rental.Status != RentalStatus.Active)
                    throw ErrorHelper.Conflict("Only active stall rentals can be terminated.");

                rental.Status = RentalStatus.Terminated;
                await _unitOfWork.StallRentals.Update(rental);
                await _unitOfWork.SaveChangesAsync();

                await ReleaseStallInventoryAsync(rental);
                await RefreshStallStatusAsync(rental.StallId);

                await _notificationService.SendAsync(NotificationCatalog.StallRentalTerminated(
                    rental.Organizer.UserId, rental.Id, rental.Organizer.BrandName, rental.Stall.Code));
                await _unitOfWork.SaveChangesAsync();
                return true;
            });

            return await GetByIdAsync(id);
        }

        public async Task<int> SyncStatusesAsync()
        {
            var today = GetVietnamToday();
            var rentalIds = await _unitOfWork.StallRentals.GetQueryable()
                .AsNoTracking()
                .Where(r =>
                    (r.Status == RentalStatus.Confirmed && r.StartDate <= today)
                    || (r.Status == RentalStatus.Active && r.EndDate.HasValue && r.EndDate.Value < today))
                .OrderBy(r => r.StartDate)
                .Take(MaxBatchSize)
                .Select(r => r.Id)
                .ToListAsync();

            var changed = 0;
            foreach (var id in rentalIds)
            {
                var updated = await _unitOfWork.ExecuteTransactionAsync(async () =>
                {
                    var rental = await _unitOfWork.StallRentals.GetByIdAsync(id, r => r.Stall, r => r.Organizer);
                    if (rental is null)
                        return false;

                    var expired = rental.EndDate.HasValue && rental.EndDate.Value < today;
                    if (rental.Status == RentalStatus.Confirmed && rental.StartDate <= today && !expired)
                    {
                        rental.Status = RentalStatus.Active;
                        await _unitOfWork.StallRentals.Update(rental);
                        await _unitOfWork.SaveChangesAsync();
                        await RefreshStallStatusAsync(rental.StallId);
                        await _notificationService.SendAsync(NotificationCatalog.StallRentalActivated(
                            rental.Organizer.UserId, rental.Id, rental.Organizer.BrandName, rental.Stall.Code));
                        await _unitOfWork.SaveChangesAsync();
                        return true;
                    }

                    if ((rental.Status == RentalStatus.Active || rental.Status == RentalStatus.Confirmed) && expired)
                    {
                        rental.Status = RentalStatus.Expired;
                        await _unitOfWork.StallRentals.Update(rental);
                        await _unitOfWork.SaveChangesAsync();
                        await ReleaseStallInventoryAsync(rental);
                        await RefreshStallStatusAsync(rental.StallId);
                        await _notificationService.SendAsync(NotificationCatalog.StallRentalExpired(
                            rental.Organizer.UserId, rental.Id, rental.Organizer.BrandName, rental.Stall.Code));
                        await _unitOfWork.SaveChangesAsync();
                        return true;
                    }

                    return false;
                });

                if (updated)
                    changed++;
            }

            var overdueIds = await _unitOfWork.OrganizerRegistrations.GetQueryable()
                .AsNoTracking()
                .Where(r => r.Type == RegistrationType.Stall
                    && r.Status == RegistrationStatus.Pending
                    && r.StartDate.HasValue
                    && r.StartDate.Value < today)
                .OrderBy(r => r.CreatedAt)
                .Take(MaxBatchSize)
                .Select(r => r.Id)
                .ToListAsync();

            foreach (var id in overdueIds)
            {
                var updated = await _unitOfWork.ExecuteTransactionAsync(async () =>
                {
                    var registration = await _unitOfWork.OrganizerRegistrations.GetByIdAsync(
                        id, r => r.Stall!, r => r.User);

                    if (registration is null
                        || registration.Type != RegistrationType.Stall
                        || registration.Status != RegistrationStatus.Pending
                        || registration.StartDate >= today)
                        return false;

                    registration.Status = RegistrationStatus.Cancelled;
                    registration.CancelledAt = _currentTime.GetCurrentTime();
                    registration.CancelReason = "Requested stall start date has passed before review.";
                    await _unitOfWork.OrganizerRegistrations.Update(registration);

                    var recipientRole = registration.User.Role == UserRole.Organization
                        ? NotificationRecipientRole.Organizer
                        : NotificationRecipientRole.User;

                    await _notificationService.SendAsync(NotificationCatalog.StallRegistrationOverdueCancelled(
                        registration.UserId,
                        recipientRole,
                        registration.Id,
                        registration.BrandName,
                        registration.Stall!.Code));
                    await _unitOfWork.SaveChangesAsync();
                    return true;
                });

                if (updated)
                    changed++;
            }

            return changed;
        }

        private async Task ReleaseStallInventoryAsync(StallRental rental)
        {
            var productStocks = await _unitOfWork.StallProductItems.GetQueryable()
                .Where(s => s.StallId == rental.StallId
                    && s.ProductItem.Product.OrganizerId == rental.OrganizerId
                    && s.IsActive)
                .ToListAsync();

            var boxStocks = await _unitOfWork.StallBoxItems.GetQueryable()
                .Where(s => s.StallId == rental.StallId
                    && s.BoxItem.OrganizerId == rental.OrganizerId
                    && s.IsActive)
                .ToListAsync();

            foreach (var stock in productStocks)
                stock.IsActive = false;
            foreach (var stock in boxStocks)
                stock.IsActive = false;

            if (productStocks.Count > 0)
                await _unitOfWork.StallProductItems.UpdateRange(productStocks);
            if (boxStocks.Count > 0)
                await _unitOfWork.StallBoxItems.UpdateRange(boxStocks);

            if (productStocks.Count == 0 && boxStocks.Count == 0)
                return;

            await _unitOfWork.SaveChangesAsync();

            var productItemIds = productStocks.Select(s => s.ProductItemId).ToList();
            var productIds = await _unitOfWork.ProductItems.GetQueryable()
                .Where(i => productItemIds.Contains(i.Id))
                .Select(i => i.ProductId)
                .Distinct()
                .ToListAsync();

            var productsToHide = await _unitOfWork.Products.GetQueryable()
                .Where(p => productIds.Contains(p.Id)
                    && p.Status == ProductStatus.Active
                    && !p.Items.Any(i => i.Stocks.Any(s => s.IsActive)))
                .ToListAsync();

            var boxIds = boxStocks.Select(s => s.BoxItemId).ToList();
            var boxesToHide = await _unitOfWork.BoxItems.GetQueryable()
                .Where(b => boxIds.Contains(b.Id)
                    && b.Status == ProductStatus.Active
                    && !b.Stocks.Any(s => s.IsActive))
                .ToListAsync();

            foreach (var product in productsToHide)
                product.Status = ProductStatus.Inactive;
            foreach (var box in boxesToHide)
                box.Status = ProductStatus.Inactive;

            if (productsToHide.Count > 0)
                await _unitOfWork.Products.UpdateRange(productsToHide);
            if (boxesToHide.Count > 0)
                await _unitOfWork.BoxItems.UpdateRange(boxesToHide);
        }

        private async Task RefreshStallStatusAsync(Guid stallId)
        {
            var stall = await _unitOfWork.Stalls.GetByIdAsync(stallId);
            if (stall is null)
                return;

            var hasValidRental = await _unitOfWork.StallRentals.GetQueryable()
                .AnyAsync(r => r.StallId == stallId
                    && (r.Status == RentalStatus.Confirmed || r.Status == RentalStatus.Active));

            var newStatus = hasValidRental ? StallStatus.Rented : StallStatus.Available;
            if (stall.Status != newStatus)
            {
                stall.Status = newStatus;
                await _unitOfWork.Stalls.Update(stall);
            }
        }

        private DateOnly GetVietnamToday()
            => DateOnly.FromDateTime(_currentTime.GetCurrentTime().AddHours(VietnamUtcOffsetHours));

        private static async Task<Pagination<StallRentalResponse>> GetPageAsync(
            IQueryable<StallRental> query,
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            RentalStatus? status)
        {
            var pagination = new PaginationParameter { PageIndex = page, PageSize = pageSize };

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(r => r.Stall.Code.ToLower().Contains(keyword)
                    || r.Stall.Name.ToLower().Contains(keyword)
                    || r.Organizer.BrandName.ToLower().Contains(keyword)
                    || r.Organizer.User.Email.ToLower().Contains(keyword));
            }

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            var totalCount = await query.CountAsync();

            query = sortBy?.Trim().ToLowerInvariant() switch
            {
                "startdate" => isDescending ? query.OrderByDescending(r => r.StartDate) : query.OrderBy(r => r.StartDate),
                "enddate" => isDescending ? query.OrderByDescending(r => r.EndDate) : query.OrderBy(r => r.EndDate),
                "status" => isDescending ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status),
                "stallcode" => isDescending ? query.OrderByDescending(r => r.Stall.Code) : query.OrderBy(r => r.Stall.Code),
                "brandname" => isDescending ? query.OrderByDescending(r => r.Organizer.BrandName) : query.OrderBy(r => r.Organizer.BrandName),
                _ => isDescending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt)
            };

            var items = await ProjectToResponse(query
                    .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                    .Take(pagination.PageSize))
                .ToListAsync();

            return new Pagination<StallRentalResponse>(items, totalCount, pagination.PageIndex, pagination.PageSize);
        }

        private static IQueryable<StallRentalResponse> ProjectToResponse(IQueryable<StallRental> query)
        {
            return query.Select(r => new StallRentalResponse
            {
                Id = r.Id,
                StallId = r.StallId,
                StallCode = r.Stall.Code,
                StallName = r.Stall.Name,
                OrganizerId = r.OrganizerId,
                OrganizerUserId = r.Organizer.UserId,
                OrganizerBrandName = r.Organizer.BrandName,
                OrganizerEmail = r.Organizer.User.Email,
                RegistrationId = r.RegistrationId,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Status = r.Status,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            });
        }
    }
}
