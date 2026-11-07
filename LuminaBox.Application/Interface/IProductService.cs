using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Product;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Interface
{
    public interface IProductService
    {
        Task<Pagination<ProductSummaryResponse>> GetAllAsync(ProductListQuery filter);

        Task<ProductResponse> GetByIdAsync(Guid id);

        Task<Pagination<ProductResponse>> GetMineAsync(MyProductListQuery filter);

        Task<ProductResponse> CreateAsync(CreateProductRequest request);

        Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request);

        Task<ProductResponse> SetStatusAsync(Guid id, UpdateProductStatusRequest request);

        Task DeleteAsync(Guid id);
    }
}
