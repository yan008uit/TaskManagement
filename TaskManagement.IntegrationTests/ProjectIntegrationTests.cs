using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TaskManagementApi.Models.DTOs;
using TaskManagementWeb.Services;

namespace TaskManagementApi.IntegrationTests;

public class ProjectIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly ApiClient _api;

    public ProjectIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        var http = factory.CreateClient();
        _api = new ApiClient(http, new MockJsRuntime());
    }

    private static string GenerateUser() => $"user_{Guid.NewGuid():N}";

    // Helper: Register + Login + Set Token
    private async Task AuthenticateAsync()
    {
        var username = GenerateUser();
        var password = "Password123!";

        // Register
        var reg = await _api.PostAsync<RegisterRequest, RegisterResponse>("/api/auth/register",
            new RegisterRequest
            {
                Username = username,
                Email = $"{username}@example.com",
                Password = password
            });

        reg.Should().NotBeNull();

        // Login
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

    // ------------------- CREATE TESTS -------------------

    // Success case: Creating a new project
    [Fact(DisplayName = "Create project succeeds")]
    public async Task CreateProject_Succeeds()
    {
        await AuthenticateAsync();

        var createRequest = new ProjectCreateDto
        {
            Name = "Test Project",
            Description = "Integration test project"
        };

        var httpResponse = await _api.PostAsync("/api/project", createRequest);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await httpResponse.Content.ReadFromJsonAsync<ProjectDto>();
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);
        created.Name.Should().Be("Test Project");
    }

    // Unauthorized case: Creating without authentication
    [Fact(DisplayName = "Create project without auth returns 401")]
    public async Task CreateProject_Unauthorized_401()
    {
        await _api.SetToken(null);

        var response = await _api.PostAsync("/api/project",
            new ProjectCreateDto { Name = "X", Description = "Y" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Invalid input: Missing fields (simulate bad request)
    [Fact(DisplayName = "Create project with missing fields returns 400")]
    public async Task CreateProject_BadRequest_400()
    {
        await AuthenticateAsync();

        var response = await _api.PostAsync("/api/project",
            new ProjectCreateDto { Name = "", Description = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    // ------------------- GET TESTS -------------------

    // Success case: Getting a project by ID
    [Fact(DisplayName = "Get project by ID returns project")]
    public async Task GetProjectById_Succeeds()
    {
        await AuthenticateAsync();

        var create = await _api.PostAsync<ProjectCreateDto, ProjectDto>("/api/project",
            new ProjectCreateDto { Name = "P1", Description = "D1" });

        create.Should().NotBeNull();

        var fetched = await _api.GetAsync<ProjectDto>($"/api/project/{create!.Id}");

        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(create.Id);
        fetched.Name.Should().Be("P1");
    }

    // NotFound case: Invalid or non-existing project ID
    [Fact(DisplayName = "Get project by ID returns 404 for wrong user or missing project")]
    public async Task GetProject_NotFound_404()
    {
        await AuthenticateAsync();

        var fetched = await _api.GetAsync<ProjectDto>("/api/project/99999");

        fetched.Should().BeNull();
    }

    // Success case: Getting all projects
    [Fact(DisplayName = "Get projects returns list for authenticated user")]
    public async Task GetProjects_ReturnsList()
    {
        await AuthenticateAsync();

        await _api.PostAsync<ProjectCreateDto, ProjectDto>("/api/project",
            new ProjectCreateDto { Name = "ListTest", Description = "D" });

        var projects = await _api.GetAsync<List<ProjectDto>>("/api/project");

        projects.Should().NotBeNull();
        projects!.Count.Should().BeGreaterThan(0);
    }


    // ------------------- UPDATE TESTS -------------------

    // Success case: Updating a project
    [Fact(DisplayName = "Update project succeeds")]
    public async Task UpdateProject_Succeeds()
    {
        await AuthenticateAsync();

        var created = await _api.PostAsync<ProjectCreateDto, ProjectDto>("/api/project",
            new ProjectCreateDto { Name = "Before", Description = "Desc" });

        created.Should().NotBeNull();

        var updateResponse = await _api.PutAsync(
            "/api/project/" + created!.Id,
            (object)new ProjectUpdateDto
            {
                Name = "After",
                Description = "Updated"
            });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await _api.GetAsync<ProjectDto>("/api/project/" + created.Id);

        updated!.Name.Should().Be("After");
        updated.Description.Should().Be("Updated");
    }

    // NotFound case: Updating a non-existing project
    [Fact(DisplayName = "Update project returns 404 for invalid ID")]
    public async Task UpdateProject_404()
    {
        await AuthenticateAsync();

        var response = await _api.PutAsync("/api/project/99999",
            (object)new ProjectUpdateDto { Name = "x" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Invalid input: Missing/invalid fields (simulate bad request)
    [Fact(DisplayName = "Update project with invalid data returns 400")]
    public async Task UpdateProject_BadRequest_400()
    {
        await AuthenticateAsync();

        // Create a valid project first
        var created = await _api.PostAsync<ProjectCreateDto, ProjectDto>("/api/project",
            new ProjectCreateDto { Name = "Valid", Description = "Desc" });

        created.Should().NotBeNull();

        // Send invalid update
        var response = await _api.PutAsync("/api/project/" + created!.Id,
            new ProjectUpdateDto { Name = "", Description = "Updated" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    // ------------------- DELETE TESTS -------------------

    // Success case: Deleting a valid project
    [Fact(DisplayName = "Delete project succeeds")]
    public async Task DeleteProject_Succeeds()
    {
        await AuthenticateAsync();

        var project = await _api.PostAsync<ProjectCreateDto, ProjectDto>("/api/project",
            new ProjectCreateDto { Name = "ToDelete", Description = "x" });

        project.Should().NotBeNull();

        var result = await _api.DeleteAsync("/api/project/" + project!.Id);

        result.Should().BeTrue();

        var getAfterDelete = await _api.GetAsync<ProjectDto>("/api/project/" + project.Id);
        getAfterDelete.Should().BeNull();
    }

    // NotFound case: Deleting an invalid ID
    [Fact(DisplayName = "Delete project returns 404 for invalid ID")]
    public async Task DeleteProject_404()
    {
        await AuthenticateAsync();

        var result = await _api.DeleteAsync("/api/project/99999");

        result.Should().BeFalse();
    }


    // ------------------- VISIBLE PROJECTS TESTS -------------------

    // Success case: Authenticated user
    [Fact(DisplayName = "Get visible projects returns list")]
    public async Task GetVisibleProjects_ReturnsList()
    {
        await AuthenticateAsync();

        await _api.PostAsync<ProjectCreateDto, ProjectDto>("/api/project",
            new ProjectCreateDto { Name = "VisibleTest", Description = "X" });

        var visible = await _api.GetAsync<List<ProjectDto>>("/api/project/visible");

        visible.Should().NotBeNull();
        visible!.Count.Should().BeGreaterThan(0);
    }
}