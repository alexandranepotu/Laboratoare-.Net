using AutoMapper;
using laboratorul4.Entities;
using laboratorul4.Features.Dtos;

namespace laboratorul4.Features.Mappings.Resolvers;

public class AvailabilityStatusResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
    {
        if (source == null)
            return "Out of Stock";

        // Normalize stock to non-negative
        var stock = source.StockQuantity < 0 ? 0 : source.StockQuantity;

        // If not available (business flag) -> Out of Stock
        if (!source.IsAvailable)
            return "Out of Stock";

        // At this point IsAvailable == true
        if (stock == 0)
            return "Unavailable";

        if (stock == 1)
            return "Last Copy";

        if (stock <= 5)
            return "Limited Stock";

        return "In Stock";
    }
}