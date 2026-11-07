using LuminaBox.Application.Commons;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Voucher
{
    public class VoucherListQuery : ListQuery
    {
        public VoucherStatus? Status { get; set; }
    }

    public class AllVoucherListQuery : VoucherListQuery
    {
        public VoucherScope? Scope { get; set; }
    }
}
