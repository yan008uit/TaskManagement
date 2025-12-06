using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using TaskManagementApi.Models.DTOs;
using TaskManagementWeb.Services;

namespace TaskManagementApi.IntegrationTests;

public class TaskIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly ApiClient _api;
    private readonly HttpClient _client;

    public TaskIntegrationTests(CustomWebApplicationFactory<Program> factory)
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

    // ------------------- CREATE TASK TESTS -------------------

    // Success case: Creating a new task
    [Fact(DisplayName = "Create task succeeds")]
    public async Task CreateTask_Succeeds()
    {
        await AuthenticateAsync();

        // Must create a project first
        var project = await _api.PostAsync<ProjectCreateDto, ProjectDto>("/api/project",
            new ProjectCreateDto { Name = "Proj1", Description = "D" });

        var dto = new TaskCreateDto
        {
            Title = "New Task",
            Description = "Test",
            ProjectId = project!.Id
        };

        var httpResponse = await _api.PostAsync("/api/task", dto);

        httpResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdTask = await httpResponse.Content.ReadFromJsonAsync<TaskDetailsDto>();
        createdTask.Should().NotBeNull();
        createdTask!.Title.Should().Be("New Task");
        createdTask.ProjectId.Should().Be(project.Id);
    }

    // Unauthorized case: Creating task without authentication
    [Fact(DisplayName = "Create task without auth returns 401")]
    public async Task CreateTask_Unauthorized_401()
    {
        await _api.SetToken(null); // remove token

        var response = await _api.PostAsync("/api/task",
            new TaskCreateDto { Title = "X", ProjectId = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // Invalid input: Missing required fields
    [Fact(DisplayName = "Create task with missing fields returns 400")]
    public async Task CreateTask_BadRequest_400()
    {
        await AuthenticateAsync();

        var response = await _api.PostAsync("/api/task",
            new TaskCreateDto { ProjectId = 0 }); // missing title

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // NotFound case: Creating a task for invalid project
    [Fact(DisplayName = "Create task for invalid project returns 400")]
    public async Task CreateTask_InvalidProject_400()
    {
        await AuthenticateAsync();

        var dto = new TaskCreateDto
        {
            Title = "Test",
            ProjectId = 99999
        };

        var response = await _api.PostAsync("/api/task", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ------------------- GET TASK TESTS -------------------

    // Success case: Get task by ID
    [Fact(DisplayName = "Get task by ID succeeds")]
    public async Task GetTask_Succeeds()
    {
        await AuthenticateAsync();

        var project = await _api.PostAsync<ProjectCreateDto, ProjectDto>("/api/project",
            new ProjectCreateDto { Name = "Proj2", Description = "D" });

        var created = await _api.PostAsync<TaskCreateDto, TaskDetailsDto>("/api/task",
            new TaskCreateDto { Title = "T1", ProjectId = project!.Id });

        var fetched = await _api.GetAsync<TaskDetailsDto>($"/api/task/{created!.Id}");

        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(created.Id);
        fetched.Title.Should().Be("T1");
    }

    // NotFound case: Invalid task ID
    [Fact(DisplayName = "Get task returns 404 when not found")]
    public async Task GetTask_NotFound_404()
    {
        await AuthenticateAsync();

        var fetched = await _api.GetAsync<TaskDetailsDto>("/api/task/99999");

        fetched.Should().BeNull();
    }

    // Success case: Get task details by ID
    [Fact(DisplayName = "Get task details succeeds")]
    public async Task GetTaskDetails_Succeeds()
    {
        await AuthenticateAsync();

        var project = await _api.PostAsync<ProjectCreateDto, ProjectDto>("/api/project",
            new ProjectCreateDto { Name = "Proj3", Description = "D" });

        var created = await _api.PostAsync<TaskCreateDto, TaskDetailsDto>("/api/task",
            new TaskCreateDto { Title = "TDetails", ProjectId = project!.Id });

        var fetched = await _api.GetAsync<TaskDetailsDto>($"/api/task/{created!.Id}/details");

        fetched.Should().NotBeNull();
        fetched!.Id.Should().Be(created.Id);
    }

    // ------------------- GET TASKS BY PROJECT -------------------

    // Success case: Get tasks by project
    [Fact(DisplayName = "Get tasks by project returns list")]
    public async Task GetTasksByProject_ReturnsList()
    {
        await AuthenticateAsync();

        var project = await _api.PostAsync<ProjectCreateDto, ProjectDto>("/api/project",
            new ProjectCreateDto { Name = "Proj4", Description = "D" });

        await _api.PostAsync<TaskCreateDto, TaskDetailsDto>("/api/task",
            new TaskCreateDto { Title = "Tx", ProjectId = project!.Id });

        var tasks = await _api.GetAsync<List<TaskSummaryDto>>($"/api/task/project/{project.Id}");

        tasks.Should().NotBeNull();
        tasks!.Count.Should().BeGreaterThan(0);
    }

    // NotFound case: Project has no tasks
    [Fact(DisplayName = "Get tasks by invalid project returns 404")]
    public async Task GetTasksByProject_404()
    {
        await AuthenticateAsync();

        var response = await _client.GetAsync("/api/task/project/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ------------------- UPDATE TASK TESTS -------------------

    // Success case: Update task succeeds
    [Fact(DisplayName = "Update task succeeds")]
    public async Task UpdateTask_Succeeds()
    {
        await AuthenticateAsync();

        var project = await _api.PostAsync<ProjectCreateDto, ProjectDto>("/api/project",
            new ProjectCreateDto { Name = "Proj5", Description = "D" });

        var created = await _api.PostAsync<TaskCreateDto, TaskDetailsDto>("/api/task",
            new TaskCreateDto { Title = "Old", ProjectId = project!.Id });

        var response = await _api.PutAsync(
            "/api/task/" + created!.Id,
            (object)new TaskUpdateDto { Title = "New", Description = "Updated" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await _api.GetAsync<TaskDetailsDto>("/api/task/" + created.Id);
        updated!.Title.Should().Be("New");
        updated.Description.Should().Be("Updated");
    }

    // NotFound case: Update invalid task
    [Fact(DisplayName = "Update task returns 404 when not found")]
    public async Task UpdateTask_404()
    {
        await AuthenticateAsync();

        var response = await _api.PutAsync(
            "/api/task/99999",
            (object)new TaskUpdateDto { Title = "X" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // BadRequest case: Invalid body (task exists but update body is invalid)
    [Fact(DisplayName = "Update task with invalid data returns 400")]
    public async Task UpdateTask_BadRequest_400()
    {
        await AuthenticateAsync();

        // Create project
        var project = await _api.PostAsync<ProjectCreateDto, ProjectDto>("/api/project",
            new ProjectCreateDto { Name = "ProjBad", Description = "D" });

        // Create valid task
        var created = await _api.PostAsync<TaskCreateDto, TaskDetailsDto>("/api/task",
            new TaskCreateDto { Title = "Valid", ProjectId = project!.Id });

        // Attempt invalid update
        var response = await _api.PutAsync(
            "/api/task/" + created!.Id,
            (object)new TaskUpdateDto { Title = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ------------------- UPDATE STATUS TESTS -------------------

    // Success case: Update status
    [Fact(DisplayName = "Update task status succeeds")]
    public async Task UpdateStatus_Succeeds()
    {
        await AuthenticateAsync();

        var project = await _api.PostAsync<ProjectCreateDto, ProjectDto>("/api/project",
            new ProjectCreateDto { Name = "Proj6", Description = "D" });

        var created = await _api.PostAsync<TaskCreateDto, TaskDetailsDto>("/api/task",
            new TaskCreateDto { Title = "StatusTask", ProjectId = project!.Id });

        var response = await _client.PatchAsJsonAsync(
            "/api/task/" + created!.Id + "/status",
            new UpdateStatusDto { Status = "Completed" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // BadRequest case: invalid status
    [Fact(DisplayName = "Update task status invalid returns 400")]
    public async Task UpdateStatus_BadRequest_400()
    {
        await AuthenticateAsync();

        var response = await _client.PatchAsJsonAsync(
            "/api/task/1/status",
            new UpdateStatusDto { Status = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ------------------- ASSIGN USER TESTS -------------------

    // BadRequest case: missing user ID
    [Fact(DisplayName = "Assign task user with invalid id returns 400")]
    public async Task AssignTaskUser_BadRequest_400()
    {
        await AuthenticateAsync();

        var response = await _client.PatchAsJsonAsync(
            "/api/task/1/assign",
            new TaskAssignUserDto { UserId = 0 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ------------------- DELETE TASK TESTS -------------------

    // Success case: Delete task succeeds
    [Fact(DisplayName = "Delete task succeeds")]
    public async Task DeleteTask_Succeeds()
    {
        await AuthenticateAsync();

        var project = await _api.PostAsync<ProjectCreateDto, ProjectDto>("/api/project",
            new ProjectCreateDto { Name = "Proj7", Description = "D" });

        var created = await _api.PostAsync<TaskCreateDto, TaskDetailsDto>("/api/task",
            new TaskCreateDto { Title = "DeleteMe", ProjectId = project!.Id });

        var response = await _client.DeleteAsync("/api/task/" + created!.Id);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var after = await _api.GetAsync<TaskDetailsDto>("/api/task/" + created.Id);
        after.Should().BeNull();
    }

    // NotFound case: Delete invalid task
    [Fact(DisplayName = "Delete task returns 404 for invalid ID")]
    public async Task DeleteTask_404()
    {
        await AuthenticateAsync();

        var response = await _client.DeleteAsync("/api/task/99999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}