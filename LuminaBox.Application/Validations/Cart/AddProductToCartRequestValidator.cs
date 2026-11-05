using FluentValidation;
using LuminaBox.Application.Dtos.Cart;

namespace LuminaBox.Application.Validations.Cart
{
    public class AddProductToCartRequestValidator : AbstractValidator<AddProductToCartRequest>
    {
        public AddProductToCartRequestValidator()
        {
            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("ProductId is required.");

            RuleFor(x => x.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required.");

            RuleFor(x => x.FillingId)
                .NotEmpty().WithMessage("FillingId is required.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
                .LessThanOrEqualTo(1000).WithMessage("Quantity must not exceed 1000.");
        }
    }
}
