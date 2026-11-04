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
}
