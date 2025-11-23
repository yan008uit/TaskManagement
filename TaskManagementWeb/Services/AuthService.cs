using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services;

public class AuthService
{
    private readonly ApiClient _apiClient;
    private readonly CustomAuthStateProvider _authProvider;

    public AuthService(ApiClient apiClient, AuthenticationStateProvider authProvider)
    {
        _apiClient = apiClient;
        _authProvider = authProvider as CustomAuthStateProvider
                        ?? throw new ArgumentException("AuthenticationStateProvider must be CustomAuthStateProvider");
    }

    public string? GetToken() => _apiClient.GetToken();

    public bool IsAuthenticated => !string.IsNullOrEmpty(_apiClient.GetToken());

    public int UserId
    {
        get
        {
            var token = _apiClient.GetToken();
            if (string.IsNullOrEmpty(token)) return 0;

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jwt;
            try
            {
                jwt = handler.ReadJwtToken(token);
            }
            catch
            {
                return 0;
            }

            var claim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }

    public async Task InitializeAsync()
    {
        await _apiClient.InitializeAsync();
        _authProvider.NotifyAuthenticationStateChanged();
    }

    public async Task<bool> Register(string username, string email, string password)
    {
        try
        {
            var dto = new { Username = username, Email = email, Password = password };
            var response = await _apiClient.PostAsync<object, JsonElement>("Auth/register", dto);
            return response.ValueKind != JsonValueKind.Undefined;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> Login(string username, string password)
    {
        var dto = new { Username = username, Password = password };

        try
        {
            var response = await _apiClient.PostAsync<object, LoginResponse>("Auth/login", dto);

            if (!string.IsNullOrEmpty(response?.Token))
            {
                await _apiClient.SetToken(response.Token);
                _authProvider.NotifyAuthenticationStateChanged();
                return response.Token;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task Logout()
    {
        await _apiClient.SetToken(null);
        _authProvider.NotifyAuthenticationStateChanged();
    }
}