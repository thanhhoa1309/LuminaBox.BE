using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Realtime
{
    public static class RealtimeGroups
    {
        public static string Role(UserRole role) => $"role:{role}";
    }
}
