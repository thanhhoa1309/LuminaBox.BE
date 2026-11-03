using FluentValidation;
using LuminaBox.Application.Dtos.Auth;

namespace LuminaBox.Application.Validations.Auth
{
    public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email is invalid.");

            RuleFor(x => x.OtpCode)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("OTP code is required.")
                .Matches(@"^\d{6}$").WithMessage("OTP code must be 6 digits.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required.")
                .MinimumLength(8).WithMessage("New password must be at least 8 characters.")
                .Matches("[A-Z]").WithMessage("New password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("New password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("New password must contain at least one digit.")
                .Matches("[^a-zA-Z0-9]").WithMessage("New password must contain at least one special character.");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("Confirm new password is required.")
                .Equal(x => x.NewPassword).WithMessage("Confirm new password does not match.");
        }
    }
}
