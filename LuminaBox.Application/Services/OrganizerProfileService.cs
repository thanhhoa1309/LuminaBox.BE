using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Dtos.Organizer;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class OrganizerProfileService : IOrganizerProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;

        public OrganizerProfileService(IUnitOfWork unitOfWork, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
        }

        public async Task<OrganizerProfileResponse> GetMineAsync()
        {
            var userId = _claimsService.GetRequiredUserId();

            return await _unitOfWork.Organizers.GetQueryable()
                .AsNoTracking()
                .Where(o => o.UserId == userId)
                .Select(o => new OrganizerProfileResponse
                {
                    Id = o.Id,
                    BrandName = o.BrandName,
                    ContactPhone = o.ContactPhone,
                    ContactEmail = o.ContactEmail,
                    BusinessInfo = o.BusinessInfo,
                    Status = o.Status,
                    ApprovedAt = o.ApprovedAt,
                    SuspendReason = o.SuspendReason,
                    SuspendedAt = o.SuspendedAt,
                    CreatedAt = o.CreatedAt
                })
                .FirstOrDefaultAsync()
                ?? throw ErrorHelper.NotFound("Organizer profile not found.", "E3.9");
        }

        public async Task<OrganizerProfileResponse> UpdateMineAsync(UpdateOrganizerProfileRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();

            var organizer = await _unitOfWork.Organizers.FirstOrDefaultAsync(o => o.UserId == userId)
                ?? throw ErrorHelper.NotFound("Organizer profile not found.", "E3.9");

            if (request.BrandName is not null)
                organizer.BrandName = request.BrandName.Trim();

            if (request.ContactPhone is not null)
                organizer.ContactPhone = string.IsNullOrWhiteSpace(request.ContactPhone) ? null : request.ContactPhone.Trim();

            if (request.ContactEmail is not null)
                organizer.ContactEmail = string.IsNullOrWhiteSpace(request.ContactEmail) ? null : request.ContactEmail.Trim();

            if (request.BusinessInfo is not null)
                organizer.BusinessInfo = string.IsNullOrWhiteSpace(request.BusinessInfo) ? null : request.BusinessInfo.Trim();

            await _unitOfWork.Organizers.Update(organizer);
            await _unitOfWork.SaveChangesAsync();

            return await GetMineAsync();
        }
    }
}
