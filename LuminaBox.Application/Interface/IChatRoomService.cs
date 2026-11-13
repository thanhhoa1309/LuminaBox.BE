using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.AiChat;

namespace LuminaBox.Application.Interface
{
    /// <summary>The current user's AI chat rooms and their stored messages.</summary>
    public interface IChatRoomService
    {
        /// <summary>Most recently used first.</summary>
        Task<Pagination<ChatRoomResponse>> GetMyRoomsAsync(int page, int pageSize);

        Task<ChatRoomResponse> CreateAsync(CreateChatRoomRequest request);

        Task<ChatRoomResponse> RenameAsync(Guid roomId, RenameChatRoomRequest request);

        /// <summary>Soft delete of the room and its messages.</summary>
        Task DeleteAsync(Guid roomId);

        /// <summary>Page 1 = the newest messages; items inside a page are oldest first (ready to render).</summary>
        Task<Pagination<ChatMessageResponse>> GetMessagesAsync(Guid roomId, int page, int pageSize);

        /// <summary>
        /// Cuts the room back to before one of the user's questions: that question and every later message are
        /// hidden and left out of the AI history, but kept in the database. Returns how many messages were cut.
        /// </summary>
        Task<int> RollbackAsync(Guid roomId, Guid messageId);
    }
}
