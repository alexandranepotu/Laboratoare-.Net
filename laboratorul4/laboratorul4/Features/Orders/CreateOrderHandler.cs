using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using laboratorul4.Entities;
using laboratorul4.Features.Dtos;
using laboratorul4.Features.Orders.Logging;

namespace laboratorul4.Features.Orders;

//trateaza cereri de creare a profilului si returneaza un dto cu informatiile despre profil folosind MediatR
public class CreateOrderHandler : IRequestHandler<CreateOrderProfileRequest, OrderProfileDto>
{
    private static readonly object
        CacheLock = new object(); //cachelock->pentru blocare si a evita probleme de concurenta

    private readonly IMapper _mapper; //automapper
    private readonly ILogger<CreateOrderHandler> _logger;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "all_orders"; //lista cache-ului

    public CreateOrderHandler(
        IMapper mapper,
        ILogger<CreateOrderHandler> logger,
        IMemoryCache cache)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public Task<OrderProfileDto> Handle(CreateOrderProfileRequest request, CancellationToken cancellationToken)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var operationId = Guid.NewGuid().ToString("N")[..8];
        var totalStopwatch = Stopwatch.StartNew();
        var validationStopwatch = new Stopwatch();
        var dbStopwatch = new Stopwatch();

        using (_logger.BeginScope(new Dictionary<string, object>
               {
                   ["OperationId"] = operationId,
                   ["ISBN"] = request.ISBN,
                   ["Category"] = request.Category.ToString()
               }))
        {
            _logger.LogInformation(
                "({EventId}) OrderCreationStarted | Title={Title}, Author={Author}, Category={Category}, ISBN={ISBN}",
                LogEvents.OrderCreationStarted, request.Title, request.Author, request.Category, request.ISBN);
            try
            {
                // === VALIDARE ===
                validationStopwatch.Start();
                _logger.LogInformation("({EventId}) ISBNValidationPerformed | ISBN={ISBN}",
                    LogEvents.ISBNValidationPerformed, request.ISBN);

                if (string.IsNullOrWhiteSpace(request.Title))
                    throw new ArgumentException("Title is required.", nameof(request.Title));
                if (string.IsNullOrWhiteSpace(request.Author))
                    throw new ArgumentException("Author is required.", nameof(request.Author));
                if (string.IsNullOrWhiteSpace(request.ISBN))
                    throw new ArgumentException("ISBN is required.", nameof(request.ISBN));

                var orders = _cache.Get<List<Order>>(CacheKey) ?? new List<Order>();

                lock (CacheLock)
                {
                    if (orders.Any(o => string.Equals(o.ISBN, request.ISBN, StringComparison.OrdinalIgnoreCase)))
                    {
                        _logger.LogWarning("({EventId}) OrderValidationFailed | Reason=Duplicate ISBN",
                            LogEvents.OrderValidationFailed);
                        throw new InvalidOperationException("An order with the same ISBN already exists.");
                    }

                    _logger.LogInformation("({EventId}) StockValidationPerformed | Stock validated for ISBN={ISBN}",
                        LogEvents.StockValidationPerformed, request.ISBN);
                    validationStopwatch.Stop();

                    // === SALVARE ÎN „BAZĂ DE DATE” (cache) ===
                    dbStopwatch.Start();
                    _logger.LogInformation("({EventId}) DatabaseOperationStarted", LogEvents.DatabaseOperationStarted);

                    var order = _mapper.Map<Order>(request);
                    order.Id = Guid.NewGuid();
                    order.CreatedAt = DateTime.UtcNow;

                    orders.Add(order);
                    _cache.Set(CacheKey, orders);

                    dbStopwatch.Stop();
                    _logger.LogInformation("({EventId}) DatabaseOperationCompleted | Duration={Duration}ms",
                        LogEvents.DatabaseOperationCompleted, dbStopwatch.ElapsedMilliseconds);

                    // === OPERAȚIE DE CACHE ===
                    _logger.LogInformation("({EventId}) CacheOperationPerformed | CacheKey={CacheKey}",
                        LogEvents.CacheOperationPerformed, CacheKey);

                    totalStopwatch.Stop();

                    // === LOG METRICS ===
                    var metrics = new OrderCreationMetrics(
                        operationId,
                        order.Title,
                        order.ISBN,
                        order.Category,
                        validationStopwatch.Elapsed,
                        dbStopwatch.Elapsed,
                        totalStopwatch.Elapsed,
                        true
                    );

                    _logger.LogOrderCreationMetrics(metrics);

                    var dto = _mapper.Map<OrderProfileDto>(order);
                    return Task.FromResult(dto);
                }
            }
            catch (Exception ex)
            {
                totalStopwatch.Stop();

                var metrics = new OrderCreationMetrics(
                    operationId,
                    request.Title ?? "Unknown",
                    request.ISBN ?? "Unknown",
                    request.Category,
                    validationStopwatch.Elapsed,
                    dbStopwatch.Elapsed,
                    totalStopwatch.Elapsed,
                    false,
                    ex.Message
                );

                _logger.LogOrderCreationMetrics(metrics);
                _logger.LogError(ex, "({EventId}) OrderCreationFailed | Title={Title} | ISBN={ISBN}",
                    LogEvents.OrderValidationFailed, request.Title, request.ISBN);

                throw; // retrimite excepția pentru handler global
            }
        }
    }
}