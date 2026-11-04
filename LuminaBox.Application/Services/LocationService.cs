using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Geography;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class LocationService : ILocationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LocationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Pagination<ProvinceResponse>> GetProvincesAsync(string? search, int page, int pageSize)
        {
            var pagination = new PaginationParameter { PageIndex = page, PageSize = pageSize };

            var query = _unitOfWork.Provinces.GetQueryable().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(keyword));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Name)
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(p => new ProvinceResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    GhnProvinceId = p.GhnProvinceId
                })
                .ToListAsync();

            return new Pagination<ProvinceResponse>(items, totalCount, pagination.PageIndex, pagination.PageSize);
        }

        public async Task<Pagination<DistrictResponse>> GetDistrictsAsync(Guid provinceId, string? search, int page, int pageSize)
        {
            if (!await _unitOfWork.Provinces.GetQueryable().AnyAsync(p => p.Id == provinceId))
                throw ErrorHelper.NotFound("Province not found.", "E16.3");

            var pagination = new PaginationParameter { PageIndex = page, PageSize = pageSize };

            var query = _unitOfWork.Districts.GetQueryable().AsNoTracking().Where(d => d.ProvinceId == provinceId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(d => d.Name.ToLower().Contains(keyword));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(d => d.Name)
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(d => new DistrictResponse
                {
                    Id = d.Id,
                    ProvinceId = d.ProvinceId,
                    Name = d.Name,
                    Code = d.Code,
                    GhnDistrictId = d.GhnDistrictId
                })
                .ToListAsync();

            return new Pagination<DistrictResponse>(items, totalCount, pagination.PageIndex, pagination.PageSize);
        }

        public async Task<Pagination<WardResponse>> GetWardsAsync(Guid districtId, string? search, int page, int pageSize)
        {
            if (!await _unitOfWork.Districts.GetQueryable().AnyAsync(d => d.Id == districtId))
                throw ErrorHelper.NotFound("District not found.", "E16.3");

            var pagination = new PaginationParameter { PageIndex = page, PageSize = pageSize };

            var query = _unitOfWork.Wards.GetQueryable().AsNoTracking().Where(w => w.DistrictId == districtId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(w => w.Name.ToLower().Contains(keyword));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(w => w.Name)
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(w => new WardResponse
                {
                    Id = w.Id,
                    DistrictId = w.DistrictId,
                    Name = w.Name,
                    Code = w.Code,
                    GhnWardCode = w.GhnWardCode
                })
                .ToListAsync();

            return new Pagination<WardResponse>(items, totalCount, pagination.PageIndex, pagination.PageSize);
        }

        public async Task<ResolvedLocation> ResolveWardAsync(Guid wardId)
        {
            return await _unitOfWork.Wards.GetQueryable()
                .AsNoTracking()
                .Where(w => w.Id == wardId)
                .Select(w => new ResolvedLocation
                {
                    WardId = w.Id,
                    DistrictId = w.DistrictId,
                    ProvinceId = w.District.ProvinceId
                })
                .FirstOrDefaultAsync()
                ?? throw ErrorHelper.NotFound("Ward not found.", "E16.3");
        }
    }
}
