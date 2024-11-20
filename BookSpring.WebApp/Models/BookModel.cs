namespace BookSpring.WebApp.Models;

public class BookModel
{
    public string Name { get; set; } = "";
    public UserModel? CreatedBy { get; set; } = new();
    public UserModel? LendTo { get; set; } = new();
    public string? LendDate { get; set; } = "";
    public string? ReturnDate { get; set; } = "";
    public string Id { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public bool IsEBook { get; set; }

    public void Update(BookModel model)
    {
        if (!string.IsNullOrEmpty(model.Name)) Name = model.Name;
        if (!string.IsNullOrEmpty(model.LendDate)) LendDate = model.LendDate;
        if (!string.IsNullOrEmpty(model.ReturnDate)) ReturnDate = model.ReturnDate;
        if (!string.IsNullOrEmpty(model.ImageUrl)) ImageUrl = model.ImageUrl;
        if (!string.IsNullOrEmpty(model.Description)) Description = model.Description;
        if (!string.IsNullOrEmpty(model.Category)) Category = model.Category;
        IsEBook = model.IsEBook;
    }
}