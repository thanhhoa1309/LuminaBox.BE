namespace LuminaBox.Application.Dtos.Product
{
    public class StockRequest
    {
        public Guid? StallId { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
