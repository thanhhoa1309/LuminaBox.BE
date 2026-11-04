using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Order
{
    public class CreateOrderFromCartRequest
    {
        public Guid? AddressId { get; set; }
        public Guid? ShippingAddressId { get; set; }
        public FulfillmentType FulfillmentType { get; set; } = FulfillmentType.Delivery;
        public PaymentMethod PaymentMethod { get; set; }
        public string? ReceiverName { get; set; }
        public string? ReceiverPhone { get; set; }
        public string? Note { get; set; }
        public List<Guid> CartItemIds { get; set; } = new();
        public List<CartBoxItemSelectionRequest>? BoxItems { get; set; }
        public List<Guid>? VoucherIds { get; set; }
    }

    public class CartBoxItemSelectionRequest
    {
        public Guid CartItemId { get; set; }
        public List<CreateOrderBoxSelectionRequest> Selections { get; set; } = new();
    }
}
