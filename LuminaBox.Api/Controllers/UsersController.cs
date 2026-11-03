using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.User;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "Manager")]
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get paginated list of users",
            Description = "Gets a paginated list of users with search, filter and sort. Requires Manager role.")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] bool isDescending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] UserRole? role = null,
            [FromQuery] UserStatus? status = null)
        {
            var result = await _userService.GetAllAsync(search, sortBy, isDescending, page, pageSize, role, status);
            return Ok(ApiResult<Pagination<UserResponse>>.Success(result));
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get user details by id",
            Description = "Gets the details of a user by id. Requires Manager role.")]
        public async Task<IActionResult> GetUserByDetail(Guid id)
        {
            var result = await _userService.GetUserByDetail(id);
            return Ok(ApiResult<UserResponse>.Success(result));
        }

        [Authorize]
        [HttpGet("me")]
        [SwaggerOperation(
            Summary = "Get current user profile",
            Description = "Gets the profile of the current user. Requires authentication.")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var result = await _userService.GetCurrentUserAsync();
            return Ok(ApiResult<UserResponse>.Success(result));
        }

        [Authorize]
        [HttpPut("me")]
        [SwaggerOperation(
            Summary = "Update current user profile",
            Description = "Updates the profile of the current user. Requires authentication.")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            var result = await _userService.UpdateUserAsync(request);
            return Ok(ApiResult<UserResponse>.Success(result));
        }
    }
}
