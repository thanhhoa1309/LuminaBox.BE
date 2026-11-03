using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Notification;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Notifications;
using LuminaBox.Application.Realtime;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class NotificationService : INotificationService
    {
        // Managers are shown as a team to non-manager recipients
        private const string ManagerDisplayName = "Ban quản lý";
        private const string SystemDisplayName = "Hệ thống";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly ICurrentTime _currentTime;
        private readonly IRealtimePublisher _realtimePublisher;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IClaimsService claimsService,
            ICurrentTime currentTime,
            IRealtimePublisher realtimePublisher)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _currentTime = currentTime;
            _realtimePublisher = realtimePublisher;
        }

        public async Task<Pagination<NotificationResponse>> GetMyNotificationsAsync(
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            NotificationType? type = null,
            bool? isRead = null)
        {
            var userId = _claimsService.GetRequiredUserId();
            var viewerIsManager = await IsManagerAsync(userId);

            var pagination = new PaginationParameter { PageIndex = page, PageSize = pageSize };

            // Email rows are only a delivery log; the in-app inbox is the Realtime channel
            var query = _unitOfWork.Notifications.GetQueryable()
                .AsNoTracking()
                .Where(n => n.UserId == userId && n.Channel == NotificationChannel.Realtime);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(n =>
                    n.Title.ToLower().Contains(keyword) ||
                    (n.Content != null && n.Content.ToLower().Contains(keyword)));
            }

            if (type.HasValue)
                query = query.Where(n => n.Type == type.Value);

            if (isRead.HasValue)
                query = query.Where(n => n.IsRead == isRead.Value);

            var totalCount = await query.CountAsync();

            query = sortBy?.Trim().ToLowerInvariant() switch
            {
                "title" => isDescending ? query.OrderByDescending(n => n.Title) : query.OrderBy(n => n.Title),
                "type" => isDescending ? query.OrderByDescending(n => n.Type) : query.OrderBy(n => n.Type),
                "isread" => isDescending ? query.OrderByDescending(n => n.IsRead) : query.OrderBy(n => n.IsRead),
                _ => isDescending ? query.OrderByDescending(n => n.CreatedAt) : query.OrderBy(n => n.CreatedAt)
            };

            var items = await query
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(n => new NotificationResponse
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    OrderId = n.OrderId,
                    ActorUserId = n.ActorUser != null && n.ActorUser.Role == UserRole.Manager && !viewerIsManager
                        ? null
                        : n.ActorUserId,
                    ActorName = n.ActorUser == null
                        ? null
                        : n.ActorUser.Role == UserRole.Manager && !viewerIsManager
                            ? ManagerDisplayName
                            : n.ActorUser.FullName,
                    EntityType = n.EntityType,
                    EntityId = n.EntityId,
                    Type = n.Type,
                    Channel = n.Channel,
                    Title = n.Title,
                    Content = n.Content,
                    IsRead = n.IsRead,
                    SendStatus = n.SendStatus,
                    SentAt = n.SentAt,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            return new Pagination<NotificationResponse>(items, totalCount, pagination.PageIndex, pagination.PageSize);
        }

        public async Task<NotificationUnreadCountResponse> GetMyUnreadCountAsync()
        {
            var userId = _claimsService.GetRequiredUserId();

            return new NotificationUnreadCountResponse { UnreadCount = await CountUnreadAsync(userId) };
        }

        public async Task<NotificationResponse> MarkNotificationAsReadAsync(Guid id)
        {
            var userId = _claimsService.GetRequiredUserId();

            // Another user's notification is reported as missing so its existence is not leaked
            var notification = await _unitOfWork.Notifications.FirstOrDefaultAsync(n =>
                    n.Id == id && n.UserId == userId && n.Channel == NotificationChannel.Realtime)
                ?? throw ErrorHelper.NotFound("Notification not found.");

            if (!notification.IsRead)
            {
                // Counted before saving, so the database still includes this notification
                var unreadCount = await CountUnreadAsync(userId) - 1;

                notification.IsRead = true;

                await _unitOfWork.Notifications.Update(notification);

                // Keeps the badge of the user's other tabs and devices in sync
                _realtimePublisher.PublishToUser(userId, RealtimeEvents.NotificationUnreadCountChanged,
                    new NotificationUnreadCountResponse { UnreadCount = unreadCount });

                await _unitOfWork.SaveChangesAsync();
            }

            var viewerIsManager = await IsManagerAsync(userId);

            return await _unitOfWork.Notifications.GetQueryable()
                .AsNoTracking()
                .Where(n => n.Id == id)
                .Select(n => new NotificationResponse
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    OrderId = n.OrderId,
                    ActorUserId = n.ActorUser != null && n.ActorUser.Role == UserRole.Manager && !viewerIsManager
                        ? null
                        : n.ActorUserId,
                    ActorName = n.ActorUser == null
                        ? null
                        : n.ActorUser.Role == UserRole.Manager && !viewerIsManager
                            ? ManagerDisplayName
                            : n.ActorUser.FullName,
                    EntityType = n.EntityType,
                    EntityId = n.EntityId,
                    Type = n.Type,
                    Channel = n.Channel,
                    Title = n.Title,
                    Content = n.Content,
                    IsRead = n.IsRead,
                    SendStatus = n.SendStatus,
                    SentAt = n.SentAt,
                    CreatedAt = n.CreatedAt
                })
                .FirstAsync();
        }

        public async Task<NotificationUnreadCountResponse> MarkNotificationsAsReadAsync(MarkNotificationsReadRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();

            var query = _unitOfWork.Notifications.GetQueryable()
                .Where(n => n.UserId == userId && n.Channel == NotificationChannel.Realtime && !n.IsRead);

            // Ids that are unknown, already read or owned by someone else are skipped
            if (!request.MarkAll)
            {
                var ids = request.NotificationIds!.Distinct().ToList();
                query = query.Where(n => ids.Contains(n.Id));
            }

            var notifications = await query.ToListAsync();

            // Counted before saving, so the database still includes the notifications being marked
            var response = new NotificationUnreadCountResponse
            {
                UnreadCount = await CountUnreadAsync(userId) - notifications.Count
            };

            if (notifications.Count > 0)
            {
                foreach (var notification in notifications)
                    notification.IsRead = true;

                await _unitOfWork.Notifications.UpdateRange(notifications);

                _realtimePublisher.PublishToUser(userId, RealtimeEvents.NotificationUnreadCountChanged, response);

                await _unitOfWork.SaveChangesAsync();
            }

            return response;
        }

        // Only stages the rows; the caller commits them together with its own business changes
        public async Task SendAsync(NotificationCommand command)
        {
            var actorId = _claimsService.GetCurrentUserId;

            var actor = actorId == Guid.Empty
                ? null
                : await _unitOfWork.Users.GetQueryable()
                    .AsNoTracking()
                    .Where(u => u.Id == actorId)
                    .Select(u => new { u.Id, u.FullName, u.Role })
                    .FirstOrDefaultAsync();

            var recipients = command.Audience.Recipients.ToList();

            if (command.Audience.IncludeManagers)
            {
                var managerIds = await _unitOfWork.Users.GetQueryable()
                    .AsNoTracking()
                    .Where(u => u.Role == UserRole.Manager && u.Status == UserStatus.Active && u.Id != actorId)
                    .Select(u => u.Id)
                    .ToListAsync();

                recipients.AddRange(managerIds.Select(id => new NotificationRecipient(id, NotificationRecipientRole.Manager)));
            }

            var now = _currentTime.GetCurrentTime();
            var notifications = new List<Notification>();
            var realtimeNotifications = new List<(Notification Notification, Guid? ActorUserId, string? ActorName)>();

            foreach (var recipient in recipients.Distinct())
            {
                var template = command.Templates.Get(recipient.Role);
                if (template == null)
                    continue;

                var hideActor = actor?.Role == UserRole.Manager && recipient.Role != NotificationRecipientRole.Manager;

                var actorName = actor == null
                    ? SystemDisplayName
                    : hideActor
                        ? ManagerDisplayName
                        : actor.FullName;

                var tokens = new Dictionary<string, string>(command.Tokens)
                {
                    [NotificationTokenKeys.ActorName] = actorName
                };

                var title = NotificationTemplateRenderer.Render(template.Title, tokens);
                var content = NotificationTemplateRenderer.Render(template.Content, tokens);

                foreach (var channel in template.Channels)
                {
                    // Realtime rows are delivered once stored; Email rows wait for the email worker
                    var isRealtime = channel == NotificationChannel.Realtime;

                    var notification = new Notification
                    {
                        UserId = recipient.UserId,
                        OrderId = command.OrderId,
                        ActorUserId = actor?.Id,
                        EntityType = command.EntityType,
                        EntityId = command.EntityId,
                        Type = command.Type,
                        Channel = channel,
                        Title = title,
                        Content = content,
                        IsRead = false,
                        SendStatus = isRealtime ? SendStatus.Sent : SendStatus.Pending,
                        SentAt = isRealtime ? now : null
                    };

                    notifications.Add(notification);

                    // Same actor masking as the inbox API so the pushed item matches what the list returns
                    if (isRealtime)
                        realtimeNotifications.Add((notification, hideActor ? null : actor?.Id, actor == null ? null : actorName));
                }
            }

            if (notifications.Count == 0)
                return;

            await _unitOfWork.Notifications.AddRangeAsync(notifications);

            // Ids and audit fields are only assigned once the rows are tracked
            foreach (var (notification, actorUserId, actorName) in realtimeNotifications)
            {
                _realtimePublisher.PublishToUser(notification.UserId, RealtimeEvents.NotificationReceived, new NotificationResponse
                {
                    Id = notification.Id,
                    UserId = notification.UserId,
                    OrderId = notification.OrderId,
                    ActorUserId = actorUserId,
                    ActorName = actorName,
                    EntityType = notification.EntityType,
                    EntityId = notification.EntityId,
                    Type = notification.Type,
                    Channel = notification.Channel,
                    Title = notification.Title,
                    Content = notification.Content,
                    IsRead = notification.IsRead,
                    SendStatus = notification.SendStatus,
                    SentAt = notification.SentAt,
                    CreatedAt = notification.CreatedAt
                });
            }
        }

        private Task<int> CountUnreadAsync(Guid userId)
            => _unitOfWork.Notifications.GetQueryable()
                .CountAsync(n => n.UserId == userId && n.Channel == NotificationChannel.Realtime && !n.IsRead);

        private Task<bool> IsManagerAsync(Guid userId)
            => _unitOfWork.Users.GetQueryable()
                .AnyAsync(u => u.Id == userId && u.Role == UserRole.Manager);
    }
}
