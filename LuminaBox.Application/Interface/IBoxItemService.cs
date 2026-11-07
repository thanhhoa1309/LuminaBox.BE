using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Box;
using LuminaBox.Application.Dtos.Product;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Interface
{
    public interface IBoxItemService
    {
        Task<Pagination<BoxItemResponse>> GetAllAsync(
            string? search,
            int page,
            int pageSize,
            Guid? stallId = null,
            Guid? organizerId = null);

        Task<BoxItemResponse> GetByIdAsync(Guid id);

        Task<Pagination<BoxItemResponse>> GetMineAsync(string? search, int page, int pageSize, ProductStatus? status = null);

        Task<BoxItemResponse> CreateAsync(CreateBoxItemRequest request);

        Task<BoxItemResponse> UpdateAsync(Guid id, UpdateBoxItemRequest request);

        Task<BoxItemResponse> SetStatusAsync(Guid id, ProductStatus status);

        Task DeleteAsync(Guid id);

        Task<BoxItemResponse> AddImageAsync(Guid id, AddProductItemImageRequest request);

        Task<BoxItemResponse> RemoveImageAsync(Guid id, Guid imageId);

        Task<BoxItemResponse> SetPriceTiersAsync(Guid id, List<PriceTierRequest> tiers);

        Task<BoxItemResponse> SetStocksAsync(Guid id, List<StockRequest> stocks);
    }
}
