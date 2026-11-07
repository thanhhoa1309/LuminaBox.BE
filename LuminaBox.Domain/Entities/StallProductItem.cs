namespace LuminaBox.Domain.Entities
{
    public class StallProductItem : BaseEntity
    {
        public Guid? StallId { get; set; }
        public Stall? Stall { get; set; }

        public Guid ProductItemId { get; set; }
        public ProductItem ProductItem { get; set; } = null!;

        public int StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;

        // Maps to the PostgreSQL xmin system column; used as the optimistic concurrency token
        public uint Version { get; set; }
    }
}
