using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.AiChat;
using LuminaBox.Application.Exceptions;
using LuminaBox.Application.Interface;
using Microsoft.Extensions.Options;

namespace LuminaBox.Application.Services
{
    public class AiChatService : IAiChatService
    {
        public const int MaxMessages = 40;
        public const int MaxMessageLength = 4000;

        private readonly IAiChatClient _client;
        private readonly AiChatSettings _settings;

        public AiChatService(IAiChatClient client, IOptions<AiChatSettings> settings)
        {
            _client = client;
            _settings = settings.Value;
        }

        public IAsyncEnumerable<AiChatStreamEvent> StreamChat(Guid userId, string? userRole, AiChatRequest request, CancellationToken cancellationToken)
        {
            // Not an iterator on purpose: validation errors surface when the hub calls this method,
            // before the stream starts, so the client gets a clear error instead of a broken stream.
            Validate(request);

            var turn = new AiChatTurn(
                Guid.NewGuid().ToString("N"), userId, userRole, request.CurrentRoute,
                request.Messages, request.EnableThinking);
            return CoalesceAsync(_client.StreamChatAsync(turn, cancellationToken), turn.TurnId, cancellationToken);
        }

        private static void Validate(AiChatRequest request)
        {
            if (request.Messages is null || request.Messages.Count == 0)
                throw new ExceptionBadRequest("Messages must not be empty.");
            if (request.Messages.Count > MaxMessages)
                throw new ExceptionBadRequest($"At most {MaxMessages} messages per request.");
            if (request.Messages[^1].Role != AiChatRole.User)
                throw new ExceptionBadRequest("The last message must be from the user.");
            if (request.Messages.Any(m => string.IsNullOrWhiteSpace(m.Content) || m.Content.Length > MaxMessageLength))
                throw new ExceptionBadRequest($"Each message must have 1 to {MaxMessageLength} characters.");
            if (request.CurrentRoute is { } route && (route.Length > 200 || !route.StartsWith('/')))
                throw new ExceptionBadRequest("CurrentRoute must be a path starting with '/' (max 200 characters).");
        }

        /// <summary>
        /// Merges consecutive Thinking / Content deltas that arrive within <see cref="AiChatSettings.DeltaFlushMilliseconds"/>
        /// so the browser gets a few messages per second instead of one per token. Re-numbers <c>Seq</c>.
        /// </summary>
        private async IAsyncEnumerable<AiChatStreamEvent> CoalesceAsync(
            IAsyncEnumerable<AiChatStreamEvent> source,
            string turnId,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var seq = 0;
            var buffer = new StringBuilder();
            AiChatEventType? bufferType = null;
            var bufferAge = new Stopwatch();
            var flushAfter = TimeSpan.FromMilliseconds(Math.Max(0, _settings.DeltaFlushMilliseconds));

            AiChatStreamEvent Flush()
            {
                var e = new AiChatStreamEvent { TurnId = turnId, Seq = ++seq, Type = bufferType!.Value, Text = buffer.ToString() };
                buffer.Clear();
                bufferType = null;
                return e;
            }

            await foreach (var e in source.WithCancellation(cancellationToken))
            {
                var isDelta = e.Type is AiChatEventType.Thinking or AiChatEventType.Content;

                // Different kind of event → send what is buffered first to keep the order.
                if (bufferType is not null && (!isDelta || e.Type != bufferType))
                    yield return Flush();

                if (!isDelta)
                {
                    e.TurnId = turnId;
                    e.Seq = ++seq;
                    yield return e;
                    continue;
                }

                if (bufferType is null)
                {
                    bufferType = e.Type;
                    bufferAge.Restart();
                }
                buffer.Append(e.Text);

                if (bufferAge.Elapsed >= flushAfter)
                    yield return Flush();
            }

            // After a cancel the client is gone: drop the leftover text instead of sending it.
            if (bufferType is not null && !cancellationToken.IsCancellationRequested)
                yield return Flush();
        }
    }
}
