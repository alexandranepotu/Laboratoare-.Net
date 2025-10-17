using MediatR;
using laboratorul3.Features.Books;
using System.Collections.Generic;

namespace laboratorul3.Features.Books.Queries
{
    public record GetBooksQuery(int Page = 1, int PageSize = 10) : IRequest<List<BookReadDto>>;
}