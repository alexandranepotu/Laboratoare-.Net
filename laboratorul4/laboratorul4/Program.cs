using laboratorul4.Features.Orders;
using laboratorul4.Common.Middleware;
using laboratorul4.Behaviors;
using laboratorul4.Validators;
using FluentValidation;
using laboratorul4.Common.Mapping;
using laboratorul4.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using AutoMapper;
using laboratorul4.Features.Orders.Dtos;

var builder = WebApplication.CreateBuilder(args);

// LOGGING
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// DATABASE (InMemory)
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseInMemoryDatabase("OrdersDb"));

// AUTOMAPPER
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<AdvancedOrderMappingProfile>();
}, typeof(Program).Assembly);

// CACHE
builder.Services.AddMemoryCache();

// FLUENT VALIDATION
builder.Services.AddScoped<CreateOrderProfileValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateOrderProfileValidator>();

// MEDIATR PIPELINE
builder.Services.AddMediatR(typeof(CreateOrderHandler).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// JSON CONFIG (ENUMS AS STRING)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// SWAGGER / OPENAPI
builder.Services.AddEndpointsApiExplorer();   
builder.Services.AddSwaggerGen();           
builder.Services.AddOpenApi();               
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ENABLE SWAGGER UI
app.UseSwagger();
app.UseSwaggerUI(); 

app.MapOpenApi();

// MIDDLEWARE
app.UseMiddleware<CorrelationMiddleware>();
app.UseMiddleware<ValidationExceptionMiddleware>();

// ENDPOINTS

// CREATE ORDER
app.MapPost("/orders", async (CreateOrderProfileRequest request, IMediator mediator) =>
{
    var result = await mediator.Send(request);
    return Results.Created($"/orders/{result.Id}", result);
})
.WithName("CreateOrder")
.WithTags("Orders")
.WithDescription("Creates a new order with validation, mapping, and logging.")
.Produces<OrderProfileDto>(201)
.ProducesValidationProblem(400);

// GET ALL ORDERS
app.MapGet("/orders", async (OrderDbContext db, IMapper mapper) =>
{
    var orders = await db.Orders.ToListAsync();
    return Results.Ok(mapper.Map<List<OrderProfileDto>>(orders));
})
.WithName("GetAllOrders")
.WithTags("Orders");

// GET ORDER BY ID
app.MapGet("/orders/{id:guid}", async (Guid id, OrderDbContext db, IMapper mapper) =>
{
    var order = await db.Orders.FindAsync(id);
    return order is null
        ? Results.NotFound(new { Message = $"Order {id} not found." })
        : Results.Ok(mapper.Map<OrderProfileDto>(order));
})
.WithName("GetOrderById")
.WithTags("Orders");

// GET BY CATEGORY
app.MapGet("/orders/category/{category}", async (OrderCategory category, OrderDbContext db, IMapper mapper) =>
{
    var orders = await db.Orders.Where(o => o.Category == category).ToListAsync();
    return Results.Ok(mapper.Map<List<OrderProfileDto>>(orders));
})
.WithName("GetOrdersByCategory")
.WithTags("Orders");

// GET BY ISBN
app.MapGet("/orders/isbn/{isbn}", async (string isbn, OrderDbContext db, IMapper mapper) =>
{
    var order = await db.Orders.FirstOrDefaultAsync(o => o.ISBN == isbn);
    return order is null
        ? Results.NotFound(new { Message = $"Order with ISBN {isbn} not found." })
        : Results.Ok(mapper.Map<OrderProfileDto>(order));
})
.WithName("GetOrderByISBN")
.WithTags("Orders");

// SEARCH
app.MapGet("/orders/search", async (string? query, OrderDbContext db, IMapper mapper) =>
{
    if (string.IsNullOrWhiteSpace(query))
        return Results.BadRequest(new { Message = "Search query required." });

    var orders = await db.Orders
        .Where(o => o.Title.Contains(query) || o.Author.Contains(query))
        .ToListAsync();

    return Results.Ok(mapper.Map<List<OrderProfileDto>>(orders));
})
.WithName("SearchOrders")
.WithTags("Orders");

// UPDATE (PUT)
app.MapPut("/orders/{id:guid}", async (Guid id, CreateOrderProfileRequest request, OrderDbContext db, IMapper mapper) =>
{
    var existingOrder = await db.Orders.FindAsync(id);
    if (existingOrder is null)
        return Results.NotFound(new { Message = $"Order {id} not found." });

    // ISBN uniqueness
    if (existingOrder.ISBN != request.ISBN &&
        await db.Orders.AnyAsync(o => o.ISBN == request.ISBN && o.Id != id))
    {
        return Results.BadRequest(new { Message = $"ISBN {request.ISBN} already exists." });
    }

    // Update
    mapper.Map(request, existingOrder);
    await db.SaveChangesAsync();

    return Results.Ok(mapper.Map<OrderProfileDto>(existingOrder));
})
.WithName("UpdateOrder")
.WithTags("Orders");

// DELETE ORDER
app.MapDelete("/orders/{id:guid}", async (Guid id, OrderDbContext db) =>
{
    var order = await db.Orders.FindAsync(id);
    if (order is null)
        return Results.NotFound();

    db.Orders.Remove(order);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteOrder")
.WithTags("Orders");

// DELETE ALL (for testing)
app.MapDelete("/orders", async (OrderDbContext db) =>
{
    var count = await db.Orders.CountAsync();
    db.Orders.RemoveRange(db.Orders);
    await db.SaveChangesAsync();
    return Results.Ok(new { Deleted = count });
})
.WithName("DeleteAllOrders")
.WithTags("Orders");

app.Run();
