namespace LuminaBox.Application.Dtos.Address
{
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
