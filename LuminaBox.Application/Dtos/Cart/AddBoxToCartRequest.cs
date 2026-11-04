namespace LuminaBox.Application.Dtos.Cart
{
    public class AddBoxToCartRequest
    {
        public Guid BoxItemId { get; set; }
        public int Quantity { get; set; } = 1;
        public Guid? StallId { get; set; }
    }
}

