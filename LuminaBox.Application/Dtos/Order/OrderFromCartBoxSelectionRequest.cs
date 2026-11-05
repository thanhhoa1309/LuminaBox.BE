namespace LuminaBox.Application.Dtos.Order
{
    public class OrderFromCartBoxSelectionRequest
    {
        public Guid CartItemId { get; set; }
        public List<CreateOrderBoxSelectionRequest> Selections { get; set; } = new();
    }
}

