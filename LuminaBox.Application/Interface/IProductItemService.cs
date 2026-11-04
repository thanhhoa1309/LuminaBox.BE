using LuminaBox.Application.Dtos.Product;

namespace LuminaBox.Application.Interface
{
    public interface IProductItemService
    {
        Task<ProductItemResponse> CreateAsync(Guid productId, CreateProductItemRequest request);

        Task<ProductItemResponse> UpdateAsync(Guid id, UpdateProductItemRequest request);

        Task DeleteAsync(Guid id);

        Task<ProductItemResponse> AddImageAsync(Guid id, AddProductItemImageRequest request);

        Task<ProductItemResponse> RemoveImageAsync(Guid id, Guid imageId);

        Task<ProductItemResponse> SetPriceTiersAsync(Guid id, List<PriceTierRequest> tiers);

        Task<ProductItemResponse> SetStocksAsync(Guid id, List<StockRequest> stocks);
    }
}
