namespace LuminaBox.Domain.Entities
{
    /// <summary>One conversation between a user and the AI assistant.</summary>
    public class ChatRoom : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        /// <summary>Set from the first question; the user can rename it.</summary>
        public string Title { get; set; } = string.Empty;

        public DateTime LastMessageAt { get; set; }

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
