using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Dtos.Address;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class AddressService : IAddressService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly ILocationService _locationService;

        public AddressService(IUnitOfWork unitOfWork, IClaimsService claimsService, ILocationService locationService)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _locationService = locationService;
        }

        public async Task<List<AddressResponse>> GetMineAsync(AddressType? type)
        {
            var userId = _claimsService.GetRequiredUserId();

            var query = _unitOfWork.Addresses.GetQueryable().AsNoTracking().Where(a => a.UserId == userId);

            if (type.HasValue)
                query = query.Where(a => a.Type == type.Value);

            return await query
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .Select(ToResponse)
                .ToListAsync();
        }

        public async Task<AddressResponse> CreateAsync(CreateAddressRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();

            if (request.Type == AddressType.Pickup)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId)
                    ?? throw ErrorHelper.NotFound("User not found.");

                var isOnlineApplicant = await _unitOfWork.OrganizerRegistrations.GetQueryable()
                    .AnyAsync(r => r.UserId == userId
                        && r.Type == RegistrationType.Online
                        && (r.Status == RegistrationStatus.Pending
                            || r.Status == RegistrationStatus.AwaitingConfirmation
                            || r.Status == RegistrationStatus.Approved));

                if (user.Role != UserRole.Organization && !isOnlineApplicant)
                    throw ErrorHelper.Forbidden("Only organizers and online applicants can create pickup addresses.", "E16.4");
            }

            await _locationService.EnsureWardExistsAsync(request.WardId);

            var hasAnyOfType = await _unitOfWork.Addresses.GetQueryable()
                .AnyAsync(a => a.UserId == userId && a.Type == request.Type);

            var address = new Address
            {
                UserId = userId,
                Type = request.Type,
                WardId = request.WardId,
                ContactName = request.ContactName.Trim(),
                Phone = request.Phone.Trim(),
                AddressLine = request.AddressLine.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                MapRefId = Normalize(request.MapRefId),
                NewProvinceName = Normalize(request.NewProvinceName),
                NewWardName = Normalize(request.NewWardName)
            };

            var makeDefault = request.IsDefault || !hasAnyOfType;

            var id = await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                if (makeDefault)
                    await ClearDefaultAsync(userId, request.Type);

                address.IsDefault = makeDefault;
                await _unitOfWork.Addresses.AddAsync(address);
                await _unitOfWork.SaveChangesAsync();
                return address.Id;
            });

            return await GetResponseAsync(id);
        }

        public async Task<AddressResponse> UpdateAsync(Guid id, UpdateAddressRequest request)
        {
            var address = await GetOwnedAsync(id);

            if (request.WardId.HasValue && request.WardId.Value != address.WardId)
            {
                await _locationService.EnsureWardExistsAsync(request.WardId.Value);
                address.WardId = request.WardId.Value;
                address.NewProvinceName = null;
                address.NewWardName = null;
            }

            if (request.ContactName is not null)
                address.ContactName = request.ContactName.Trim();

            if (request.Phone is not null)
                address.Phone = request.Phone.Trim();

            if (request.AddressLine is not null)
                address.AddressLine = request.AddressLine.Trim();

            if (request.Latitude.HasValue)
                address.Latitude = request.Latitude;

            if (request.Longitude.HasValue)
                address.Longitude = request.Longitude;

            if (request.MapRefId is not null)
                address.MapRefId = Normalize(request.MapRefId);

            if (request.NewProvinceName is not null)
                address.NewProvinceName = Normalize(request.NewProvinceName);

            if (request.NewWardName is not null)
                address.NewWardName = Normalize(request.NewWardName);

            await _unitOfWork.Addresses.Update(address);
            await _unitOfWork.SaveChangesAsync();

            return await GetResponseAsync(id);
        }

        public async Task<AddressResponse> SetDefaultAsync(Guid id)
        {
            var address = await GetOwnedAsync(id);

            if (!address.IsDefault)
            {
                await _unitOfWork.ExecuteTransactionAsync(async () =>
                {
                    await ClearDefaultAsync(address.UserId, address.Type);

                    address.IsDefault = true;
                    await _unitOfWork.Addresses.Update(address);
                    await _unitOfWork.SaveChangesAsync();
                    return true;
                });
            }

            return await GetResponseAsync(id);
        }

        public async Task DeleteAsync(Guid id)
        {
            var address = await GetOwnedAsync(id);

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var wasDefault = address.IsDefault;

                address.IsDefault = false;
                await _unitOfWork.Addresses.Update(address);
                await _unitOfWork.Addresses.SoftRemove(address);
                await _unitOfWork.SaveChangesAsync();

                if (wasDefault)
                {
                    var next = await _unitOfWork.Addresses.GetQueryable()
                        .Where(a => a.UserId == address.UserId && a.Type == address.Type && a.Id != address.Id)
                        .OrderByDescending(a => a.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (next is not null)
                    {
                        next.IsDefault = true;
                        await _unitOfWork.Addresses.Update(next);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                return true;
            });
        }

        private async Task<Address> GetOwnedAsync(Guid id)
        {
            var userId = _claimsService.GetRequiredUserId();

            var address = await _unitOfWork.Addresses.GetByIdAsync(id)
                ?? throw ErrorHelper.NotFound("Address not found.", "E16.3");

            if (address.UserId != userId)
                throw ErrorHelper.Forbidden("You cannot modify this address.", "E16.2");

            return address;
        }

        private async Task ClearDefaultAsync(Guid userId, AddressType type)
        {
            var defaults = await _unitOfWork.Addresses.GetAllAsync(a => a.UserId == userId && a.Type == type && a.IsDefault);

            if (defaults.Count == 0)
                return;

            foreach (var item in defaults)
                item.IsDefault = false;

            await _unitOfWork.Addresses.UpdateRange(defaults);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<AddressResponse> GetResponseAsync(Guid id)
        {
            return await _unitOfWork.Addresses.GetQueryable()
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Select(ToResponse)
                .FirstAsync();
        }

        private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static readonly System.Linq.Expressions.Expression<Func<Address, AddressResponse>> ToResponse = a => new AddressResponse
        {
            Id = a.Id,
            Type = a.Type,
            ContactName = a.ContactName,
            Phone = a.Phone,
            ProvinceId = a.Ward.District.ProvinceId,
            ProvinceName = a.Ward.District.Province.Name,
            DistrictId = a.Ward.DistrictId,
            DistrictName = a.Ward.District.Name,
            WardId = a.WardId,
            WardName = a.Ward.Name,
            AddressLine = a.AddressLine,
            IsDefault = a.IsDefault,
            Latitude = a.Latitude,
            Longitude = a.Longitude,
            MapRefId = a.MapRefId,
            NewProvinceName = a.NewProvinceName,
            NewWardName = a.NewWardName,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        };
    }
}
