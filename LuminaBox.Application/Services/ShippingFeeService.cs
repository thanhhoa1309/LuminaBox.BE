using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Dtos.Shipping;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class ShippingFeeService : IShippingFeeService
    {
        private sealed record Point(int? DistrictId, string? WardCode, Guid? PickupAddressId);

        private sealed class Package
        {
            public decimal WeightGrams { get; private set; }
            public decimal LengthCm { get; private set; }
            public decimal WidthCm { get; private set; }
            public decimal HeightCm { get; private set; }

            public void Add(decimal weight, decimal length, decimal width, decimal height, int quantity)
            {
                WeightGrams += weight * quantity;
                LengthCm = Math.Max(LengthCm, length);
                WidthCm = Math.Max(WidthCm, width);
                HeightCm += height * quantity;
            }
        }

        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly IGhnClient _ghnClient;

        public ShippingFeeService(IUnitOfWork unitOfWork, IClaimsService claimsService, IGhnClient ghnClient)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _ghnClient = ghnClient;
        }

        public async Task<ShippingFeeResponse> EstimateAsync(ShippingFeeRequest request)
        {
            var destination = await ResolveDestinationAsync(request.AddressId);
            var origin = await ResolveOriginAsync(request.OrganizerId, request.StallId);
            var package = await BuildPackageAsync(request);

            if (origin.DistrictId is null || string.IsNullOrEmpty(origin.WardCode)
                || destination.DistrictId is null || string.IsNullOrEmpty(destination.WardCode))
            {
                throw ErrorHelper.BadRequest("This address is not supported by GHN.", "E18.1");
            }

            var ghnRequest = new GhnFeeRequest
            {
                FromDistrictId = origin.DistrictId.Value,
                FromWardCode = origin.WardCode,
                ToDistrictId = destination.DistrictId.Value,
                ToWardCode = destination.WardCode,
                WeightGrams = (int)Math.Ceiling(package.WeightGrams),
                LengthCm = (int)Math.Ceiling(package.LengthCm),
                WidthCm = (int)Math.Ceiling(package.WidthCm),
                HeightCm = (int)Math.Ceiling(package.HeightCm)
            };

            return new ShippingFeeResponse
            {
                Fee = await _ghnClient.CalculateFeeAsync(ghnRequest),
                WeightGrams = ghnRequest.WeightGrams,
                LengthCm = ghnRequest.LengthCm,
                WidthCm = ghnRequest.WidthCm,
                HeightCm = ghnRequest.HeightCm,
                PickupAddressId = origin.PickupAddressId
            };
        }

        private async Task<Point> ResolveDestinationAsync(Guid addressId)
        {
            var userId = _claimsService.GetRequiredUserId();

            var address = await _unitOfWork.Addresses.GetQueryable()
                .AsNoTracking()
                .Where(a => a.Id == addressId)
                .Select(a => new { a.UserId, a.Type, a.District.GhnDistrictId, a.Ward.GhnWardCode })
                .FirstOrDefaultAsync()
                ?? throw ErrorHelper.NotFound("Address not found.", "E16.3");

            if (address.UserId != userId)
                throw ErrorHelper.Forbidden("You cannot use this address.", "E16.2");

            if (address.Type != AddressType.Shipping)
                throw ErrorHelper.BadRequest("A shipping address is required.");

            return new Point(address.GhnDistrictId, address.GhnWardCode, null);
        }

        private async Task<Point> ResolveOriginAsync(Guid organizerId, Guid? stallId)
        {
            if (stallId.HasValue)
            {
                var stall = await _unitOfWork.Stalls.GetQueryable()
                    .AsNoTracking()
                    .Where(s => s.Id == stallId.Value
                        && s.Rentals.Any(r => r.OrganizerId == organizerId && r.Status == RentalStatus.Active))
                    .Select(s => new { s.District.GhnDistrictId, s.Ward.GhnWardCode })
                    .FirstOrDefaultAsync()
                    ?? throw ErrorHelper.NotFound("The organizer has no active rental at this stall.", "E4.4");

                return new Point(stall.GhnDistrictId, stall.GhnWardCode, null);
            }

            var organizerUserId = await _unitOfWork.Organizers.GetQueryable()
                .AsNoTracking()
                .Where(o => o.Id == organizerId)
                .Select(o => (Guid?)o.UserId)
                .FirstOrDefaultAsync()
                ?? throw ErrorHelper.NotFound("Organizer not found.", "E3.9");

            var pickup = await _unitOfWork.Addresses.GetQueryable()
                .AsNoTracking()
                .Where(a => a.UserId == organizerUserId && a.Type == AddressType.Pickup && a.IsDefault)
                .Select(a => new { a.Id, a.District.GhnDistrictId, a.Ward.GhnWardCode })
                .FirstOrDefaultAsync()
                ?? throw ErrorHelper.Conflict("The organizer has no pickup address.", "E18.2");

            return new Point(pickup.GhnDistrictId, pickup.GhnWardCode, pickup.Id);
        }

        private async Task<Package> BuildPackageAsync(ShippingFeeRequest request)
        {
            var itemIds = request.Items
                .Concat(request.Boxes.SelectMany(b => b.Selections))
                .Select(i => i.ProductItemId)
                .Distinct()
                .ToList();

            var items = await _unitOfWork.ProductItems.GetQueryable()
                .AsNoTracking()
                .Where(i => itemIds.Contains(i.Id) && i.Product.OrganizerId == request.OrganizerId)
                .Select(i => new { i.Id, i.Weight, i.LengthCm, i.WidthCm, i.HeightCm })
                .ToDictionaryAsync(i => i.Id);

            if (items.Count != itemIds.Count)
                throw ErrorHelper.BadRequest("Every cake must belong to the organizer.", "E9.2");

            var boxIds = request.Boxes.Select(b => b.BoxItemId).Distinct().ToList();

            var boxes = await _unitOfWork.BoxItems.GetQueryable()
                .AsNoTracking()
                .Where(b => boxIds.Contains(b.Id) && b.OrganizerId == request.OrganizerId)
                .Select(b => new { b.Id, b.Capacity, b.Weight, b.LengthCm, b.WidthCm, b.HeightCm })
                .ToDictionaryAsync(b => b.Id);

            if (boxes.Count != boxIds.Count)
                throw ErrorHelper.NotFound("Box item not found.", "E9.4");

            var package = new Package();

            foreach (var line in request.Items)
            {
                var item = items[line.ProductItemId];
                package.Add(item.Weight, item.LengthCm, item.WidthCm, item.HeightCm, line.Quantity);
            }

            foreach (var line in request.Boxes)
            {
                var box = boxes[line.BoxItemId];

                if (line.Selections.Sum(s => s.Quantity) != box.Capacity)
                    throw ErrorHelper.BadRequest("The cakes must fill the box exactly.", "E9.1");

                var cakesWeight = line.Selections.Sum(s => items[s.ProductItemId].Weight * s.Quantity);
                package.Add(box.Weight + cakesWeight, box.LengthCm, box.WidthCm, box.HeightCm, 1);
            }

            return package;
        }
    }
}
