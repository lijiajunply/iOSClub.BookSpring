using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookSpring.WebApp.Models;

public class UserModel
{
    public string Name { get; set; } = "";


    public string Id { get; set; } = "";

    /// <summary>
    /// Founder : 创始人
    /// President : 社长,副社长,秘书长
    /// Minister : 部长/副部长
    /// Department : 部员成员
    /// Member : 普通成员
    /// </summary>
    public string Identity { get; set; } = "Member";

    public List<BookModel> CreatedBooks { get; init; } = [];
    public List<BookModel> LendBooks { get; init; } = [];

    public void Update(UserModel model)
    {
        if (!string.IsNullOrEmpty(model.Name)) Name = model.Name;
        if (!string.IsNullOrEmpty(model.Id)) Id = model.Id;
    }
}