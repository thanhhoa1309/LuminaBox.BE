using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.AiChat
{
    public class ChatRoomResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime LastMessageAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ChatMessageResponse
    {
        public Guid Id { get; set; }
        public ChatMessageRole Role { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? GuideId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateChatRoomRequest
    {
        /// <summary>Optional; when empty the room is named after its first question.</summary>
        public string? Title { get; set; }
    }

    public class RenameChatRoomRequest
    {
        public string Title { get; set; } = string.Empty;
    }
}
