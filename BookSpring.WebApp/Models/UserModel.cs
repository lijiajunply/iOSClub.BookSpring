using System.Text.Json.Serialization;

namespace BookSpring.WebApp.Models;

public class UserModel
{
    // [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    
    // [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    /// <summary>
    /// Founder : 创始人
    /// President : 社长,副社长,秘书长
    /// Minister : 部长/副部长
    /// Department : 部员成员
    /// Member : 普通成员
    /// </summary>
    /// 
    // [JsonPropertyName("identity")]
    public string Identity { get; set; } = "Member";

    // [JsonPropertyName("createdBooks")]
    public List<BookModel> CreatedBooks { get; init; } = [];
    // [JsonPropertyName("lendBooks")]
    public List<BookModel> LendBooks { get; init; } = [];

    public void Update(UserModel model)
    {
        if (!string.IsNullOrEmpty(model.Name)) Name = model.Name;
        if (!string.IsNullOrEmpty(model.Id)) Id = model.Id;
    }
}