using FluentAssertions;
using System.Net;
using TaskManagementApi.Models.DTOs;
using TaskManagementWeb.Services;

namespace TaskManagementApi.IntegrationTests;

public class UserIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly ApiClient _api;
    private readonly HttpClient _client;

    public UserIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
        _api = new ApiClient(_client, new MockJsRuntime());
    }

    private static string GenerateUser() => $"user_{Guid.NewGuid():N}";

    // Helper: Register + Login + Set Token
    private async Task AuthenticateAsync()
    {
        var username = GenerateUser();
        var password = "Password123!";

        var reg = await _api.PostAsync<RegisterRequest, RegisterResponse>("/api/auth/register",
            new RegisterRequest
            {
                Username = username,
                Email = $"{username}@example.com",
                Password = password
            });

        reg.Should().NotBeNull();

        var login = await _api.PostAsync<LoginRequest, AuthResponse>("/api/auth/login",
            new LoginRequest
            {
                Username = username,
                Password = password
            });

        login.Should().NotBeNull();
        login!.Token.Should().NotBeNullOrEmpty();

        await _api.SetToken(login.Token);
    }


    // ---------------------------------------------------------
    // GET USERS TESTS
    // ---------------------------------------------------------

    // Success case: Retrieve all users
    [Fact(DisplayName = "Get users succeeds")]
    public async Task GetUsers_Succeeds()
    {
        await AuthenticateAsync();

        var users = await _api.GetAsync<List<UserDto>>("/api/user");

        users.Should().NotBeNull();
        users!.Count.Should().BeGreaterThan(0);

        // Verify at least the authenticated user is present
        users.Any(u => !string.IsNullOrWhiteSpace(u.Username)).Should().BeTrue();
    }

    // Unauthorized case: Request user list without authentication
    [Fact(DisplayName = "Get users without auth returns 401")]
    public async Task GetUsers_Unauthorized_401()
    {
        await _api.SetToken(null);

        var response = await _client.GetAsync("/api/user");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}