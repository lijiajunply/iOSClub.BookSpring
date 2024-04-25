using System.Text;
using BookSpring.DataLib;
using BookSpring.DataLib.DataModels;
using iOSClub.BookSpring.WebApp.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration; // 读取配置文件

// 将服务添加到容器
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.Configure<HubOptions>(option => option.MaximumReceiveMessageSize = null);
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContextFactory<BookContext>(opt =>
        opt.UseSqlite(configuration.GetConnectionString("SQLite")));
}
else if (builder.Environment.IsProduction())
{
    builder.Services.AddDbContextFactory<BookContext>(opt =>
        opt.UseNpgsql(configuration.GetConnectionString("PostgreSQL")!));
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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