using BookSpring.DataLib.DataModels;

namespace BookSpring.WebApi.Controllers;

public class UserBookModel
{
    public List<BookModel> CreatedBooks { get; set; } = [];
    public List<BookModel> LendBooks { get; set; } = [];
}