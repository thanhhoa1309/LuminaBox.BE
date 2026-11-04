namespace LuminaBox.Application.Dtos.Shipping
{
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
