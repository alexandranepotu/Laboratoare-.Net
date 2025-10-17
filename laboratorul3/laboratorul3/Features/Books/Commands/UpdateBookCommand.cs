using MediatR;
using laboratorul3.Features.Books;

public class UpdateBookCommand : IRequest<BookReadDto?>
{
    public int Id { get; }
    public string Title { get; }
    public string Author { get; }
    public int Year { get; }

    public UpdateBookCommand(int id, string title, string author, int year)
    {
        Id = id;
        Title = title;
        Author = author;
        Year = year;
    }
}