using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using laboratorul4.Entities;
using laboratorul4.Features.Dtos;

namespace laboratorul4.Features.Orders;

public class CreateOrderHandler
{
    private readonly IOrderRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateOrderHandler> _logger;
    private readonly IMemoryCache _cache;

    public CreateOrderHandler(
        IOrderRepository repository,
        IMapper mapper,
        ILogger<CreateOrderHandler> logger,
        IMemoryCache cache)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    public async Task<OrderProfileDto> Handle(CreateOrderProfileRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        // Basic validation (maintain existing validation behaviour)
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required.", nameof(request.Title));
        if (string.IsNullOrWhiteSpace(request.Author))
            throw new ArgumentException("Author is required.", nameof(request.Author));
        if (string.IsNullOrWhiteSpace(request.ISBN))
            throw new ArgumentException("ISBN is required.", nameof(request.ISBN));

        // Logging with requested fields
        _logger.LogInformation(
            "Creating order. Title: {Title}, Author: {Author}, Category: {Category}, ISBN: {ISBN}",
            request.Title, request.Author, request.Category, request.ISBN);

        // Check ISBN uniqueness (instead of email)
        var isbnTaken = await _repository.IsIsbnTakenAsync(request.ISBN, cancellationToken);
        if (isbnTaken)
            throw new InvalidOperationException("An order with the same ISBN already exists.");

        // Use advanced mapping to create Order (mapping profile handles enum parsing, etc.)
        var order = _mapper.Map<Order>(request);

        // Persist
        var created = await _repository.AddAsync(order, cancellationToken);

        // Update cache: invalidate "all_orders"
        _cache.Remove("all_orders");

        // Return advanced DTO
        var dto = _mapper.Map<OrderProfileDto>(created);
        return dto;
    }
}
