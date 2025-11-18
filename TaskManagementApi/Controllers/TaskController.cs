using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagementApi.Models.DTOs;
using TaskManagementApi.Services;
using TaskManagementApi.Models;

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

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        /// <summary>
        /// Retrieves task related to a specific project using the project ID.
        /// </summary>
        /// <remarks>Authorization required.</remarks>
        /// <param name="projectId">Project ID.</param>
        /// <response code = "200">Project tasks have been retrieved successfully.</response>
        /// <response code = "401">Authorization is missing or invalid.</response>
        /// <response code = "404">The specified project ID does not exist.</response>
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetTasks(int projectId)
        {
            int userId = GetUserId();
            var tasks = await _taskService.GetTasksByProjectAsync(projectId, userId);
            
            if (tasks == null)
                return NotFound("Project with that specified id doesn't exist, and therefore there are no related task(s).");
            
            return Ok(tasks);
        }

        /// <summary>
        /// Retrieves a task using the task ID.
        /// </summary>
        /// <remarks>Authorization required.</remarks>
        /// <param name="id">Task ID.</param>
        /// <response code = "200">Tasks has been retrieved successfully.</response>
        /// <response code = "401">Authorization is missing or invalid.</response>
        /// <response code = "404">The specified task ID does not exist.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(int id)
        {
            int userId = GetUserId();
            var task = await _taskService.GetTaskByIdAsync(id, userId);
            if (task == null) return NotFound("Task not found or no access.");
            return Ok(task);
        }

        /// <summary>
        /// Creates a new task.
        /// </summary>
        /// <remarks>Authorization required.</remarks>
        /// <response code = "201">Tasks has been created successfully.</response>
        /// <response code = "401">Authorization is missing or invalid.</response>
        /// <response code = "400">The request was invalid or invalid input.</response>
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskCreateDto dto)
        {
            int userId = GetUserId();
            var created = await _taskService.CreateTaskAsync(dto, userId);
            
            if (created == null) 
                return BadRequest("Project not found or no access.");
            
            return CreatedAtAction(nameof(GetTask), new { id = created.Id }, created);
        }

        /// <summary>
        /// Updates a task by using its ID
        /// </summary>
        /// <remarks>Authorization required.</remarks>
        /// <param name="id">Task ID.</param>
        /// <param name="dto">Updated task data.</param>
        /// <response code = "200">Tasks has been updated successfully.</response>
        /// <response code = "401">Authorization is missing or invalid.</response>
        /// <response code = "404">The specified task ID does not exist.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskUpdateDto dto)
        {
            int userId = GetUserId();
            var success = await _taskService.UpdateTaskAsync(id, dto, userId);
            
            if (!success) 
                return NotFound("Task not found or no access.");
            
            return Ok("Task updated.");
        }

        /// <summary>
        /// Updates a task status using the task id.
        /// </summary>
        /// <remarks>Authorization required.</remarks>
        /// <param name="id">Task ID.</param>
        /// <param name="dto">Updated task status data.</param>
        /// <response code = "200">Task status has been updated successfully.</response>
        /// <response code = "401">Authorization is missing or invalid.</response>
        /// <response code = "404">The specified task ID does not exist, or user does not have access to it.</response>
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            int userId = GetUserId();
            var success = await _taskService.UpdateStatusAsync(id, dto.Status, userId);
            
            if (!success) 
                return NotFound("Task not found or no access.");
            
            return Ok($"Status updated to '{dto.Status}'.");
        }

        /// <summary>
        ///  Updates user assignment for a specific task by using the task ID.
        /// </summary>
        /// <remarks>Authorization required.</remarks>
        /// <param name="id">Task ID.</param>
        /// <param name="assignedUserId">ID of assigned user.</param>
        /// <response code = "200">Task has been assigned successfully.</response>
        /// <response code = "401">Authorization is missing or invalid.</response>
        /// <response code = "404">The specified task ID or user ID does not exist.</response>
        [HttpPatch("{id}/assign/{assignedUserId}")]
        public async Task<IActionResult> AssignTask(int id, int assignedUserId)
        {
            int userId = GetUserId();
            var success = await _taskService.AssignTaskAsync(id, assignedUserId, userId);
            if (!success) return NotFound("Task or user not found.");
            return Ok("Task assigned to user.");
        }

        /// <summary>
        /// Deletes a task using its id.
        /// </summary>
        /// <remarks>Authorization required.</remarks>
        /// <param name="id">Task ID.</param>
        /// <response code = "200">Task has been deleted successfully.</response>
        /// <response code = "401">Authorization is missing or invalid.</response>
        /// <response code = "404">The specified task ID does not exist, or user does not have access to it.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            int userId = GetUserId();
            var success = await _taskService.DeleteTaskAsync(id, userId);
            if (!success) return NotFound("Task not found or no access.");
            return Ok("Task deleted.");
        }
    }
}