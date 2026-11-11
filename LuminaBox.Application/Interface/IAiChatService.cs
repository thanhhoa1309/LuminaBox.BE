using LuminaBox.Application.Dtos.AiChat;

namespace LuminaBox.Application.Interface
{
    public interface IAiChatService
    {
        /// <summary>
        /// Validates <paramref name="request"/> immediately (throws <c>ExceptionBadRequest</c>), then returns the
        /// event stream of one chat turn. Cancelling <paramref name="cancellationToken"/> stops the turn end to end.
        /// </summary>
        IAsyncEnumerable<AiChatStreamEvent> StreamChat(Guid userId, string? userRole, AiChatRequest request, CancellationToken cancellationToken);
    }
}
