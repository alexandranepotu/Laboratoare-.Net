using laboratorul4.Features.Dtos;
using laboratorul4.Features.Orders;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

//configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddMemoryCache();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderHandler).Assembly);
});builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Use(async (context, next) =>
{
    const string headerName = "X-Correlation-ID";

    // verify if an correlation id exists in request
    if (!context.Request.Headers.TryGetValue(headerName, out var correlationId))
    {
        correlationId = Guid.NewGuid().ToString("N")[..8];
        context.Request.Headers[headerName] = correlationId;
    }

    // add to response headers
    context.Response.Headers[headerName] = correlationId;

    //save in context items for further usage
    context.Items["CorrelationId"] = correlationId.ToString();

    await next();
});


app.UseHttpsRedirection();

app.MapPost("/orders", async (CreateOrderProfileRequest request, IMediator mediator, ILogger<Program> logger) =>
{
    logger.LogInformation("Received CreateOrder request for {Title}", request.Title); 
    var result = await mediator.Send(request);
    return Results.Ok(result);
});

app.MapGet("/", ()=>"Order API is running...");

app.Run();

