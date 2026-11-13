using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    /// <summary>
    /// One message of a chat room. Only the final text is stored: the model's thinking and
    /// intermediate tool steps are streamed but never persisted.
    /// </summary>
    public class ChatMessage : BaseEntity
    {
        public Guid ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; } = null!;

        public ChatMessageRole Role { get; set; }
        public string Content { get; set; } = string.Empty;

        /// <summary>Screen the assistant opened for this answer (guide id), shown again when the history is reloaded.</summary>
        public string? GuideId { get; set; }

        /// <summary>
        /// Set when the user rolled the room back to an earlier question: the message stays in the database
        /// but is no longer shown nor sent to the AI as history.
        /// </summary>
        public DateTime? RolledBackAt { get; set; }
    }
}
