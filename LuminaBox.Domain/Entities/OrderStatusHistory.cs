using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class OrderStatusHistory : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public OrderStatus? FromStatus { get; set; }
        public OrderStatus ToStatus { get; set; }

        public Guid? ChangedBy { get; set; }
        public User? Changer { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}
