namespace LuminaBox.Application.Dtos.Shipping
{
    public class ShippingFeeResponse
    {
        public decimal Fee { get; set; }
        public int WeightGrams { get; set; }
        public int LengthCm { get; set; }
        public int WidthCm { get; set; }
        public int HeightCm { get; set; }
        public Guid? PickupAddressId { get; set; }
    }
}
