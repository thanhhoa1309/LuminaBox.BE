using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Voucher
{
    public class CreateVoucherRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal MinOrderAmount { get; set; }
        public int? UsageLimit { get; set; }
        public int UsageLimitPerUser { get; set; } = 1;
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
    }
}

