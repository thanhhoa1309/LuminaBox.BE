using FluentValidation;
using LuminaBox.Application.Dtos.OrganizerRegistration;

namespace LuminaBox.Application.Validations.OrganizerRegistration
{
    public class ApproveOrganizerRegistrationRequestValidator : AbstractValidator<ApproveOrganizerRegistrationRequest>
    {
        public ApproveOrganizerRegistrationRequestValidator()
        {
            RuleFor(x => x.CommissionRate)
                .Cascade(CascadeMode.Stop)
                .InclusiveBetween(0, 100).WithMessage("Commission rate must be between 0 and 100.")
                .PrecisionScale(5, 2, false).WithMessage("Commission rate must have at most 2 decimal places.")
                .When(x => x.CommissionRate.HasValue);
        }
    }
}
