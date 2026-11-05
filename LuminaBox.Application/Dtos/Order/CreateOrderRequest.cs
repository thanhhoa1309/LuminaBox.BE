using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Order
{
    public class CreateOrderRequest
    {
        public FulfillmentType FulfillmentType { get; set; } = FulfillmentType.Delivery;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Stripe;
        public string? Note { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; } = new();
        public List<Guid>? VoucherIds { get; set; }
    }

    public class CreateOrderItemRequest
    {
        public OrderItemType ItemType { get; set; } = OrderItemType.Product;
        public Guid? ProductId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? FillingId { get; set; }
        public int Quantity { get; set; } = 1;
        public Guid? BoxItemId { get; set; }
        public Guid? StallId { get; set; }
        public List<CreateOrderBoxSelectionRequest>? Selections { get; set; }
    }

    public class CreateOrderBoxSelectionRequest
    {
        public Guid ProductId { get; set; }
        public Guid CategoryId { get; set; }
        public Guid FillingId { get; set; }
        public int Quantity { get; set; }
    }
}
