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
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        }

        /// <summary>
        /// Retrieves comments for a specific task.
        /// </summary>
        /// <remarks>Authorization required.</remarks>
        /// <param name="taskId">ID of the task.</param>
        /// <response code = "200">Comment for task has successfully been retrieved.</response>
        /// <response code = "401">Authorization is missing or invalid.</response>
        /// <response code = "404">The specified task ID does not exist.</response>
        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetComments(int taskId)
        {
            int userId = GetUserId();
            var comments = await _commentService.GetCommentsByTaskAsync(taskId, userId);

            if (comments == null)
                return NotFound("Task not found, or you don't have access to this task.");

            return Ok(comments);
        }

        /// <summary>
        /// Creates a comment for a task.
        /// </summary>
        /// <remarks>Authorization required.</remarks>
        /// <response code = "201">Comment has been created successfully.</response>
        /// <response code = "400">Bad request.</response>
        /// <response code = "401">Authorization is missing or invalid.</response>
        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CommentCreateUpdateDto dto)
        {
            int userId = GetUserId();
            var created = await _commentService.CreateCommentAsync(dto, userId);
            if (created == null) return BadRequest("Task not found or access denied.");
            return CreatedAtAction(nameof(GetComments), new { taskId = created.TaskItemId }, created);
        }

        /// <summary>
        /// Deletes a comment based on ID.
        /// </summary>
        /// <remarks>Authorization required.</remarks>
        /// <param name="id">Comment ID.</param>
        /// <response code = "200">Comment has been deleted.</response>
        /// <response code = "401">Authorization is missing or invalid.</response>
        /// <response code = "404">Comment ID does not exist.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            int userId = GetUserId();
            var success = await _commentService.DeleteCommentAsync(id, userId);
            if (!success)
                return NotFound("Comment not found or access denied.");

            return Ok("Comment deleted.");
        }
    }
}