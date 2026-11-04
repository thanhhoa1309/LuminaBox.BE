namespace LuminaBox.Application.Dtos.Cart
{
    public class UpdateCartItemRequest
    {
        public Guid? CategoryId { get; set; }
        public Guid? FillingId { get; set; }
        public int? Quantity { get; set; }
    }
}

