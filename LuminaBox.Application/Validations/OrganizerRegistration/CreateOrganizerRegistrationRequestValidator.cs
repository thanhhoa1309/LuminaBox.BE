using FluentValidation;
using LuminaBox.Application.Dtos.OrganizerRegistration;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Validations.OrganizerRegistration
{
    public class CreateOrganizerRegistrationRequestValidator : AbstractValidator<CreateOrganizerRegistrationRequest>
    {
        public CreateOrganizerRegistrationRequestValidator()
        {
            RuleFor(x => x.Type)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Registration type is required.")
                .IsInEnum().WithMessage("Registration type is invalid.");

            RuleFor(x => x.BrandName)
                .NotEmpty().WithMessage("Brand name is required.")
                .MaximumLength(150).WithMessage("Brand name must not exceed 150 characters.");

            RuleFor(x => x.ContactPhone)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Contact phone is required.")
                .Matches(@"^(0|\+84)(3|5|7|8|9)\d{8}$").WithMessage("Contact phone is invalid.");

            RuleFor(x => x.BusinessInfo)
                .MaximumLength(2000).WithMessage("Business info must not exceed 2000 characters.");

            When(x => x.Type == RegistrationType.Stall, () =>
            {
                RuleFor(x => x.StallId)
                    .NotEmpty().WithMessage("Stall id is required for a stall registration.");

                RuleFor(x => x.StartDate)
                    .NotNull().WithMessage("Start date is required for a stall registration.");

                RuleFor(x => x.EndDate)
                    .NotNull().WithMessage("End date is required for a stall registration.");

                RuleFor(x => x)
                    .Must(x => x.EndDate > x.StartDate).WithMessage("End date must be after start date.")
                    .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
            });

            When(x => x.Type == RegistrationType.Online, () =>
            {
                RuleFor(x => x.StallId)
                    .Null().WithMessage("Stall id must not be provided for an online registration.");

                RuleFor(x => x.StartDate)
                    .Null().WithMessage("Start date must not be provided for an online registration.");

                RuleFor(x => x.EndDate)
                    .Null().WithMessage("End date must not be provided for an online registration.");
            });
        }
    }
}
