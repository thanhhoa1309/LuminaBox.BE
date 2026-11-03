using LuminaBox.Domain.Enums;

namespace LuminaBox.Domain.Entities
{
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
