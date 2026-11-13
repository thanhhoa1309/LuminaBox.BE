using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.AiChat;
using LuminaBox.Application.Exceptions;
using LuminaBox.Application.Interface;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LuminaBox.Application.Services
{
    public class AiChatService : IAiChatService
    {
        /// <summary>Stored messages sent back to the model as context (newest ones).</summary>
        public const int HistoryMessages = 20;
        public const int MaxMessageLength = 4000;
        public const int RoomTitleLength = 60;
        // Guide texts go into the model's tool description, so they are capped to keep the prompt small.
        public const int MaxGuides = 30;
        public const int MaxGuideParams = 3;
        public const int MaxGuideLabelLength = 60;
        public const int MaxGuideDescriptionLength = 200;

        private static bool IsIdentifier(string? value, int maxLength)
            => !string.IsNullOrEmpty(value) && value.Length <= maxLength
               && value.All(c => c is (>= 'a' and <= 'z') or (>= '0' and <= '9') or '_');

        private static bool IsText(string? value, int maxLength)
            => !string.IsNullOrWhiteSpace(value) && value.Length <= maxLength && !value.Any(char.IsControl);

        private readonly IAiChatClient _client;
        private readonly AiChatSettings _settings;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentTime _currentTime;

        public AiChatService(IAiChatClient client, IOptions<AiChatSettings> settings, IUnitOfWork unitOfWork, ICurrentTime currentTime)
        {
            _client = client;
            _settings = settings.Value;
            _unitOfWork = unitOfWork;
            _currentTime = currentTime;
        }

        public IAsyncEnumerable<AiChatStreamEvent> StreamChat(Guid userId, string? userRole, AiChatRequest request, CancellationToken cancellationToken)
        {
            // Not an iterator on purpose: validation errors surface when the hub calls this method,
            // before the stream starts, so the client gets a clear error instead of a broken stream.
            Validate(request);
            return RunTurnAsync(userId, userRole, request, cancellationToken);
        }

        /// <summary>
        /// Loads the room history, stores the question, streams the answer and stores the final text on <c>Done</c>
        /// (content after the last <c>RoundReset</c>; thinking is never stored). A cancelled or failed turn stores no answer.
        /// </summary>
        private async IAsyncEnumerable<AiChatStreamEvent> RunTurnAsync(
            Guid userId, string? userRole, AiChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var turnId = Guid.NewGuid().ToString("N");
            var room = await _unitOfWork.ChatRooms.FirstOrDefaultAsync(r => r.Id == request.RoomId && r.UserId == userId);
            if (room is null)
            {
                yield return new AiChatStreamEvent
                {
                    TurnId = turnId, Seq = 1, Type = AiChatEventType.Error,
                    ErrorCode = "room_not_found", ErrorMessage = "Không tìm thấy cuộc trò chuyện.",
                };
                yield break;
            }

            var history = await _unitOfWork.ChatMessages.GetQueryable()
                .AsNoTracking()
                .Where(m => m.ChatRoomId == room.Id && m.RolledBackAt == null)
                .OrderByDescending(m => m.CreatedAt)
                .Take(HistoryMessages)
                .Select(m => new AiChatMessageDto
                {
                    Role = m.Role == ChatMessageRole.Assistant ? AiChatRole.Assistant : AiChatRole.User,
                    Content = m.Content,
                    GuideId = m.GuideId,
                })
                .ToListAsync(cancellationToken);
            history.Reverse();

            var question = request.Message.Trim();
            await _unitOfWork.ChatMessages.AddAsync(new ChatMessage { ChatRoomId = room.Id, Role = ChatMessageRole.User, Content = question });
            if (room.Title == ChatRoomService.DefaultTitle)
                room.Title = ChatRoomService.CleanTitle(question.Length <= RoomTitleLength ? question : question[..RoomTitleLength] + "…") ?? room.Title;
            room.LastMessageAt = _currentTime.GetCurrentTime();
            await _unitOfWork.ChatRooms.Update(room);
            await _unitOfWork.SaveChangesAsync();

            history.Add(new AiChatMessageDto { Role = AiChatRole.User, Content = question });
            var turn = new AiChatTurn(
                turnId, userId, userRole, request.CurrentRoute,
                request.Guides ?? new List<AiGuideOptionDto>(), history, request.EnableThinking);

            var answer = new StringBuilder();
            string? guideId = null;
            await foreach (var e in CoalesceAsync(_client.StreamChatAsync(turn, cancellationToken), turnId, cancellationToken))
            {
                switch (e.Type)
                {
                    case AiChatEventType.Content:
                        answer.Append(e.Text);
                        break;
                    case AiChatEventType.RoundReset:
                        answer.Clear();
                        break;
                    case AiChatEventType.UiAction when e.GuideId is not null:
                        guideId = e.GuideId;
                        break;
                    case AiChatEventType.Done:
                        e.MessageId = await SaveAnswerAsync(room, answer.ToString().Trim(), guideId);
                        break;
                }
                yield return e;
            }
        }

        private async Task<Guid?> SaveAnswerAsync(ChatRoom room, string answer, string? guideId)
        {
            if (answer.Length == 0 && guideId is null)
                return null;

            var message = new ChatMessage { ChatRoomId = room.Id, Role = ChatMessageRole.Assistant, Content = answer, GuideId = guideId };
            await _unitOfWork.ChatMessages.AddAsync(message);
            room.LastMessageAt = _currentTime.GetCurrentTime();
            await _unitOfWork.ChatRooms.Update(room);
            await _unitOfWork.SaveChangesAsync();
            return message.Id;
        }

        private static void Validate(AiChatRequest request)
        {
            if (request.RoomId == Guid.Empty)
                throw new ExceptionBadRequest("RoomId is required.");
            if (string.IsNullOrWhiteSpace(request.Message) || request.Message.Length > MaxMessageLength)
                throw new ExceptionBadRequest($"Message must have 1 to {MaxMessageLength} characters.");
            if (request.CurrentRoute is { } route && (route.Length > 200 || !route.StartsWith('/')))
                throw new ExceptionBadRequest("CurrentRoute must be a path starting with '/' (max 200 characters).");
            if (request.Guides is { } guides)
                ValidateGuides(guides);
        }

        private static void ValidateGuides(List<AiGuideOptionDto> guides)
        {
            if (guides.Count > MaxGuides)
                throw new ExceptionBadRequest($"Guides: at most {MaxGuides} items.");
            if (guides.Select(g => g.Id).Distinct().Count() != guides.Count)
                throw new ExceptionBadRequest("Guides: ids must be unique.");

            foreach (var guide in guides)
            {
                if (!IsIdentifier(guide.Id, 64))
                    throw new ExceptionBadRequest("Guides: id must be lower-case letters, digits and '_' (max 64).");
                if (!IsText(guide.Label, MaxGuideLabelLength) || !IsText(guide.Description, MaxGuideDescriptionLength))
                    throw new ExceptionBadRequest($"Guides: label (max {MaxGuideLabelLength}) and description (max {MaxGuideDescriptionLength}) are required, one line.");
                if (guide.Params is null || guide.Params.Count > MaxGuideParams)
                    throw new ExceptionBadRequest($"Guides: at most {MaxGuideParams} params.");
                if (guide.Params.Any(p => !IsIdentifier(p.Name, 32) || !IsText(p.Description, MaxGuideDescriptionLength)))
                    throw new ExceptionBadRequest("Guides: each param needs a name ([a-z0-9_], max 32) and a description.");
            }
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
