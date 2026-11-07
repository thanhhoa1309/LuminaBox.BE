using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Order
{
    public class OrderVoucherUsageResponse
    {
        public Guid Id { get; set; }
        public Guid VoucherId { get; set; }
        public string VoucherCode { get; set; } = string.Empty;
        public VoucherScope Scope { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime UsedAt { get; set; }
    }
}

