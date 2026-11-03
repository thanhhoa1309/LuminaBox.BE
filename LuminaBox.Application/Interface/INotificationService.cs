using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Notification;
using LuminaBox.Application.Notifications;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Interface
{
    public interface INotificationService
    {
        Task<Pagination<NotificationResponse>> GetMyNotificationsAsync(
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            NotificationType? type = null,
            bool? isRead = null);
        Task<NotificationUnreadCountResponse> GetMyUnreadCountAsync();
        Task<NotificationResponse> MarkNotificationAsReadAsync(Guid id);
        Task<NotificationUnreadCountResponse> MarkNotificationsAsReadAsync(MarkNotificationsReadRequest request);
        Task SendAsync(NotificationCommand command);
    }
}
