using FluentValidation;
using LuminaBox.Application.Dtos.Cart;

namespace LuminaBox.Application.Validations.Cart
{
    public class AddProductToCartRequestValidator : AbstractValidator<AddProductToCartRequest>
    {
        public AddProductToCartRequestValidator()
        {
            RuleFor(x => x.ProductItemId)
                .NotEmpty().WithMessage("ProductItemId is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
                .LessThanOrEqualTo(1000).WithMessage("Quantity must not exceed 1000.");
        }
    }
}
