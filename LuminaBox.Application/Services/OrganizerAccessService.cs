using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class OrganizerAccessService : IOrganizerAccessService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;

        public OrganizerAccessService(IUnitOfWork unitOfWork, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
        }

        public async Task<Organizer> GetSellingOrganizerAsync()
        {
            var userId = _claimsService.GetRequiredUserId();

            var organizer = await _unitOfWork.Organizers.FirstOrDefaultAsync(o => o.UserId == userId);

            if (organizer is null || organizer.Status != OrganizerStatus.Active)
                throw ErrorHelper.Forbidden("You are not allowed to sell products.", "E5.1");

            var hasOnlineContract = await _unitOfWork.OrganizerRegistrations.GetQueryable()
                .AnyAsync(r => r.OrganizerId == organizer.Id
                    && r.Type == RegistrationType.Online
                    && r.Status == RegistrationStatus.Approved);

            if (!hasOnlineContract)
                throw ErrorHelper.Forbidden("You do not have an approved online selling contract.", "E5.1");

            return organizer;
        }
    }
}
