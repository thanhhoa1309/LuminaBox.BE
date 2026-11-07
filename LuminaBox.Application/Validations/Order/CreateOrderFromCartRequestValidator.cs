using FluentValidation;
using LuminaBox.Application.Dtos.Order;

namespace LuminaBox.Application.Validations.Order
{
    public class CreateOrderFromCartRequestValidator : AbstractValidator<CreateOrderFromCartRequest>
    {
        public CreateOrderFromCartRequestValidator()
        {
            RuleFor(x => x.PaymentMethod)
                .IsInEnum().WithMessage("Invalid payment method.");

            RuleFor(x => x.Note)
                .MaximumLength(1000).WithMessage("Note must not exceed 1000 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Note));

            RuleFor(x => x.CartItemIds)
                .NotEmpty().WithMessage("At least one CartItemId is required.");
        }
    }
}
