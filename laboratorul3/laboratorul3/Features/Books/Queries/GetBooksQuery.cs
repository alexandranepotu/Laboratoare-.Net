using MediatR;

namespace laboratorul3.Features.Books.Queries
{
    public record GetBooksQuery(int Page = 1, int PageSize = 10) : IRequest<PaginatedResult<BookReadDto>>;
}