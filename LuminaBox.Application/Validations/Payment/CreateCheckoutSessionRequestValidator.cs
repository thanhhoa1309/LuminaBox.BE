using FluentValidation;
using LuminaBox.Application.Dtos.Payment;

namespace LuminaBox.Application.Validations.Payment
{
    public class CreateCheckoutSessionRequestValidator : AbstractValidator<CreateCheckoutSessionRequest>
    {
        public CreateCheckoutSessionRequestValidator()
        {
            RuleFor(x => x)
                .Must(x => (x.PaymentId.HasValue && x.PaymentId.Value != Guid.Empty) ||
                           (x.OrderId.HasValue && x.OrderId.Value != Guid.Empty))
                .WithMessage("Either PaymentId or OrderId is required.");

            RuleFor(x => x.SuccessUrl)
                .MaximumLength(500).WithMessage("SuccessUrl must not exceed 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.SuccessUrl));

            RuleFor(x => x.CancelUrl)
                .MaximumLength(500).WithMessage("CancelUrl must not exceed 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.CancelUrl));
        }
    }
}

