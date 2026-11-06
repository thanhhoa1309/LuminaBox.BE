using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Order
{
    public class CreateOrderItemRequest
    {
        public OrderItemType ItemType { get; set; } = OrderItemType.Product;
        public Guid? ProductItemId { get; set; }
        public int Quantity { get; set; } = 1;
        public Guid? BoxItemId { get; set; }
        public Guid? StallId { get; set; }
        public List<CreateOrderBoxSelectionRequest>? Selections { get; set; }
    }
}

