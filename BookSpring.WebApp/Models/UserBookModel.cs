namespace BookSpring.WebApp.Models;

public class UserBookModel
{
    public List<BookModel> CreatedBooks { get; init; } = [];
    public List<BookModel> LendBooks { get; init; } = [];
}