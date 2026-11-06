namespace LuminaBox.Application.Dtos.Order
{
    public class CreateOrderBoxSelectionRequest
    {
        public Guid ProductItemId { get; set; }
        public int Quantity { get; set; }
    }
}

