using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TaskManagementWeb.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ApiClient _apiClient;

    // Holds the current authenticated user (or empty identity if not logged in).
    private ClaimsPrincipal _currentUser = new(new ClaimsIdentity());

    public CustomAuthStateProvider(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // REQUIRED BY BLAZOR
    // Blazor calls this method anytime it needs to know "who is logged in?".
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // If a JWT token exists in local storage.
        if (!string.IsNullOrEmpty(_apiClient.GetToken()))
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(_apiClient.GetToken());

                // If the token is expired = treat the user as logged out.
                if (jwt.ValidTo < DateTime.UtcNow)
                {
                    _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
                    return Task.FromResult(new AuthenticationState(_currentUser));
                }

                // If the token is valid = extract claims and create authenticated user.
                _currentUser = new ClaimsPrincipal(
                    new ClaimsIdentity(jwt.Claims, "jwt")
                );
            }
            catch
            {
                // If token decoding failed for any reason = logout state.
                _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            }
        }
        else
        {
            // If no token = not authenticated.
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        }

        return Task.FromResult(new AuthenticationState(_currentUser));
    }

    // Replaces the current identity and notifies Blazor about the change.
    public void SetUserAuthenticated(ClaimsIdentity? identity)
    {
        if (identity == null)
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        }
        else
        {
            _currentUser = new ClaimsPrincipal(identity);
        }

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    // Forces Blazor to re-evaluate authentication state.
    public void NotifyStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}