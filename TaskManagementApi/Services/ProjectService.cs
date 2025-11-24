using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Models;
using TaskManagementApi.Models.DTOs;

namespace TaskManagementApi.Services
{
    public class ProjectService
    {
        private readonly AppDbContext _context;

        public ProjectService(AppDbContext context)
        {
            _context = context;
        }

        //  Get all projects that belongs to the user
        public async Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(int userId)
        {
            var projects = await _context.Projects
                .Include(p => p.Tasks)
                .Where(p => p.UserId == userId)
                .ToListAsync();

            // Creates a new Project DTO (that also contains a TaskSummary DTO) and returns it
            return projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CreatedDate = p.CreatedDate,
                Tasks = p.Tasks.Select(t => new TaskSummaryDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status,
                    CreatedDate = t.CreatedDate,
                    DueDate = t.DueDate
                }).ToList()
            });
        }

        // Gets a single project that belongs to a user
        public async Task<ProjectDto?> GetProjectByIdAsync(int id, int userId)
        {
            var project = await _context.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            // Returns null if the project does not exist or user doesnt have access
            if (project == null)
                return null;

            // Retruns a DTO
            return new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedDate = project.CreatedDate,
                Tasks = project.Tasks.Select(t => new TaskSummaryDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status,
                    CreatedDate = t.CreatedDate,
                    DueDate = t.DueDate
                }).ToList()
            };
        }

        // Creates a new project
        public async Task<ProjectDto> CreateProjectAsync(ProjectCreateDto dto, int userId)
        {
            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedDate = DateTime.UtcNow,
                UserId = userId
            };

            // Saves to database
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            // Returns a Project DTO with a new empty task list
            return new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedDate = project.CreatedDate,
                Tasks = new()
            };
        }

        // Update
        public async Task<bool> UpdateProjectAsync(int id, ProjectUpdateDto dto, int userId)
        {
            // Checks if project exists
            var existing = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            // Returns null if it does not exist
            if (existing == null)
                return false;

            // Updates only if new values are given by user
            existing.Name = dto.Name ?? existing.Name;
            existing.Description = dto.Description ?? existing.Description;

            // Saves the updated data
            await _context.SaveChangesAsync();
            return true;
        }

        // Deletes a project if user has access
        public async Task<bool> DeleteProjectAsync(int id, int userId)
        {
            // Retrieves project
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            // Returns false if project id does not exist
            if (project == null)
                return false;

            // Deletes the project from the database
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}