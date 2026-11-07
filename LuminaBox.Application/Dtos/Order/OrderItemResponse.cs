using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Order
{
    public class OrderItemResponse
    {
        public Guid Id { get; set; }
        public OrderItemType ItemType { get; set; }
        public Guid? ProductId { get; set; }
        public string? ProductName { get; set; }
        public Guid? BoxItemId { get; set; }
        public string? BoxItemName { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public Guid? FillingId { get; set; }
        public string? FillingName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public bool HasFeedback { get; set; }
        public List<OrderBoxSelectionResponse> BoxSelections { get; set; } = new();
    }
}

