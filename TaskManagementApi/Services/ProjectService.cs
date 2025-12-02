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

        // --------------------
        // Helper: Map TaskItem -> TaskSummaryDto
        // --------------------
        private static TaskSummaryDto MapToTaskSummary(TaskItem t) => new()
        {
            Id = t.Id,
            Title = t.Title,
            Status = t.Status.ToString(),  // <- convert enum to string
            CreatedDate = t.CreatedDate,
            DueDate = t.DueDate,
            AssignedUserId = t.AssignedUserId,
            CreatedByUserId = t.CreatedByUserId
        };

        // Helper: Map TaskItem -> TaskDetailsDto
        private static TaskDetailsDto MapToTaskDetails(TaskItem t) => new()
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Status = t.Status.ToString(),  // <- convert enum to string
            CreatedDate = t.CreatedDate,
            DueDate = t.DueDate,
            ProjectId = t.ProjectId,
            ProjectName = t.Project?.Name,
            CreatedByUserId = t.CreatedByUserId,
            CreatedByUsername = t.CreatedByUser?.Username,
            AssignedUserId = t.AssignedUserId,
            AssignedUsername = t.AssignedUser?.Username,
            AssignedUserEmail = t.AssignedUser?.Email,
            Comments = t.Comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Text = c.Text,
                UserId = c.UserId,
                CreatedDate = c.CreatedDate
            }).ToList()
        };

        // --------------------
        // Get all projects owned by user
        // --------------------
        public async Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(int userId)
        {
            var projects = await _context.Projects
                .Include(p => p.Tasks)
                .Where(p => p.UserId == userId)
                .ToListAsync();

            return projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                CreatedDate = p.CreatedDate,
                Tasks = p.Tasks.Select(MapToTaskSummary).ToList()
            });
        }

        // --------------------
        // Get a single project by id (if user owns or has tasks)
        // --------------------
        public async Task<ProjectDto?> GetProjectByIdAsync(int id, int userId)
        {
            var project = await _context.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id &&
                    (p.UserId == userId || p.Tasks.Any(t => t.AssignedUserId == userId || t.CreatedByUserId == userId)));

            if (project == null) return null;

            return new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedDate = project.CreatedDate,
                UserId = project.UserId,
                Tasks = project.Tasks.Select(MapToTaskSummary).ToList()
            };
        }

        // --------------------
        // Create a new project
        // --------------------
        public async Task<ProjectDto> CreateProjectAsync(ProjectCreateDto dto, int userId)
        {
            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedDate = DateTime.UtcNow,
                UserId = userId
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                CreatedDate = project.CreatedDate,
                Tasks = new()
            };
        }

        // --------------------
        // Update a project
        // --------------------
        public async Task<bool> UpdateProjectAsync(int id, ProjectUpdateDto dto, int userId)
        {
            var existing = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            if (existing == null) return false;

            existing.Name = dto.Name ?? existing.Name;
            existing.Description = dto.Description ?? existing.Description;

            await _context.SaveChangesAsync();
            return true;
        }

        // --------------------
        // Delete a project
        // --------------------
        public async Task<bool> DeleteProjectAsync(int id, int userId)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            if (project == null) return false;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }

        // --------------------
        // Get all projects visible to user
        // --------------------
        public async Task<List<ProjectDto>> GetVisibleProjectsAsync(int userId)
        {
            var projects = await _context.Projects
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.AssignedUser)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.CreatedByUser)
                .Where(p => p.UserId == userId || p.Tasks.Any(t => t.AssignedUserId == userId || t.CreatedByUserId == userId))
                .ToListAsync();

            return projects.Select(p => new ProjectDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                UserId = p.UserId,
                Tasks = p.Tasks.Select(MapToTaskSummary).ToList()
            }).ToList();
        }
    }
}