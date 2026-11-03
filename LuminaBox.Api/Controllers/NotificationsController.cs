using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Notification;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get paginated list of my notifications",
            Description = "Gets a paginated list of the current user's in-app notifications with search, filter and sort. Requires authentication.")]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] bool isDescending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] NotificationType? type = null,
            [FromQuery] bool? isRead = null)
        {
            var result = await _notificationService.GetMyNotificationsAsync(search, sortBy, isDescending, page, pageSize, type, isRead);
            return Ok(ApiResult<Pagination<NotificationResponse>>.Success(result));
        }

        [HttpGet("unread-count")]
        [SwaggerOperation(
            Summary = "Get my unread notification count",
            Description = "Gets the number of unread in-app notifications of the current user. Requires authentication.")]
        public async Task<IActionResult> GetMyUnreadCount()
        {
            var result = await _notificationService.GetMyUnreadCountAsync();
            return Ok(ApiResult<NotificationUnreadCountResponse>.Success(result));
        }

        [HttpPut("{id:guid}/read")]
        [SwaggerOperation(
            Summary = "Mark a notification as read",
            Description = "Marks one of the current user's notifications as read. Requires authentication.")]
        public async Task<IActionResult> MarkNotificationAsRead(Guid id)
        {
            var result = await _notificationService.MarkNotificationAsReadAsync(id);
            return Ok(ApiResult<NotificationResponse>.Success(result));
        }

        [HttpPut("read")]
        [SwaggerOperation(
            Summary = "Mark notifications as read",
            Description = "Marks the selected or all notifications of the current user as read. Requires authentication.")]
        public async Task<IActionResult> MarkNotificationsAsRead([FromBody] MarkNotificationsReadRequest request)
        {
            var result = await _notificationService.MarkNotificationsAsReadAsync(request);
            return Ok(ApiResult<NotificationUnreadCountResponse>.Success(result));
        }
    }
}
