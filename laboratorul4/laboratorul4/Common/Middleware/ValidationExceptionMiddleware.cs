using FluentValidation;
using laboratorul4.Common.Logging;
using System.Text.Json;

namespace laboratorul4.Common.Middleware;

public class ValidationExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationExceptionMiddleware> _logger;

    public ValidationExceptionMiddleware(RequestDelegate next, ILogger<ValidationExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            var correlationId = context.Items["X-Correlation-ID"]?.ToString() ?? "unknown";

            _logger.LogWarning(
                LogEvents.OrderValidationFailed,
                "OrderValidationFailed | CorrelationId={CorrelationId} | Errors={Errors}",
                correlationId,
                string.Join("; ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"))
            );
            
            var responseObj = new
            {
                Message = "Order validation failed. Please correct the errors and retry.",
                CorrelationId = correlationId,
                Errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            await context.Response.WriteAsync(JsonSerializer.Serialize(
                responseObj,
                new JsonSerializerOptions { WriteIndented = true }
            ));
        }
    }
}
