using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using laboratorul4.Data;
using laboratorul4.Common.Mapping;
using laboratorul4.Features.Orders;
using laboratorul4.Features.Mappings;
using laboratorul4.Features.Orders.Dtos;

namespace laboratorul4.Tests;

public class CreateOrderHandlerIntegrationTests : IDisposable
{
    private readonly OrderDbContext _db;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<CreateOrderHandler>> _loggerMock;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerIntegrationTests()
    {
        var dbOptions = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        _db = new OrderDbContext(dbOptions);

        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<OrderMappingProfile>();
            cfg.AddProfile<AdvancedOrderMappingProfile>();
        });

        _mapper = mapperConfig.CreateMapper();

        _loggerMock = new Mock<ILogger<CreateOrderHandler>>();

        var http = new DefaultHttpContext();
        http.Items["X-Correlation-ID"] = "test-id";

        _contextAccessor = new HttpContextAccessor { HttpContext = http };

        _handler = new CreateOrderHandler(_db, _mapper, _loggerMock.Object, _contextAccessor);
    }

    [Fact]
    public async Task Handle_ValidTechnicalOrder_CreatesOrder()
    {
        var request = new CreateOrderProfileRequest
        {
            Title = "Microservices Architecture",
            Author = "Mark Richards",
            ISBN = "978-1119432655",
            Category = OrderCategory.Technical,
            Price = 40.00m,
            PublishedDate = DateTime.UtcNow.AddYears(-2),
            StockQuantity = 10
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Technical & Professional", result.CategoryDisplayName);
        Assert.True(result.IsAvailable);
    }

    [Fact]
    public async Task Handle_DuplicateISBN_ThrowsException()
    {
        var req1 = new CreateOrderProfileRequest
        {
            Title = "Book A",
            Author = "John Doe",
            ISBN = "123456",
            Category = OrderCategory.Fiction,
            Price = 10,
            PublishedDate = DateTime.UtcNow
        };

        var req2 = new CreateOrderProfileRequest
        {
            Title = "Book B",
            Author = "Jane Doe",
            ISBN = "123456",
            Category = OrderCategory.Fiction,
            Price = 10,
            PublishedDate = DateTime.UtcNow
        };

        await _handler.Handle(req1, CancellationToken.None);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(req2, CancellationToken.None));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task Handle_ChildrensOrderRequest_AppliesDiscountAndConditionalMapping()
    {
        var request = new CreateOrderProfileRequest
        {
            Title = "The Magic Adventure",
            Author = "Alice Wonder",
            ISBN = "978-1234567890",
            Category = OrderCategory.Children,
            Price = 30.00m,
            PublishedDate = DateTime.UtcNow.AddYears(-1),
            StockQuantity = 50,
            CoverImageUrl = "https://example.com/cover.jpg"
        };

        var result = await _handler.Handle(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Children's Orders", result.CategoryDisplayName);
        
        // Verifica discountul (30.00 * 0.90 = 27.00)
        var savedOrder = await _db.Orders.FirstOrDefaultAsync(o => o.ISBN == request.ISBN);
        Assert.NotNull(savedOrder);
        Assert.Equal(27.00m, savedOrder.Price);
        
        // Verifica maparea conditionala pentru CoverImageUrl
        Assert.Equal("https://example.com/cover.jpg", savedOrder.CoverImageUrl);
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
