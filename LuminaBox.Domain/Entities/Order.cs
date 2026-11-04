using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class Order : BaseEntity
    {
        public string OrderCode { get; set; } = string.Empty;

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // Null for orders of online organizers
        public Guid? StallId { get; set; }
        public Stall? Stall { get; set; }

        public Guid OrganizerId { get; set; }
        public Organizer Organizer { get; set; } = null!;

        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string? ShippingAddress { get; set; }
        public string? Note { get; set; }

        public Guid? AddressId { get; set; }
        public Address? Address { get; set; }

        public Guid? PickupAddressId { get; set; }
        public Address? PickupAddress { get; set; }

        public FulfillmentType FulfillmentType { get; set; } = FulfillmentType.Delivery;
        public decimal ShippingFee { get; set; }

        public decimal SubtotalAmount { get; set; }

        // Discount funded by the platform
        public decimal PlatformDiscount { get; set; }

        // Discount funded by the organizer
        public decimal OrganizerDiscount { get; set; }
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public ICollection<VoucherUsage> VoucherUsages { get; set; } = new List<VoucherUsage>();
        public ICollection<OrderStatusHistory> StatusHistories { get; set; } = new List<OrderStatusHistory>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public Commission? Commission { get; set; }
        public Payment? Payment { get; set; }
    }
}
