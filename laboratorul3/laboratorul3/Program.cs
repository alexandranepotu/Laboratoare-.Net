using Microsoft.EntityFrameworkCore;
using laboratorul3.Persistence.Data;
using laboratorul3.Features.Books;
using laboratorul3.Features.Books.Commands;
using laboratorul3.Features.Books.Queries;
using laboratorul3.Features.Books.Validators;
using MediatR;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext
builder.Services.AddDbContext<BookDbContext>(opt =>
    opt.UseSqlite("Data Source=books.db"));

// Register MediatR (use simpler overload to avoid ambiguous overload resolution)
builder.Services.AddMediatR(typeof(Program).Assembly);

// Register FluentValidation validators manually (avoid needing DependencyInjectionExtensions)
builder.Services.AddScoped<IValidator<CreateBookCommand>, CreateBookCommandValidator>();

// Register ValidationBehavior for pipeline
builder.Services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// CORS (optional)
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookDbContext>();
    db.Database.EnsureCreated();
}

// Middleware
app.UseHttpsRedirection();
app.UseCors();

// API endpoints
var group = app.MapGroup("/books").WithName("Books").WithOpenApi();

group.MapPost("/", CreateBook)
    .WithName("CreateBook")
    .WithOpenApi()
    .Produces<BookReadDto>(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

group.MapGet("/{id}", GetBookById)
    .WithName("GetBookById")
    .WithOpenApi()
    .Produces<BookReadDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

group.MapGet("/", GetAllBooks)
    .WithName("GetAllBooks")
    .WithOpenApi()
    .Produces<List<BookReadDto>>(StatusCodes.Status200OK);

group.MapPut("/{id}", UpdateBook)
    .WithName("UpdateBook")
    .WithOpenApi()
    .Produces<BookReadDto>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status400BadRequest);

group.MapDelete("/{id}", DeleteBook)
    .WithName("DeleteBook")
    .WithOpenApi()
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound);

app.Run();


// HANDLER FUNCTIONS 

async Task<IResult> CreateBook(CreateBookCommand command, IMediator mediator)
{
    try
    {
        var result = await mediator.Send(command);
        return Results.Created($"/books/{result.Id}", result);
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
    }
}

async Task<IResult> GetBookById(int id, IMediator mediator)
{
    var query = new GetBookByIdQuery(id);
    var result = await mediator.Send(query);
    return result != null ? Results.Ok(result) : Results.NotFound();
}

async Task<IResult> GetAllBooks(int page = 1, int pageSize = 10, IMediator mediator = null!)
{
    var query = new GetBooksQuery(page, pageSize);
    var result = await mediator.Send(query);
    return Results.Ok(result);
}

async Task<IResult> UpdateBook(int id, UpdateBookCommand command, IMediator mediator)
{
    try
    {
        // Inject id from URL into command
        var commandWithId = new UpdateBookCommand(id, command.Title, command.Author, command.Year);
        var result = await mediator.Send(commandWithId);
        return result != null ? Results.Ok(result) : Results.NotFound();
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
    }
}

async Task<IResult> DeleteBook(int id, IMediator mediator)
{
    var command = new DeleteBookCommand(id);
    var result = await mediator.Send(command);
    return result ? Results.NoContent() : Results.NotFound();
}
