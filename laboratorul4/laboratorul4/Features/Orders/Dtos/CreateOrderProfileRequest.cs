using MediatR;
using laboratorul4.Entities;

namespace laboratorul4.Features.Dtos;

public class CreateOrderProfileRequest : IRequest<OrderProfileDto>
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public OrderCategory Category { get; set; } = OrderCategory.Fiction;
}