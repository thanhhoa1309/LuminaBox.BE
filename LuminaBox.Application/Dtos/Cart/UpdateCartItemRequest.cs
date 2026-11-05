namespace LuminaBox.Application.Dtos.Cart
{
    public class UpdateCartItemRequest
    {
        public int? Quantity { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? FillingId { get; set; }
    }
}
