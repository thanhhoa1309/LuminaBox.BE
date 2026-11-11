using FluentValidation;
using LuminaBox.Application.Dtos.Organizer;

namespace LuminaBox.Application.Validations.Organizer
{
    public class UpdateOrganizerProfileRequestValidator : AbstractValidator<UpdateOrganizerProfileRequest>
    {
        public UpdateOrganizerProfileRequestValidator()
        {
            RuleFor(x => x.BrandName)
                .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Brand name must not be empty.")
                .MaximumLength(150).WithMessage("Brand name must not exceed 150 characters.")
                .When(x => x.BrandName is not null);

            RuleFor(x => x.ContactPhone)
                .Matches(@"^(0|\+84)(3|5|7|8|9)\d{8}$").WithMessage("Contact phone is invalid.")
                .When(x => !string.IsNullOrWhiteSpace(x.ContactPhone));

            RuleFor(x => x.ContactEmail)
                .EmailAddress().WithMessage("Contact email is invalid.")
                .MaximumLength(255).WithMessage("Contact email must not exceed 255 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

            RuleFor(x => x.BusinessInfo)
                .MaximumLength(2000).WithMessage("Business info must not exceed 2000 characters.");
        }
    }
}
