using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TaskManagementWeb.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ApiClient _apiClient;

    public CustomAuthStateProvider(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = _apiClient.GetToken();

        if (string.IsNullOrEmpty(token))
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            if (jwt.ValidTo < DateTime.UtcNow)
            {
                return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            }

            var identity = new ClaimsIdentity(jwt.Claims, "jwt");
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        }
        catch
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}