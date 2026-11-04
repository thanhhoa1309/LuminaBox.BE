namespace LuminaBox.Application.Dtos.Product
{
    public class StockResponse
    {
        public Guid Id { get; set; }
        public Guid? StallId { get; set; }
        public string? StallName { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
    }
}
