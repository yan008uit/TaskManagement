using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;
using TaskManagementApi.Models.DTOs;
using TaskManagementWeb.Services;

namespace TaskManagementApi.IntegrationTests;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly ApiClient _apiClient;

    public AuthIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        // Create HttpClient from the test server factory
        var httpClient = factory.CreateClient(); // BaseAddress is automatically set
        _apiClient = new ApiClient(httpClient, new MockJsRuntime());
    }

    // Helper method to generate unique usernames to avoid collisions
    private static string GenerateUniqueUsername() => $"user_{Guid.NewGuid():N}";

    // ------------------- REGISTER TESTS -------------------

    // Success case: Register a valid user
    [Fact(DisplayName = "Register a new valid user succeeds")]
    public async Task Register_ValidUser_ReturnsSuccess()
    {
        var username = GenerateUniqueUsername();
        var request = new RegisterRequest
        {
            Username = username,
            Email = $"{username}@example.com",
            Password = "Password123!"
        };

        var response = await _apiClient.PostAsync<RegisterRequest, RegisterResponse>("/api/auth/register", request);

        response.Should().NotBeNull();
        response!.Username.Should().Be(username);
        response.Message.Should().Be("User registered successfully");
    }

    // Conflict case: Duplicate username/email
    [Fact(DisplayName = "Register duplicate user returns conflict")]
    public async Task Register_DuplicateUser_ReturnsConflict()
    {
        var username = GenerateUniqueUsername();
        var request = new RegisterRequest
        {
            Username = username,
            Email = $"{username}@example.com",
            Password = "Password123!"
        };

        // First registration succeeds
        var firstResponse = await _apiClient.PostAsync<RegisterRequest, RegisterResponse>("/api/auth/register", request);
        firstResponse.Should().NotBeNull();

        // Second registration should fail
        var conflictResponse = await _apiClient.PostAsync<RegisterRequest, ErrorResponse>("/api/auth/register", request);

        conflictResponse.Should().NotBeNull();
        conflictResponse!.Message.Should().Be("Username or email is already in use.");
    }

    // Invalid input: Missing fields (simulate bad request)
    [Fact(DisplayName = "Register with invalid input returns BadRequest")]
    public async Task Register_InvalidInput_ReturnsBadRequest()
    {
        var request = new RegisterRequest
        {
            Username = "",
            Email = "invalid-email",
            Password = ""
        };

        var httpResponse = await _apiClient.PostAsync("/api/auth/register", request);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    // ------------------- LOGIN TESTS -------------------

    // Success case: Login with valid credentials
    [Fact(DisplayName = "Login with valid credentials returns token")]
    public async Task Login_ValidUser_ReturnsToken()
    {
        var username = GenerateUniqueUsername();
        var password = "Password123!";

        await _apiClient.PostAsync<RegisterRequest, RegisterResponse>("/api/auth/register", new RegisterRequest
        {
            Username = username,
            Email = $"{username}@example.com",
            Password = password
        });

        var loginResponse = await _apiClient.PostAsync<LoginRequest, AuthResponse>("/api/auth/login", new LoginRequest
        {
            Username = username,
            Password = password
        });

        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrEmpty();
    }

    // Unauthorized case: Invalid password
    [Fact(DisplayName = "Login with invalid password returns Unauthorized")]
    public async Task Login_InvalidPassword_ReturnsUnauthorized()
    {
        var username = GenerateUniqueUsername();
        var password = "Password123!";
        var wrongPassword = "WrongPassword";

        await _apiClient.PostAsync<RegisterRequest, RegisterResponse>("/api/auth/register", new RegisterRequest
        {
            Username = username,
            Email = $"{username}@example.com",
            Password = password
        });

        var response = await _apiClient.PostAsync<LoginRequest, ErrorResponse>("/api/auth/login", new LoginRequest
        {
            Username = username,
            Password = wrongPassword
        });

        response.Should().NotBeNull();
        response!.Message.Should().Be("Invalid username or password.");
    }

    // Unauthorized case: Non-existent user
    [Fact(DisplayName = "Login with non-existent user returns Unauthorized")]
    public async Task Login_NonExistentUser_ReturnsUnauthorized()
    {
        var response = await _apiClient.PostAsync<LoginRequest, ErrorResponse>("/api/auth/login", new LoginRequest
        {
            Username = "doesnotexist",
            Password = "Password123!"
        });

        response.Should().NotBeNull();
        response!.Message.Should().Be("Invalid username or password.");
    }

    // Edge case: Empty username/password
    [Fact(DisplayName = "Login with empty credentials returns BadRequest")]
    public async Task Login_EmptyCredentials_ReturnsBadRequest()
    {
        var httpResponse = await _apiClient.PostAsync("/api/auth/login", new LoginRequest
        {
            Username = "",
            Password = ""
        });

        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await httpResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        problem.Should().NotBeNull();
        problem!.Errors.Should().ContainKey("Username");
        problem.Errors["Username"][0]
            .Should().ContainEquivalentOf("required");

        problem.Errors.Should().ContainKey("Password");
        problem.Errors["Password"][0]
            .Should().ContainEquivalentOf("required");
    }

    // Edge case: Login after registering multiple users
    [Fact(DisplayName = "Login with multiple users succeeds individually")]
    public async Task Login_MultipleUsers_ReturnsCorrectToken()
    {
        var user1 = GenerateUniqueUsername();
        var user2 = GenerateUniqueUsername();

        var password = "Password123!";

        // Register two users
        await _apiClient.PostAsync<RegisterRequest, RegisterResponse>("/api/auth/register", new RegisterRequest
        {
            Username = user1,
            Email = $"{user1}@example.com",
            Password = password
        });

        await _apiClient.PostAsync<RegisterRequest, RegisterResponse>("/api/auth/register", new RegisterRequest
        {
            Username = user2,
            Email = $"{user2}@example.com",
            Password = password
        });

        // Login user1
        var login1 = await _apiClient.PostAsync<LoginRequest, AuthResponse>("/api/auth/login", new LoginRequest
        {
            Username = user1,
            Password = password
        });

        // Login user2
        var login2 = await _apiClient.PostAsync<LoginRequest, AuthResponse>("/api/auth/login", new LoginRequest
        {
            Username = user2,
            Password = password
        });

        login1.Should().NotBeNull();
        login2.Should().NotBeNull();
        login1!.Token.Should().NotBeNullOrEmpty();
        login2!.Token.Should().NotBeNullOrEmpty();
        login1.Token.Should().NotBe(login2.Token);
    }
}