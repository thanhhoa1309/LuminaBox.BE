using FluentValidation;
using LuminaBox.Application.Dtos.Auth;

namespace LuminaBox.Application.Validations.Auth
{
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .MaximumLength(100).WithMessage("Full name must not exceed 100 characters.");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email is invalid.")
                .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required.")
                .Equal(x => x.Password).WithMessage("Confirm password does not match.");

            RuleFor(x => x.Phone)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Phone is required.")
                .Matches(@"^(0|\+84)(3|5|7|8|9)\d{8}$").WithMessage("Phone number is invalid.");

            RuleFor(x => x.Address)
                .MaximumLength(255).WithMessage("Address must not exceed 255 characters.");
        }
    }
}
