using System;
using AutoMapper;
using laboratorul4.Entities;
using laboratorul4.Features.Dtos;

namespace laboratorul4.Features.Mappings.Resolvers;

public class PublishedAgeResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
    {
        if (source == null)
            return "Unknown";

        var published = source.PublishedDate;
        if (published == default)
            return "Unknown";

        var now = DateTime.UtcNow.Date;
        var pub = published.Date;
        // If published in the future, treat as New Release
        if (pub > now)
            return "New Release";

        var days = (now - pub).Days;

        if (days < 30)
            return "New Release";

        if (days < 365)  
        {
            // compute whole months between dates
            int months = ((now.Year - pub.Year) * 12) + (now.Month - pub.Month);
            if (now.Day < pub.Day) months--;
            if (months < 1) months = 1;
            return months == 1 ? "1 month old" : $"{months} months old";
        }

        if (days == 1825)
            return "Classic";

        // For 1+ years (including >1825), compute whole years
        int years = now.Year - pub.Year;
        if (now.Month < pub.Month || (now.Month == pub.Month && now.Day < pub.Day))
            years--;
        if (years < 1) years = 1;
        return years == 1 ? "1 year old" : $"{years} years old";
    }
}