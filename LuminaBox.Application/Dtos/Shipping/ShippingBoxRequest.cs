namespace LuminaBox.Application.Dtos.Shipping
{
    public class ShippingBoxRequest
    {
        public Guid BoxItemId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
