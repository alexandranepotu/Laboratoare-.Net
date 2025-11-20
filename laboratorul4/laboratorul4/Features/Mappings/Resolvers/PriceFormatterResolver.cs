using System.Globalization;
using AutoMapper;
using laboratorul4.Features.Orders;
using laboratorul4.Features.Orders.Dtos;

namespace laboratorul4.Features.Mappings.Resolvers;

public class PriceFormatterResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
    {
        if (source == null)
            return decimal.Zero.ToString("C2", CultureInfo.CurrentCulture);

        return source.Price.ToString("C2", CultureInfo.CurrentCulture);
    }
}