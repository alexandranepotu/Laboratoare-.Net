using MediatR;
using laboratorul3.Persistence.Data;
using laboratorul3.Features.Books;
using laboratorul3.Features.Books.Commands;

namespace laboratorul3.Features.Books.Handlers
{
    public class UpdateBookHandler : IRequestHandler<UpdateBookCommand, BookReadDto?>
    {
        private readonly BookDbContext _context;

        public UpdateBookHandler(BookDbContext context) => _context = context;

        public async Task<BookReadDto?> Handle(UpdateBookCommand request, CancellationToken ct)
        {
            var book = await _context.Books.FindAsync(new object[] { request.Id }, ct);
            if (book == null) return null;

            book.Title = request.Title;
            book.Author = request.Author;
            book.Year = request.Year;

            await _context.SaveChangesAsync(ct);

            return new BookReadDto
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Year = book.Year
            };
        }
    }
}