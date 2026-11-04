using FluentValidation;
using LuminaBox.Application.Dtos.Order;

namespace LuminaBox.Application.Validations.Order
{
    public class CancelOrderRequestValidator : AbstractValidator<CancelOrderRequest>
    {
        public CancelOrderRequestValidator()
        {
            RuleFor(x => x.Reason)
                .MaximumLength(500).WithMessage("Reason must not exceed 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Reason));
        }
    }
}

