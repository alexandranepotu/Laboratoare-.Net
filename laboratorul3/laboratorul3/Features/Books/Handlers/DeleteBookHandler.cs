using MediatR;
using laboratorul3.Persistence.Data;
using laboratorul3.Features.Books.Commands;

namespace laboratorul3.Features.Books.Handlers
{
    public class DeleteBookHandler : IRequestHandler<DeleteBookCommand, bool>
    {
        private readonly BookDbContext _context;

        public DeleteBookHandler(BookDbContext context) => _context = context;

        public async Task<bool> Handle(DeleteBookCommand request, CancellationToken ct)
        {
            var book = await _context.Books.FindAsync(new object[] { request.Id }, ct);
            if (book == null) return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync(ct);
            return true;
        }
    }
}