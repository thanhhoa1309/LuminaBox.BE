using FluentValidation;
using LuminaBox.Application.Dtos.Notification;

namespace LuminaBox.Application.Validations.Notification
{
    public class MarkNotificationsReadRequestValidator : AbstractValidator<MarkNotificationsReadRequest>
    {
        public MarkNotificationsReadRequestValidator()
        {
            RuleFor(x => x.NotificationIds)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Notification ids are required when not marking all.")
                .Must(ids => ids!.Count <= 100).WithMessage("Cannot mark more than 100 notifications at once.")
                .When(x => !x.MarkAll);

            RuleForEach(x => x.NotificationIds)
                .NotEmpty().WithMessage("Notification id must not be empty.")
                .When(x => !x.MarkAll);
        }
    }
}
