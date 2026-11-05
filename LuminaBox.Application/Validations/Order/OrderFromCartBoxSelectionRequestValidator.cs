using FluentValidation;
using LuminaBox.Application.Dtos.Order;

namespace LuminaBox.Application.Validations.Order
{
    public class OrderFromCartBoxSelectionRequestValidator : AbstractValidator<OrderFromCartBoxSelectionRequest>
    {
        public OrderFromCartBoxSelectionRequestValidator()
        {
            RuleFor(b => b.CartItemId)
                .NotEmpty().WithMessage("CartItemId is required for box selections.");

            RuleFor(b => b.Selections)
                .NotEmpty().WithMessage("Selections are required for box items.");

            RuleForEach(b => b.Selections)
                .SetValidator(new CreateOrderBoxSelectionRequestValidator());
        }
    }
}

