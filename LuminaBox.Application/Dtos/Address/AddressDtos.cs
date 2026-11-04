using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Address
{
    public class AddressResponse
    {
        public Guid Id { get; set; }
        public AddressType Type { get; set; }
        public string ContactName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public Guid ProvinceId { get; set; }
        public string ProvinceName { get; set; } = string.Empty;
        public Guid DistrictId { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public Guid WardId { get; set; }
        public string WardName { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? MapRefId { get; set; }
        public string? NewProvinceName { get; set; }
        public string? NewWardName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

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

    public class UpdateAddressRequest
    {
        public string? ContactName { get; set; }
        public string? Phone { get; set; }
        public Guid? WardId { get; set; }
        public string? AddressLine { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? MapRefId { get; set; }
        public string? NewProvinceName { get; set; }
        public string? NewWardName { get; set; }
    }
}
