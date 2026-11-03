using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Realtime;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Infrastructure.Realtime
{
    // Scoped buffer flushed by UnitOfWork once the data is committed
    public class SignalRRealtimePublisher : IRealtimePublisher
    {
        private readonly IHubContext<AppHub> _hubContext;
        private readonly ILogger<SignalRRealtimePublisher> _logger;
        private readonly List<PendingEvent> _pendingEvents = new();

        public SignalRRealtimePublisher(IHubContext<AppHub> hubContext, ILogger<SignalRRealtimePublisher> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public void PublishToUser(Guid userId, string eventName, object payload)
            => _pendingEvents.Add(new PendingEvent(clients => clients.User(userId.ToString()), eventName, payload));

        public void PublishToUsers(IEnumerable<Guid> userIds, string eventName, object payload)
        {
            var ids = userIds.Distinct().Select(id => id.ToString()).ToList();
            if (ids.Count == 0)
                return;

            _pendingEvents.Add(new PendingEvent(clients => clients.Users(ids), eventName, payload));
        }

        public void PublishToRole(UserRole role, string eventName, object payload)
            => _pendingEvents.Add(new PendingEvent(clients => clients.Group(RealtimeGroups.Role(role)), eventName, payload));

        public void PublishToAll(string eventName, object payload)
            => _pendingEvents.Add(new PendingEvent(clients => clients.All, eventName, payload));

        public async Task FlushAsync()
        {
            if (_pendingEvents.Count == 0)
                return;

            var events = _pendingEvents.ToList();
            _pendingEvents.Clear();

            foreach (var pendingEvent in events)
            {
                // Delivery is best-effort: the data is already committed and clients resync through the REST API
                try
                {
                    await pendingEvent.Target(_hubContext.Clients).SendAsync(pendingEvent.EventName, pendingEvent.Payload);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to push realtime event {EventName}.", pendingEvent.EventName);
                }
            }
        }

        public void Clear() => _pendingEvents.Clear();

        private sealed record PendingEvent(Func<IHubClients, IClientProxy> Target, string EventName, object Payload);
    }
}
