using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// ReSharper disable MemberCanBePrivate.Global

namespace BookSpring.DataLib.DataModels;

public class BookModel
{
    [Column(TypeName = "varchar(64)")] public string Name { get; set; } = "";
    [Column(TypeName = "varchar(10)")] public UserModel CreatedBy { get; set; } = new();
    [Column(TypeName = "varchar(10)")] public UserModel LendTo { get; set; } = new();
    [Column(TypeName = "varchar(10)")] public string? LendDate { get; set; } = "";
    [Column(TypeName = "varchar(10)")] public string? ReturnDate { get; set; } = "";
    [Key] public int Id { get; init; }
    [Column(TypeName = "varchar(64)")] public string ImageUrl { get; set; } = "";
    [Column(TypeName = "varchar(10)")] public string Description { get; set; } = "";
    [Column(TypeName = "varchar(10)")] public string Category { get; set; } = "";
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