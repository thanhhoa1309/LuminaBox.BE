namespace LuminaBox.Application.Dtos.Stall
{
    public class UpdateStallRequest
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? WardId { get; set; }
        public string? AddressLine { get; set; }
        public decimal? RentalPrice { get; set; }
        public string? Description { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? MapRefId { get; set; }
        public string? NewProvinceName { get; set; }
        public string? NewWardName { get; set; }
    }
}
