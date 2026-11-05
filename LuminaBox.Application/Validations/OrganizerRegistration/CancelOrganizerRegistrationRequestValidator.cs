using FluentValidation;
using LuminaBox.Application.Dtos.OrganizerRegistration;

namespace LuminaBox.Application.Validations.OrganizerRegistration
{
    public class CancelOrganizerRegistrationRequestValidator : AbstractValidator<CancelOrganizerRegistrationRequest>
    {
        public CancelOrganizerRegistrationRequestValidator()
        {
            RuleFor(x => x.CancelReason)
                .MaximumLength(500).WithMessage("Cancel reason must not exceed 500 characters.");
        }
    }
}
