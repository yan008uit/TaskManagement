using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TaskManagementApi.Models.DTOs;
using TaskManagementWeb.Services;

namespace TaskManagementApi.IntegrationTests;

public class CommentIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly ApiClient _api;
    private readonly HttpClient _client;

    public CommentIntegrationTests(CustomWebApplicationFactory<Program> factory)
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
    // CREATE COMMENT TESTS
    // ---------------------------------------------------------

    // Success case: Create a new comment
    [Fact(DisplayName = "Create comment succeeds")]
    public async Task CreateComment_Succeeds()
    {
        await AuthenticateAsync();

        // Need project + task first
        var project = await _api.PostAsync<ProjectCreateDto, ProjectDto>("/api/project",
            new ProjectCreateDto { Name = "ProjC1", Description = "D" });

        var task = await _api.PostAsync<TaskCreateDto, TaskDetailsDto>("/api/task",
            new TaskCreateDto { Title = "TaskA", ProjectId = project!.Id });

        var dto = new CommentCreateUpdateDto
        {
            TaskItemId = task!.Id,
            Text = "Nice task!"
        };

        var response = await _api.PostAsync("/api/comment", dto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CommentDto>();

        created.Should().NotBeNull();
        created!.Text.Should().Be("Nice task!");
        created.TaskItemId.Should().Be(task.Id);
    }

    // Unauthorized case: Create comment without authentication
    [Fact(DisplayName = "Create comment without auth returns 401")]
    public async Task CreateComment_Unauthorized_401()
    {
        await _api.SetToken(null);

        var response = await _api.PostAsync("/api/comment",
            new CommentCreateUpdateDto { TaskItemId = 1, Text = "Test" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // BadRequest case: Missing content
    [Fact(DisplayName = "Create comment missing content returns 400")]
    public async Task CreateComment_BadRequest_400()
    {
        await AuthenticateAsync();

        var response = await _api.PostAsync("/api/comment",
            new CommentCreateUpdateDto { TaskItemId = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // NotFound case: Task doesn't exist
    [Fact(DisplayName = "Create comment for invalid task returns 400")]
    public async Task CreateComment_InvalidTask_400()
    {
        await AuthenticateAsync();

        var dto = new CommentCreateUpdateDto
        {
            TaskItemId = 99999,
            Text = "Hello"
        };

        var response = await _api.PostAsync("/api/comment", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    // ---------------------------------------------------------
    // GET COMMENTS TESTS
    // ---------------------------------------------------------

    // Success case: Get comments for task
    [Fact(DisplayName = "Get comments for task succeeds")]
    public async Task GetComments_Succeeds()
    {
        await AuthenticateAsync();

        var project = await _api.PostAsync<ProjectCreateDto, ProjectDto>("/api/project",
            new ProjectCreateDto { Name = "ProjC2", Description = "D" });

        var task = await _api.PostAsync<TaskCreateDto, TaskDetailsDto>("/api/task",
            new TaskCreateDto { Title = "TaskB", ProjectId = project!.Id });

        await _api.PostAsync<CommentCreateUpdateDto, CommentDto>("/api/comment",
            new CommentCreateUpdateDto { TaskItemId = task!.Id, Text = "First!" });

        var comments = await _api.GetAsync<List<CommentDto>>($"/api/comment/task/{task.Id}");

        comments.Should().NotBeNull();
        comments!.Count.Should().BeGreaterThan(0);
    }

    // NotFound case: Task doesn't exist
    [Fact(DisplayName = "Get comments for invalid task returns 404")]
    public async Task GetComments_NotFound_404()
    {
        await AuthenticateAsync();

        var response = await _client.GetAsync("/api/comment/task/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }


    // ---------------------------------------------------------
    // DELETE COMMENT TESTS
    // ---------------------------------------------------------

    // Success case: Delete comment
    [Fact(DisplayName = "Delete comment succeeds")]
    public async Task DeleteComment_Succeeds()
    {
        await AuthenticateAsync();

        var project = await _api.PostAsync<ProjectCreateDto, ProjectDto>("/api/project",
            new ProjectCreateDto { Name = "ProjC3", Description = "D" });

        var task = await _api.PostAsync<TaskCreateDto, TaskDetailsDto>("/api/task",
            new TaskCreateDto { Title = "TaskC", ProjectId = project!.Id });

        var created = await _api.PostAsync<CommentCreateUpdateDto, CommentDto>("/api/comment",
            new CommentCreateUpdateDto { TaskItemId = task!.Id, Text = "Delete me" });

        var response = await _client.DeleteAsync("/api/comment/" + created!.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // NotFound case: Deleting invalid ID
    [Fact(DisplayName = "Delete comment returns 404 for invalid ID")]
    public async Task DeleteComment_404()
    {
        await AuthenticateAsync();

        var response = await _client.DeleteAsync("/api/comment/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // Unauthorized case: delete comment without auth
    [Fact(DisplayName = "Delete comment without auth returns 401")]
    public async Task DeleteComment_Unauthorized_401()
    {
        await _api.SetToken(null);

        var response = await _client.DeleteAsync("/api/comment/1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}