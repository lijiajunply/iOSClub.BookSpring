using System.Text;
using BookSpring.DataLib;
using BookSpring.DataLib.DataModels;
using iOSClub.BookSpring.WebApp;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

var builder = WebApplication.CreateBuilder(args);

// 将服务添加到容器
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAntDesign();

builder.Services.Configure<HubOptions>(option => option.MaximumReceiveMessageSize = null);
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

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

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<BookContext>();
    try
    {
        context.Database.Migrate();
        context.Database.EnsureCreated();
    }
    catch
    {
        var databaseCreator = (RelationalDatabaseCreator)context.Database.GetService<IDatabaseCreator>();
        databaseCreator.CreateTables();
        context.Database.Migrate();
    }

    if(!context.Users.Any())
    {
        context.Users.Add(new UserModel()
        {
            Name = "iOS Club 官方",
            Id = "iosclub2024"
        });
    }
    
    context.SaveChanges();
    context.Dispose();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();