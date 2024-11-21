using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

// ReSharper disable MemberCanBePrivate.Global

namespace BookSpring.DataLib.DataModels;

public class BookModel : DataModel
{
    [Column(TypeName = "varchar(64)")] public string Name { get; set; } = "";
    [JsonIgnore] public UserModel? CreatedBy { get; set; }
    
    [Column(TypeName = "varchar(10)")] public string? CreatedById { get; set; }
    [Column(TypeName = "varchar(10)")] public string? LendToId { get; set; }

    [JsonIgnore] public UserModel? LendTo { get; set; }
    [Column(TypeName = "varchar(10)")] public string? LendDate { get; set; } = "";
    [Column(TypeName = "varchar(10)")] public string? ReturnDate { get; set; } = "";

    [Key]
    [Column(TypeName = "varchar(64)")]
    public string Id { get; set; } = "";

    [Column(TypeName = "varchar(64)")] public string ImageUrl { get; set; } = "";
    [Column(TypeName = "varchar(10)")] public string Description { get; set; } = "";
    [Column(TypeName = "varchar(10)")] public string Category { get; set; } = "";
    [Column(TypeName = "boolean")] public bool IsEBook { get; set; }

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