using System;

namespace laboratorul4.Common.Logging;

public class OrderCreationMetrics
{
    public string OperationId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    
    // Pentru a masura performanta
    public TimeSpan ValidationDuration { get; set; }
    public TimeSpan DatabaseSaveDuration { get; set; }
    public TimeSpan TotalDuration { get; set; }
    
    public bool Success { get; set; }
    public string? ErrorReason { get; set; }

    // Constructor pentru cazul de succes
    public OrderCreationMetrics(
        string operationId,
        string title,
        string isbn,
        object category,
        TimeSpan validationDuration,
        TimeSpan databaseSaveDuration,
        TimeSpan totalDuration,
        bool success)
    {
        OperationId = operationId;
        Title = title ?? string.Empty;
        ISBN = isbn ?? string.Empty;
        Category = category?.ToString() ?? string.Empty;
        ValidationDuration = validationDuration;
        DatabaseSaveDuration = databaseSaveDuration;
        TotalDuration = totalDuration;
        Success = success;
    }

    // Constructor pentru cazul de esec
    public OrderCreationMetrics(
        string operationId,
        string title,
        string isbn,
        object category,
        TimeSpan validationDuration,
        TimeSpan databaseSaveDuration,
        TimeSpan totalDuration,
        bool success,
        string errorReason)
        : this(operationId, title, isbn, category, validationDuration, databaseSaveDuration, totalDuration, success)
    {
        ErrorReason = errorReason;
    }

    public OrderCreationMetrics() { }
}

public static class LogEvents
{
    public const int OrderCreationStarted = 2001;
    public const int OrderValidationFailed = 2002;
    public const int OrderCreationCompleted = 2003;
    public const int DatabaseOperationStarted = 2004;
    public const int DatabaseOperationCompleted = 2005;
    public const int CacheOperationPerformed = 2006;
    public const int ISBNValidationPerformed = 2007;
    public const int StockValidationPerformed = 2008;
}
