using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TaskManagementApi.Controllers;
using TaskManagementApi.Models.DTOs;
using TaskManagementApi.Services;

namespace TaskManagementApi.Tests
{
    public class ProjectControllerTests
    {
        private readonly Mock<IProjectService> _projectServiceMock;
        private readonly ProjectController _controller;

        public ProjectControllerTests()
        {
            _projectServiceMock = new Mock<IProjectService>();
            _controller = new ProjectController(_projectServiceMock.Object);

            // Mock user with ID = 1
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        // ------------------- GET PROJECTS -------------------

        // Success case: Retrieve all projects for the user
        [Fact]
        public async Task GetProjects_ReturnsOk_WhenProjectsExist()
        {
            var projects = new List<ProjectDto> { new ProjectDto { Id = 1, Name = "Test Project" } };
            _projectServiceMock.Setup(s => s.GetUserProjectsAsync(1)).ReturnsAsync(projects);

            var result = await _controller.GetProjects();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProjects = Assert.IsType<List<ProjectDto>>(okResult.Value);
            Assert.Single(returnedProjects);
            Assert.Equal("Test Project", returnedProjects[0].Name);
        }

        // Edge case: No projects exist for the user
        [Fact]
        public async Task GetProjects_ReturnsOk_WhenNoProjectsExist()
        {
            _projectServiceMock.Setup(s => s.GetUserProjectsAsync(1)).ReturnsAsync(new List<ProjectDto>());

            var result = await _controller.GetProjects();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProjects = Assert.IsType<List<ProjectDto>>(okResult.Value);
            Assert.Empty(returnedProjects);
        }

        // ------------------- GET PROJECT BY ID -------------------

        // Success case: Retrieve a project by ID
        [Fact]
        public async Task GetProject_ReturnsOk_WhenProjectExists()
        {
            var project = new ProjectDto { Id = 2, Name = "Found Project" };
            _projectServiceMock.Setup(s => s.GetProjectByIdAsync(2, 1)).ReturnsAsync(project);

            var result = await _controller.GetProject(2);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProject = Assert.IsType<ProjectDto>(okResult.Value);
            Assert.Equal(2, returnedProject.Id);
            Assert.Equal("Found Project", returnedProject.Name);
        }

        // Not found case: Project does not exist
        [Fact]
        public async Task GetProject_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            _projectServiceMock.Setup(s => s.GetProjectByIdAsync(99, 1)).ReturnsAsync((ProjectDto?)null);

            var result = await _controller.GetProject(99);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Project not found or access denied.", notFoundResult.Value);
        }

        // ------------------- GET VISIBLE PROJECTS -------------------

        // Success case: Retrieve all visible projects for the user
        [Fact]
        public async Task GetVisibleProjects_ReturnsOk_WhenProjectsExist()
        {
            var projects = new List<ProjectDto> { new ProjectDto { Id = 5, Name = "Visible Project" } };
            _projectServiceMock.Setup(s => s.GetVisibleProjectsAsync(1)).ReturnsAsync(projects);

            var result = await _controller.GetVisibleProjects();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedProjects = Assert.IsType<List<ProjectDto>>(okResult.Value);
            Assert.Single(returnedProjects);
            Assert.Equal("Visible Project", returnedProjects[0].Name);
        }

        // ------------------- CREATE PROJECT -------------------

        // Success case: Create a new project
        [Fact]
        public async Task CreateProject_ReturnsCreated_WhenProjectCreatedSuccessfully()
        {
            var dto = new ProjectCreateDto { Name = "New Project" };
            var createdProject = new ProjectDto { Id = 10, Name = "New Project" };

            _projectServiceMock.Setup(s => s.CreateProjectAsync(dto, 1)).ReturnsAsync(createdProject);

            var result = await _controller.CreateProject(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var project = Assert.IsType<ProjectDto>(createdResult.Value);
            Assert.Equal(10, project.Id);
            Assert.Equal("New Project", project.Name);
        }

        // Invalid input case: ModelState is invalid (Missing required fields)
        [Fact]
        public async Task CreateProject_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Name", "Required");

            var dto = new ProjectCreateDto();
            var result = await _controller.CreateProject(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequest.Value);
        }

        // ------------------- UPDATE PROJECT -------------------

        // Success case: Update an existing project
        [Fact]
        public async Task UpdateProject_ReturnsOk_WhenProjectUpdatedSuccessfully()
        {
            var dto = new ProjectUpdateDto { Name = "Updated Project" };
            _projectServiceMock.Setup(s => s.UpdateProjectAsync(1, dto, 1)).ReturnsAsync(true);

            var result = await _controller.UpdateProject(1, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Project successfully updated.", okResult.Value);
        }

        // Not found case: Project does not exist
        [Fact]
        public async Task UpdateProject_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            var dto = new ProjectUpdateDto { Name = "Updated Project" };
            _projectServiceMock.Setup(s => s.UpdateProjectAsync(99, dto, 1)).ReturnsAsync(false);

            var result = await _controller.UpdateProject(99, dto);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Project not found or access denied.", notFoundResult.Value);
        }

        // ------------------- DELETE PROJECT -------------------

        // Success case: Delete an existing project
        [Fact]
        public async Task DeleteProject_ReturnsOk_WhenProjectDeletedSuccessfully()
        {
            _projectServiceMock.Setup(s => s.DeleteProjectAsync(1, 1)).ReturnsAsync(true);

            var result = await _controller.DeleteProject(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Project successfully deleted.", okResult.Value);
        }

        // Not found case: Project does not exist
        [Fact]
        public async Task DeleteProject_ReturnsNotFound_WhenProjectDoesNotExist()
        {
            _projectServiceMock.Setup(s => s.DeleteProjectAsync(99, 1)).ReturnsAsync(false);

            var result = await _controller.DeleteProject(99);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Project not found or access denied.", notFoundResult.Value);
        }
    }
}