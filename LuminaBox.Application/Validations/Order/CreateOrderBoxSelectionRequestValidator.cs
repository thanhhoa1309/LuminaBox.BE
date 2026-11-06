using FluentValidation;
using LuminaBox.Application.Dtos.Order;

namespace LuminaBox.Application.Validations.Order
{
    public class CreateOrderBoxSelectionRequestValidator : AbstractValidator<CreateOrderBoxSelectionRequest>
    {
        public CreateOrderBoxSelectionRequestValidator()
        {
            RuleFor(s => s.ProductItemId)
                .NotEmpty().WithMessage("Selection ProductItemId is required.");

            RuleFor(s => s.Quantity)
                .GreaterThan(0).WithMessage("Selection Quantity must be greater than 0.");
        }
    }
}

