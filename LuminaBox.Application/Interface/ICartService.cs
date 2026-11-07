using LuminaBox.Application.Dtos.Cart;

namespace LuminaBox.Application.Interface
{
    public interface ICartService
    {
        Task<CartResponse> GetMyCartAsync();
        Task<CartResponse> AddProductToCartAsync(AddProductToCartRequest request);
        Task<CartResponse> AddBoxToCartAsync(AddBoxToCartRequest request);
        Task<CartResponse> UpdateCartItemAsync(Guid itemId, UpdateCartItemRequest request);
        Task RemoveCartItemAsync(Guid itemId);
    }
}

