using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Order
{
    public class OrderSummaryResponse
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string BuyerName { get; set; } = string.Empty;
        public Guid OrganizerId { get; set; }
        public string OrganizerBrandName { get; set; } = string.Empty;
        public Guid? StallId { get; set; }
        public string? StallCode { get; set; }
        public string? StallName { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public decimal SubtotalAmount { get; set; }
        public decimal PlatformDiscount { get; set; }
        public decimal OrganizerDiscount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
        public Guid? PaymentId { get; set; }
        public string? GhnOrderCode { get; set; }
        public string? GhnStatus { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

