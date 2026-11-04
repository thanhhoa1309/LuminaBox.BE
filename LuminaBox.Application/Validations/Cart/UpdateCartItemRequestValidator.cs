using FluentValidation;
using LuminaBox.Application.Dtos.Cart;

namespace LuminaBox.Application.Validations.Cart
{
    public class UpdateCartItemRequestValidator : AbstractValidator<UpdateCartItemRequest>
    {
        public UpdateCartItemRequestValidator()
        {
            RuleFor(x => x.CategoryId)
                .NotEqual(Guid.Empty).WithMessage("CategoryId must not be empty.")
                .When(x => x.CategoryId.HasValue);

            RuleFor(x => x.FillingId)
                .NotEqual(Guid.Empty).WithMessage("FillingId must not be empty.")
                .When(x => x.FillingId.HasValue);

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
                .LessThanOrEqualTo(1000).WithMessage("Quantity must not exceed 1000.")
                .When(x => x.Quantity.HasValue);
        }
    }
}

