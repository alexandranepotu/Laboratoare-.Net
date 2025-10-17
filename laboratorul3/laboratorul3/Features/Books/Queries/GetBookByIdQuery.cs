using MediatR;
using laboratorul3.Features.Books;

namespace laboratorul3.Features.Books.Queries
{
    public record GetBookByIdQuery(int Id) : IRequest<BookReadDto?>;
}