using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookSpring.DataLib.DataModels;

public class UserModel
{
    [Column(TypeName = "varchar(15)")] public string Name { get; set; } = "";
    [Key] [Column(TypeName = "varchar(10)")] public string Id { get; set; } = "";

    public List<BookModel> CreatedBooks { get; init; } = [];
    public List<BookModel> LendBooks { get; init; } = [];

    public void Update(UserModel model)
    {
        if (!string.IsNullOrEmpty(model.Name)) Name = model.Name;
        if (!string.IsNullOrEmpty(model.Id)) Id = model.Id;
    }
}