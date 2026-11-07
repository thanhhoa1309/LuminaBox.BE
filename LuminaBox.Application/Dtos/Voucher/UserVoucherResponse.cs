using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Voucher
{
    public class UserVoucherResponse
    {
        public Guid Id { get; set; }
        public Guid VoucherId { get; set; }
        public string Code { get; set; } = string.Empty;
        public VoucherScope Scope { get; set; }
        public Guid? OrganizerId { get; set; }
        public string? OrganizerBrandName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; }
        public decimal MinOrderAmount { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime EndAt { get; set; }
        public VoucherStatus Status { get; set; }
        public DateTime ClaimedAt { get; set; }
    }
}

