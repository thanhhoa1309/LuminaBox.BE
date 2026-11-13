using System.Runtime.CompilerServices;
using Grpc.Core;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.AiChat;
using LuminaBox.Application.Interface;
using LuminaBox.Contracts.AiChat;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LuminaBox.Infrastructure.Service
{
    /// <summary>gRPC client of LuminaBox.AiService: maps DTOs ⇄ proto messages and turns RPC failures into an Error event.</summary>
    public class GrpcAiChatClient : IAiChatClient
    {
        private readonly AiChat.AiChatClient _grpc;
        private readonly AiChatSettings _settings;
        private readonly ILogger<GrpcAiChatClient> _logger;

        public GrpcAiChatClient(AiChat.AiChatClient grpc, IOptions<AiChatSettings> settings, ILogger<GrpcAiChatClient> logger)
        {
            _grpc = grpc;
            _settings = settings.Value;
            _logger = logger;
        }

        public async IAsyncEnumerable<AiChatStreamEvent> StreamChatAsync(
            AiChatTurn turn,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var request = new ChatRequest
            {
                TurnId = turn.TurnId,
                UserId = turn.UserId.ToString(),
                EnableThinking = turn.EnableThinking,
                UserRole = turn.UserRole ?? string.Empty,
                ClientRoute = turn.ClientRoute ?? string.Empty,
            };
            foreach (var guide in turn.Guides)
            {
                var option = new GuideOption { Id = guide.Id, Label = guide.Label, Description = guide.Description };
                option.Params.AddRange(guide.Params.Select(p => new GuideParam { Name = p.Name, Description = p.Description }));
                request.Guides.Add(option);
            }
            request.Messages.AddRange(turn.Messages.Select(m => new ChatMessage
            {
                Role = m.Role == AiChatRole.Assistant ? ChatRole.Assistant : ChatRole.User,
                Content = m.Content,
                GuideId = m.GuideId ?? string.Empty,
            }));

            var headers = new Metadata();
            if (!string.IsNullOrEmpty(_settings.ApiKey))
                headers.Add("x-api-key", _settings.ApiKey);

            using var call = _grpc.StreamChat(
                request,
                headers,
                deadline: DateTime.UtcNow.AddSeconds(_settings.TurnTimeoutSeconds),
                cancellationToken: cancellationToken);

            var stream = call.ResponseStream;
            AiChatStreamEvent? failure = null;

            while (true)
            {
                // C# cannot yield inside try/catch, so only MoveNext is guarded.
                try
                {
                    if (!await stream.MoveNext(cancellationToken))
                        break;
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled && cancellationToken.IsCancellationRequested)
                {
                    yield break; // the user stopped the turn
                }
                catch (RpcException ex)
                {
                    _logger.LogWarning(ex, "AI service call failed for turn {TurnId}: {Status}", turn.TurnId, ex.StatusCode);
                    failure = Error(turn.TurnId, ex.StatusCode switch
                    {
                        StatusCode.DeadlineExceeded => ("ai_timeout", "The assistant took too long to answer."),
                        StatusCode.Unavailable => ("ai_unavailable", "The assistant is offline. Please try again later."),
                        _ => ("ai_error", "The assistant could not answer."),
                    });
                    break;
                }
                if (Map(stream.Current) is { } e)
                    yield return e;
            }

            if (failure is not null)
                yield return failure;
        }

        private static AiChatStreamEvent? Map(ChatEvent e)
        {
            var dto = new AiChatStreamEvent { TurnId = e.TurnId, Seq = e.Seq };

            switch (e.PayloadCase)
            {
                case ChatEvent.PayloadOneofCase.Thinking:
                    dto.Type = AiChatEventType.Thinking;
                    dto.Text = e.Thinking.Text;
                    break;
                case ChatEvent.PayloadOneofCase.Content:
                    dto.Type = AiChatEventType.Content;
                    dto.Text = e.Content.Text;
                    break;
                case ChatEvent.PayloadOneofCase.ToolStarted:
                    dto.Type = AiChatEventType.ToolStarted;
                    dto.ToolCallId = e.ToolStarted.CallId;
                    dto.ToolName = e.ToolStarted.Name;
                    dto.DisplayText = e.ToolStarted.DisplayText;
                    break;
                case ChatEvent.PayloadOneofCase.ToolFinished:
                    dto.Type = AiChatEventType.ToolFinished;
                    dto.ToolCallId = e.ToolFinished.CallId;
                    dto.ToolName = e.ToolFinished.Name;
                    dto.Success = e.ToolFinished.Success;
                    break;
                case ChatEvent.PayloadOneofCase.Done:
                    dto.Type = AiChatEventType.Done;
                    dto.FinishReason = e.Done.FinishReason;
                    dto.PromptTokens = e.Done.PromptTokens;
                    dto.CompletionTokens = e.Done.CompletionTokens;
                    break;
                case ChatEvent.PayloadOneofCase.Error:
                    dto.Type = AiChatEventType.Error;
                    dto.ErrorCode = e.Error.Code;
                    dto.ErrorMessage = e.Error.Message;
                    break;
                case ChatEvent.PayloadOneofCase.UiAction:
                    dto.Type = AiChatEventType.UiAction;
                    dto.ActionType = e.UiAction.Type;
                    dto.GuideId = e.UiAction.GuideId;
                    dto.GuideParams = e.UiAction.Params.Count > 0
                        ? new Dictionary<string, string>(e.UiAction.Params)
                        : null;
                    break;
                case ChatEvent.PayloadOneofCase.RoundReset:
                    dto.Type = AiChatEventType.RoundReset;
                    dto.ResetReason = e.RoundReset.Reason;
                    break;
                default:
                    return null; // unknown payload from a newer AI service: skip
            }

            return dto;
        }

        private static AiChatStreamEvent Error(string turnId, (string Code, string Message) error) => new()
        {
            TurnId = turnId,
            Type = AiChatEventType.Error,
            ErrorCode = error.Code,
            ErrorMessage = error.Message,
        };
    }
}
