using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using LuminaBox.Application.Realtime;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Infrastructure.Realtime
{
    // Single hub shared by every realtime feature; users are addressed by their NameIdentifier claim
    [Authorize]
    public class AppHub : Hub
    {
        public const string Route = "/hubs/app";

        public override async Task OnConnectedAsync()
        {
            var role = Context.User?.FindFirstValue(ClaimTypes.Role);

            if (Enum.TryParse<UserRole>(role, out var userRole))
                await Groups.AddToGroupAsync(Context.ConnectionId, RealtimeGroups.Role(userRole));

            await base.OnConnectedAsync();
        }
    }
}
