using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookSpring.DataLib;
using BookSpring.DataLib.DataModels;
using Microsoft.AspNetCore.Authorization;

namespace BookSpring.WebApi.Controllers;

[Route("[controller]")]
[ApiController]
public class BookController(
    BookContext context,
    IHttpContextAccessor httpContextAccessor) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<string>> GetBooks()
    {
        var list = await context.Books.Include(x => x.Categories).ToListAsync();
        return Convert.ToBase64String(GZipServer.Compress(JsonSerializer.SerializeToUtf8Bytes(list)));
    }

    // GET: api/Book/5
    [HttpGet("{id}")]
    public async Task<ActionResult<BookModel>> GetBookModel(string id)
    {
        var bookModel = await context.Books.FindAsync(id);

        if (bookModel == null)
        {
            return NotFound("找不到该书籍");
        }

        return bookModel;
    }

    #region 图书管理

    // POST: /Book
    [TokenActionFilter]
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<BookModel>> PostBookModel(BookModel bookModel)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();

        member = await context.Users
            .FirstOrDefaultAsync(x => x.Id == member.Id && x.Name == member.Name);
        if (member == null) return NotFound();

        bookModel.Categories.Clear();

        var categories = bookModel.Category.Split(',');
        foreach (var category in categories)
        {
            if (bookModel.Categories.Any(x => x.Name == category)) continue;
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

            bookModel.Categories.Add(cate);
        }

        if (string.IsNullOrEmpty(bookModel.Id))
        {
            bookModel.Id = DataTool.StringToHash(bookModel.ToString());
            member.CreatedBooks.Add(bookModel);

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (await context.Books.AnyAsync(e => e.Id == bookModel.Id))
                    return Conflict();
                throw;
            }

            return bookModel;
        }

        var book = await context.Books.Include(x => x.Categories)
            .ThenInclude(categoryModel => categoryModel.Books)
            .FirstOrDefaultAsync(x => x.Id == bookModel.Id);
        if (book == null) return NotFound();

        if (member.Id != book.CreatedById || member.Identity != "Admin") return NotFound();
        book.Update(bookModel);

        foreach (var model in from model in book.Categories
                 where model.Books.Count == 1
                 let a = model.Books.First()
                 where a.Id == book.Id
                 select model)
        {
            context.Categories.Remove(model);
        }

        await context.SaveChangesAsync();

        book.Categories.Clear();
        await context.SaveChangesAsync();

        book.Categories.AddRange(bookModel.Categories);
        await context.SaveChangesAsync();

        return book;
    }

    // DELETE: /Book/5
    [TokenActionFilter]
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBookModel(string id)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();
        member = await context.Users
            .FirstOrDefaultAsync(x => x.Id == member.Id && x.Name == member.Name);
        if (member == null) return NotFound();

        var bookModel = await context.Books
            .Include(x => x.Categories)
            .ThenInclude(categoryModel => categoryModel.Books)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (bookModel == null || member.Id != bookModel.CreatedById || member.Identity != "Admin")
        {
            return NotFound();
        }

        foreach (var model in from model in bookModel.Categories
                 where model.Books.Count == 1
                 let a = model.Books.First()
                 where a.Id == id
                 select model)
        {
            context.Categories.Remove(model);
        }

        context.Books.Remove(bookModel);
        await context.SaveChangesAsync();

        return Ok();
    }

    #endregion

    [TokenActionFilter]
    [Authorize]
    [HttpPost("/Lend")]
    public async Task<IActionResult> Lend(BookModel bookModel)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound("用户未登录或验证已超时");
        member = await context.Users
            .FirstOrDefaultAsync(x => x.Id == member.Id && x.Name == member.Name);
        if (member == null) return NotFound("找不到用户");

        var book = await context.Books
            .Include(x => x.LendTo)
            .FirstOrDefaultAsync(x => x.Id == bookModel.Id);

        if (book == null || book.CreatedById == member.Id || book.LendToId == member.Id)
            return NotFound("找不到该书籍");

        book.LendTo = member;
        book.LendDate = bookModel.LendDate;
        book.ReturnDate = bookModel.ReturnDate;

        await context.SaveChangesAsync();
        return Ok();
    }
}