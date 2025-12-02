using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskManagementWeb.Models.DTOs;

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

    // Authentication change notifications
    public event Action? OnChange;
    private void NotifyStateChanged() => OnChange?.Invoke();

    // Public state
    public bool IsAuthenticated => !string.IsNullOrEmpty(_apiClient.GetToken());

    public int UserId
    {
        get
        {
            var token = _apiClient.GetToken();
            if (string.IsNullOrEmpty(token)) return 0;

            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                var idClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                return idClaim != null ? int.Parse(idClaim.Value) : 0;
            }
            catch
            {
                return 0;
            }
        }
    }

    // Initialize (load token from storage)
    public async Task InitializeAsync()
    {
        await _apiClient.InitializeAsync();
        _authProvider.NotifyAuthenticationStateChanged();
        NotifyStateChanged();
    }

    // Registration
    public async Task<(bool Success, string? Error)> Register(string username, string email, string password)
    {
        var request = new RegisterRequest
        {
            Username = username,
            Email = email,
            Password = password
        };

        try
        {
            var response = await _apiClient.PostAsync<RegisterRequest, RegisterResponse>("Auth/register", request);

            if (response != null && !string.IsNullOrEmpty(response.Message) &&
                response.Message.ToLower().Contains("success"))
            {
                return (true, null);
            }

            return (false, response?.Message ?? "Registration failed.");
        }
        catch (Exception ex)
        {
            return (false, $"Registration failed: {ex.Message}");
        }
    }

    // Login
    public async Task<string?> Login(string username, string password)
    {
        var request = new LoginRequest
        {
            Username = username,
            Password = password
        };

        try
        {
            var response = await _apiClient.PostAsync<LoginRequest, LoginResponse>("Auth/login", request);

            if (!string.IsNullOrEmpty(response?.Token))
            {
                await _apiClient.SetToken(response.Token);
                _authProvider.NotifyAuthenticationStateChanged();
                NotifyStateChanged();
                return response.Token;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    // Logout
    public async Task Logout()
    {
        await _apiClient.SetToken(null);
        _authProvider.NotifyAuthenticationStateChanged();
        NotifyStateChanged();
    }
}