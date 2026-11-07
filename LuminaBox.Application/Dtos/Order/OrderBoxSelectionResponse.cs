namespace LuminaBox.Application.Dtos.Order
{
    public class OrderBoxSelectionResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public Guid FillingId { get; set; }
        public string FillingName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}

