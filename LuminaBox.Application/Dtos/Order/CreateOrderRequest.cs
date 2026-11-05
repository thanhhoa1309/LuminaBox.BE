using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Order
{
    public class CreateOrderRequest
    {
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Stripe;
        public string? Note { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; } = new();
        public List<Guid>? VoucherIds { get; set; }
    }
}
