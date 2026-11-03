using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace LuminaBox.Infrastructure.Commons
{
    public static class AuthenTools
    {
        public static string? GetCurrentUserId(ClaimsIdentity? identity, ILogger? logger = null)
        {
            if (identity == null)
                return null;

            // "sub" covers tokens read with MapInboundClaims disabled, where NameIdentifier is not mapped
            var userId = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? identity.FindFirst("sub")?.Value;

            logger?.LogDebug("Extracted UserId from claims: {UserId}", userId);
            return userId;
        }
    }
}
