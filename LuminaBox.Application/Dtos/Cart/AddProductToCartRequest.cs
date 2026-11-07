namespace LuminaBox.Application.Dtos.Cart
{
    public class AddProductToCartRequest
    {
        public Guid ProductItemId { get; set; }
        public int Quantity { get; set; } = 1;
        public Guid? StallId { get; set; }
    }
}
