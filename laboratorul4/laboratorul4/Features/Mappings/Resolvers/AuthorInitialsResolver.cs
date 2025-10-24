using System;
using AutoMapper;
using laboratorul4.Entities;
using laboratorul4.Features.Dtos;

namespace laboratorul4.Features.Mappings.Resolvers;

public class AuthorInitialsResolver : IValueResolver<Order, OrderProfileDto, string>
{
    public string Resolve(Order source, OrderProfileDto destination, string destMember, ResolutionContext context)
    {
        var name = source?.Author;
        if (string.IsNullOrWhiteSpace(name))
            return "?";

        var parts = name.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return "?";

        if (parts.Length == 1)
            return char.ToUpperInvariant(parts[0][0]).ToString();

        var first = parts[0];
        var last = parts[parts.Length - 1];
        var f = char.ToUpperInvariant(first[0]);
        var l = char.ToUpperInvariant(last[0]);
        return string.Concat(f, l);
    }
}