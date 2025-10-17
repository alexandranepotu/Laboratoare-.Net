using MediatR;
using Microsoft.EntityFrameworkCore;
using laboratorul3.Persistence.Data;
using laboratorul3.Features.Books;
using laboratorul3.Features.Books.Queries;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace laboratorul3.Features.Books.Handlers
{
    public class GetBooksHandler : IRequestHandler<GetBooksQuery, List<BookReadDto>>
    {
        private readonly BookDbContext _context;

        public GetBooksHandler(BookDbContext context) => _context = context;

        public async Task<List<BookReadDto>> Handle(GetBooksQuery request, CancellationToken ct)
        {
            return await _context.Books
                .Select(b => new BookReadDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Year = b.Year
                })
                .ToListAsync(ct);
        }
    }
}