using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Order
{
    public class OrderStatusHistoryResponse
    {
        public Guid Id { get; set; }
        public OrderStatus? FromStatus { get; set; }
        public OrderStatus ToStatus { get; set; }
        public Guid? ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}

