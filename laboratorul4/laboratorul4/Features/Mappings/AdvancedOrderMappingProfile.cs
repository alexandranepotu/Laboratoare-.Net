using System;
using AutoMapper;
using laboratorul4.Entities;
using laboratorul4.Features.Dtos;

namespace laboratorul4.Features.Mappings;

public class AdvancedOrderMappingProfile : Profile
{
    public AdvancedOrderMappingProfile()
    {
        //Map Order->OrderProfileDto
        CreateMap<Order, OrderProfileDto>()
            .ForMember<string>(dest => dest.CategoryDisplayName,
        opt => opt.MapFrom(src => src.Category.ToString()))
            // Conditional CoverImageUrl mapping: null for Children, otherwise actual URL
            .ForMember(dest => dest.CoverImageUrl,
                opt => opt.MapFrom(src => src.Category == OrderCategory.Children ? null : src.CoverImageUrl))
            // Conditional Price mapping: 10% discount for Children, otherwise actual price
            .ForMember(dest => dest.Price,
                opt => opt.MapFrom(src => src.Category == OrderCategory.Children ? src.Price * 0.9m : src.Price));

        //Map CreateOrderProfileRequest->Order with safe enum parsing via helper
        CreateMap<CreateOrderProfileRequest, Order>()
            .ForMember(dest => dest.Category,
                opt => opt.MapFrom(src => ParseCategory(src.Category)))
            .ForMember(dest => dest.PublishedDate, opt => opt.Ignore());
    }
    private static OrderCategory ParseCategory(string? categoryString)
    {
        if (string.IsNullOrWhiteSpace(categoryString))
            return OrderCategory.Fiction;

        return Enum.TryParse<OrderCategory>(categoryString, true, out var parsed)
            ? parsed
            : OrderCategory.Fiction;
    }
}
