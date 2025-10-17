namespace laboratorul3.Domain.Entities;

public class Book
{
    public int Id { get; set; }  //primary key
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int Year { get; set; }
}