namespace LuminaBox.Domain.Entities
{
    public class Filling : BaseEntity
    {
        public Guid OrganizerId { get; set; }
        public Organizer Organizer { get; set; } = null!;

        public string Name { get; set; } = string.Empty;

        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    }
}
