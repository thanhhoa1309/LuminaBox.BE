using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Address
{
    public class CreateAddressRequest
    {
        public AddressType Type { get; set; } = AddressType.Shipping;
        public string ContactName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public Guid WardId { get; set; }
        public string AddressLine { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? MapRefId { get; set; }
        public string? NewProvinceName { get; set; }
        public string? NewWardName { get; set; }
    }
}
