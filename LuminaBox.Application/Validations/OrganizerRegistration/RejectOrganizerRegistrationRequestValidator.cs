using FluentValidation;
using LuminaBox.Application.Dtos.OrganizerRegistration;

namespace LuminaBox.Application.Validations.OrganizerRegistration
{
    public class RejectOrganizerRegistrationRequestValidator : AbstractValidator<RejectOrganizerRegistrationRequest>
    {
        public RejectOrganizerRegistrationRequestValidator()
        {
            RuleFor(x => x.RejectReason)
                .NotEmpty().WithMessage("Reject reason is required.")
                .MaximumLength(500).WithMessage("Reject reason must not exceed 500 characters.");
        }
    }
}
