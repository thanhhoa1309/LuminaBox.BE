using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Stall;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class StallService : IStallService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly IMapper _mapper;

        public StallService(IUnitOfWork unitOfWork, IClaimsService claimsService, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
        }

        public async Task<Pagination<StallResponse>> GetAllAsync(
            string? search,
            int page,
            int pageSize,
            StallStatus? status = null,
            Guid? provinceId = null,
            Guid? districtId = null,
            Guid? wardId = null)
        {
            var pagination = new PaginationParameter { PageIndex = page, PageSize = pageSize };

            var query = _unitOfWork.Stalls.GetQueryable().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(s => s.Name.ToLower().Contains(keyword) || s.Code.ToLower().Contains(keyword));
            }

            if (status.HasValue)
                query = query.Where(s => s.Status == status.Value);

            if (provinceId.HasValue)
                query = query.Where(s => s.Ward.District.ProvinceId == provinceId.Value);

            if (districtId.HasValue)
                query = query.Where(s => s.Ward.DistrictId == districtId.Value);

            if (wardId.HasValue)
                query = query.Where(s => s.WardId == wardId.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(s => s.Code)
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ProjectTo<StallResponse>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new Pagination<StallResponse>(items, totalCount, pagination.PageIndex, pagination.PageSize);
        }

        public async Task<StallResponse> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.Stalls.GetQueryable()
                .AsNoTracking()
                .Where(s => s.Id == id)
                .ProjectTo<StallResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync()
                ?? throw ErrorHelper.NotFound("Stall not found.", "E4.4");
        }

        public async Task<List<StallResponse>> GetMineAsync()
        {
            var userId = _claimsService.GetRequiredUserId();

            var organizer = await _unitOfWork.Organizers.FirstOrDefaultAsync(o => o.UserId == userId)
                ?? throw ErrorHelper.NotFound("Organizer profile not found.", "E3.9");

            return await _unitOfWork.Stalls.GetQueryable()
                .AsNoTracking()
                .Where(s => s.Rentals.Any(r => r.OrganizerId == organizer.Id && r.Status == RentalStatus.Active))
                .OrderBy(s => s.Code)
                .ProjectTo<StallResponse>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<StallResponse> CreateAsync(CreateStallRequest request)
        {
            await EnsureCodeAvailableAsync(request.Code.Trim(), null);

            await EnsureWardExistsAsync(request.WardId);

            var stall = _mapper.Map<Stall>(request);

            await _unitOfWork.Stalls.AddAsync(stall);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(stall.Id);
        }

        public async Task<StallResponse> UpdateAsync(Guid id, UpdateStallRequest request)
        {
            var stall = await _unitOfWork.Stalls.GetByIdAsync(id)
                ?? throw ErrorHelper.NotFound("Stall not found.", "E4.4");

            if (request.Code is not null)
                await EnsureCodeAvailableAsync(request.Code.Trim(), id);

            if (request.WardId.HasValue && request.WardId.Value != stall.WardId)
            {
                await EnsureWardExistsAsync(request.WardId.Value);
                stall.WardId = request.WardId.Value;
            }

            _mapper.Map(request, stall);

            await _unitOfWork.Stalls.Update(stall);
            await _unitOfWork.SaveChangesAsync();

            return await GetByIdAsync(id);
        }

        public async Task DeleteAsync(Guid id)
        {
            var stall = await _unitOfWork.Stalls.GetByIdAsync(id)
                ?? throw ErrorHelper.NotFound("Stall not found.", "E4.4");

            var hasRental = await _unitOfWork.StallRentals.GetQueryable().AnyAsync(r => r.StallId == id);

            if (stall.Status != StallStatus.Available || hasRental)
                throw ErrorHelper.Conflict("Only an available stall without rentals can be deleted.", "E4.3");

            await _unitOfWork.Stalls.SoftRemove(stall);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task EnsureCodeAvailableAsync(string code, Guid? exceptId)
        {
            var exists = await _unitOfWork.Stalls.GetQueryable()
                .AnyAsync(s => s.Code == code && (exceptId == null || s.Id != exceptId));

            if (exists)
                throw ErrorHelper.Conflict("Stall code already exists.", "E4.1");
        }

        private async Task EnsureWardExistsAsync(Guid wardId)
        {
            if (!await _unitOfWork.Wards.GetQueryable().AnyAsync(w => w.Id == wardId))
                throw ErrorHelper.NotFound("Ward not found.", "E4.6");
        }
    }
}
