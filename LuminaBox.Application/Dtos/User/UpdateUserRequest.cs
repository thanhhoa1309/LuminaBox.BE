namespace LuminaBox.Application.Dtos.User
{
    public class UpdateUserRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }
}
