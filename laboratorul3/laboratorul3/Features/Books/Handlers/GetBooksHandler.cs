using MediatR;
using Microsoft.EntityFrameworkCore;
using laboratorul3.Persistence.Data;
using laboratorul3.Features.Books;
using laboratorul3.Features.Books.Queries;

namespace laboratorul3.Features.Books.Handlers
{
    public class GetBooksHandler : IRequestHandler<GetBooksQuery, PaginatedResult<BookReadDto>>
    {
        private readonly BookDbContext _context;

        public GetBooksHandler(BookDbContext context) => _context = context;

        public async Task<PaginatedResult<BookReadDto>> Handle(GetBooksQuery request, CancellationToken ct)
        {
            var totalCount = await _context.Books.CountAsync(ct);

            var items = await _context.Books
                .OrderBy(b => b.Id)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(b => new BookReadDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Year = b.Year
                })
                .ToListAsync(ct);

            return new PaginatedResult<BookReadDto>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}