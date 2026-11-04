using LuminaBox.Domain.Entities;

namespace LuminaBox.Application.Interface
{
    public interface IOrganizerAccessService
    {
        /// <summary>The current user's organizer, ensured active with an approved online registration; throws Forbidden (E5.1) otherwise.</summary>
        Task<Organizer> GetSellingOrganizerAsync();
    }
}
