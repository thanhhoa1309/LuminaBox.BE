using Grpc.Core;
using LuminaBox.AiService.Chat;
using LuminaBox.Contracts.AiChat;

namespace LuminaBox.AiService.Grpc;

/// <summary>gRPC entry point: validates the request and pipes orchestrator events into the response stream.</summary>
public sealed class AiChatGrpcService : AiChat.AiChatBase
{
    private readonly ChatOrchestrator _orchestrator;

    public AiChatGrpcService(ChatOrchestrator orchestrator) => _orchestrator = orchestrator;

    public override Task StreamChat(ChatRequest request, IServerStreamWriter<ChatEvent> responseStream, ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.TurnId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "turn_id is required."));
        if (request.Messages.Count == 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "messages must not be empty."));

        // context.CancellationToken fires when the BE cancels the call or its deadline passes;
        // it flows down to the Ollama HTTP request, so generation stops too.
        return _orchestrator.RunAsync(
            request,
            e => responseStream.WriteAsync(e, context.CancellationToken),
            context.CancellationToken);
    }
}
