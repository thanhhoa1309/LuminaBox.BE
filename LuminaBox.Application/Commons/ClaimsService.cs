using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;

namespace LuminaBox.Application.Commons
{
    public class ClaimsService : IClaimsService
    {
        // Cached in HttpContext.Items (not a field) so the value stays per request even if this
        // service ends up captured by a singleton; only resolved ids are cached so a read before
        // authentication runs cannot pin the request to anonymous
        private static readonly object CurrentUserIdKey = new();

        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClaimsService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetCurrentUserId
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext is null)
                    return Guid.Empty;

                if (httpContext.Items.TryGetValue(CurrentUserIdKey, out var cached) && cached is Guid cachedId)
                    return cachedId;

                var user = httpContext.User;
                var id = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");

                if (!Guid.TryParse(id, out var userId) || userId == Guid.Empty)
                    return Guid.Empty;

                httpContext.Items[CurrentUserIdKey] = userId;
                return userId;
            }
        }

        public Guid GetRequiredUserId()
        {
            var userId = GetCurrentUserId;
            return userId != Guid.Empty ? userId : throw ErrorHelper.Unauthorized();
        }
    }
}
