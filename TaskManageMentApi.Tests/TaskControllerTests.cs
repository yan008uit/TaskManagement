using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TaskManagementApi.Controllers;
using TaskManagementApi.Models.DTOs;
using TaskManagementApi.Services;

namespace TaskManagementApi.Tests
{
    public class TaskControllerTests
    {
        private readonly Mock<ITaskService> _mockService;
        private readonly TaskController _controller;

        public TaskControllerTests()
        {
            _mockService = new Mock<ITaskService>();
            _controller = new TaskController(_mockService.Object);

            // Mock authenticated user with ID = 1
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        // ------------------- GET TASKS BY PROJECT -------------------

        // Success case: Tasks exist for project
        [Fact]
        public async Task GetTasksByProject_ReturnsOk_WhenTasksExist()
        {
            var tasks = new List<TaskDto>
            {
                new TaskDto { Id = 1, Title = "Task 1" }
            };

            _mockService.Setup(s => s.GetTasksByProjectAsync(1, 1))
                        .ReturnsAsync(tasks);

            var result = await _controller.GetTasksByProject(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTasks = Assert.IsType<List<TaskDto>>(okResult.Value);
            Assert.Single(returnedTasks);
            Assert.Equal("Task 1", returnedTasks[0].Title);
        }

        // NotFound case: No tasks or project not found
        [Fact]
        public async Task GetTasksByProject_ReturnsNotFound_WhenNoTasks()
        {
            _mockService.Setup(s => s.GetTasksByProjectAsync(99, 1))
                        .ReturnsAsync(new List<TaskDto>());

            var result = await _controller.GetTasksByProject(99);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Project not found or no tasks available.", notFound.Value);
        }

        // ------------------- GET TASK BY ID -------------------

        // Success case: Task exists
        [Fact]
        public async Task GetTask_ReturnsOk_WhenTaskExists()
        {
            var task = new TaskDto { Id = 1, Title = "Task 1" };
            _mockService.Setup(s => s.GetTaskByIdAsync(1, 1)).ReturnsAsync(task);

            var result = await _controller.GetTask(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTask = Assert.IsType<TaskDto>(okResult.Value);
            Assert.Equal(1, returnedTask.Id);
        }

        // NotFound case: Task does not exist
        [Fact]
        public async Task GetTask_ReturnsNotFound_WhenTaskNotFound()
        {
            _mockService.Setup(s => s.GetTaskByIdAsync(99, 1)).ReturnsAsync((TaskDto?)null);

            var result = await _controller.GetTask(99);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Task not found or no access.", notFound.Value);
        }

        // ------------------- GET TASK DETAILS BY ID -------------------

        // Success case: Task details exist
        [Fact]
        public async Task GetTaskDetailsByIdAsync_ReturnsOk_WhenTaskExists()
        {
            var taskDetails = new TaskDetailsDto
            {
                Id = 1,
                Title = "Task 1",
                Description = "Task details",
                Status = "Pending"
            };

            _mockService.Setup(s => s.GetTaskDetailsByIdAsync(1, 1))
                        .ReturnsAsync(taskDetails);

            var result = await _controller.GetTaskDetailsByIdAsync(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedTask = Assert.IsType<TaskDetailsDto>(okResult.Value);
            Assert.Equal(1, returnedTask.Id);
            Assert.Equal("Task 1", returnedTask.Title);
        }

        // NotFound case: Task details not found or no access
        [Fact]
        public async Task GetTaskDetailsByIdAsync_ReturnsNotFound_WhenTaskNotFound()
        {
            _mockService.Setup(s => s.GetTaskDetailsByIdAsync(99, 1))
                        .ReturnsAsync((TaskDetailsDto?)null);

            var result = await _controller.GetTaskDetailsByIdAsync(99);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Task not found or no access.", notFound.Value);
        }

        // ------------------- CREATE TASK -------------------

        // Success case: Task created successfully
        [Fact]
        public async Task CreateTask_ReturnsCreatedTask_WhenValidInput()
        {
            var dto = new TaskCreateDto { Title = "New Task" };
            var createdTask = new TaskDto { Id = 10, Title = "New Task" };

            _mockService.Setup(s => s.CreateTaskAsync(dto, 1))
                        .ReturnsAsync(createdTask);

            var result = await _controller.CreateTask(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var task = Assert.IsType<TaskDto>(createdResult.Value);
            Assert.Equal(10, task.Id);
            Assert.Equal("New Task", task.Title);
        }

        // Invalid input: ModelState is invalid
        [Fact]
        public async Task CreateTask_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Title", "Required");
            var dto = new TaskCreateDto(); // Missing title

            var result = await _controller.CreateTask(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequest.Value);
        }

        // ------------------- UPDATE TASK -------------------

        // Success case: Status updated by task creator
        [Fact]
        public async Task UpdateStatus_ReturnsOk_WhenCreatorUpdatesStatus()
        {
            var dto = new UpdateStatusDto { Status = "InProgress" };
            _mockService.Setup(s => s.UpdateStatusAsync(1, "InProgress", 1)).ReturnsAsync(true);

            var result = await _controller.UpdateStatus(1, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Task status updated to 'InProgress'.", okResult.Value);
        }

        // Success case: Status updated by assigned user
        [Fact]
        public async Task UpdateStatus_ReturnsOk_WhenAssignedUserUpdatesStatus()
        {
            var dto = new UpdateStatusDto { Status = "Completed" };
            _mockService.Setup(s => s.UpdateStatusAsync(1, "Completed", 1)).ReturnsAsync(true);

            var result = await _controller.UpdateStatus(1, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Task status updated to 'Completed'.", okResult.Value);
        }

        // NotFound case: Task does not exist
        [Fact]
        public async Task UpdateTask_ReturnsNotFound_WhenTaskNotFound()
        {
            var dto = new TaskUpdateDto { Title = "Updated Task" };
            _mockService.Setup(s => s.UpdateTaskAsync(99, dto, 1)).ReturnsAsync(false);

            var result = await _controller.UpdateTask(99, dto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Task not found or no access.", notFound.Value);
        }

        // Invalid input: ModelState invalid
        [Fact]
        public async Task UpdateTask_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Title", "Required");
            var dto = new TaskUpdateDto();

            var result = await _controller.UpdateTask(1, dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequest.Value);
        }

        // NotFound case: Unauthorized user attempts to update status
        [Fact]
        public async Task UpdateStatus_ReturnsNotFound_WhenUserNotAssignedOrCreator()
        {
            var dto = new UpdateStatusDto { Status = "Blocked" };
            _mockService.Setup(s => s.UpdateStatusAsync(1, "Blocked", 1)).ReturnsAsync(false);

            var result = await _controller.UpdateStatus(1, dto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Task not found or no access.", notFound.Value);
        }

        // ------------------- DELETE TASK -------------------

        // Success case: Task deleted
        [Fact]
        public async Task DeleteTask_ReturnsOk_WhenTaskDeleted()
        {
            _mockService.Setup(s => s.DeleteTaskAsync(1, 1)).ReturnsAsync(true);

            var result = await _controller.DeleteTask(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Task deleted successfully.", okResult.Value);
        }

        // NotFound case: Task not found
        [Fact]
        public async Task DeleteTask_ReturnsNotFound_WhenTaskNotFound()
        {
            _mockService.Setup(s => s.DeleteTaskAsync(99, 1)).ReturnsAsync(false);

            var result = await _controller.DeleteTask(99);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Task not found or no access.", notFound.Value);
        }

        // ------------------- UPDATE STATUS -------------------

        // Success case: Status updated
        [Fact]
        public async Task UpdateStatus_ReturnsOk_WhenStatusUpdated()
        {
            var dto = new UpdateStatusDto { Status = "Done" };
            _mockService.Setup(s => s.UpdateStatusAsync(1, "Done", 1)).ReturnsAsync(true);

            var result = await _controller.UpdateStatus(1, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Task status updated to 'Done'.", okResult.Value);
        }

        // NotFound case: Status update failed
        [Fact]
        public async Task UpdateStatus_ReturnsNotFound_WhenTaskNotFound()
        {
            var dto = new UpdateStatusDto { Status = "Done" };
            _mockService.Setup(s => s.UpdateStatusAsync(99, "Done", 1)).ReturnsAsync(false);

            var result = await _controller.UpdateStatus(99, dto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Task not found or no access.", notFound.Value);
        }

        // Invalid input: ModelState invalid
        [Fact]
        public async Task UpdateStatus_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            _controller.ModelState.AddModelError("Status", "Required");
            var dto = new UpdateStatusDto();

            var result = await _controller.UpdateStatus(1, dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequest.Value);
        }

        // ------------------- ASSIGN USER -------------------

        // Success case: Assign user to task
        [Fact]
        public async Task AssignUserToTask_ReturnsOk_WhenAssignmentSucceeds()
        {
            var dto = new TaskAssignUserDto { UserId = 2 };
            _mockService.Setup(s => s.AssignUserAsync(1, 2, 1)).ReturnsAsync(true);

            var result = await _controller.AssignUserToTask(1, dto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Task assigned to user successfully.", okResult.Value);
        }

        // BadRequest: Invalid user ID
        [Fact]
        public async Task AssignUserToTask_ReturnsBadRequest_WhenUserIdInvalid()
        {
            var dto = new TaskAssignUserDto { UserId = 0 };

            var result = await _controller.AssignUserToTask(1, dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Assigned user ID is required.", badRequest.Value);
        }

        // NotFound: Assignment fails
        [Fact]
        public async Task AssignUserToTask_ReturnsNotFound_WhenAssignmentFails()
        {
            var dto = new TaskAssignUserDto { UserId = 2 };
            _mockService.Setup(s => s.AssignUserAsync(1, 2, 1)).ReturnsAsync(false);

            var result = await _controller.AssignUserToTask(1, dto);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Task not found or user does not exist.", notFound.Value);
        }
    }
}