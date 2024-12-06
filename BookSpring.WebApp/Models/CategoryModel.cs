namespace BookSpring.WebApp.Models;

public class CategoryModel
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public List<BookModel> Books { get; init; } = [];
}