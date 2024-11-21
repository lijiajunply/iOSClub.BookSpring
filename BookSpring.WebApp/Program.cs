using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BookSpring.WebApp;
using BookSpring.WebApp.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Minio;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var endpoint = "https://miniocon.hkg1.zeabur.app/";
var accessKey = "fwjUZer4vuPLXeXkiO1I";
var secretKey = "lJgRGl0QO3gzlbJJdOke9Ifr2oggSEOVzSuw0Bc1";

builder.Services.AddMinio(configureClient => configureClient
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL()
            .Build());

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<AuthenticationStateProvider, JwtProvider>();
builder.Services.AddScoped<AppState>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();