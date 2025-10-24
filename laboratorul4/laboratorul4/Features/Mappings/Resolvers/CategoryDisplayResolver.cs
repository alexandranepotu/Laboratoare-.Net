using AutoMapper;
using laboratorul4.Entities;
using laboratorul4.Features.Dtos;

namespace laboratorul4.Features.Mappings.Resolvers;

public class CategoryDisplayResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
    {
        if (source == null)
            return "Uncategorized";

        return source.Category switch
        {
            OrderCategory.Fiction => "Fiction & Literature",
            OrderCategory.NonFiction => "Non-Fiction",
            OrderCategory.Technical => "Technical & Professional",
            OrderCategory.Children => "Children's Orders",
            _ => "Uncategorized",
        };
    }
}