using FluentValidation;
using LuminaBox.Application.Dtos.Order;

namespace LuminaBox.Application.Validations.Order
{
    public class CreateOrderFromCartRequestValidator : AbstractValidator<CreateOrderFromCartRequest>
    {
        public CreateOrderFromCartRequestValidator()
        {
            RuleFor(x => x.ShippingAddressId)
                .NotEmpty().WithMessage("ShippingAddressId is required.");

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
                    sel.RuleFor(s => s.ProductId)
                        .NotEmpty().WithMessage("Selection ProductId is required.");
                    sel.RuleFor(s => s.CategoryId)
                        .NotEmpty().WithMessage("Selection CategoryId is required.");
                    sel.RuleFor(s => s.FillingId)
                        .NotEmpty().WithMessage("Selection FillingId is required.");
                    sel.RuleFor(s => s.Quantity)
                        .GreaterThan(0).WithMessage("Selection Quantity must be greater than 0.");
                });
            }).When(x => x.BoxItems != null);
        }
    }
}

