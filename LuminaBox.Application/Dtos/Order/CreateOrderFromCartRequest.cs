using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Order
{
    public class CreateOrderFromCartRequest
    {
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Stripe;
        public string? Note { get; set; }
        public List<Guid> CartItemIds { get; set; } = new();
        public List<Guid>? VoucherIds { get; set; }
    }
}
