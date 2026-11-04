using FluentValidation;
using LuminaBox.Application.Dtos.Address;

namespace LuminaBox.Application.Validations.Address
{
    internal static class AddressRules
    {
        public const string PhonePattern = @"^(0|\+84)(3|5|7|8|9)\d{8}$";
    }

    public class CreateAddressRequestValidator : AbstractValidator<CreateAddressRequest>
    {
        public CreateAddressRequestValidator()
        {
            RuleFor(x => x.Type).IsInEnum().WithMessage("Address type is invalid.");

            RuleFor(x => x.ContactName)
                .NotEmpty().WithMessage("Contact name is required.")
                .MaximumLength(100).WithMessage("Contact name must not exceed 100 characters.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(AddressRules.PhonePattern).WithMessage("Phone number is invalid.");

            RuleFor(x => x.WardId).NotEmpty().WithMessage("Ward is required.");

            RuleFor(x => x.AddressLine)
                .NotEmpty().WithMessage("Address line is required.")
                .MaximumLength(255).WithMessage("Address line must not exceed 255 characters.");

            RuleFor(x => x.Latitude).InclusiveBetween(-90m, 90m).When(x => x.Latitude.HasValue)
                .WithMessage("Latitude is invalid.");
            RuleFor(x => x.Longitude).InclusiveBetween(-180m, 180m).When(x => x.Longitude.HasValue)
                .WithMessage("Longitude is invalid.");

            RuleFor(x => x.MapRefId).MaximumLength(255);
            RuleFor(x => x.NewProvinceName).MaximumLength(100);
            RuleFor(x => x.NewWardName).MaximumLength(100);
        }
    }

    public class UpdateAddressRequestValidator : AbstractValidator<UpdateAddressRequest>
    {
        public UpdateAddressRequestValidator()
        {
            RuleFor(x => x.ContactName)
                .NotEmpty().WithMessage("Contact name must not be empty.")
                .MaximumLength(100).WithMessage("Contact name must not exceed 100 characters.")
                .When(x => x.ContactName is not null);

            RuleFor(x => x.Phone)
                .Matches(AddressRules.PhonePattern).WithMessage("Phone number is invalid.")
                .When(x => x.Phone is not null);

            RuleFor(x => x.WardId).NotEmpty().WithMessage("Ward is invalid.")
                .When(x => x.WardId.HasValue);

            RuleFor(x => x.AddressLine)
                .NotEmpty().WithMessage("Address line must not be empty.")
                .MaximumLength(255).WithMessage("Address line must not exceed 255 characters.")
                .When(x => x.AddressLine is not null);

            RuleFor(x => x.Latitude).InclusiveBetween(-90m, 90m).When(x => x.Latitude.HasValue)
                .WithMessage("Latitude is invalid.");
            RuleFor(x => x.Longitude).InclusiveBetween(-180m, 180m).When(x => x.Longitude.HasValue)
                .WithMessage("Longitude is invalid.");

            RuleFor(x => x.MapRefId).MaximumLength(255);
            RuleFor(x => x.NewProvinceName).MaximumLength(100);
            RuleFor(x => x.NewWardName).MaximumLength(100);
        }
    }
}
