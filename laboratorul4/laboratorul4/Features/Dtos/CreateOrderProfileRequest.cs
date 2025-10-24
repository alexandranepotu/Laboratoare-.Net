namespace laboratorul4.Features.Dtos;

public class CreateOrderProfileRequest
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? CoverImageUrl { get; set; } = null;
    public int StockQuantity { get; set; } = 1;
}