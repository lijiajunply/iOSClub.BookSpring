using System.ComponentModel.DataAnnotations;

namespace BookSpring.WebApp.Models;

public class LoginModel
{
    [Required]
    [StringLength(15, ErrorMessage = "请正确输入姓名")]
    public string Name { get; set; } = "";

    [Required]
    [StringLength(10, ErrorMessage = "请正确输入学号")]
    public string Id { get; set; } = "";
}