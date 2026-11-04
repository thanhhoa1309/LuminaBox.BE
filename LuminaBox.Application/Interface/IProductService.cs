using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Product;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Interface
{
    public interface IProductService
    {
        Task<Pagination<ProductSummaryResponse>> GetAllAsync(
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            Guid? categoryId = null,
            Guid? fillingId = null,
            Guid? organizerId = null,
            Guid? stallId = null);

        Task<ProductResponse> GetByIdAsync(Guid id);

        Task<Pagination<ProductResponse>> GetMineAsync(
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            ProductStatus? status = null,
            Guid? categoryId = null);

        Task<ProductResponse> CreateAsync(CreateProductRequest request);

        Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request);

        Task<ProductResponse> SetStatusAsync(Guid id, UpdateProductStatusRequest request);

        Task DeleteAsync(Guid id);
    }
}
