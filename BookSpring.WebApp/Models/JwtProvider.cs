using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace BookSpring.WebApp.Models;

public class JwtProvider(IJSRuntime js) : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public async Task Logout()
    {
        await UpdateAuthState(null);
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // 从本地存储或 Cookie 中获取 JWT 令牌
        var token = await js.InvokeAsync<string>("getLocalStorage", "jwt");

        if (string.IsNullOrEmpty(token))
            return await Task.FromResult(new AuthenticationState(_anonymous));

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var claimsIdentity = new ClaimsIdentity(jwtToken.Claims, "Jwt");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        return await Task.FromResult(new AuthenticationState(claimsPrincipal));
    }

    public async Task UpdateAuthState(string? jwt)
    {
        if (!string.IsNullOrEmpty(jwt))
        {
            await js.InvokeVoidAsync("setLocalStorage", "jwt", jwt);
        }
        else
        {
            await js.InvokeVoidAsync("removeLocalStorage", "jwt");
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    public async Task<UserModel?> GetPermission()
    {
        var token = await js.InvokeAsync<string>("getLocalStorage", "jwt");

        if (string.IsNullOrEmpty(token))
            return null;

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var name = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
        var identity = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
        var id = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(identity) || string.IsNullOrEmpty(id)) return null;

        return new UserModel()
        {
            Name = name,
            Identity = identity,
            Id = id
        };
    }

    public async Task<string> GetCookie() =>
        await js.InvokeAsync<string>("getLocalStorage", "jwt");
}