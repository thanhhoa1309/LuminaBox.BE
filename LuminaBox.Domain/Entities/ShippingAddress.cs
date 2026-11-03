using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class ShippingAddress : BaseEntity
    {
        public AddressOwnerType OwnerType { get; set; }
        public Guid OwnerId { get; set; }

        public string? Label { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;

        public int ToProvinceId { get; set; }
        public int ToWardId { get; set; }
        public string ToProvinceName { get; set; } = string.Empty;
        public string ToWardName { get; set; } = string.Empty;
        public string AddressDetail { get; set; } = string.Empty;

        /// <summary>Only meaningful when OwnerType = User.</summary>
        public bool IsDefault { get; set; }
    }
}
