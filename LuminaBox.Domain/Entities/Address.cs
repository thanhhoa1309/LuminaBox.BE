using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class Address : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid WardId { get; set; }
        public Ward Ward { get; set; } = null!;

        public string ContactName { get; set; } = string.Empty;
        public AddressType Type { get; set; } = AddressType.Shipping;
        public string Phone { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public bool IsDefault { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? MapRefId { get; set; }
        public string? NewProvinceName { get; set; }
        public string? NewWardName { get; set; }
    }
}
