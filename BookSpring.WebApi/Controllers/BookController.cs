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

    // POST: api/Book
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
                if (BookModelExists(bookModel.Id))
                {
                    return Conflict();
                }

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

    // DELETE: api/Book/5
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

    private bool BookModelExists(string id)
    {
        return context.Books.Any(e => e.Id == id);
    }
}