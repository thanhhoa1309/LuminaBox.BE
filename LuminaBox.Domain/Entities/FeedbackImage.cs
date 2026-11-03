namespace LuminaBox.Domain.Entities
{
    public class FeedbackImage : BaseEntity
    {
        public Guid FeedbackId { get; set; }
        public Feedback Feedback { get; set; } = null!;

        public string ImageUrl { get; set; } = string.Empty;
    }
}
