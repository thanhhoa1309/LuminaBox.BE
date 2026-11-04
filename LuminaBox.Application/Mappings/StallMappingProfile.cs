using AutoMapper;
using LuminaBox.Application.Dtos.Stall;
using LuminaBox.Domain.Entities;

namespace LuminaBox.Application.Mappings
{
    public class StallMappingProfile : Profile
    {
        public StallMappingProfile()
        {
            CreateMap<decimal?, decimal>().ConvertUsing((source, destination) => source ?? destination);

            CreateMap<Stall, StallResponse>();

            CreateMap<CreateStallRequest, Stall>(MemberList.Source)
                .AddTransform<string?>(value => value == null ? null : value.Trim());

            CreateMap<UpdateStallRequest, Stall>(MemberList.Source)
                .ForMember(d => d.WardId, o => o.Ignore())
                .AddTransform<string?>(value => value == null ? null : value.Trim())
                .ForAllMembers(o => o.Condition((_, _, member) => member != null));
        }
    }
}
