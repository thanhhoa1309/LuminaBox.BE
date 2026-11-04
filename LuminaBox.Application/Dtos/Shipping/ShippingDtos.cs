namespace LuminaBox.Application.Dtos.Shipping
{
    public class ShippingFeeRequest
    {
        public Guid OrganizerId { get; set; }
        public Guid? StallId { get; set; }
        public Guid AddressId { get; set; }
        public List<ShippingItemRequest> Items { get; set; } = new();
        public List<ShippingBoxRequest> Boxes { get; set; } = new();
    }

    public class ShippingItemRequest
    {
        public Guid ProductItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class ShippingBoxRequest
    {
        public Guid BoxItemId { get; set; }
        public List<ShippingItemRequest> Selections { get; set; } = new();
    }

    public class ShippingFeeResponse
    {
        public decimal Fee { get; set; }
        public int WeightGrams { get; set; }
        public int LengthCm { get; set; }
        public int WidthCm { get; set; }
        public int HeightCm { get; set; }
        public Guid? PickupAddressId { get; set; }
    }

    public class GhnFeeRequest
    {
        public int FromDistrictId { get; set; }
        public string FromWardCode { get; set; } = string.Empty;
        public int ToDistrictId { get; set; }
        public string ToWardCode { get; set; } = string.Empty;
        public int WeightGrams { get; set; }
        public int LengthCm { get; set; }
        public int WidthCm { get; set; }
        public int HeightCm { get; set; }
    }
}
