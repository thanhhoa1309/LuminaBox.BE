using LuminaBox.Application.Dtos.AiChat;

namespace LuminaBox.Application.Interface
{
    /// <summary>
    /// Transport to LuminaBox.AiService (gRPC in Infrastructure). Yields raw events as they arrive;
    /// transport failures are turned into a final <see cref="AiChatEventType.Error"/> event instead of throwing.
    /// </summary>
    public interface IAiChatClient
    {
        IAsyncEnumerable<AiChatStreamEvent> StreamChatAsync(AiChatTurn turn, CancellationToken cancellationToken);
    }
}
