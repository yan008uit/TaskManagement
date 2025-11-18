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
    public class ProjectController : ControllerBase
    {
        private readonly ProjectService _projectService;

        public ProjectController(ProjectService projectService)
        {
            _projectService = projectService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        /// <summary>
        /// Retrieves all projects and their task(s).
        /// </summary>
        /// <remarks>Authorization required.</remarks>
        /// <response code = "200">Projects have been retrieved successfully.</response>
        /// <response code = "401">Authorization is missing or invalid.</response>
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var userId = GetUserId();
            var projects = await _projectService.GetUserProjectsAsync(userId);
            return Ok(projects);
        }

        /// <summary>
        /// Retrieves a project and its task by using the project ID.
        /// </summary>
        /// <remarks>Authorization required.</remarks>
        /// <param name="id">Project ID.</param>
        /// <response code = "200">Project has been retrieved successfully.</response>
        /// <response code = "401">Authorization is missing or invalid.</response>
        /// <response code = "404">The specified project ID does not exist.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var userId = GetUserId();
            var project = await _projectService.GetProjectByIdAsync(id, userId);

            if (project == null)
                return NotFound("Project not found or access denied.");

            return Ok(project);
        }

        /// <summary>
        /// Creates a new project.
        /// </summary>
        /// <remarks>Authorization required.</remarks>
        /// <response code = "201">Successfully created a new project.</response>
        /// <response code = "400">Invalid input data.</response>
        /// <response code = "401">Authorization is missing or invalid.</response>
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] ProjectCreateDto dto)
        {
            var userId = GetUserId();
            var createdProject = await _projectService.CreateProjectAsync(dto, userId);
            return CreatedAtAction(nameof(GetProject), new { id = createdProject.Id }, createdProject);
        }

        /// <summary>
        /// Updates a project name or/and description using the project ID.
        /// </summary>
        /// <remarks>Authorization required.</remarks>
        /// <param name="id">Project ID.</param>
        /// <param name="dto">Updated project daata.</param>
        /// <response code = "200">Project has been updated successfully.</response>
        /// <response code = "401">Authorization is missing or invalid.</response>
        /// <response code = "404">The specified project ID does not exist.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] ProjectUpdateDto dto)
        {
            var userId = GetUserId();
            var updated = await _projectService.UpdateProjectAsync(id, dto, userId);

            if (!updated)
                return NotFound("Project not found or access denied.");

            return Ok("Project successfully updated.");
        }

        /// <summary>
        /// Deletes a project using its ID.
        /// </summary>
        /// <remarks>Authorization required.</remarks>
        /// <param name="id">Project ID.</param>
        /// <response code = "200">Project has been deleted successfully.</response>
        /// <response code = "401">Authorization is missing or invalid.</response>
        /// <response code = "404">The specified project ID does not exist.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var userId = GetUserId();
            var deleted = await _projectService.DeleteProjectAsync(id, userId);

            if (!deleted)
                return NotFound("Project not found or access denied.");

            return Ok("Project successfully deleted.");
        }
    }
}