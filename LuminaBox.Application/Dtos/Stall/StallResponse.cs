using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.Stall
{
    public class StallResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid ProvinceId { get; set; }
        public string ProvinceName { get; set; } = string.Empty;
        public Guid DistrictId { get; set; }
        public string DistrictName { get; set; } = string.Empty;
        public Guid WardId { get; set; }
        public string WardName { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public decimal RentalPrice { get; set; }
        public string? Description { get; set; }
        public StallStatus Status { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? MapRefId { get; set; }
        public string? NewProvinceName { get; set; }
        public string? NewWardName { get; set; }
        public List<StallBookingResponse> Bookings { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
