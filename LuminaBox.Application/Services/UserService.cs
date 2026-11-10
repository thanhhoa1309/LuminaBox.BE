using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.User;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;

        public UserService(IUnitOfWork unitOfWork, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
        }

        public async Task<Pagination<UserResponse>> GetAllAsync(
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            UserRole? role = null,
            UserStatus? status = null)
        {
            var pagination = new PaginationParameter { PageIndex = page, PageSize = pageSize };

            var query = _unitOfWork.Users.GetQueryable().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(keyword) ||
                    u.Email.ToLower().Contains(keyword) ||
                    (u.Phone != null && u.Phone.Contains(keyword)));
            }

            if (role.HasValue)
                query = query.Where(u => u.Role == role.Value);

            if (status.HasValue)
                query = query.Where(u => u.Status == status.Value);

            var totalCount = await query.CountAsync();

            query = sortBy?.Trim().ToLowerInvariant() switch
            {
                "fullname" => isDescending ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName),
                "email" => isDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                _ => isDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
            };

            var items = await query
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    Address = u.Address,
                    Role = u.Role,
                    Status = u.Status,
                    IsEmailVerified = u.IsEmailVerified,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .ToListAsync();

            return new Pagination<UserResponse>(items, totalCount, pagination.PageIndex, pagination.PageSize);
        }

        public async Task<UserResponse> GetUserByDetail(Guid id)
        {
            return await _unitOfWork.Users.GetQueryable()
                .AsNoTracking()
                .Where(u => u.Id == id)
                .Select(u => new UserResponse
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    Phone = u.Phone,
                    Address = u.Address,
                    Role = u.Role,
                    Status = u.Status,
                    IsEmailVerified = u.IsEmailVerified,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    OrganizerId = u.Organizer != null ? u.Organizer.Id : null,
                    OrganizerBrandName = u.Organizer != null ? u.Organizer.BrandName : null,
                    OrganizerStatus = u.Organizer != null ? u.Organizer.Status : null
                })
                .FirstOrDefaultAsync()
                ?? throw ErrorHelper.NotFound("User not found.");
        }

        public async Task<UserResponse> UpdateUserAsync(UpdateUserRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();

            var user = await _unitOfWork.Users.GetByIdAsync(userId)
                ?? throw ErrorHelper.NotFound("User not found.");

            user.FullName = request.FullName.Trim();
            user.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
            user.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();

            await _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Address = user.Address,
                Role = user.Role,
                Status = user.Status,
                IsEmailVerified = user.IsEmailVerified,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        public async Task<UserResponse> GetCurrentUserAsync()
        {
            var userId = _claimsService.GetRequiredUserId();

            return await GetUserByDetail(userId);
        }
    }
}
