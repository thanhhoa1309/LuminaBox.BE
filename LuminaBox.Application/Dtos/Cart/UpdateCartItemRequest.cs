namespace LuminaBox.Application.Dtos.Cart
{
    public class UpdateCartItemRequest
    {
        public Guid? ProductItemId { get; set; }
        public int? Quantity { get; set; }
    }
}
