using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Cart
{
    public class CartResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public int TotalItems { get; set; }
        public List<CartGroupResponse> Groups { get; set; } = new();
    }

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

    public class CartItemResponse
    {
        public Guid Id { get; set; }
        public OrderItemType ItemType { get; set; }
        public Guid OrganizerId { get; set; }
        public string OrganizerBrandName { get; set; } = string.Empty;
        public Guid? StallId { get; set; }
        public string? StallCode { get; set; }
        public string? StallName { get; set; }
        public Guid? ProductId { get; set; }
        public string? ProductName { get; set; }
        public Guid? ProductItemId { get; set; }
        public string? ProductItemName { get; set; }
        public int? EggCount { get; set; }
        public int? Weight { get; set; }
        public Guid? BoxItemId { get; set; }
        public string? BoxItemName { get; set; }
        public int? BoxCapacity { get; set; }
        public string? ImageUrl { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public Guid? FillingId { get; set; }
        public string? FillingName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
