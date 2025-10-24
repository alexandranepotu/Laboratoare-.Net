// Explanation: Implement IValueResolver<Order, OrderProfileDto, string> that formats the Order.Price as currency using "C2" and the current culture.
using System.Globalization;
using AutoMapper;
using laboratorul4.Entities;
using laboratorul4.Features.Dtos;

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