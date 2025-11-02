using System;
using Microsoft.Extensions.Logging;
using laboratorul4.Features.Orders.Logging;

namespace laboratorul4.Features.Orders.Logging;

public static class LoggingExtensions
{
    public static void LogOrderCreationMetrics(this ILogger logger, OrderCreationMetrics metrics)
    {
        if (logger == null) throw new ArgumentNullException(nameof(logger));
        if (metrics == null) throw new ArgumentNullException(nameof(metrics));
        var message =$"[Order Metrics] " +
            $"OperationId={metrics.OperationId}, " +
            $"Title={metrics.Title}, " +
            $"ISBN={metrics.ISBN}, " +
            $"Category={metrics.Category}, " +
            $"ValidationDuration={metrics.ValidationDuration.TotalMilliseconds}ms " +
            $"DatabaseSaveDuration={metrics.DatabaseSaveDuration.TotalMilliseconds}ms " +
            $"TotalDuration={metrics.TotalDuration.TotalMilliseconds}ms " +
            $"Success={metrics.Success}" +
            (metrics.ErrorReason != null ? $", Error={metrics.ErrorReason}" : string.Empty);
        
        var eventId = new EventId(LogEvents.OrderCreationCompleted, nameof(LogEvents.OrderCreationCompleted));
        logger.LogInformation(eventId, message);
    }
}