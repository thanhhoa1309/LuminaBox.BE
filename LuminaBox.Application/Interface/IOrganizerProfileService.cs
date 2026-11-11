using LuminaBox.Application.Dtos.Organizer;

namespace LuminaBox.Application.Interface
{
    public interface IOrganizerProfileService
    {
        Task<OrganizerProfileResponse> GetMineAsync();
        Task<OrganizerProfileResponse> UpdateMineAsync(UpdateOrganizerProfileRequest request);
    }
}
