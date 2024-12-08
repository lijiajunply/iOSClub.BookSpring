using System.Text.Json;
using BookSpring.DataLib;
using BookSpring.DataLib.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookSpring.WebApi.Controllers;

[TokenActionFilter]
[Authorize(Roles = "Admin")]
[Route("[controller]")]
[ApiController]
public class AdminsController(
    BookContext context,
    IHttpContextAccessor httpContextAccessor) : ControllerBase
{
    [HttpPost("/AddBooks")]
    public async Task<ActionResult> AddBooks([FromBody] string content)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();

        member = await context.Users
            .FirstOrDefaultAsync(x => x.Id == member.Id && x.Name == member.Name);
        if (member is not { Identity: "Admin" }) return NotFound();

        content = GZipServer.DecompressString(content);

        var data = JsonSerializer.Deserialize<BookModel[]>(content) ?? [];

        foreach (var model in data)
        {
            if (await context.Books.AnyAsync(e => e.Id == model.Id)) continue;
            model.Categories.Clear();
            var categories = model.Category.Split(',');
            foreach (var category in categories)
            {
                if (model.Categories.Any(x => x.Name == category)) continue;
                var cate = await context.Categories.FirstOrDefaultAsync(x => x.Name == category);
                if (cate == null)
                {
                    cate = new CategoryModel
                    {
                        Name = category
                    };
                    cate.Key = DataTool.StringToHash(cate.ToString());
                    context.Categories.Add(cate);
                    await context.SaveChangesAsync();
                }

                model.Categories.Add(cate);
            }

            await context.Books.AddAsync(model);
        }

        await context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult<string>> GetUsers()
    {
        var users = await context.Users.ToListAsync();
        return Convert.ToBase64String(GZipServer.Compress(JsonSerializer.SerializeToUtf8Bytes(users)));
    }

    [HttpPost]
    public async Task<IActionResult> UpdateUserInfo([FromBody] UserModel userModel)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userModel.Id);
        if (user == null) return NotFound();

        user.Update(userModel);
        await context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id);
        if (user == null) return NotFound();

        context.Users.Remove(user);
        await context.SaveChangesAsync();
        return Ok();
    }
}