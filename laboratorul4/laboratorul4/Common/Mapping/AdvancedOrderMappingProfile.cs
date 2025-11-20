using AutoMapper;
using laboratorul4.Features.Mappings.Resolvers;
using laboratorul4.Features.Orders;
using laboratorul4.Features.Orders.Dtos;

namespace laboratorul4.Common.Mapping;

public class AdvancedOrderMappingProfile : Profile
{
    public AdvancedOrderMappingProfile()
    {
        // Request -> Entity with conditional mapping for children's orders
        CreateMap<CreateOrderProfileRequest, Order>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsAvailable, opt => opt.Ignore())
            // Conditional: Apply 10% discount for children's orders
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src =>
                src.Category == OrderCategory.Children ? src.Price * 0.90m : src.Price))
            // Conditional: Clear CoverImageUrl for non-children's orders
            .ForMember(dest => dest.CoverImageUrl, opt => opt.MapFrom(src =>
                src.Category == OrderCategory.Children ? src.CoverImageUrl : null));

        // Entity -> DTO with advanced resolvers
        CreateMap<Order, OrderProfileDto>()
            .ForMember(dest => dest.CategoryDisplayName, 
                opt => opt.MapFrom<CategoryDisplayResolver>())
            .ForMember(dest => dest.FormattedPrice, 
                opt => opt.MapFrom<PriceFormatterResolver>())
            .ForMember(dest => dest.AuthorInitials, 
                opt => opt.MapFrom<AuthorInitialsResolver>())
            .ForMember(dest => dest.AvailabilityStatus, 
                opt => opt.MapFrom<AvailabilityStatusResolver>())
            .ForMember(dest => dest.PublishedAge, 
                opt => opt.MapFrom<PublishedAgeResolver>());
    }
}
