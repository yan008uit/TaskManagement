using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace TaskManagementWeb.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ApiClient _apiClient;

    public CustomAuthStateProvider(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = _apiClient.GetToken();

        ClaimsIdentity identity;

        if (!string.IsNullOrEmpty(token))
        {
            identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "User")
            }, "jwt");
        }
        else
        {
            identity = new ClaimsIdentity();
        }

        var user = new ClaimsPrincipal(identity);
        return await Task.FromResult(new AuthenticationState(user));
    }

    public void NotifyAuthenticationStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}