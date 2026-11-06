using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Voucher;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly ICurrentTime _currentTime;

        public VoucherService(
            IUnitOfWork unitOfWork,
            IClaimsService claimsService,
            ICurrentTime currentTime)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _currentTime = currentTime;
        }

        public async Task<VoucherResponse> CreatePlatformVoucherAsync(CreateVoucherRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();
            var normalizedCode = request.Code.Trim().ToUpperInvariant();

            var codeExists = await _unitOfWork.Vouchers.GetQueryable()
                .AnyAsync(v => v.Code.ToUpper() == normalizedCode);

            if (codeExists)
                throw ErrorHelper.Conflict($"Voucher code '{normalizedCode}' already exists.");

            var voucher = new Voucher
            {
                Code = normalizedCode,
                Scope = VoucherScope.Platform,
                OrganizerId = null,
                CreatedBy = userId,
                Name = request.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                MaxDiscountAmount = request.DiscountType == DiscountType.Percent ? request.MaxDiscountAmount : null,
                MinOrderAmount = request.MinOrderAmount,
                UsageLimit = request.UsageLimit,
                UsageLimitPerUser = request.UsageLimitPerUser,
                UsedCount = 0,
                StartAt = request.StartAt,
                EndAt = request.EndAt,
                Status = VoucherStatus.Active
            };

            await _unitOfWork.Vouchers.AddAsync(voucher);
            await _unitOfWork.SaveChangesAsync();

            return await QueryVoucherByIdAsync(voucher.Id);
        }

        public async Task<VoucherResponse> CreateOrganizerVoucherAsync(CreateVoucherRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();

            var organizer = await _unitOfWork.Organizers.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.UserId == userId)
                ?? throw ErrorHelper.Forbidden("Organizer profile not found.");

            if (organizer.Status != OrganizerStatus.Active)
                throw ErrorHelper.Conflict("Organizer account is currently suspended.");

            var hasOnlineApproved = await _unitOfWork.OrganizerRegistrations.GetQueryable()
                .AnyAsync(r => r.OrganizerId == organizer.Id
                            && r.Type == RegistrationType.Online
                            && r.Status == RegistrationStatus.Approved);

            if (!hasOnlineApproved)
                throw ErrorHelper.Forbidden("Organizer must have an approved online registration to create vouchers.");

            var normalizedCode = request.Code.Trim().ToUpperInvariant();

            var codeExists = await _unitOfWork.Vouchers.GetQueryable()
                .AnyAsync(v => v.Code.ToUpper() == normalizedCode);

            if (codeExists)
                throw ErrorHelper.Conflict($"Voucher code '{normalizedCode}' already exists.");

            var voucher = new Voucher
            {
                Code = normalizedCode,
                Scope = VoucherScope.Organizer,
                OrganizerId = organizer.Id,
                CreatedBy = userId,
                Name = request.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue,
                MaxDiscountAmount = request.DiscountType == DiscountType.Percent ? request.MaxDiscountAmount : null,
                MinOrderAmount = request.MinOrderAmount,
                UsageLimit = request.UsageLimit,
                UsageLimitPerUser = request.UsageLimitPerUser,
                UsedCount = 0,
                StartAt = request.StartAt,
                EndAt = request.EndAt,
                Status = VoucherStatus.Active
            };

            await _unitOfWork.Vouchers.AddAsync(voucher);
            await _unitOfWork.SaveChangesAsync();

            return await QueryVoucherByIdAsync(voucher.Id);
        }

        public async Task<Pagination<VoucherResponse>> GetAllVouchersAsync(AllVoucherListQuery filter)
        {
            var query = _unitOfWork.Vouchers.GetQueryable()
                .AsNoTracking();

            if (filter.Scope.HasValue)
                query = query.Where(v => v.Scope == filter.Scope.Value);

            if (filter.Status.HasValue)
                query = query.Where(v => v.Status == filter.Status.Value);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var keyword = filter.Search.Trim().ToLower();
                query = query.Where(v =>
                    v.Code.ToLower().Contains(keyword) ||
                    v.Name.ToLower().Contains(keyword));
            }

            var totalCount = await query.CountAsync();

            query = filter.SortBy?.ToLower() switch
            {
                "code" => filter.IsDescending ? query.OrderByDescending(v => v.Code) : query.OrderBy(v => v.Code),
                "name" => filter.IsDescending ? query.OrderByDescending(v => v.Name) : query.OrderBy(v => v.Name),
                "startat" => filter.IsDescending ? query.OrderByDescending(v => v.StartAt) : query.OrderBy(v => v.StartAt),
                "endat" => filter.IsDescending ? query.OrderByDescending(v => v.EndAt) : query.OrderBy(v => v.EndAt),
                _ => filter.IsDescending ? query.OrderByDescending(v => v.CreatedAt) : query.OrderBy(v => v.CreatedAt)
            };

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(v => new VoucherResponse
                {
                    Id = v.Id,
                    Code = v.Code,
                    Scope = v.Scope,
                    OrganizerId = v.OrganizerId,
                    OrganizerBrandName = v.Organizer != null ? v.Organizer.BrandName : null,
                    Name = v.Name,
                    Description = v.Description,
                    DiscountType = v.DiscountType,
                    DiscountValue = v.DiscountValue,
                    MaxDiscountAmount = v.MaxDiscountAmount,
                    MinOrderAmount = v.MinOrderAmount,
                    UsageLimit = v.UsageLimit,
                    UsageLimitPerUser = v.UsageLimitPerUser,
                    UsedCount = v.UsedCount,
                    StartAt = v.StartAt,
                    EndAt = v.EndAt,
                    Status = v.Status,
                    CreatedAt = v.CreatedAt
                })
                .ToListAsync();

            return new Pagination<VoucherResponse>(items, totalCount, filter.Page, filter.PageSize);
        }

        public async Task<Pagination<VoucherResponse>> GetMyOrganizerVouchersAsync(VoucherListQuery filter)
        {
            var userId = _claimsService.GetRequiredUserId();

            var organizer = await _unitOfWork.Organizers.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.UserId == userId)
                ?? throw ErrorHelper.Forbidden("Organizer profile not found.");

            var query = _unitOfWork.Vouchers.GetQueryable()
                .AsNoTracking()
                .Where(v => v.OrganizerId == organizer.Id);

            if (filter.Status.HasValue)
                query = query.Where(v => v.Status == filter.Status.Value);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var keyword = filter.Search.Trim().ToLower();
                query = query.Where(v =>
                    v.Code.ToLower().Contains(keyword) ||
                    v.Name.ToLower().Contains(keyword));
            }

            var totalCount = await query.CountAsync();

            query = filter.SortBy?.ToLower() switch
            {
                "code" => filter.IsDescending ? query.OrderByDescending(v => v.Code) : query.OrderBy(v => v.Code),
                "name" => filter.IsDescending ? query.OrderByDescending(v => v.Name) : query.OrderBy(v => v.Name),
                "startat" => filter.IsDescending ? query.OrderByDescending(v => v.StartAt) : query.OrderBy(v => v.StartAt),
                "endat" => filter.IsDescending ? query.OrderByDescending(v => v.EndAt) : query.OrderBy(v => v.EndAt),
                _ => filter.IsDescending ? query.OrderByDescending(v => v.CreatedAt) : query.OrderBy(v => v.CreatedAt)
            };

            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(v => new VoucherResponse
                {
                    Id = v.Id,
                    Code = v.Code,
                    Scope = v.Scope,
                    OrganizerId = v.OrganizerId,
                    OrganizerBrandName = v.Organizer != null ? v.Organizer.BrandName : null,
                    Name = v.Name,
                    Description = v.Description,
                    DiscountType = v.DiscountType,
                    DiscountValue = v.DiscountValue,
                    MaxDiscountAmount = v.MaxDiscountAmount,
                    MinOrderAmount = v.MinOrderAmount,
                    UsageLimit = v.UsageLimit,
                    UsageLimitPerUser = v.UsageLimitPerUser,
                    UsedCount = v.UsedCount,
                    StartAt = v.StartAt,
                    EndAt = v.EndAt,
                    Status = v.Status,
                    CreatedAt = v.CreatedAt
                })
                .ToListAsync();

            return new Pagination<VoucherResponse>(items, totalCount, filter.Page, filter.PageSize);
        }

        public async Task<VoucherResponse> GetVoucherByDetail(Guid id)
        {
            var voucher = await GetAccessibleVoucherEntityAsync(id);
            return await QueryVoucherByIdAsync(voucher.Id);
        }

        public async Task<VoucherResponse> UpdateVoucherAsync(Guid id, UpdateVoucherRequest request)
        {
            var voucher = await GetAccessibleVoucherEntityAsync(id);

            if (request.Name is not null)
                voucher.Name = request.Name.Trim();

            if (request.Description is not null)
                voucher.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

            if (request.DiscountType.HasValue)
                voucher.DiscountType = request.DiscountType.Value;

            if (request.DiscountValue.HasValue)
                voucher.DiscountValue = request.DiscountValue.Value;

            if (voucher.DiscountType == DiscountType.Percent && voucher.DiscountValue > 100)
                throw ErrorHelper.BadRequest("Percentage discount value must not exceed 100.");

            if (voucher.DiscountType == DiscountType.Fixed)
                voucher.MaxDiscountAmount = null;
            else if (request.MaxDiscountAmount.HasValue)
                voucher.MaxDiscountAmount = request.MaxDiscountAmount.Value;

            if (request.MinOrderAmount.HasValue)
                voucher.MinOrderAmount = request.MinOrderAmount.Value;

            if (request.UsageLimit.HasValue)
                voucher.UsageLimit = request.UsageLimit.Value;

            if (request.UsageLimitPerUser.HasValue)
                voucher.UsageLimitPerUser = request.UsageLimitPerUser.Value;

            if (request.StartAt.HasValue)
                voucher.StartAt = request.StartAt.Value;

            if (request.EndAt.HasValue)
                voucher.EndAt = request.EndAt.Value;

            if (voucher.EndAt <= voucher.StartAt)
                throw ErrorHelper.BadRequest("EndAt must be after StartAt.");

            await _unitOfWork.Vouchers.Update(voucher);
            await _unitOfWork.SaveChangesAsync();

            return await QueryVoucherByIdAsync(voucher.Id);
        }

        public async Task<VoucherResponse> UpdateVoucherStatusAsync(Guid id, UpdateVoucherStatusRequest request)
        {
            var voucher = await GetAccessibleVoucherEntityAsync(id);

            voucher.Status = request.Status;
            await _unitOfWork.Vouchers.Update(voucher);
            await _unitOfWork.SaveChangesAsync();

            return await QueryVoucherByIdAsync(voucher.Id);
        }

        public async Task DeleteVoucherAsync(Guid id)
        {
            var voucher = await GetAccessibleVoucherEntityAsync(id);

            if (voucher.UsedCount > 0)
                throw ErrorHelper.Conflict("Cannot delete a voucher that has already been used.");

            await _unitOfWork.Vouchers.SoftRemove(voucher);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<VoucherResponse>> GetAvailableVouchersAsync(Guid? organizerId = null)
        {
            var userId = _claimsService.GetRequiredUserId();
            var now = _currentTime.GetCurrentTime();

            var claimedIds = await _unitOfWork.UserVouchers.GetQueryable()
                .AsNoTracking()
                .Where(uv => uv.UserId == userId)
                .Select(uv => uv.VoucherId)
                .ToListAsync();

            var query = _unitOfWork.Vouchers.GetQueryable()
                .AsNoTracking()
                .Where(v => v.Status == VoucherStatus.Active
                         && v.StartAt <= now
                         && v.EndAt >= now
                         && (!v.UsageLimit.HasValue || v.UsedCount < v.UsageLimit.Value)
                         && !claimedIds.Contains(v.Id));

            if (organizerId.HasValue)
            {
                query = query.Where(v => v.Scope == VoucherScope.Platform || v.OrganizerId == organizerId.Value);
            }

            return await query
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => new VoucherResponse
                {
                    Id = v.Id,
                    Code = v.Code,
                    Scope = v.Scope,
                    OrganizerId = v.OrganizerId,
                    OrganizerBrandName = v.Organizer != null ? v.Organizer.BrandName : null,
                    Name = v.Name,
                    Description = v.Description,
                    DiscountType = v.DiscountType,
                    DiscountValue = v.DiscountValue,
                    MaxDiscountAmount = v.MaxDiscountAmount,
                    MinOrderAmount = v.MinOrderAmount,
                    UsageLimit = v.UsageLimit,
                    UsageLimitPerUser = v.UsageLimitPerUser,
                    UsedCount = v.UsedCount,
                    StartAt = v.StartAt,
                    EndAt = v.EndAt,
                    Status = v.Status,
                    CreatedAt = v.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<UserVoucherResponse> ClaimVoucherAsync(Guid voucherId)
        {
            var userId = _claimsService.GetRequiredUserId();
            var now = _currentTime.GetCurrentTime();

            var voucher = await _unitOfWork.Vouchers.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == voucherId)
                ?? throw ErrorHelper.NotFound("Voucher not found.");

            if (voucher.Status != VoucherStatus.Active)
                throw ErrorHelper.BadRequest("Voucher is not active.");

            if (now < voucher.StartAt || now > voucher.EndAt)
                throw ErrorHelper.BadRequest("Voucher is outside its valid date range.");

            if (voucher.UsageLimit.HasValue && voucher.UsedCount >= voucher.UsageLimit.Value)
                throw ErrorHelper.Conflict("Voucher has reached its usage limit.");

            var alreadyClaimed = await _unitOfWork.UserVouchers.GetQueryable()
                .AnyAsync(uv => uv.UserId == userId && uv.VoucherId == voucherId);

            if (alreadyClaimed)
                throw ErrorHelper.Conflict("You have already claimed this voucher.");

            var userUsedCount = await _unitOfWork.VoucherUsages.GetQueryable()
                .CountAsync(vu => vu.VoucherId == voucherId && vu.UserId == userId);

            if (userUsedCount >= voucher.UsageLimitPerUser)
                throw ErrorHelper.Conflict("You have reached the usage limit for this voucher.");

            var userVoucher = new UserVoucher
            {
                UserId = userId,
                VoucherId = voucherId,
                ClaimedAt = now
            };

            await _unitOfWork.UserVouchers.AddAsync(userVoucher);
            await _unitOfWork.SaveChangesAsync();

            return await _unitOfWork.UserVouchers.GetQueryable()
                .AsNoTracking()
                .Where(uv => uv.Id == userVoucher.Id)
                .Select(uv => new UserVoucherResponse
                {
                    Id = uv.Id,
                    VoucherId = uv.VoucherId,
                    Code = uv.Voucher.Code,
                    Scope = uv.Voucher.Scope,
                    OrganizerId = uv.Voucher.OrganizerId,
                    OrganizerBrandName = uv.Voucher.Organizer != null ? uv.Voucher.Organizer.BrandName : null,
                    Name = uv.Voucher.Name,
                    Description = uv.Voucher.Description,
                    DiscountType = uv.Voucher.DiscountType,
                    DiscountValue = uv.Voucher.DiscountValue,
                    MaxDiscountAmount = uv.Voucher.MaxDiscountAmount,
                    MinOrderAmount = uv.Voucher.MinOrderAmount,
                    StartAt = uv.Voucher.StartAt,
                    EndAt = uv.Voucher.EndAt,
                    Status = uv.Voucher.Status,
                    ClaimedAt = uv.ClaimedAt
                })
                .FirstAsync();
        }

        public async Task<List<UserVoucherResponse>> GetMyClaimedVouchersAsync()
        {
            var userId = _claimsService.GetRequiredUserId();

            return await _unitOfWork.UserVouchers.GetQueryable()
                .AsNoTracking()
                .Where(uv => uv.UserId == userId)
                .OrderByDescending(uv => uv.ClaimedAt)
                .Select(uv => new UserVoucherResponse
                {
                    Id = uv.Id,
                    VoucherId = uv.VoucherId,
                    Code = uv.Voucher.Code,
                    Scope = uv.Voucher.Scope,
                    OrganizerId = uv.Voucher.OrganizerId,
                    OrganizerBrandName = uv.Voucher.Organizer != null ? uv.Voucher.Organizer.BrandName : null,
                    Name = uv.Voucher.Name,
                    Description = uv.Voucher.Description,
                    DiscountType = uv.Voucher.DiscountType,
                    DiscountValue = uv.Voucher.DiscountValue,
                    MaxDiscountAmount = uv.Voucher.MaxDiscountAmount,
                    MinOrderAmount = uv.Voucher.MinOrderAmount,
                    StartAt = uv.Voucher.StartAt,
                    EndAt = uv.Voucher.EndAt,
                    Status = uv.Voucher.Status,
                    ClaimedAt = uv.ClaimedAt
                })
                .ToListAsync();
        }

        public async Task RemoveClaimedVoucherAsync(Guid userVoucherId)
        {
            var userId = _claimsService.GetRequiredUserId();

            var userVoucher = await _unitOfWork.UserVouchers.GetQueryable()
                .FirstOrDefaultAsync(uv => uv.Id == userVoucherId && uv.UserId == userId)
                ?? throw ErrorHelper.NotFound("Claimed voucher not found in your wallet.");

            await _unitOfWork.UserVouchers.SoftRemove(userVoucher);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Dictionary<int, OrderGroupVoucherResult>> ValidateAndCalculateOrderVouchersAsync(
            Guid userId,
            List<Guid>? requestedVoucherIds,
            IReadOnlyList<(int GroupIndex, Guid OrganizerId, decimal Subtotal)> groups,
            DateTime now)
        {
            var results = groups.ToDictionary(g => g.GroupIndex, _ => new OrderGroupVoucherResult());

            if (requestedVoucherIds is null || requestedVoucherIds.Count == 0)
                return results;

            var distinctReqVoucherIds = requestedVoucherIds.Distinct().ToList();
            var claimedUserVouchers = await _unitOfWork.UserVouchers.GetQueryable()
                .Include(uv => uv.Voucher)
                .Where(uv => uv.UserId == userId
                          && (distinctReqVoucherIds.Contains(uv.Id) || distinctReqVoucherIds.Contains(uv.VoucherId)))
                .ToListAsync();

            if (claimedUserVouchers.Count != distinctReqVoucherIds.Count)
                throw ErrorHelper.BadRequest("One or more selected vouchers were not found in your claimed voucher wallet.");

            foreach (var uv in claimedUserVouchers)
            {
                var v = uv.Voucher;
                if (v.Status != VoucherStatus.Active)
                    throw ErrorHelper.BadRequest($"Voucher '{v.Code}' is not active.");

                if (now < v.StartAt || now > v.EndAt)
                    throw ErrorHelper.BadRequest($"Voucher '{v.Code}' is outside its valid date range.");

                if (v.UsageLimit.HasValue && v.UsedCount >= v.UsageLimit.Value)
                    throw ErrorHelper.BadRequest($"Voucher '{v.Code}' has reached its usage limit.");

                var userUsedCount = await _unitOfWork.VoucherUsages.GetQueryable()
                    .CountAsync(vu => vu.VoucherId == v.Id && vu.UserId == userId);

                if (userUsedCount >= v.UsageLimitPerUser)
                    throw ErrorHelper.BadRequest($"You have reached the usage limit for voucher '{v.Code}'.");
            }

            var platformUserVouchers = claimedUserVouchers
                .Where(uv => uv.Voucher.Scope == VoucherScope.Platform)
                .ToList();

            if (platformUserVouchers.Count > 1)
                throw ErrorHelper.BadRequest("Only one platform voucher can be applied per order.");

            var organizerUserVouchers = claimedUserVouchers
                .Where(uv => uv.Voucher.Scope == VoucherScope.Organizer)
                .ToList();

            if (organizerUserVouchers.GroupBy(uv => uv.Voucher.OrganizerId).Any(g => g.Count() > 1))
                throw ErrorHelper.BadRequest("Only one organizer voucher can be applied per organizer.");

            var platformVoucherConsumed = false;
            var consumedOrganizerVoucherIds = new HashSet<Guid>();

            foreach (var (groupIndex, organizerId, subtotal) in groups)
            {
                decimal organizerDiscount = 0m;
                UserVoucher? appliedOrganizerUv = organizerUserVouchers
                    .FirstOrDefault(uv => uv.Voucher.OrganizerId == organizerId
                                       && !consumedOrganizerVoucherIds.Contains(uv.Id));

                if (appliedOrganizerUv is not null)
                {
                    var ov = appliedOrganizerUv.Voucher;
                    if (subtotal < ov.MinOrderAmount)
                        throw ErrorHelper.BadRequest($"Order subtotal does not meet minimum amount for voucher '{ov.Code}'.");

                    organizerDiscount = ov.DiscountType == DiscountType.Percent
                        ? Math.Round(subtotal * ov.DiscountValue / 100m, 0, MidpointRounding.AwayFromZero)
                        : ov.DiscountValue;

                    if (ov.DiscountType == DiscountType.Percent && ov.MaxDiscountAmount.HasValue)
                        organizerDiscount = Math.Min(organizerDiscount, ov.MaxDiscountAmount.Value);

                    organizerDiscount = Math.Min(subtotal, organizerDiscount);
                    consumedOrganizerVoucherIds.Add(appliedOrganizerUv.Id);
                }

                decimal platformDiscount = 0m;
                UserVoucher? appliedPlatformUv = null;

                if (!platformVoucherConsumed && platformUserVouchers.Count == 1)
                {
                    var pv = platformUserVouchers[0].Voucher;
                    if (subtotal < pv.MinOrderAmount)
                        throw ErrorHelper.BadRequest($"Order subtotal does not meet minimum amount for platform voucher '{pv.Code}'.");

                    var remainingAfterOrganizer = Math.Max(0m, subtotal - organizerDiscount);
                    platformDiscount = pv.DiscountType == DiscountType.Percent
                        ? Math.Round(remainingAfterOrganizer * pv.DiscountValue / 100m, 0, MidpointRounding.AwayFromZero)
                        : pv.DiscountValue;

                    if (pv.DiscountType == DiscountType.Percent && pv.MaxDiscountAmount.HasValue)
                        platformDiscount = Math.Min(platformDiscount, pv.MaxDiscountAmount.Value);

                    platformDiscount = Math.Min(remainingAfterOrganizer, platformDiscount);
                    appliedPlatformUv = platformUserVouchers[0];
                    platformVoucherConsumed = true;
                }

                results[groupIndex] = new OrderGroupVoucherResult
                {
                    OrganizerDiscount = organizerDiscount,
                    PlatformDiscount = platformDiscount,
                    AppliedOrganizerUserVoucher = appliedOrganizerUv,
                    AppliedPlatformUserVoucher = appliedPlatformUv
                };
            }

            foreach (var orgUv in organizerUserVouchers)
            {
                if (!consumedOrganizerVoucherIds.Contains(orgUv.Id))
                    throw ErrorHelper.BadRequest($"Organizer voucher '{orgUv.Voucher.Code}' does not match any organizer in this order.");
            }

            return results;
        }

        public async Task ConsumeVoucherAsync(
            UserVoucher userVoucher,
            Guid orderId,
            Guid userId,
            decimal discountAmount,
            DateTime now)
        {
            var voucher = userVoucher.Voucher;
            voucher.UsedCount += 1;
            await _unitOfWork.Vouchers.Update(voucher);
            await _unitOfWork.UserVouchers.SoftRemove(userVoucher);

            await _unitOfWork.VoucherUsages.AddAsync(new VoucherUsage
            {
                VoucherId = voucher.Id,
                UserId = userId,
                OrderId = orderId,
                DiscountAmount = discountAmount,
                UsedAt = now
            });
        }

        public async Task RestoreOrderVouchersAsync(Order order, DateTime now)
        {
            if (order.VoucherUsages.Count == 0)
                return;

            foreach (var usage in order.VoucherUsages.ToList())
            {
                var voucher = usage.Voucher;
                if (voucher.UsedCount > 0)
                {
                    voucher.UsedCount -= 1;
                    await _unitOfWork.Vouchers.Update(voucher);
                }

                var alreadyHasActiveClaim = await _unitOfWork.UserVouchers.GetQueryable()
                    .AnyAsync(uv => uv.UserId == order.UserId && uv.VoucherId == usage.VoucherId);

                if (!alreadyHasActiveClaim)
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
        }

        private async Task<Voucher> GetAccessibleVoucherEntityAsync(Guid id)
        {
            var userId = _claimsService.GetRequiredUserId();

            var voucher = await _unitOfWork.Vouchers.GetByIdAsync(id)
                ?? throw ErrorHelper.NotFound("Voucher not found.");

            var user = await _unitOfWork.Users.GetByIdAsync(userId)
                ?? throw ErrorHelper.NotFound("User not found.");

            if (user.Role == UserRole.Manager)
            {
                if (voucher.Scope != VoucherScope.Platform)
                    throw ErrorHelper.Forbidden("Managers can only modify platform vouchers.");

                return voucher;
            }

            var organizer = await _unitOfWork.Organizers.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.UserId == userId)
                ?? throw ErrorHelper.Forbidden("You do not have permission to access this voucher.");

            if (voucher.Scope != VoucherScope.Organizer || voucher.OrganizerId != organizer.Id)
                throw ErrorHelper.Forbidden("You can only modify vouchers belonging to your organizer.");

            return voucher;
        }

        private async Task<VoucherResponse> QueryVoucherByIdAsync(Guid id)
        {
            return await _unitOfWork.Vouchers.GetQueryable()
                .AsNoTracking()
                .Where(v => v.Id == id)
                .Select(v => new VoucherResponse
                {
                    Id = v.Id,
                    Code = v.Code,
                    Scope = v.Scope,
                    OrganizerId = v.OrganizerId,
                    OrganizerBrandName = v.Organizer != null ? v.Organizer.BrandName : null,
                    Name = v.Name,
                    Description = v.Description,
                    DiscountType = v.DiscountType,
                    DiscountValue = v.DiscountValue,
                    MaxDiscountAmount = v.MaxDiscountAmount,
                    MinOrderAmount = v.MinOrderAmount,
                    UsageLimit = v.UsageLimit,
                    UsageLimitPerUser = v.UsageLimitPerUser,
                    UsedCount = v.UsedCount,
                    StartAt = v.StartAt,
                    EndAt = v.EndAt,
                    Status = v.Status,
                    CreatedAt = v.CreatedAt
                })
                .FirstOrDefaultAsync()
                ?? throw ErrorHelper.NotFound("Voucher not found.");
        }
    }
}

