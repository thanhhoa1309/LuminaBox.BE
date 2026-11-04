using FluentValidation;
using LuminaBox.Application.Dtos.Product;

namespace LuminaBox.Application.Validations.Product
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");
        }
    }

    public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
    {
        public UpdateProductRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name must not be empty.")
                .MaximumLength(200).WithMessage("Product name must not exceed 200 characters.")
                .When(x => x.Name is not null);

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");
        }
    }

    public class UpdateProductStatusRequestValidator : AbstractValidator<UpdateProductStatusRequest>
    {
        public UpdateProductStatusRequestValidator()
        {
            RuleFor(x => x.Status).IsInEnum().WithMessage("Status is invalid.");
        }
    }

    public class CreateProductItemRequestValidator : AbstractValidator<CreateProductItemRequest>
    {
        public CreateProductItemRequestValidator()
        {
            RuleFor(x => x.CategoryId).NotEmpty().WithMessage("Category is required.");
            RuleFor(x => x.FillingId).NotEmpty().WithMessage("Filling is required.");
            RuleFor(x => x.Name).MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
            RuleFor(x => x.EggCount).GreaterThanOrEqualTo(0).WithMessage("Egg count must not be negative.");
            RuleFor(x => x.Weight).GreaterThan(0).WithMessage("Weight must be greater than 0.");
            RuleFor(x => x.LengthCm).GreaterThan(0).WithMessage("Length must be greater than 0.");
            RuleFor(x => x.WidthCm).GreaterThan(0).WithMessage("Width must be greater than 0.");
            RuleFor(x => x.HeightCm).GreaterThan(0).WithMessage("Height must be greater than 0.");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x.ImageUrls)
                .NotEmpty().WithMessage("At least one image is required.")
                .Must(urls => urls.Count <= 10).WithMessage("A product item can have at most 10 images.");

            RuleForEach(x => x.ImageUrls)
                .NotEmpty().WithMessage("Image url is required.")
                .MaximumLength(1000).WithMessage("Image url must not exceed 1000 characters.");
        }
    }

    public class UpdateProductItemRequestValidator : AbstractValidator<UpdateProductItemRequest>
    {
        public UpdateProductItemRequestValidator()
        {
            RuleFor(x => x.CategoryId).NotEmpty().When(x => x.CategoryId.HasValue).WithMessage("Category is invalid.");
            RuleFor(x => x.FillingId).NotEmpty().When(x => x.FillingId.HasValue).WithMessage("Filling is invalid.");
            RuleFor(x => x.Name).MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
            RuleFor(x => x.EggCount).GreaterThanOrEqualTo(0).When(x => x.EggCount.HasValue).WithMessage("Egg count must not be negative.");
            RuleFor(x => x.Weight).GreaterThan(0).When(x => x.Weight.HasValue).WithMessage("Weight must be greater than 0.");
            RuleFor(x => x.LengthCm).GreaterThan(0).When(x => x.LengthCm.HasValue).WithMessage("Length must be greater than 0.");
            RuleFor(x => x.WidthCm).GreaterThan(0).When(x => x.WidthCm.HasValue).WithMessage("Width must be greater than 0.");
            RuleFor(x => x.HeightCm).GreaterThan(0).When(x => x.HeightCm.HasValue).WithMessage("Height must be greater than 0.");
            RuleFor(x => x.Price).GreaterThan(0).When(x => x.Price.HasValue).WithMessage("Price must be greater than 0.");
        }
    }

    public class AddProductItemImageRequestValidator : AbstractValidator<AddProductItemImageRequest>
    {
        public AddProductItemImageRequestValidator()
        {
            RuleFor(x => x.Url)
                .NotEmpty().WithMessage("Image url is required.")
                .MaximumLength(1000).WithMessage("Image url must not exceed 1000 characters.");

            RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0).When(x => x.SortOrder.HasValue)
                .WithMessage("Sort order must not be negative.");
        }
    }

    public class PriceTierRequestValidator : AbstractValidator<PriceTierRequest>
    {
        public PriceTierRequestValidator()
        {
            RuleFor(x => x.MinQuantity).GreaterThanOrEqualTo(2).WithMessage("Minimum quantity must be at least 2.");
            RuleFor(x => x.UnitPrice).GreaterThan(0).WithMessage("Unit price must be greater than 0.");
        }
    }

    public class StockRequestValidator : AbstractValidator<StockRequest>
    {
        public StockRequestValidator()
        {
            RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0).WithMessage("Stock quantity must not be negative.");
        }
    }
}
