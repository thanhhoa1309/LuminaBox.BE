using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Order
{
    public class UpdateOrderStatusRequest
    {
        public OrderStatus Status { get; set; }
    }
}

