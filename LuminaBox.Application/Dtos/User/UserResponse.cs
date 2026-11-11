using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Dtos.User
{
    public class UserResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? OrganizerId { get; set; }
        public string? OrganizerBrandName { get; set; }
        public OrganizerStatus? OrganizerStatus { get; set; }
    }
}
