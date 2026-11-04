using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Product
{
    public class UpdateProductStatusRequest
    {
        public ProductStatus Status { get; set; }
    }
}
