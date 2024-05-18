using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace iOSClub.BookSpring.WebApp.Models;

public class Provider(ProtectedSessionStorage sessionStorage) : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var storageResult = await sessionStorage.GetAsync<InfoModel>("Provider");
        var permission = storageResult.Success ? storageResult.Value : null;
        if (permission == null)
        {
            return await Task.FromResult(new AuthenticationState(_anonymous));
        }

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new(ClaimTypes.Name, permission.Name),
            new(ClaimTypes.Role, permission.Identity),
            new(ClaimTypes.NameIdentifier, permission.UserId)
        }, "Auth"));

        return await Task.FromResult(new AuthenticationState(claimsPrincipal));
    }

    public async Task UpdateAuthState(InfoModel? permission)
    {
        ClaimsPrincipal claimsPrincipal;
        if (permission is not null)
        {
            await sessionStorage.SetAsync("Provider", permission);
            claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new(ClaimTypes.Name, permission.Name),
                new(ClaimTypes.Role, permission.Identity),
                new(ClaimTypes.NameIdentifier, permission.UserId)
            }));
        }
        else
        {
            await sessionStorage.DeleteAsync("Provider");
            claimsPrincipal = _anonymous;
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }
}