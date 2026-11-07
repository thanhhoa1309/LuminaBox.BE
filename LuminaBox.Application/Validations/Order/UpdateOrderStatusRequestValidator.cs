using FluentValidation;
using LuminaBox.Application.Dtos.Order;

namespace LuminaBox.Application.Validations.Order
{
    public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
    {
        public UpdateOrderStatusRequestValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Invalid order status.");
        }
    }
}

