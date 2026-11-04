namespace LuminaBox.Application.Dtos.Shipping
{
    public class ShippingBoxRequest
    {
        public Guid BoxItemId { get; set; }
        public List<ShippingItemRequest> Selections { get; set; } = new();
    }
}
