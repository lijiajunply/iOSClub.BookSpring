﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookSpring.DataLib.DataModels;

public class UserModel
{
    [Column(TypeName = "varchar(15)")] public string Name { get; set; } = "";

    [Key]
    [Column(TypeName = "varchar(10)")]
    public string Id { get; set; } = "";

    /// <summary>
    /// Founder : 创始人
    /// President : 社长,副社长,秘书长
    /// Minister : 部长/副部长
    /// Department : 部员成员
    /// Member : 普通成员
    /// </summary>
    [Column(TypeName = "varchar(20)")]
    public string Identity { get; set; } = "Member";

    public List<BookModel> CreatedBooks { get; init; } = [];
    public List<BookModel> LendBooks { get; init; } = [];

    public void Update(UserModel model)
    {
        if (!string.IsNullOrEmpty(model.Name)) Name = model.Name;
        if (!string.IsNullOrEmpty(model.Id)) Id = model.Id;
    }
}