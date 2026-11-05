using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
    // Overlapping Confirmed/Active rentals of a stall are also rejected by the database
    // through the EX_StallRentals_NoOverlap constraint, created by a raw SQL migration
    public class StallRental : BaseEntity
    {
        public Guid StallId { get; set; }
        public Stall Stall { get; set; } = null!;

        public Guid OrganizerId { get; set; }
        public Organizer Organizer { get; set; } = null!;

        public Guid? RegistrationId { get; set; }
        public OrganizerRegistration? Registration { get; set; }

        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public RentalStatus Status { get; set; } = RentalStatus.Active;
    }
}
