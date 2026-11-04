using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Order
{
    public class CreateOrderRequest
    {
        public Guid? AddressId { get; set; }
        public Guid? ShippingAddressId { get; set; }
        public FulfillmentType FulfillmentType { get; set; } = FulfillmentType.Delivery;
        public PaymentMethod PaymentMethod { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverPhone { get; set; }
        public string? Note { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; } = new();
        public List<Guid>? VoucherIds { get; set; }
    }

    public class CreateOrderItemRequest
    {
        public OrderItemType ItemType { get; set; }
        public Guid? ProductItemId { get; set; }
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
        public Guid? ProductItemId { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? FillingId { get; set; }
        public int Quantity { get; set; }
    }
}
