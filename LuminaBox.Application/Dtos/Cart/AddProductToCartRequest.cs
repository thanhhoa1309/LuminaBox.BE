namespace LuminaBox.Application.Dtos.Cart
{
    public class AddProductToCartRequest
    {
        public Guid ProductId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid FillingId { get; set; }
        public int Quantity { get; set; } = 1;
        public Guid? StallId { get; set; }
    }
}
