using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Interface
{
    // Events are queued and pushed by the next IUnitOfWork.SaveChangesAsync (or transaction commit),
    // so publish before saving; they are discarded when the transaction rolls back
    public interface IRealtimePublisher
    {
        void PublishToUser(Guid userId, string eventName, object payload);
        void PublishToUsers(IEnumerable<Guid> userIds, string eventName, object payload);
        void PublishToRole(UserRole role, string eventName, object payload);
        void PublishToAll(string eventName, object payload);
    }
}
