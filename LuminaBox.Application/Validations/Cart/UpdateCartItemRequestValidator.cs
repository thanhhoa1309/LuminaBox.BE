using FluentValidation;
using LuminaBox.Application.Dtos.Cart;

namespace LuminaBox.Application.Validations.Cart
{
    public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
    {
        public UpdateCartItemRequestValidator()
        {
            RuleFor(x => x.ProductItemId)
                .NotEqual(Guid.Empty).WithMessage("ProductItemId must not be empty.")
                .When(x => x.ProductItemId.HasValue);

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
                .LessThanOrEqualTo(1000).WithMessage("Quantity must not exceed 1000.")
                .When(x => x.Quantity.HasValue);
        }
    }
}
