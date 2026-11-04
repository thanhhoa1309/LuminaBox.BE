using FluentValidation;
using LuminaBox.Application.Dtos.Shipping;

namespace LuminaBox.Application.Validations.Shipping
{
    public class ShippingFeeRequestValidator : AbstractValidator<ShippingFeeRequest>
    {
        public ShippingFeeRequestValidator()
        {
            RuleFor(x => x.OrganizerId).NotEmpty().WithMessage("Organizer is required.");
            RuleFor(x => x.AddressId).NotEmpty().WithMessage("Address is required.");

            RuleFor(x => x)
                .Must(x => x.Items.Count > 0 || x.Boxes.Count > 0)
                .WithMessage("At least one item or box is required.");

            RuleForEach(x => x.Items).SetValidator(new ShippingItemRequestValidator());

            RuleForEach(x => x.Boxes).ChildRules(box =>
            {
                box.RuleFor(b => b.BoxItemId).NotEmpty().WithMessage("Box is required.");
                box.RuleFor(b => b.Selections).NotEmpty().WithMessage("A box needs its cakes.");
                box.RuleForEach(b => b.Selections).SetValidator(new ShippingItemRequestValidator());
            });
        }
    }

    public class ShippingItemRequestValidator : AbstractValidator<ShippingItemRequest>
    {
        public ShippingItemRequestValidator()
        {
            RuleFor(x => x.ProductItemId).NotEmpty().WithMessage("Product item is required.");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        }
    }
}
