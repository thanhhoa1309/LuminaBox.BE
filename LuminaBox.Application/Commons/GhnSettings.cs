namespace LuminaBox.Application.Commons
{
    public sealed class GhnSettings
    {
        public string Token { get; set; } = string.Empty;
        public int ShopId { get; set; }
        public string BaseUrl { get; set; } = "https://dev-online-gateway.ghn.vn";
        public int DefaultWeightGrams { get; set; } = 500;
        public int DefaultLengthCm { get; set; } = 20;
        public int DefaultWidthCm { get; set; } = 15;
        public int DefaultHeightCm { get; set; } = 10;
        public int ServiceTypeId { get; set; } = 2;
        public string RequiredNote { get; set; } = "CHOXEMHANGKHONGTHU";

        /// <summary>
        /// When true and Token is empty, CalculateFee returns <see cref="FallbackFeeVnd"/> (local/dev only).
        /// </summary>
        public bool UseFallbackFee { get; set; }

        public decimal FallbackFeeVnd { get; set; } = 30000;

        /// <summary>
        /// When true, CreateOrder skips GHN and returns a fake waybill (local/dev only).
        /// </summary>
        public bool UseFakeWaybill { get; set; }
    }
}

