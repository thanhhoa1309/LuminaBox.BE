using LuminaBox.Application.Commons;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Product
{
    public class ProductListQuery : ListQuery
    {
        public Guid? CategoryId { get; set; }
        public Guid? FillingId { get; set; }
        public Guid? OrganizerId { get; set; }
        public Guid? StallId { get; set; }
    }

    public class MyProductListQuery : ListQuery
    {
        public ProductStatus? Status { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
