using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class Stall : BaseEntity
    {
        public Guid WardId { get; set; }
        public Ward Ward { get; set; } = null!;

        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public decimal RentalPrice { get; set; }
        public string? Description { get; set; }
        public StallStatus Status { get; set; } = StallStatus.Available;

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? MapRefId { get; set; }
        public string? NewProvinceName { get; set; }
        public string? NewWardName { get; set; }

        public ICollection<OrganizerRegistration> Registrations { get; set; } = new List<OrganizerRegistration>();
        public ICollection<StallRental> Rentals { get; set; } = new List<StallRental>();
        public ICollection<StallProductItem> StallProductItems { get; set; } = new List<StallProductItem>();
        public ICollection<StallBoxItem> StallBoxItems { get; set; } = new List<StallBoxItem>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
