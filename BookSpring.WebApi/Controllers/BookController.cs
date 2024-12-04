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
    public async Task<ActionResult<IEnumerable<BookModel>>> GetBooks()
    {
        return await context.Books.ToListAsync();
    }

    // GET: api/Book/5
    [HttpGet("{id}")]
    public async Task<ActionResult<BookModel>> GetBookModel(string id)
    {
        var bookModel = await context.Books.FindAsync(id);

        if (bookModel == null)
        {
            return NotFound();
        }

        return bookModel;
    }

    [TokenActionFilter]
    [Authorize(Roles = "Admin")]
    [HttpPost("/AddBooks")]
    public async Task<ActionResult> Post(Stream content)
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();

        member = await context.Users
            .FirstOrDefaultAsync(x => x.Id == member.Id && x.Name == member.Name);
        if (member is not { Identity: "Admin" }) return NotFound();

        var data = await JsonSerializer.DeserializeAsync<BookModel[]>(content) ?? [];

        await context.Books.AddRangeAsync(data);

        await context.SaveChangesAsync();
        return Ok();
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

        var book = await context.Books.Include(x => x.CreatedBy)
            .FirstOrDefaultAsync(x => x.Id == bookModel.Id);
        if (book == null) return NotFound();


        if (member.Id != book.CreatedBy?.Id || member.Identity != "Admin") return NotFound();
        book.Update(bookModel);
        context.Update(book);
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
            .Include(x => x.CreatedBy)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (bookModel == null || member.Id != bookModel.CreatedBy?.Id || member.Identity != "Admin")
        {
            return NotFound();
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
        if (member == null) return NotFound();
        member = await context.Users
            .FirstOrDefaultAsync(x => x.Id == member.Id && x.Name == member.Name);
        if (member == null) return NotFound();

        var book = await context.Books
            .Include(x => x.LendTo)
            .FirstOrDefaultAsync(x => x.Id == bookModel.Id);

        if (book == null || book.CreatedById == member.Id || book.LendToId == member.Id)
            return NotFound();

        book.LendTo = member;
        book.LendDate = bookModel.LendDate;
        book.ReturnDate = bookModel.ReturnDate;

        await context.SaveChangesAsync();
        return Ok();
    }
}