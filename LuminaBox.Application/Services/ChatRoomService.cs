using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.AiChat;
using LuminaBox.Application.Exceptions;
using LuminaBox.Application.Interface;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class ChatRoomService : IChatRoomService
    {
        public const string DefaultTitle = "Cuộc trò chuyện mới";
        public const int MaxTitleLength = 100;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;
        private readonly ICurrentTime _currentTime;

        public ChatRoomService(IUnitOfWork unitOfWork, IClaimsService claimsService, ICurrentTime currentTime)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
            _currentTime = currentTime;
        }

        public async Task<Pagination<ChatRoomResponse>> GetMyRoomsAsync(int page, int pageSize)
        {
            var userId = _claimsService.GetRequiredUserId();
            var pagination = new PaginationParameter { PageIndex = page, PageSize = pageSize };

            var query = _unitOfWork.ChatRooms.GetQueryable()
                .AsNoTracking()
                .Where(r => r.UserId == userId);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.LastMessageAt)
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(r => ToResponse(r))
                .ToListAsync();

            return new Pagination<ChatRoomResponse>(items, totalCount, pagination.PageIndex, pagination.PageSize);
        }

        public async Task<ChatRoomResponse> CreateAsync(CreateChatRoomRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();
            var room = new ChatRoom
            {
                UserId = userId,
                Title = CleanTitle(request.Title) ?? DefaultTitle,
                LastMessageAt = _currentTime.GetCurrentTime(),
            };

            await _unitOfWork.ChatRooms.AddAsync(room);
            await _unitOfWork.SaveChangesAsync();
            return ToResponse(room);
        }

        public async Task<ChatRoomResponse> RenameAsync(Guid roomId, RenameChatRoomRequest request)
        {
            var room = await GetOwnedRoomAsync(roomId);
            room.Title = CleanTitle(request.Title) ?? throw new ExceptionBadRequest($"Title must have 1 to {MaxTitleLength} characters.");

            await _unitOfWork.ChatRooms.Update(room);
            await _unitOfWork.SaveChangesAsync();
            return ToResponse(room);
        }

        public async Task DeleteAsync(Guid roomId)
        {
            var room = await GetOwnedRoomAsync(roomId);
            var messages = await _unitOfWork.ChatMessages.GetAllAsync(m => m.ChatRoomId == room.Id);

            if (messages.Count > 0)
                await _unitOfWork.ChatMessages.SoftRemoveRange(messages);
            await _unitOfWork.ChatRooms.SoftRemove(room);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Pagination<ChatMessageResponse>> GetMessagesAsync(Guid roomId, int page, int pageSize)
        {
            var room = await GetOwnedRoomAsync(roomId);
            var pagination = new PaginationParameter { PageIndex = page, PageSize = pageSize };

            var query = _unitOfWork.ChatMessages.GetQueryable()
                .AsNoTracking()
                .Where(m => m.ChatRoomId == room.Id && m.RolledBackAt == null);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(m => new ChatMessageResponse
                {
                    Id = m.Id,
                    Role = m.Role,
                    Content = m.Content,
                    GuideId = m.GuideId,
                    CreatedAt = m.CreatedAt,
                })
                .ToListAsync();

            items.Reverse();
            return new Pagination<ChatMessageResponse>(items, totalCount, pagination.PageIndex, pagination.PageSize);
        }

        public async Task<int> RollbackAsync(Guid roomId, Guid messageId)
        {
            var room = await GetOwnedRoomAsync(roomId);
            var target = await _unitOfWork.ChatMessages.FirstOrDefaultAsync(m =>
                    m.Id == messageId && m.ChatRoomId == room.Id && m.RolledBackAt == null)
                ?? throw new ExceptionNotFound("Message not found.");
            if (target.Role != ChatMessageRole.User)
                throw new ExceptionBadRequest("Only a question can be rolled back.");

            var cut = await _unitOfWork.ChatMessages.GetAllAsync(m =>
                m.ChatRoomId == room.Id && m.RolledBackAt == null && m.CreatedAt >= target.CreatedAt);
            var now = _currentTime.GetCurrentTime();
            foreach (var message in cut)
                message.RolledBackAt = now;

            await _unitOfWork.ChatMessages.UpdateRange(cut);
            await _unitOfWork.SaveChangesAsync();
            return cut.Count;
        }

        private async Task<ChatRoom> GetOwnedRoomAsync(Guid roomId)
        {
            var userId = _claimsService.GetRequiredUserId();
            return await _unitOfWork.ChatRooms.FirstOrDefaultAsync(r => r.Id == roomId && r.UserId == userId)
                ?? throw new ExceptionNotFound("Chat room not found.");
        }

        /// <summary>Trimmed, one line, cut to <see cref="MaxTitleLength"/>; null when nothing is left.</summary>
        public static string? CleanTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return null;
            var oneLine = string.Join(' ', title.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            return oneLine.Length <= MaxTitleLength ? oneLine : oneLine[..(MaxTitleLength - 1)].TrimEnd() + "…";
        }

        private static ChatRoomResponse ToResponse(ChatRoom room) => new()
        {
            Id = room.Id,
            Title = room.Title,
            LastMessageAt = room.LastMessageAt,
            CreatedAt = room.CreatedAt,
        };
    }
}
