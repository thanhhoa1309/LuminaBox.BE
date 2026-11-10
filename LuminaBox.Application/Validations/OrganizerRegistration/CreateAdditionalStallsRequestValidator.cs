using FluentValidation;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.OrganizerRegistration;

namespace LuminaBox.Application.Validations.OrganizerRegistration
{
    public class CreateAdditionalStallsRequestValidator : AbstractValidator<CreateAdditionalStallsRequest>
    {
        private const int MaxStallsPerRequest = 10;

        public CreateAdditionalStallsRequestValidator()
        {
            RuleFor(x => x.StallIds)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("At least one stall is required.")
                .Must(ids => ids.Count <= MaxStallsPerRequest)
                .WithMessage($"Cannot request more than {MaxStallsPerRequest} stalls at once.")
                .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Stall ids must not be duplicated.");

            RuleForEach(x => x.StallIds)
                .NotEmpty().WithMessage("Stall id must not be empty.");

            RuleFor(x => x.StartDate)
                .NotNull().WithMessage("Start date is required.");

            RuleFor(x => x.EndDate)
                .NotNull().WithMessage("End date is required.");

            RuleFor(x => x)
                .Must(x => x.EndDate > x.StartDate).WithMessage("End date must be after start date.")
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

            RuleFor(x => x)
                .Must(x => RentalRules.IsLongEnough(x.StartDate, x.EndDate))
                .WithMessage($"A stall must be rented for at least {RentalRules.MinDays} days.")
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue && x.EndDate > x.StartDate);
        }
    }
}
