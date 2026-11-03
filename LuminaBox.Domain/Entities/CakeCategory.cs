namespace LuminaBox.Domain.Entities
{
    public class CakeCategory : BaseEntity
    {
        public Guid OrganizerId { get; set; }
        public Organizer Organizer { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    }
}
