using LuminaBox.Domain.Entities;

namespace LuminaBox.Application.Dtos.Voucher
{
    public class OrderGroupVoucherResult
    {
        public decimal OrganizerDiscount { get; set; }
        public decimal PlatformDiscount { get; set; }
        public UserVoucher? AppliedOrganizerUserVoucher { get; set; }
        public UserVoucher? AppliedPlatformUserVoucher { get; set; }
    }
}

