using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BookSpring.DataLib.DataModels;

public class CategoryModel : DataModel
{
    [Key]
    [Column(TypeName = "varchar(64)")]
    public string Key { get; set; } = "";
    [Column(TypeName = "varchar(64)")] public string Name { get; set; } = "";
    
    [Column(TypeName = "varchar(64)")] public string? Description { get; set; }
    [Column(TypeName = "varchar(64)")] public string? Type { get; set; }
    
    [JsonIgnore]
    public List<BookModel> Books { get; init; } = [];
}