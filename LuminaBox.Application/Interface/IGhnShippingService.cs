using LuminaBox.Domain.Entities;

namespace LuminaBox.Application.Interface
{
    public interface IGhnShippingService
    {
        Task<decimal> CalculateFeeAsync(
            ShippingAddress fromAddress,
            ShippingAddress toAddress,
            decimal insuranceValue,
            int itemCount);

        Task<(string orderCode, string status)> CreateShippingOrderAsync(
            Order order,
            ShippingAddress fromAddress,
            int totalItemQuantity);

        Task<string> GetShippingOrderStatusAsync(string ghnOrderCode);
    }
}

