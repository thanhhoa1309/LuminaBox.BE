namespace LuminaBox.Application.Dtos.Order
{
    public class CreateOrderBoxSelectionRequest
    {
        public Guid ProductId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid FillingId { get; set; }
        public int Quantity { get; set; }
    }
}

