using FluentValidation;
using LuminaBox.Application.Dtos.Stall;

namespace LuminaBox.Application.Validations.Stall
{
    public class CreateStallRequestValidator : AbstractValidator<CreateStallRequest>
    {
        public CreateStallRequestValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Stall code is required.")
                .MaximumLength(50).WithMessage("Stall code must not exceed 50 characters.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Stall name is required.")
                .MaximumLength(200).WithMessage("Stall name must not exceed 200 characters.");

            RuleFor(x => x.WardId).NotEmpty().WithMessage("Ward is required.");

            RuleFor(x => x.AddressLine)
                .NotEmpty().WithMessage("Address line is required.")
                .MaximumLength(500).WithMessage("Address line must not exceed 500 characters.");

            RuleFor(x => x.RentalPrice).GreaterThan(0).WithMessage("Rental price must be greater than 0.");
            RuleFor(x => x.Description).MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

            RuleFor(x => x.Latitude).InclusiveBetween(-90m, 90m).When(x => x.Latitude.HasValue)
                .WithMessage("Latitude is invalid.");
            RuleFor(x => x.Longitude).InclusiveBetween(-180m, 180m).When(x => x.Longitude.HasValue)
                .WithMessage("Longitude is invalid.");

            RuleFor(x => x.MapRefId).MaximumLength(255);
            RuleFor(x => x.NewProvinceName).MaximumLength(100);
            RuleFor(x => x.NewWardName).MaximumLength(100);
        }
    }

    public class UpdateStallRequestValidator : AbstractValidator<UpdateStallRequest>
    {
        public UpdateStallRequestValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Stall code must not be empty.")
                .MaximumLength(50).WithMessage("Stall code must not exceed 50 characters.")
                .When(x => x.Code is not null);

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Stall name must not be empty.")
                .MaximumLength(200).WithMessage("Stall name must not exceed 200 characters.")
                .When(x => x.Name is not null);

            RuleFor(x => x.WardId).NotEmpty().When(x => x.WardId.HasValue).WithMessage("Ward is invalid.");

            RuleFor(x => x.AddressLine)
                .NotEmpty().WithMessage("Address line must not be empty.")
                .MaximumLength(500).WithMessage("Address line must not exceed 500 characters.")
                .When(x => x.AddressLine is not null);

            RuleFor(x => x.RentalPrice).GreaterThan(0).When(x => x.RentalPrice.HasValue)
                .WithMessage("Rental price must be greater than 0.");
            RuleFor(x => x.Description).MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

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
