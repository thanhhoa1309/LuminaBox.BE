using FluentValidation;
using LuminaBox.Application.Dtos.Voucher;

namespace LuminaBox.Application.Validations.Voucher
{
    public class UpdateVoucherStatusRequestValidator : AbstractValidator<UpdateVoucherStatusRequest>
    {
        public UpdateVoucherStatusRequestValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid voucher status.");
        }
    }
}

