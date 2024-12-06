namespace BookSpring.WebApp.Models;

public class CategoryModel
{
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? Type { get; set; }
    public List<BookModel> Books { get; set; } = [];
}