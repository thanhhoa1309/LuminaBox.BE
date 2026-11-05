namespace LuminaBox.Application.Dtos.Cart
{
    public class CartResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int TotalItems { get; set; }
        public List<CartGroupResponse> Groups { get; set; } = new();
    }
}
