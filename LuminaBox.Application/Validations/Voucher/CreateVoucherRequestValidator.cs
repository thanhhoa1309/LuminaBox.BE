using FluentValidation;
using LuminaBox.Application.Dtos.Voucher;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Validations.Voucher
{
    public class CreateVoucherRequestValidator : AbstractValidator<CreateVoucherRequest>
    {
        public CreateVoucherRequestValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Voucher code is required.")
                .MaximumLength(50).WithMessage("Voucher code must not exceed 50 characters.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Voucher name is required.")
                .MaximumLength(150).WithMessage("Voucher name must not exceed 150 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

            RuleFor(x => x.DiscountType)
                .IsInEnum().WithMessage("Invalid discount type.");

            RuleFor(x => x.DiscountValue)
                .GreaterThan(0).WithMessage("Discount value must be greater than 0.");

            RuleFor(x => x.DiscountValue)
                .LessThanOrEqualTo(100).WithMessage("Percentage discount value must not exceed 100.")
                .When(x => x.DiscountType == DiscountType.Percent);

            RuleFor(x => x.MaxDiscountAmount)
                .GreaterThan(0).WithMessage("Max discount amount must be greater than 0.")
                .When(x => x.MaxDiscountAmount.HasValue);

            RuleFor(x => x.MinOrderAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum order amount must be non-negative.");

            RuleFor(x => x.UsageLimit)
                .GreaterThan(0).WithMessage("Usage limit must be greater than 0.")
                .When(x => x.UsageLimit.HasValue);

            RuleFor(x => x.UsageLimitPerUser)
                .GreaterThan(0).WithMessage("Usage limit per user must be greater than 0.");

            RuleFor(x => x.EndAt)
                .GreaterThan(x => x.StartAt).WithMessage("EndAt must be after StartAt.");
        }
    }
}

