using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagementApi.Models.DTOs;
using TaskManagementApi.Services;

namespace TaskManagementApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly TaskService _taskService;

        public TaskController(TaskService taskService)
        {
            _taskService = taskService;
        }

        // Helper to get current authenticated user's ID
        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        // GET: api/task/project/{projectId}
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetTasksByProject(int projectId)
        {
            int userId = GetUserId();
            var tasks = await _taskService.GetTasksByProjectAsync(projectId, userId);

            if (tasks == null || !tasks.Any())
                return NotFound("Project not found or no tasks available.");

            return Ok(tasks);
        }

        // GET: api/task/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            int userId = GetUserId();
            var task = await _taskService.GetTaskByIdAsync(id, userId);

            if (task == null)
                return NotFound("Task not found or no access.");

            return Ok(task);
        }

        // POST: api/task
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int userId = GetUserId();
            var createdTask = await _taskService.CreateTaskAsync(dto, userId);

            if (createdTask == null)
                return BadRequest("Project not found or no access.");

            return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
        }

        // PUT: api/task/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int userId = GetUserId();
            bool updated = await _taskService.UpdateTaskAsync(id, dto, userId);

            if (!updated)
                return NotFound("Task not found or no access.");

            return Ok("Task updated successfully.");
        }

        // PATCH: api/task/{id}/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int userId = GetUserId();
            bool updated = await _taskService.UpdateStatusAsync(id, dto.Status, userId);

            if (!updated)
                return NotFound("Task not found or no access.");

            return Ok($"Task status updated to '{dto.Status}'.");
        }

        // PATCH: api/task/{id}/assign
        [HttpPatch("{id}/assign")]
        public async Task<IActionResult> AssignUserToTask(int id, [FromBody] TaskAssignUsersDto dto)
        {
            if (dto.UserId <= 0)
                return BadRequest("Assigned user ID is required.");

            int userId = GetUserId();
            bool assigned = await _taskService.AssignUserAsync(id, dto.UserId, userId);

            if (!assigned)
                return NotFound("Task not found or user does not exist.");

            return Ok("Task assigned to user successfully.");
        }

        // DELETE: api/task/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            int userId = GetUserId();
            bool deleted = await _taskService.DeleteTaskAsync(id, userId);

            if (!deleted)
                return NotFound("Task not found or no access.");

            return Ok("Task deleted successfully.");
        }
    }
}