using FluentValidation;
using LuminaBox.Application.Dtos.Box;

namespace LuminaBox.Application.Validations.Box
{
    public class CreateBoxItemRequestValidator : AbstractValidator<CreateBoxItemRequest>
    {
        public CreateBoxItemRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Box name is required.")
                .MaximumLength(200).WithMessage("Box name must not exceed 200 characters.");

            RuleFor(x => x.Description).MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");
            RuleFor(x => x.Capacity).GreaterThan(0).WithMessage("Capacity must be greater than 0.");
            RuleFor(x => x.Weight).GreaterThan(0).WithMessage("Weight must be greater than 0.");
            RuleFor(x => x.LengthCm).GreaterThan(0).WithMessage("Length must be greater than 0.");
            RuleFor(x => x.WidthCm).GreaterThan(0).WithMessage("Width must be greater than 0.");
            RuleFor(x => x.HeightCm).GreaterThan(0).WithMessage("Height must be greater than 0.");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x.ImageUrls)
                .NotEmpty().WithMessage("At least one image is required.")
                .Must(urls => urls.Count <= 10).WithMessage("A box item can have at most 10 images.");

            RuleForEach(x => x.ImageUrls)
                .NotEmpty().WithMessage("Image url is required.")
                .MaximumLength(1000).WithMessage("Image url must not exceed 1000 characters.");
        }
    }

    public class UpdateBoxItemRequestValidator : AbstractValidator<UpdateBoxItemRequest>
    {
        public UpdateBoxItemRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Box name must not be empty.")
                .MaximumLength(200).WithMessage("Box name must not exceed 200 characters.")
                .When(x => x.Name is not null);

            RuleFor(x => x.Description).MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");
            RuleFor(x => x.Capacity).GreaterThan(0).When(x => x.Capacity.HasValue).WithMessage("Capacity must be greater than 0.");
            RuleFor(x => x.Weight).GreaterThan(0).When(x => x.Weight.HasValue).WithMessage("Weight must be greater than 0.");
            RuleFor(x => x.LengthCm).GreaterThan(0).When(x => x.LengthCm.HasValue).WithMessage("Length must be greater than 0.");
            RuleFor(x => x.WidthCm).GreaterThan(0).When(x => x.WidthCm.HasValue).WithMessage("Width must be greater than 0.");
            RuleFor(x => x.HeightCm).GreaterThan(0).When(x => x.HeightCm.HasValue).WithMessage("Height must be greater than 0.");
            RuleFor(x => x.Price).GreaterThan(0).When(x => x.Price.HasValue).WithMessage("Price must be greater than 0.");
        }
    }
}
