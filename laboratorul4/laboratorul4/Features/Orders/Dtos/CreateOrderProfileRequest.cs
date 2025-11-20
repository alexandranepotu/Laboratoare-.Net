using System.ComponentModel.DataAnnotations;
using laboratorul4.Validators.Attributes;
using MediatR;

namespace laboratorul4.Features.Orders.Dtos;

public class CreateOrderProfileRequest : IRequest<OrderProfileDto>
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MinLength(5, ErrorMessage = "Author name must contain full name.")]
    public string Author { get; set; } = string.Empty;

    [Required]
    [ValidISBN] // custom attribute
    public string ISBN { get; set; } = string.Empty;

    [Required]
    [OrderCategory(OrderCategory.Technical, OrderCategory.Children, OrderCategory.Fiction, OrderCategory.NonFiction)]
    public OrderCategory Category { get; set; }

    [Required]
    [PriceRange(5, 500)] // global rule
    public decimal Price { get; set; }

    [Required]
    public DateTime PublishedDate { get; set; }

    [Range(0, 500)]
    public int StockQuantity { get; set; }

    public string? CoverImageUrl { get; set; }
}