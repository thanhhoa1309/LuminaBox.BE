using FluentValidation;
using LuminaBox.Application.Dtos.Order;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Validations.Order
{
    public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
    {
        public CreateOrderRequestValidator()
        {
            RuleFor(x => x.FulfillmentType)
                .IsInEnum().WithMessage("Invalid fulfillment type.");

            RuleFor(x => x)
                .Must(x => x.FulfillmentType == FulfillmentType.Pickup
                        || (x.AddressId.HasValue && x.AddressId.Value != Guid.Empty)
                        || (x.ShippingAddressId.HasValue && x.ShippingAddressId.Value != Guid.Empty))
                .WithMessage("AddressId is required for Delivery orders.");

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

                item.RuleFor(i => i)
                    .Must(i => (i.ProductItemId.HasValue && i.ProductItemId.Value != Guid.Empty)
                            || (i.ProductId.HasValue && i.ProductId.Value != Guid.Empty
                                && i.CategoryId.HasValue && i.CategoryId.Value != Guid.Empty
                                && i.FillingId.HasValue && i.FillingId.Value != Guid.Empty))
                    .WithMessage("ProductItemId (or ProductId, CategoryId, and FillingId) is required for Product item.")
                    .When(i => i.ItemType == OrderItemType.Product);

                item.RuleFor(i => i.BoxItemId)
                    .NotEmpty().WithMessage("BoxItemId is required for Box item.")
                    .When(i => i.ItemType == OrderItemType.Box);

                item.RuleFor(i => i.Selections)
                    .NotEmpty().WithMessage("Selections are required for Box item.")
                    .When(i => i.ItemType == OrderItemType.Box);

                item.RuleForEach(i => i.Selections).ChildRules(sel =>
                {
                    sel.RuleFor(s => s)
                        .Must(s => (s.ProductItemId.HasValue && s.ProductItemId.Value != Guid.Empty)
                                || (s.ProductId.HasValue && s.ProductId.Value != Guid.Empty
                                    && s.CategoryId.HasValue && s.CategoryId.Value != Guid.Empty
                                    && s.FillingId.HasValue && s.FillingId.Value != Guid.Empty))
                        .WithMessage("Selection ProductItemId (or ProductId, CategoryId, and FillingId) is required.");

                    sel.RuleFor(s => s.Quantity)
                        .GreaterThan(0).WithMessage("Selection Quantity must be greater than 0.");
                }).When(i => i.Selections != null);
            });
        }
    }
}
