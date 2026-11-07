using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Order
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public string BuyerEmail { get; set; } = string.Empty;
        public Guid OrganizerId { get; set; }
        public string OrganizerBrandName { get; set; } = string.Empty;
        public Guid? StallId { get; set; }
        public string? StallCode { get; set; }
        public string? StallName { get; set; }
        public Guid? AddressId { get; set; }
        public Guid? PickupAddressId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string? ShippingAddress { get; set; }
        public string? Note { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal PlatformDiscount { get; set; }
        public decimal OrganizerDiscount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }
        public OrderPaymentSummaryResponse? Payment { get; set; }
        public List<OrderItemResponse> Items { get; set; } = new();
        public List<OrderVoucherUsageResponse> VoucherUsages { get; set; } = new();
        public List<OrderStatusHistoryResponse> StatusHistories { get; set; } = new();
    }
}
