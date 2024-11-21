using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using BookSpring.DataLib;
using BookSpring.DataLib.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookSpring.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(
    BookContext context,
    JwtHelper jwtHelper,
    IHttpClientFactory httpClientFactory,
    IHttpContextAccessor httpContextAccessor) : ControllerBase
{
    [TokenActionFilter]
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<string>> GetData()
    {
        var member = httpContextAccessor.HttpContext?.User.GetUser();
        if (member == null) return NotFound();
        member = await context.Users.Include(x => x.LendBooks)
            .Include(x => x.CreatedBooks)
            .FirstOrDefaultAsync(x => x.Id == member.Id && x.Name == member.Name);
        if (member == null) return NotFound();

        var compressAfterByte = GZipServer.Compress(JsonSerializer.SerializeToUtf8Bytes(member));
        return Convert.ToBase64String(compressAfterByte);
    }


    [HttpPost]
    public async Task<ActionResult<string>> Login(LoginModel login)
    {
        var user = await context.Users.FirstOrDefaultAsync(x => x.Id == login.Id && x.Name == login.Name);
        if (user != null) return jwtHelper.GetMemberToken(user);

        var client = httpClientFactory.CreateClient();
        var response = await client.PostAsJsonAsync("https://www.xauat.site/api/Member/Login", login);
        if (!response.IsSuccessStatusCode) return NotFound();

        var Jwt = await response.Content.ReadAsStringAsync();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Jwt);
        response = await client.GetAsync("https://www.xauat.site/api/Member/GetData");

        if (!response.IsSuccessStatusCode) return NotFound();

        var json = await response.Content.ReadAsStringAsync();
        var jsonDoc = JsonNode.Parse(json);
        if (jsonDoc == null) return NotFound();
        var userData = new UserModel()
        {
            Name = jsonDoc["userName"]?.GetValue<string>() ?? "",
            Id = jsonDoc["userId"]?.GetValue<string>() ?? "",
            Identity = jsonDoc["identity"]?.GetValue<string>() ?? ""
        };

        if (userData.Identity != "Member") userData.Identity = "Admin";
        context.Users.Add(userData);

        await context.SaveChangesAsync();
        return jwtHelper.GetMemberToken(userData);
    }
}