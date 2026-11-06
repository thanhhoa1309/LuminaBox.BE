using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.OrganizerRegistration;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Interface
{
    public interface IOrganizerRegistrationService
    {
        Task<Pagination<OrganizerRegistrationResponse>> GetAllAsync(
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            RegistrationType? type = null,
            RegistrationStatus? status = null,
            Guid? stallId = null);
        Task<Pagination<OrganizerRegistrationResponse>> GetMyRegistrationsAsync(
            string? search,
            string? sortBy,
            bool isDescending,
            int page,
            int pageSize,
            RegistrationType? type = null,
            RegistrationStatus? status = null);
        Task<OrganizerRegistrationResponse> GetOrganizerRegistrationByDetail(Guid id);
        Task<OrganizerRegistrationResponse> CreateOrganizerRegistrationAsync(CreateOrganizerRegistrationRequest request);
        Task<List<OrganizerRegistrationResponse>> CreateAdditionalStallsAsync(CreateAdditionalStallsRequest request);
        Task<OrganizerRegistrationResponse> CreateAdditionalOnlineAsync();
        Task<OrganizerRegistrationResponse> ApproveOrganizerRegistrationAsync(Guid id, ApproveOrganizerRegistrationRequest? request);
        Task<OrganizerRegistrationResponse> RejectOrganizerRegistrationAsync(Guid id, RejectOrganizerRegistrationRequest request);
        Task<OrganizerRegistrationResponse> ConfirmOrganizerRegistrationAsync(Guid id);
        Task<OrganizerRegistrationResponse> CancelOrganizerRegistrationAsync(Guid id, CancelOrganizerRegistrationRequest? request);
    }
}
