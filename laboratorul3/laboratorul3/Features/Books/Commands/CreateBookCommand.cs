using MediatR;
using laboratorul3.Features.Books;

namespace laboratorul3.Features.Books.Commands;

public record CreateBookCommand(string Title, string Author, int Year) : IRequest<BookReadDto>;
