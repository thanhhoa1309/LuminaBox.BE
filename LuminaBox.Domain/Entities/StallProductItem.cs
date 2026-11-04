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
    }
}
