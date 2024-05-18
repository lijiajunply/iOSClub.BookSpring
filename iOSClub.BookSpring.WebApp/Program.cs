using System.Text.Encodings.Web;
using System.Text.Unicode;
using BookSpring.DataLib;
using iOSClub.BookSpring.WebApp;
using iOSClub.BookSpring.WebApp.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddAntDesign();

// 身份验证
builder.Services.AddHttpClient();
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<AuthenticationStateProvider, Provider>();
builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();
builder.Services.AddAuthentication();

// 数据库
var sql = Environment.GetEnvironmentVariable("SQL", EnvironmentVariableTarget.Process);
if (string.IsNullOrEmpty(sql))
{
    builder.Services.AddDbContextFactory<BookContext>(opt =>
        opt.UseSqlite("Data Source=Data.db",
            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
}
else
{
    builder.Services.AddDbContextFactory<BookContext>(opt =>
        opt.UseNpgsql(sql,
            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
}

builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<BookContext>();
    
    context.SaveChanges();
    context.Dispose();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();

app.Run();