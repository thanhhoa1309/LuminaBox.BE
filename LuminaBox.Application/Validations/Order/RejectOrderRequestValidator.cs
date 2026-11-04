using FluentValidation;
using LuminaBox.Application.Dtos.Order;

namespace LuminaBox.Application.Validations.Order
{
    public class RejectOrderRequestValidator : AbstractValidator<RejectOrderRequest>
    {
        public RejectOrderRequestValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reject reason is required.")
                .MaximumLength(500).WithMessage("Reject reason must not exceed 500 characters.");
        }
    }
}

