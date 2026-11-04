using LuminaBox.Application.Dtos.Address;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Interface
{
    public interface IAddressService
    {
        Task<List<AddressResponse>> GetMineAsync(AddressType? type);

        Task<AddressResponse> CreateAsync(CreateAddressRequest request);

        Task<AddressResponse> UpdateAsync(Guid id, UpdateAddressRequest request);

        Task<AddressResponse> SetDefaultAsync(Guid id);

        Task DeleteAsync(Guid id);
    }
}
