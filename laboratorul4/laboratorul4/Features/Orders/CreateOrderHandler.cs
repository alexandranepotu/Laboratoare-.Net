using System.Diagnostics;
using AutoMapper;
using laboratorul4.Data;
using laboratorul4.Common.Logging;
using laboratorul4.Features.Orders.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace laboratorul4.Features.Orders;

public class CreateOrderHandler : IRequestHandler<CreateOrderProfileRequest, OrderProfileDto>
{
    private readonly OrderDbContext _db;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateOrderHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateOrderHandler(
        OrderDbContext db,
        IMapper mapper,
        ILogger<CreateOrderHandler> logger,
        IHttpContextAccessor accessor)
    {
        _db = db;
        _mapper = mapper;
        _logger = logger;
        _httpContextAccessor = accessor;
    }

    public async Task<OrderProfileDto> Handle(CreateOrderProfileRequest request, CancellationToken cancellationToken)
    {
        var operationId = Guid.NewGuid().ToString("N")[..8];
        var correlationId = _httpContextAccessor.HttpContext?.Items["X-Correlation-ID"]?.ToString() 
                            ?? Guid.NewGuid().ToString();

        var totalStopwatch = Stopwatch.StartNew();
        var dbStopwatch = new Stopwatch();

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["OperationId"] = operationId,
            ["CorrelationId"] = correlationId,
            ["ISBN"] = request.ISBN,
            ["Category"] = request.Category.ToString()
        }))
        {
            try
            {
                // === VALIDATION PHASE ===
                _logger.LogInformation(
                    LogEvents.ISBNValidationPerformed,
                    "Validating ISBN uniqueness: ISBN={ISBN}",
                    request.ISBN
                );

                // Check for duplicate ISBN
                var existingOrder = await _db.Orders
                    .FirstOrDefaultAsync(o => o.ISBN == request.ISBN, cancellationToken);

                if (existingOrder != null)
                {
                    _logger.LogWarning(
                        LogEvents.OrderValidationFailed,
                        "Duplicate ISBN detected: ISBN={ISBN}",
                        request.ISBN
                    );

                    throw new InvalidOperationException($"An order with ISBN {request.ISBN} already exists.");
                }

                // === DATABASE OPERATION PHASE ===
                dbStopwatch.Start();
                _logger.LogInformation(
                    LogEvents.DatabaseOperationStarted,
                    "Saving order to database: ISBN={ISBN}",
                    request.ISBN
                );

                // Map to entity
                var order = _mapper.Map<Order>(request);
                order.Id = Guid.NewGuid();
                order.CreatedAt = DateTime.UtcNow;

                _db.Orders.Add(order);
                await _db.SaveChangesAsync(cancellationToken);

                dbStopwatch.Stop();

                _logger.LogInformation(
                    LogEvents.DatabaseOperationCompleted,
                    "Order saved successfully | OrderId={OrderId}, Duration={Duration}ms",
                    order.Id, dbStopwatch.ElapsedMilliseconds
                );

                totalStopwatch.Stop();

                // Log metrics
                var metrics = new OrderCreationMetrics(
                    operationId,
                    order.Title,
                    order.ISBN,
                    order.Category,
                    TimeSpan.Zero,
                    dbStopwatch.Elapsed,
                    totalStopwatch.Elapsed,
                    true
                );

                _logger.LogOrderCreationMetrics(metrics);

                return _mapper.Map<OrderProfileDto>(order);
            }
            catch (Exception ex)
            {
                totalStopwatch.Stop();

                var failureMetrics = new OrderCreationMetrics(
                    operationId,
                    request.Title ?? "Unknown",
                    request.ISBN ?? "Unknown",
                    request.Category,
                    TimeSpan.Zero,
                    dbStopwatch.Elapsed,
                    totalStopwatch.Elapsed,
                    false,
                    ex.Message
                );

                _logger.LogOrderCreationMetrics(failureMetrics);

                _logger.LogError(
                    new EventId(LogEvents.OrderValidationFailed, nameof(LogEvents.OrderValidationFailed)),
                    ex,
                    "Order creation failed | Title={Title}, ISBN={ISBN}",
                    request.Title, request.ISBN
                );

                throw;
            }
        }
    }
}
