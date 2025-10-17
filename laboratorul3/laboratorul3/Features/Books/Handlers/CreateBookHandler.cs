using MediatR;
using laboratorul3.Persistence.Data;
using laboratorul3.Domain.Entities;
using laboratorul3.Features.Books.Commands;
using laboratorul3.Features.Books;

namespace laboratorul3.Features.Books.Handlers
{
    public class CreateBookHandler : IRequestHandler<CreateBookCommand, BookReadDto>
    {
        private readonly BookDbContext _context;

        public CreateBookHandler(BookDbContext context) => _context = context;

        public async Task<BookReadDto> Handle(CreateBookCommand request, CancellationToken ct)
        {
            var book = new Book
            {
                Title = request.Title,
                Author = request.Author,
                Year = request.Year
            };

            _context.Books.Add(book);
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