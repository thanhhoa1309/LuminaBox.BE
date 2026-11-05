using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Order
{
    public class CreateOrderFromCartRequest
    {
        public FulfillmentType FulfillmentType { get; set; } = FulfillmentType.Delivery;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Stripe;
        public string? Note { get; set; }
        public List<Guid> CartItemIds { get; set; } = new();
        public List<OrderFromCartBoxSelectionRequest>? BoxItems { get; set; }
        public List<Guid>? VoucherIds { get; set; }
    }

    public class OrderFromCartBoxSelectionRequest
    {
        public Guid CartItemId { get; set; }
        public List<CreateOrderBoxSelectionRequest> Selections { get; set; } = new();
    }
}
