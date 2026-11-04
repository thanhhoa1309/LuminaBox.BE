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
        public Guid? ShippingAddressId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public int ToProvinceId { get; set; }
        public string ToProvinceName { get; set; } = string.Empty;
        public int ToWardId { get; set; }
        public string ToWardName { get; set; } = string.Empty;
        public string AddressDetail { get; set; } = string.Empty;
        public string? Note { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal PlatformDiscount { get; set; }
        public decimal OrganizerDiscount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public string? GhnOrderCode { get; set; }
        public string? GhnStatus { get; set; }
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

    public class OrderPaymentSummaryResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public string? CheckoutSessionId { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }
    }

    public class OrderItemResponse
    {
        public Guid Id { get; set; }
        public OrderItemType ItemType { get; set; }
        public Guid? ProductId { get; set; }
        public string? ProductName { get; set; }
        public Guid? BoxItemId { get; set; }
        public string? BoxItemName { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public Guid? FillingId { get; set; }
        public string? FillingName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
        public bool HasFeedback { get; set; }
        public List<OrderBoxSelectionResponse> BoxSelections { get; set; } = new();
    }

    public class OrderBoxSelectionResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public Guid FillingId { get; set; }
        public string FillingName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class OrderVoucherUsageResponse
    {
        public Guid Id { get; set; }
        public Guid VoucherId { get; set; }
        public string VoucherCode { get; set; } = string.Empty;
        public VoucherScope Scope { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime UsedAt { get; set; }
    }

    public class OrderStatusHistoryResponse
    {
        public Guid Id { get; set; }
        public OrderStatus? FromStatus { get; set; }
        public OrderStatus ToStatus { get; set; }
        public Guid? ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }
    }
}

