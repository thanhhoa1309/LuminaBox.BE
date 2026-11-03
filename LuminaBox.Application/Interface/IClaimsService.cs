namespace LuminaBox.Application.Interface
{
    public interface IClaimsService
    {
        /// <summary>Id of the authenticated user, or <see cref="Guid.Empty"/> when anonymous.</summary>
        Guid GetCurrentUserId { get; }

        /// <summary>Id of the authenticated user; throws Unauthorized when anonymous.</summary>
        Guid GetRequiredUserId();
    }
}
