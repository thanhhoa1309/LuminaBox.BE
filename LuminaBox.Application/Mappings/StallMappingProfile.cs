using AutoMapper;
using LuminaBox.Application.Dtos.Stall;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Mappings
{
    public class StallMappingProfile : Profile
    {
        public StallMappingProfile()
        {
            CreateMap<decimal?, decimal>().ConvertUsing((source, destination) => source ?? destination);

            CreateMap<Stall, StallResponse>()
                .ForMember(d => d.ProvinceId, o => o.MapFrom(s => s.Ward.District.ProvinceId))
                .ForMember(d => d.ProvinceName, o => o.MapFrom(s => s.Ward.District.Province.Name))
                .ForMember(d => d.DistrictId, o => o.MapFrom(s => s.Ward.DistrictId))
                .ForMember(d => d.DistrictName, o => o.MapFrom(s => s.Ward.District.Name))
                .ForMember(d => d.Bookings, o => o.MapFrom(s => s.Rentals
                    .Where(r => !r.IsDeleted && (r.Status == RentalStatus.Confirmed || r.Status == RentalStatus.Active))
                    .OrderBy(r => r.StartDate)));

            CreateMap<StallRental, StallBookingResponse>();

            CreateMap<CreateStallRequest, Stall>(MemberList.Source)
                .AddTransform<string?>(value => value == null ? null : value.Trim());

            CreateMap<UpdateStallRequest, Stall>(MemberList.Source)
                .ForMember(d => d.WardId, o => o.Ignore())
                .AddTransform<string?>(value => value == null ? null : value.Trim())
                .ForAllMembers(o => o.Condition((_, _, member) => member != null));
        }
    }
}
