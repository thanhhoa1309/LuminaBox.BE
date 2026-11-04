namespace LuminaBox.Domain.Entities
{
    public class StallBoxItem : BaseEntity
    {
        public Guid? StallId { get; set; }
        public Stall? Stall { get; set; }

        public Guid BoxItemId { get; set; }
        public BoxItem BoxItem { get; set; } = null!;

        public int StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
