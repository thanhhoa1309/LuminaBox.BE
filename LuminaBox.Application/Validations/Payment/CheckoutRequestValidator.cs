using FluentValidation;
using LuminaBox.Application.Dtos.Payment;

namespace LuminaBox.Application.Validations.Payment
{
    public class CheckoutRequestValidator : AbstractValidator<CheckoutRequest>
    {
        public CheckoutRequestValidator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty().WithMessage("Order id is required.");
        }
    }
}