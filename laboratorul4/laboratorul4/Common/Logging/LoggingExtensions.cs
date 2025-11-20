using System;
using Microsoft.Extensions.Logging;

namespace laboratorul4.Common.Logging;

public static class LoggingExtensions
{
    public static void LogOrderCreationMetrics(this ILogger logger, OrderCreationMetrics metrics)
    {
        if (logger == null) throw new ArgumentNullException(nameof(logger));
        if (metrics == null) throw new ArgumentNullException(nameof(metrics));

        var message = $"[Order Metrics] " +
            $"OperationId={metrics.OperationId}, " +
            $"Title={metrics.Title}, " +
            $"ISBN={metrics.ISBN}, " +
            $"Category={metrics.Category}, " +
            $"ValidationDuration={metrics.ValidationDuration.TotalMilliseconds}ms, " +
            $"DatabaseSaveDuration={metrics.DatabaseSaveDuration.TotalMilliseconds}ms, " +
            $"TotalDuration={metrics.TotalDuration.TotalMilliseconds}ms, " +
            $"Success={metrics.Success}" +
            (metrics.ErrorReason != null ? $", Error={metrics.ErrorReason}" : string.Empty);

        var eventId = new EventId(LogEvents.OrderCreationCompleted, nameof(LogEvents.OrderCreationCompleted));
        logger.LogInformation(eventId, message);
    }

    public static void LogOrderValidationFailure(this ILogger logger, string isbn, string reason)
    {
        logger.LogWarning(
            LogEvents.OrderValidationFailed,
            "OrderValidationFailed | ISBN={ISBN}, Reason={Reason}",
            isbn, reason
        );
    }

    public static void LogOrderCreationStarted(this ILogger logger, string title, string isbn, string category)
    {
        logger.LogInformation(
            LogEvents.OrderCreationStarted,
            "OrderCreationStarted | Title={Title}, ISBN={ISBN}, Category={Category}",
            title, isbn, category
        );
    }

    public static void LogOrderCreationCompleted(this ILogger logger, Guid orderId, string isbn, long durationMs)
    {
        logger.LogInformation(
            LogEvents.OrderCreationCompleted,
            "OrderCreationCompleted | OrderId={OrderId}, ISBN={ISBN}, Duration={Duration}ms",
            orderId, isbn, durationMs
        );
    }
}
