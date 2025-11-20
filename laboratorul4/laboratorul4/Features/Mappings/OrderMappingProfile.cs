using AutoMapper;
using laboratorul4.Features.Orders;
using laboratorul4.Features.Orders.Dtos;

namespace laboratorul4.Features.Mappings;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        // Request -> Entity
        CreateMap<CreateOrderProfileRequest, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsAvailable, opt => opt.Ignore());

        // Entity -> DTO (basic)
        CreateMap<Order, OrderProfileDto>()
            .ForMember(dest => dest.CategoryDisplayName,
                opt => opt.MapFrom(src => src.Category.ToString()))
            .ForMember(dest => dest.FormattedPrice, opt => opt.Ignore());
        
    }
}