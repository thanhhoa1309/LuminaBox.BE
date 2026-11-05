using FluentValidation;
using LuminaBox.Application.Dtos.Order;

namespace LuminaBox.Application.Validations.Order
{
    public class CreateOrderBoxSelectionRequestValidator : AbstractValidator<CreateOrderBoxSelectionRequest>
    {
        public CreateOrderBoxSelectionRequestValidator()
        {
            RuleFor(s => s.ProductId)
                .NotEmpty().WithMessage("Selection ProductId is required.");

            RuleFor(s => s.CategoryId)
                .NotEmpty().WithMessage("Selection CategoryId is required.");

            RuleFor(s => s.FillingId)
                .NotEmpty().WithMessage("Selection FillingId is required.");

            RuleFor(s => s.Quantity)
                .GreaterThan(0).WithMessage("Selection Quantity must be greater than 0.");
        }
    }
}

