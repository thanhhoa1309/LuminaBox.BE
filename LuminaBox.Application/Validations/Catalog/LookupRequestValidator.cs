using FluentValidation;
using LuminaBox.Application.Dtos.Catalog;

namespace LuminaBox.Application.Validations.Catalog
{
    public class LookupRequestValidator : AbstractValidator<LookupRequest>
    {
        public LookupRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
        }
    }
}
