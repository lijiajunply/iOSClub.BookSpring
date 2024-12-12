using System.Text;
using System.Text.Json;
using BookSpring.DataLib;
using BookSpring.DataLib.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookSpring.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class CategoryController(
    BookContext context,
    IHttpContextAccessor httpContextAccessor) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<string>> GetCategories()
    {
        var list = await context.Categories.ToListAsync();
        return Convert.ToBase64String(GZipServer.Compress(JsonSerializer.SerializeToUtf8Bytes(list)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryModel>> GetCategory(string id)
    {
        var category = await context.Categories
            .FirstOrDefaultAsync(x => x.Name == id);
        if (category == null) return NotFound("找不到该标签");
        return category;
    }
    
    [HttpGet("/GetCategoryBook/{id}")]
    public async Task<ActionResult<string>> GetCategoryBook(string id)
    {
        var category = await context.Categories
            .Include(x => x.Books)
            .FirstOrDefaultAsync(x => x.Name == id);
        if (category == null) return NotFound("找不到该书籍");
        return Convert.ToBase64String(GZipServer.Compress(JsonSerializer.SerializeToUtf8Bytes(category.Books)));
    }

    [TokenActionFilter]
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> UpdateCategory(CategoryModel categoryModel)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound("用户未登录或验证已超时");
        member = await context.Users
            .FirstOrDefaultAsync(x => x.Id == member.Id && x.Name == member.Name);
        if (member is not { Identity: "Admin" }) return NotFound("找不到该用户或权限不够");

        var category = await context.Categories.Include(x => x.Books)
            .FirstOrDefaultAsync(x => x.Key == categoryModel.Key);

        if (category == null) return NotFound("找不到该书籍");

        category.Description = categoryModel.Description;
        category.Type = categoryModel.Type;

        if (category.Name != categoryModel.Name)
        {
            foreach (var book in category.Books)
            {
                var strArr = book.Category.Split(',');
                var builder = new StringBuilder();
                foreach (var str in strArr)
                {
                    builder.Append(str == category.Name ? categoryModel.Name : str);
                    builder.Append(',');
                }

                book.Category = builder.ToString().TrimEnd(',');
            }

            category.Name = categoryModel.Name;
        }

        await context.SaveChangesAsync();
        return Ok();
    }
}