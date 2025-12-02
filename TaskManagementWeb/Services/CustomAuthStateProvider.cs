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

        // Not authenticated
        if (string.IsNullOrEmpty(token))
            return Task.FromResult(EmptyState());

        // Validate and parse token
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            if (jwt.ValidTo < DateTime.UtcNow)
                return Task.FromResult(EmptyState());

            var identity = new ClaimsIdentity(jwt.Claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return Task.FromResult(new AuthenticationState(user));
        }
        catch
        {
            // Token is invalid or corrupted
            return Task.FromResult(EmptyState());
        }
    }

    public void NotifyAuthenticationStateChanged()
    {
        // Trigger re-evaluation of authentication state
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private AuthenticationState EmptyState()
    {
        return new AuthenticationState(
            new ClaimsPrincipal(new ClaimsIdentity())
        );
    }
}