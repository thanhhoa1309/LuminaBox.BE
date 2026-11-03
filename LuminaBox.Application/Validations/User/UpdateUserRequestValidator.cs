using FluentValidation;
using LuminaBox.Application.Dtos.User;

namespace LuminaBox.Application.Validations.User
{
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

            RuleFor(x => x.Phone)
                .Matches(@"^(0|\+84)(3|5|7|8|9)\d{8}$").WithMessage("Phone number is invalid.")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.Address)
                .MaximumLength(255).WithMessage("Address must not exceed 255 characters.");
        }
    }
}
