namespace LuminaBox.Application.Dtos.OrganizerRegistration
{
    public class CreateAdditionalStallsRequest
    {
        public List<Guid> StallIds { get; set; } = new();
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }
}
