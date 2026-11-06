namespace LuminaBox.Application.Dtos.Cart
{
    public class UpdateCartItemRequest
    {
        public int? Quantity { get; set; }
        public Guid? ProductItemId { get; set; }
    }
}
