using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    public class Stall : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Location { get; set; }
        public decimal RentalPrice { get; set; }
        public string? Description { get; set; }
        public StallStatus Status { get; set; } = StallStatus.Available;

        public ICollection<OrganizerRegistration> Registrations { get; set; } = new List<OrganizerRegistration>();
        public ICollection<StallRental> Rentals { get; set; } = new List<StallRental>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
        public ICollection<BoxItem> BoxItems { get; set; } = new List<BoxItem>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
