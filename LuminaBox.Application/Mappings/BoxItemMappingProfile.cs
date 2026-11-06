using AutoMapper;
using LuminaBox.Application.Dtos.Box;
using LuminaBox.Application.Dtos.Product;
using LuminaBox.Domain.Entities;

namespace LuminaBox.Application.Mappings
{
    public class BoxItemMappingProfile : Profile
    {
        public BoxItemMappingProfile()
        {
            CreateMap<int?, int>().ConvertUsing((source, destination) => source ?? destination);

            CreateMap<BoxItemImage, ProductImageResponse>();
            CreateMap<PriceTier, PriceTierResponse>();
            CreateMap<StallBoxItem, StockResponse>()
                .ForMember(d => d.StallAddress, o => o.MapFrom(s => s.Stall != null
                    ? s.Stall.AddressLine + ", " + s.Stall.Ward.Name + ", " + s.Stall.Ward.District.Name + ", " + s.Stall.Ward.District.Province.Name
                    : null));

            CreateMap<BoxItem, BoxItemResponse>()
                .ForMember(d => d.Images, o => o.MapFrom(s => s.Images.OrderBy(i => i.SortOrder)))
                .ForMember(d => d.PriceTiers, o => o.MapFrom(s => s.PriceTiers.OrderBy(t => t.MinQuantity)))
                .ForMember(d => d.Stocks, o => o.MapFrom(s => s.Stocks.OrderBy(i => i.StallId)));

            CreateMap<CreateBoxItemRequest, BoxItem>(MemberList.Source)
                .ForMember(d => d.Images, o => o.MapFrom(s => s.ImageUrls.Select((url, index) => new BoxItemImage { Url = url.Trim(), SortOrder = index })))
                .ForSourceMember(s => s.ImageUrls, o => o.DoNotValidate())
                .AddTransform<string?>(value => value == null ? null : value.Trim());

            CreateMap<UpdateBoxItemRequest, BoxItem>(MemberList.Source)
                .AddTransform<string?>(value => value == null ? null : value.Trim())
                .ForAllMembers(o => o.Condition((_, _, member) => member != null));
        }
    }
}
