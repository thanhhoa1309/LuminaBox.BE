using LuminaBox.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuminaBox.Application.Dtos.StallRental
{
    public class StallRentalResponse
    {
        public Guid Id { get; set; }
        public Guid StallId { get; set; }
        public string StallCode { get; set; } = string.Empty;
        public string StallName { get; set; } = string.Empty;
        public Guid OrganizerId { get; set; }
        public Guid OrganizerUserId { get; set; }
        public string OrganizerBrandName { get; set; } = string.Empty;
        public string OrganizerEmail { get; set; } = string.Empty;
        public Guid? RegistrationId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public RentalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
