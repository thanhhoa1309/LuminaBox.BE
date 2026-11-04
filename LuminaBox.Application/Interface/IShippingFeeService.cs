using LuminaBox.Application.Dtos.Shipping;

namespace LuminaBox.Application.Interface
{
    public interface IShippingFeeService
    {
        /// <summary>Calculates the GHN fee for one order: its pickup point, the buyer's address and the package.</summary>
        Task<ShippingFeeResponse> EstimateAsync(ShippingFeeRequest request);
    }

    public interface IGhnClient
    {
        Task<decimal> CalculateFeeAsync(GhnFeeRequest request, CancellationToken cancellationToken = default);
    }
}
