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

        // Ensure the provider is the custom one
        _authProvider = authProvider as CustomAuthStateProvider
            ?? throw new ArgumentException("AuthenticationStateProvider must be CustomAuthStateProvider");
    }

    // Event for authentication state changes
    public event Action? OnChange;
    private void NotifyStateChanged() => OnChange?.Invoke();

    // Quick client-side check for authentication
    public bool IsAuthenticated => !string.IsNullOrEmpty(_apiClient.GetToken());

    // Extract user ID from JWT if available
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

    // Initialize JWT from localStorage and update auth state
    public async Task InitializeAsync()
    {
        await _apiClient.InitializeAsync();
        var token = _apiClient.GetToken();

        if (string.IsNullOrEmpty(token))
        {
            _authProvider.SetUserAuthenticated(null);
            NotifyStateChanged();
            return;
        }

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Expired token = logout
        if (jwt.ValidTo < DateTime.UtcNow)
        {
            await Logout();
            return;
        }

        // Valid token = set claims identity
        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        _authProvider.SetUserAuthenticated(identity);
        NotifyStateChanged();
    }

    // Register a new user
    public async Task<(bool Success, string? Error)> Register(string username, string email, string password)
    {
        var request = new RegisterRequest { Username = username, Email = email, Password = password };

        try
        {
            var response = await _apiClient.PostAsync<RegisterRequest, RegisterResponse>("Auth/register", request);

            if (response != null && response.Message.Contains("success", StringComparison.OrdinalIgnoreCase))
                return (true, null);

            return (false, response?.Message ?? "Registration failed.");
        }
        catch (HttpRequestException httpEx)
        {
            return (false, $"Registration failed: {httpEx.Message}");
        }
    }

    // Login user and store JWT
    public async Task<(bool Success, string? Token, string? Error)> Login(string username, string password)
    {
        var request = new LoginRequest { Username = username, Password = password };

        try
        {
            var response = await _apiClient.PostAsync<LoginRequest, AuthResponse>("Auth/login", request);

            if (response == null || string.IsNullOrEmpty(response.Token))
                return (false, null, "Invalid username or password.");

            // Store token
            await _apiClient.SetToken(response.Token);

            // Update Blazor auth state
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(response.Token);
            var identity = new ClaimsIdentity(jwt.Claims, "jwt");
            _authProvider.SetUserAuthenticated(identity);
            NotifyStateChanged();

            return (true, response.Token, null);
        }
        catch (HttpRequestException httpEx)
        {
            return (false, null, $"Login failed: {httpEx.Message}");
        }
    }

    // Logout user
    public async Task Logout()
    {
        await _apiClient.SetToken(null);
        _authProvider.SetUserAuthenticated(null);
        NotifyStateChanged();
    }
}