using MediatR;
using Microsoft.EntityFrameworkCore;
using laboratorul3.Persistence.Data;
using laboratorul3.Features.Books;
using laboratorul3.Features.Books.Queries;

namespace laboratorul3.Features.Books.Handlers
{
    public class GetBookByIdHandler : IRequestHandler<GetBookByIdQuery, BookReadDto?>
    {
        private readonly BookDbContext _context;

        public GetBookByIdHandler(BookDbContext context) => _context = context;

        public async Task<BookReadDto?> Handle(GetBookByIdQuery request, CancellationToken ct)
        {
            return await _context.Books
                .Where(b => b.Id == request.Id)
                .Select(b => new BookReadDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Year = b.Year
                })
                .FirstOrDefaultAsync(ct);
        }
    }
}