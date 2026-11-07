namespace LuminaBox.Application.Dtos.Cart
{
    public class CartGroupResponse
    {
        public Guid OrganizerId { get; set; }
        public string OrganizerBrandName { get; set; } = string.Empty;
        public Guid? StallId { get; set; }
        public string? StallCode { get; set; }
        public string? StallName { get; set; }
        public decimal GroupSubtotal { get; set; }
        public List<CartItemResponse> Items { get; set; } = new();
    }
}

