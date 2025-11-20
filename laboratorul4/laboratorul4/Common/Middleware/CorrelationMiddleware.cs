namespace laboratorul4.Common.Middleware;

public class CorrelationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationMiddleware(RequestDelegate next, ILogger<CorrelationMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.ContainsKey(CorrelationIdHeader)
            ? context.Request.Headers[CorrelationIdHeader].ToString()
            : Guid.NewGuid().ToString();

        context.Items[CorrelationIdHeader] = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        using (_logger.BeginScope("CorrelationId: {CorrelationId}", correlationId))
        {
            _logger.LogInformation(
                "Incoming request {Method} {Path} with CorrelationId {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                correlationId);

            await _next(context);

            _logger.LogInformation(
                "Completed request {Method} {Path} with CorrelationId {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                correlationId);
        }
    }
}

