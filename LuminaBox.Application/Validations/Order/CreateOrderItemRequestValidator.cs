using FluentValidation;
using LuminaBox.Application.Dtos.Order;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Validations.Order
{
    public class CreateOrderItemRequestValidator : AbstractValidator<CreateOrderItemRequest>
    {
        public CreateOrderItemRequestValidator()
        {
            RuleFor(i => i.ItemType)
                .IsInEnum().WithMessage("Invalid item type.");

            RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Item quantity must be greater than 0.")
                .LessThanOrEqualTo(1000).WithMessage("Item quantity must not exceed 1000.");

            RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("ProductId is required for Product item.")
                .When(i => i.ItemType == OrderItemType.Product);

            RuleFor(i => i.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required for Product item.")
                .When(i => i.ItemType == OrderItemType.Product);

            RuleFor(i => i.FillingId)
                .NotEmpty().WithMessage("FillingId is required for Product item.")
                .When(i => i.ItemType == OrderItemType.Product);

            RuleFor(i => i.BoxItemId)
                .NotEmpty().WithMessage("BoxItemId is required for Box item.")
                .When(i => i.ItemType == OrderItemType.Box);

            RuleFor(i => i.Selections)
                .NotEmpty().WithMessage("Selections are required for Box item.")
                .When(i => i.ItemType == OrderItemType.Box);

            RuleForEach(i => i.Selections)
                .SetValidator(new CreateOrderBoxSelectionRequestValidator())
                .When(i => i.Selections != null);
        }
    }
}

