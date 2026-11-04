using FluentValidation;
using LuminaBox.Application.Dtos.Order;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Validations.Order
{
    public class CreateOrderFromCartRequestValidator : AbstractValidator<CreateOrderFromCartRequest>
    {
        public CreateOrderFromCartRequestValidator()
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

            RuleFor(x => x.CartItemIds)
                .NotEmpty().WithMessage("At least one CartItemId is required.");

            RuleForEach(x => x.BoxItems).ChildRules(box =>
            {
                box.RuleFor(b => b.CartItemId)
                    .NotEmpty().WithMessage("CartItemId is required for box selections.");

                box.RuleFor(b => b.Selections)
                    .NotEmpty().WithMessage("Selections are required for box items.");

                box.RuleForEach(b => b.Selections).ChildRules(sel =>
                {
                    sel.RuleFor(s => s)
                        .Must(s => (s.ProductItemId.HasValue && s.ProductItemId.Value != Guid.Empty)
                                || (s.ProductId.HasValue && s.ProductId.Value != Guid.Empty
                                    && s.CategoryId.HasValue && s.CategoryId.Value != Guid.Empty
                                    && s.FillingId.HasValue && s.FillingId.Value != Guid.Empty))
                        .WithMessage("Selection ProductItemId (or ProductId, CategoryId, and FillingId) is required.");

                    sel.RuleFor(s => s.Quantity)
                        .GreaterThan(0).WithMessage("Selection Quantity must be greater than 0.");
                });
            }).When(x => x.BoxItems != null);
        }
    }
}
