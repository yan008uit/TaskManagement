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

        // Ensure the app isn't misconfigured with the wrong provider type.
        _authProvider = authProvider as CustomAuthStateProvider
            ?? throw new ArgumentException("AuthenticationStateProvider must be CustomAuthStateProvider");
    }

    // Raised whenever the authentication state changes (login, logout, token refresh, etc.).
    public event Action? OnChange;
    private void NotifyStateChanged() => OnChange?.Invoke();

    // Quick client-side check for authentication.
    public bool IsAuthenticated => !string.IsNullOrEmpty(_apiClient.GetToken());

    // Extracts the user ID from the JWT (if available).
    public int UserId
    {
        get
        {
            var token = _apiClient.GetToken();
            if (string.IsNullOrEmpty(token))
                return 0;

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

    // Loads token from storage, validates it, and updates Blazor's auth state.
    public async Task InitializeAsync()
    {
        await _apiClient.InitializeAsync();

        var token = _apiClient.GetToken();

        if (string.IsNullOrEmpty(token))
        {
            // No token = user is anonymous.
            _authProvider.SetUserAuthenticated(null);
            NotifyStateChanged();
            return;
        }

        // Parse token to check expiry.
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // Expired = force logout and clear invalid data.
        if (jwt.ValidTo < DateTime.UtcNow)
        {
            await Logout();
            return;
        }

        // Valid token = create ClaimsIdentity and populate auth provider.
        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        _authProvider.SetUserAuthenticated(identity);

        NotifyStateChanged();
    }

    // Handles registration and wraps API error messages in a tuple.
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

            if (response != null && response.Message.Contains("success", StringComparison.OrdinalIgnoreCase))
                return (true, null);

            return (false, response?.Message ?? "Registration failed.");
        }
        catch (Exception ex)
        {
            return (false, $"Registration failed: {ex.Message}");
        }
    }

    // Performs login, stores token, updates auth state, and returns the JWT.
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

            if (string.IsNullOrEmpty(response?.Token))
                return null;

            // Persist token and update HttpClient auth header.
            await _apiClient.SetToken(response.Token);

            // Sync Blazor authentication state with JWT claims.
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);
            var identity = new ClaimsIdentity(jwt.Claims, "jwt");

            _authProvider.SetUserAuthenticated(identity);
            NotifyStateChanged();

            return response.Token;
        }
        catch
        {
            return null;
        }
    }

    // Clears token from memory and storage, then resets authentication state.
    public async Task Logout()
    {
        await _apiClient.SetToken(null);
        _authProvider.SetUserAuthenticated(null);
        NotifyStateChanged();
    }
}