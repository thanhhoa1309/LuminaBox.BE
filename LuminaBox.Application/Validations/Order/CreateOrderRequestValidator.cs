using FluentValidation;
using LuminaBox.Application.Dtos.Order;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Validations.Order
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.ShippingAddressId)
                .NotEmpty().WithMessage("ShippingAddressId is required.");

            RuleFor(x => x.PaymentMethod)
                .IsInEnum().WithMessage("Invalid payment method.");

            RuleFor(x => x.Note)
                .MaximumLength(1000).WithMessage("Note must not exceed 1000 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Note));

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Order must contain at least one item.");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ItemType)
                    .IsInEnum().WithMessage("Invalid item type.");

                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage("Item quantity must be greater than 0.")
                    .LessThanOrEqualTo(1000).WithMessage("Item quantity must not exceed 1000.");

                item.RuleFor(i => i.ProductId)
                    .NotEmpty().WithMessage("ProductId is required for Product item.")
                    .When(i => i.ItemType == OrderItemType.Product);

                item.RuleFor(i => i.CategoryId)
                    .NotEmpty().WithMessage("CategoryId is required for Product item.")
                    .When(i => i.ItemType == OrderItemType.Product);

                item.RuleFor(i => i.FillingId)
                    .NotEmpty().WithMessage("FillingId is required for Product item.")
                    .When(i => i.ItemType == OrderItemType.Product);

                item.RuleFor(i => i.BoxItemId)
                    .NotEmpty().WithMessage("BoxItemId is required for Box item.")
                    .When(i => i.ItemType == OrderItemType.Box);

                item.RuleFor(i => i.Selections)
                    .NotEmpty().WithMessage("Selections are required for Box item.")
                    .When(i => i.ItemType == OrderItemType.Box);

                item.RuleForEach(i => i.Selections).ChildRules(sel =>
                {
                    sel.RuleFor(s => s.ProductId)
                        .NotEmpty().WithMessage("Selection ProductId is required.");
                    sel.RuleFor(s => s.CategoryId)
                        .NotEmpty().WithMessage("Selection CategoryId is required.");
                    sel.RuleFor(s => s.FillingId)
                        .NotEmpty().WithMessage("Selection FillingId is required.");
                    sel.RuleFor(s => s.Quantity)
                        .GreaterThan(0).WithMessage("Selection Quantity must be greater than 0.");
                }).When(i => i.Selections != null);
            });
        }
    }
}

