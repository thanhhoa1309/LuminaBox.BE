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

            RuleFor(i => i.ProductItemId)
                .NotEmpty().WithMessage("ProductItemId is required for Product item.")
                .When(i => i.ItemType == OrderItemType.Product);

            RuleFor(i => i.BoxItemId)
                .NotEmpty().WithMessage("BoxItemId is required for Box item.")
                .When(i => i.ItemType == OrderItemType.Box);
        }
    }
}

