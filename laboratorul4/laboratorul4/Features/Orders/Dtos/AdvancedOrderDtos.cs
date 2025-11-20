namespace laboratorul4.Features.Orders.Dtos;

public class AdvancedOrderDto
{
  public Guid Id { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Author { get; set; } = string.Empty;
  public string ISBN { get; set; } = string.Empty;

  public string CategoryDisplayName { get; set; } = string.Empty;

  public string FormattedPrice { get; set; } = string.Empty;

  public string AuthorInitials { get; set; } = string.Empty;

  public string AvailabilityStatus { get; set; } = string.Empty;

  public string PublishedAge { get; set; } = string.Empty;

  public DateTime PublishedDate { get; set; }
  public DateTime CreatedAt { get; set; }
  public int StockQuantity { get; set; }
  public decimal Price { get; set; }
  public string? CoverImageUrl { get; set; }
}

public class OrderStatisticsDto
{
  public int TotalOrders { get; set; }
  public int TechnicalOrders { get; set; }
  public int ChildrenOrders { get; set; }
  public int FictionOrders { get; set; }
  public int NonFictionOrders { get; set; }
  public decimal AveragePrice { get; set; }
  public decimal TotalValue { get; set; }
  public int LowStockOrders { get; set; }
}

public class OrderSummaryDto
{
  public Guid Id { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Author { get; set; } = string.Empty;
  public string AuthorInitials { get; set; } = string.Empty;
  public string ISBN { get; set; } = string.Empty;
  public OrderCategory Category { get; set; }
  public string CategoryDisplayName { get; set; } = string.Empty;
  public decimal Price { get; set; }
  public string FormattedPrice { get; set; } = string.Empty;
  public string AvailabilityStatus { get; set; } = string.Empty;
  public string PublishedAge { get; set; } = string.Empty;
  public bool IsDiscounted { get; set; }
  public decimal? DiscountPercentage { get; set; }
}
