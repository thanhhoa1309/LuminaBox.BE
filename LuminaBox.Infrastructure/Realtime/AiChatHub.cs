using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using LuminaBox.Application.Dtos.AiChat;
using LuminaBox.Application.Exceptions;
using LuminaBox.Application.Interface;

namespace LuminaBox.Infrastructure.Realtime
{
    /// <summary>
    /// AI chat over SignalR server-to-client streaming. FE: <c>connection.stream("StreamChat", request).subscribe(...)</c>;
    /// disposing the subscription cancels the turn (SignalR → gRPC → Ollama).
    /// </summary>
    [Authorize]
    public class AiChatHub : Hub
    {
        public const string Route = "/hubs/ai-chat";

        private readonly IAiChatService _aiChatService;

        public AiChatHub(IAiChatService aiChatService)
        {
            _aiChatService = aiChatService;
        }

        public IAsyncEnumerable<AiChatStreamEvent> StreamChat(AiChatRequest request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(Context.UserIdentifier, out var userId))
                throw new HubException("Unauthorized.");

            var role = Context.User?.FindFirstValue(ClaimTypes.Role);

            try
            {
                return _aiChatService.StreamChat(userId, role, request, cancellationToken);
            }
            catch (AppException ex)
            {
                // SignalR hides plain exception messages from clients; HubException is shown as-is.
                throw new HubException(ex.Message);
            }
        }
    }
}
