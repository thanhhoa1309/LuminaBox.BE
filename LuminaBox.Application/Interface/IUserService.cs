using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.User;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Interface
{
    public interface IUserService
    {
        Task<Pagination<UserResponse>> GetAllAsync(
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            UserRole? role = null,
            UserStatus? status = null);
        Task<UserResponse> GetUserByDetail(Guid id);
        Task<UserResponse> UpdateUserAsync(UpdateUserRequest request);
        Task<UserResponse> GetCurrentUserAsync();
    }
}
