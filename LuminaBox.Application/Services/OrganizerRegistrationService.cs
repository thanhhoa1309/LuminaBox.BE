using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.OrganizerRegistration;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Notifications;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class OrganizerRegistrationService : IOrganizerRegistrationService
    {
        // Vietnam has no daylight saving, so a fixed offset avoids OS-specific time zone ids
        private const int VietnamUtcOffsetHours = 7;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly ICurrentTime _currentTime;
        private readonly INotificationService _notificationService;

        public OrganizerRegistrationService(
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

        public Task<Pagination<OrganizerRegistrationResponse>> GetAllAsync(
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            RegistrationType? type = null,
            RegistrationStatus? status = null,
            Guid? stallId = null)
        {
            var query = _unitOfWork.OrganizerRegistrations.GetQueryable().AsNoTracking();

            if (stallId.HasValue)
                query = query.Where(r => r.StallId == stallId.Value);

            return GetPageAsync(query, search, sortBy, isDescending, page, pageSize, type, status);
        }

        public Task<Pagination<OrganizerRegistrationResponse>> GetMyRegistrationsAsync(
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            RegistrationType? type = null,
            RegistrationStatus? status = null)
        {
            var userId = _claimsService.GetRequiredUserId();

            var query = _unitOfWork.OrganizerRegistrations.GetQueryable()
                .AsNoTracking()
                .Where(r => r.UserId == userId);

            return GetPageAsync(query, search, sortBy, isDescending, page, pageSize, type, status);
        }

        public async Task<OrganizerRegistrationResponse> GetOrganizerRegistrationByDetail(Guid id)
        {
            return await ProjectToResponse(_unitOfWork.OrganizerRegistrations.GetQueryable()
                    .AsNoTracking()
                    .Where(r => r.Id == id))
                .FirstOrDefaultAsync()
                ?? throw ErrorHelper.NotFound("Organizer registration not found.");
        }

        public async Task<OrganizerRegistrationResponse> CreateOrganizerRegistrationAsync(CreateOrganizerRegistrationRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();

            // Checked against the database because the token keeps the old role until it is refreshed
            if (await _unitOfWork.Organizers.GetQueryable().AnyAsync(o => o.UserId == userId))
                throw ErrorHelper.Conflict("You are already an organizer. Use the additional registration endpoints instead.");

            var registration = new OrganizerRegistration
            {
                UserId = userId,
                Type = request.Type!.Value,
                BrandName = request.BrandName.Trim(),
                ContactPhone = request.ContactPhone.Trim(),
                BusinessInfo = string.IsNullOrWhiteSpace(request.BusinessInfo) ? null : request.BusinessInfo.Trim()
            };

            if (registration.Type == RegistrationType.Stall)
            {
                var startDate = request.StartDate!.Value;
                var endDate = request.EndDate!.Value;

                EnsureStartDateNotPast(startDate);

                var stall = await _unitOfWork.Stalls.GetByIdAsync(request.StallId!.Value)
                    ?? throw ErrorHelper.NotFound("Stall not found.");

                await EnsureStallCanBeRequestedAsync(userId, stall, startDate, endDate);

                registration.StallId = stall.Id;
                registration.StartDate = startDate;
                registration.EndDate = endDate;
            }
            else
            {
                await EnsureNoOpenOnlineRegistrationAsync(userId);
            }

            await _unitOfWork.OrganizerRegistrations.AddAsync(registration);
            await _notificationService.SendAsync(NotificationCatalog.RegistrationCreated(registration.Id, registration.BrandName));
            await _unitOfWork.SaveChangesAsync();

            return await GetOrganizerRegistrationByDetail(registration.Id);
        }

        public async Task<List<OrganizerRegistrationResponse>> CreateAdditionalStallsAsync(CreateAdditionalStallsRequest request)
        {
            var organizer = await GetCurrentActiveOrganizerAsync();
            var startDate = request.StartDate!.Value;
            var endDate = request.EndDate!.Value;

            EnsureStartDateNotPast(startDate);

            var stalls = await _unitOfWork.Stalls.GetAllAsync(s => request.StallIds.Contains(s.Id));
            if (stalls.Count != request.StallIds.Count)
                throw ErrorHelper.NotFound("One or more stalls were not found.");

            var registrations = new List<OrganizerRegistration>();

            foreach (var stall in stalls)
            {
                await EnsureStallCanBeRequestedAsync(organizer.UserId, stall, startDate, endDate);

                registrations.Add(new OrganizerRegistration
                {
                    UserId = organizer.UserId,
                    Type = RegistrationType.Stall,
                    StallId = stall.Id,
                    BrandName = organizer.BrandName,
                    ContactPhone = organizer.ContactPhone,
                    BusinessInfo = organizer.BusinessInfo,
                    StartDate = startDate,
                    EndDate = endDate
                });
            }

            await _unitOfWork.OrganizerRegistrations.AddRangeAsync(registrations);

            foreach (var registration in registrations)
            {
                var stallCode = stalls.First(s => s.Id == registration.StallId).Code;
                await _notificationService.SendAsync(
                    NotificationCatalog.AdditionalStallRequested(registration.Id, registration.BrandName, stallCode));
            }

            await _unitOfWork.SaveChangesAsync();

            var ids = registrations.Select(r => r.Id).ToList();

            return await ProjectToResponse(_unitOfWork.OrganizerRegistrations.GetQueryable()
                    .AsNoTracking()
                    .Where(r => ids.Contains(r.Id))
                    .OrderBy(r => r.Stall!.Code))
                .ToListAsync();
        }

        public async Task<OrganizerRegistrationResponse> CreateAdditionalOnlineAsync()
        {
            var organizer = await GetCurrentActiveOrganizerAsync();

            await EnsureNoOpenOnlineRegistrationAsync(organizer.UserId);

            var registration = new OrganizerRegistration
            {
                UserId = organizer.UserId,
                Type = RegistrationType.Online,
                BrandName = organizer.BrandName,
                ContactPhone = organizer.ContactPhone,
                BusinessInfo = organizer.BusinessInfo
            };

            await _unitOfWork.OrganizerRegistrations.AddAsync(registration);
            await _notificationService.SendAsync(NotificationCatalog.AdditionalOnlineRequested(registration.Id, registration.BrandName));
            await _unitOfWork.SaveChangesAsync();

            return await GetOrganizerRegistrationByDetail(registration.Id);
        }

        public async Task<OrganizerRegistrationResponse> ApproveOrganizerRegistrationAsync(Guid id, ApproveOrganizerRegistrationRequest? request)
        {
            var managerId = _claimsService.GetRequiredUserId();

            // Everything is loaded inside the transaction because a retry re-runs the whole operation
            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var registration = await GetRegistrationForUpdateAsync(id);

                if (registration.Status != RegistrationStatus.Pending)
                    throw ErrorHelper.Conflict("Only pending registrations can be approved.");

                if (registration.Type == RegistrationType.Stall)
                    await ApproveStallRegistrationAsync(registration, managerId, request?.CommissionRate);
                else
                    await ApproveOnlineRegistrationAsync(registration, managerId, request?.CommissionRate);

                await _unitOfWork.OrganizerRegistrations.Update(registration);
                await _unitOfWork.SaveChangesAsync();
                return true;
            });

            return await GetOrganizerRegistrationByDetail(id);
        }

        public async Task<OrganizerRegistrationResponse> RejectOrganizerRegistrationAsync(Guid id, RejectOrganizerRegistrationRequest request)
        {
            var managerId = _claimsService.GetRequiredUserId();

            var registration = await GetRegistrationForUpdateAsync(id);

            if (registration.Status != RegistrationStatus.Pending)
                throw ErrorHelper.Conflict("Only pending registrations can be rejected.");

            MarkRejected(registration, managerId, request.RejectReason.Trim());

            await _unitOfWork.OrganizerRegistrations.Update(registration);
            await _notificationService.SendAsync(NotificationCatalog.OrganizerRejected(
                registration.UserId, registration.Id, registration.BrandName, registration.RejectReason!));
            await _unitOfWork.SaveChangesAsync();

            return await GetOrganizerRegistrationByDetail(id);
        }

        public async Task<OrganizerRegistrationResponse> ConfirmOrganizerRegistrationAsync(Guid id)
        {
            var userId = _claimsService.GetRequiredUserId();

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var registration = await GetOwnedRegistrationForUpdateAsync(id, userId);

                if (registration.Type != RegistrationType.Online || registration.Status != RegistrationStatus.AwaitingConfirmation)
                    throw ErrorHelper.Conflict("Only online registrations awaiting confirmation can be confirmed.");

                var organizer = await GetOrCreateOrganizerAsync(registration, registration.ReviewedBy);

                registration.OrganizerId = organizer.Id;
                registration.Status = RegistrationStatus.Approved;
                registration.ConfirmedAt = _currentTime.GetCurrentTime();

                await _unitOfWork.OrganizerRegistrations.Update(registration);
                await _notificationService.SendAsync(NotificationCatalog.OnlineRegistrationConfirmed(
                    registration.Id, registration.BrandName, registration.CommissionRate!.Value));
                await _unitOfWork.SaveChangesAsync();
                return true;
            });

            return await GetOrganizerRegistrationByDetail(id);
        }

        public async Task<OrganizerRegistrationResponse> CancelOrganizerRegistrationAsync(Guid id, CancelOrganizerRegistrationRequest? request)
        {
            var userId = _claimsService.GetRequiredUserId();

            var registration = await GetOwnedRegistrationForUpdateAsync(id, userId);

            var canCancel = registration.Status == RegistrationStatus.Pending
                || (registration.Type == RegistrationType.Online && registration.Status == RegistrationStatus.AwaitingConfirmation);

            if (!canCancel)
                throw ErrorHelper.Conflict("This registration can no longer be cancelled.");

            registration.Status = RegistrationStatus.Cancelled;
            registration.CancelledAt = _currentTime.GetCurrentTime();
            registration.CancelReason = string.IsNullOrWhiteSpace(request?.CancelReason) ? null : request.CancelReason.Trim();

            await _unitOfWork.OrganizerRegistrations.Update(registration);
            await _notificationService.SendAsync(
                NotificationCatalog.RegistrationCancelledByOwner(registration.Id, registration.BrandName));
            await _unitOfWork.SaveChangesAsync();

            return await GetOrganizerRegistrationByDetail(id);
        }

        private async Task ApproveStallRegistrationAsync(OrganizerRegistration registration, Guid managerId, decimal? commissionRate)
        {
            if (commissionRate.HasValue)
                throw ErrorHelper.BadRequest("Commission rate only applies to online registrations.");

            var stall = registration.Stall
                ?? throw ErrorHelper.Conflict("The requested stall is no longer available.");

            var today = GetVietnamToday();
            var startDate = registration.StartDate!.Value;
            var endDate = registration.EndDate!.Value;

            if (startDate < today)
                throw ErrorHelper.Conflict("The requested start date has already passed.");

            if (await HasOverlappingRentalAsync(stall.Id, startDate, endDate))
                throw ErrorHelper.Conflict("The stall is already rented during the requested period.");

            var organizer = await GetOrCreateOrganizerAsync(registration, managerId);

            await _unitOfWork.StallRentals.AddAsync(new StallRental
            {
                StallId = stall.Id,
                OrganizerId = organizer.Id,
                RegistrationId = registration.Id,
                StartDate = startDate,
                EndDate = endDate,
                Status = startDate <= today ? RentalStatus.Active : RentalStatus.Confirmed
            });

            stall.Status = StallStatus.Rented;
            await _unitOfWork.Stalls.Update(stall);

            registration.OrganizerId = organizer.Id;
            registration.Status = RegistrationStatus.Approved;
            MarkReviewed(registration, managerId);

            await _notificationService.SendAsync(NotificationCatalog.OrganizerApproved(
                registration.UserId, registration.Id, registration.BrandName, stall.Code));

            var conflicting = await _unitOfWork.OrganizerRegistrations.GetQueryable()
                .Where(r => r.Id != registration.Id
                    && r.StallId == stall.Id
                    && r.Status == RegistrationStatus.Pending
                    && r.StartDate <= endDate
                    && r.EndDate >= startDate)
                .ToListAsync();

            foreach (var other in conflicting)
            {
                MarkRejected(other, managerId, $"Gian hàng {stall.Code} đã được cho thuê trong khoảng thời gian này.");

                await _notificationService.SendAsync(NotificationCatalog.OrganizerRejected(
                    other.UserId, other.Id, other.BrandName, other.RejectReason!));
            }

            if (conflicting.Count > 0)
                await _unitOfWork.OrganizerRegistrations.UpdateRange(conflicting);
        }

        private async Task ApproveOnlineRegistrationAsync(OrganizerRegistration registration, Guid managerId, decimal? commissionRate)
        {
            if (!commissionRate.HasValue)
                throw ErrorHelper.BadRequest("Commission rate is required for online registrations.");

            await EnsureUserNotSuspendedAsync(registration.UserId);

            registration.CommissionRate = commissionRate.Value;
            registration.Status = RegistrationStatus.AwaitingConfirmation;
            MarkReviewed(registration, managerId);

            var recipientRole = registration.User.Role == UserRole.Organization
                ? NotificationRecipientRole.Organizer
                : NotificationRecipientRole.User;

            await _notificationService.SendAsync(NotificationCatalog.OrganizerCommissionProposed(
                registration.UserId, recipientRole, registration.Id, registration.BrandName, commissionRate.Value));
        }

        // An existing organizer keeps its profile; the registration only adds a contract to it
        private async Task<Organizer> GetOrCreateOrganizerAsync(OrganizerRegistration registration, Guid? approvedBy)
        {
            var organizer = await _unitOfWork.Organizers.FirstOrDefaultAsync(o => o.UserId == registration.UserId);

            if (organizer?.Status == OrganizerStatus.Suspended)
                throw ErrorHelper.Conflict("The organizer is suspended.");

            if (organizer is null)
            {
                organizer = new Organizer
                {
                    UserId = registration.UserId,
                    BrandName = registration.BrandName,
                    ContactPhone = registration.ContactPhone,
                    ContactEmail = registration.User.Email,
                    BusinessInfo = registration.BusinessInfo,
                    ApprovedBy = approvedBy,
                    ApprovedAt = _currentTime.GetCurrentTime()
                };

                await _unitOfWork.Organizers.AddAsync(organizer);
            }

            if (registration.User.Role == UserRole.User)
            {
                registration.User.Role = UserRole.Organization;
                await _unitOfWork.Users.Update(registration.User);
            }

            return organizer;
        }

        private async Task<Organizer> GetCurrentActiveOrganizerAsync()
        {
            var userId = _claimsService.GetRequiredUserId();

            var organizer = await _unitOfWork.Organizers.FirstOrDefaultAsync(o => o.UserId == userId)
                ?? throw ErrorHelper.NotFound("Organizer not found.");

            if (organizer.Status == OrganizerStatus.Suspended)
                throw ErrorHelper.Forbidden("Your organizer account is suspended.");

            return organizer;
        }

        private async Task<OrganizerRegistration> GetRegistrationForUpdateAsync(Guid id)
        {
            return await _unitOfWork.OrganizerRegistrations.GetByIdAsync(id, r => r.User, r => r.Stall!)
                ?? throw ErrorHelper.NotFound("Organizer registration not found.");
        }

        // Another user's registration is reported as missing so its existence is not leaked
        private async Task<OrganizerRegistration> GetOwnedRegistrationForUpdateAsync(Guid id, Guid userId)
        {
            var registration = await GetRegistrationForUpdateAsync(id);

            if (registration.UserId != userId)
                throw ErrorHelper.NotFound("Organizer registration not found.");

            return registration;
        }

        private async Task EnsureStallCanBeRequestedAsync(Guid userId, Stall stall, DateOnly startDate, DateOnly endDate)
        {
            if (await HasOverlappingRentalAsync(stall.Id, startDate, endDate))
                throw ErrorHelper.Conflict($"Stall {stall.Code} is already rented during the requested period.");

            var hasPendingRegistration = await _unitOfWork.OrganizerRegistrations.GetQueryable().AnyAsync(r =>
                r.UserId == userId
                && r.StallId == stall.Id
                && r.Status == RegistrationStatus.Pending);

            if (hasPendingRegistration)
                throw ErrorHelper.Conflict($"You already have a pending registration for stall {stall.Code}.");
        }

        private async Task EnsureNoOpenOnlineRegistrationAsync(Guid userId)
        {
            var hasOpenRegistration = await _unitOfWork.OrganizerRegistrations.GetQueryable().AnyAsync(r =>
                r.UserId == userId
                && r.Type == RegistrationType.Online
                && (r.Status == RegistrationStatus.Pending
                    || r.Status == RegistrationStatus.AwaitingConfirmation
                    || r.Status == RegistrationStatus.Approved));

            if (hasOpenRegistration)
                throw ErrorHelper.Conflict("You already have an open online registration.");
        }

        private async Task EnsureUserNotSuspendedAsync(Guid userId)
        {
            var isSuspended = await _unitOfWork.Organizers.GetQueryable()
                .AnyAsync(o => o.UserId == userId && o.Status == OrganizerStatus.Suspended);

            if (isSuspended)
                throw ErrorHelper.Conflict("The organizer is suspended.");
        }

        private Task<bool> HasOverlappingRentalAsync(Guid stallId, DateOnly startDate, DateOnly endDate)
        {
            return _unitOfWork.StallRentals.GetQueryable().AnyAsync(r =>
                r.StallId == stallId
                && (r.Status == RentalStatus.Confirmed || r.Status == RentalStatus.Active)
                && r.StartDate <= endDate
                && (r.EndDate == null || r.EndDate >= startDate));
        }

        private void EnsureStartDateNotPast(DateOnly startDate)
        {
            if (startDate < GetVietnamToday())
                throw ErrorHelper.BadRequest("Start date must not be in the past.");
        }

        private DateOnly GetVietnamToday()
            => DateOnly.FromDateTime(_currentTime.GetCurrentTime().AddHours(VietnamUtcOffsetHours));

        private void MarkReviewed(OrganizerRegistration registration, Guid managerId)
        {
            registration.ReviewedBy = managerId;
            registration.ReviewedAt = _currentTime.GetCurrentTime();
        }

        private void MarkRejected(OrganizerRegistration registration, Guid managerId, string reason)
        {
            registration.Status = RegistrationStatus.Rejected;
            registration.RejectReason = reason;
            MarkReviewed(registration, managerId);
        }

        private static async Task<Pagination<OrganizerRegistrationResponse>> GetPageAsync(
            IQueryable<OrganizerRegistration> query,
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            RegistrationType? type,
            RegistrationStatus? status)
        {
            var pagination = new PaginationParameter { PageIndex = page, PageSize = pageSize };

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(r =>
                    r.BrandName.ToLower().Contains(keyword) ||
                    r.User.FullName.ToLower().Contains(keyword) ||
                    r.User.Email.ToLower().Contains(keyword));
            }

            if (type.HasValue)
                query = query.Where(r => r.Type == type.Value);

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            var totalCount = await query.CountAsync();

            query = sortBy?.Trim().ToLowerInvariant() switch
            {
                "brandname" => isDescending ? query.OrderByDescending(r => r.BrandName) : query.OrderBy(r => r.BrandName),
                "status" => isDescending ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status),
                "startdate" => isDescending ? query.OrderByDescending(r => r.StartDate) : query.OrderBy(r => r.StartDate),
                _ => isDescending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt)
            };

            var items = await ProjectToResponse(query
                    .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                    .Take(pagination.PageSize))
                .ToListAsync();

            return new Pagination<OrganizerRegistrationResponse>(items, totalCount, pagination.PageIndex, pagination.PageSize);
        }

        private static IQueryable<OrganizerRegistrationResponse> ProjectToResponse(IQueryable<OrganizerRegistration> query)
        {
            return query.Select(r => new OrganizerRegistrationResponse
            {
                Id = r.Id,
                UserId = r.UserId,
                UserFullName = r.User.FullName,
                UserEmail = r.User.Email,
                Type = r.Type,
                StallId = r.StallId,
                StallCode = r.Stall != null ? r.Stall.Code : null,
                StallName = r.Stall != null ? r.Stall.Name : null,
                OrganizerId = r.OrganizerId,
                BrandName = r.BrandName,
                ContactPhone = r.ContactPhone,
                BusinessInfo = r.BusinessInfo,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                CommissionRate = r.CommissionRate,
                Status = r.Status,
                ReviewedBy = r.ReviewedBy,
                ReviewedAt = r.ReviewedAt,
                RejectReason = r.RejectReason,
                ConfirmedAt = r.ConfirmedAt,
                CancelledAt = r.CancelledAt,
                CancelReason = r.CancelReason,
                CreatedAt = r.CreatedAt
            });
        }
    }
}
