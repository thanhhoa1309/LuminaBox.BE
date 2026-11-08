namespace LuminaBox.Application.Dtos.Shipping
{
    public class GhnCreateWaybillRequest
    {
        public string ClientOrderCode { get; set; } = string.Empty;
        public string ToName { get; set; } = string.Empty;
        public string ToPhone { get; set; } = string.Empty;
        public string ToAddress { get; set; } = string.Empty;
        public string ToWardCode { get; set; } = string.Empty;
        public int ToDistrictId { get; set; }
        public string FromName { get; set; } = string.Empty;
        public string FromPhone { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public string FromWardCode { get; set; } = string.Empty;
        public int FromDistrictId { get; set; }
        public int CodAmount { get; set; }
        public int WeightGrams { get; set; }
        public int LengthCm { get; set; }
        public int WidthCm { get; set; }
        public int HeightCm { get; set; }
        public string? Note { get; set; }
        public List<GhnCreateWaybillItemRequest> Items { get; set; } = new();
    }

    public class GhnCreateWaybillItemRequest
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int WeightGrams { get; set; }
    }

    public class GhnCreateWaybillResponse
    {
        public string OrderCode { get; set; } = string.Empty;
        public string? Status { get; set; }
    }
}
