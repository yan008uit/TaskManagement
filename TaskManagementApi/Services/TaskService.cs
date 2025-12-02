using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Models;
using TaskManagementApi.Models.DTOs;
using TaskStatus = TaskManagementApi.Models.TaskStatus;

namespace TaskManagementApi.Services
{
    public class TaskService
    {
        private readonly AppDbContext _context;

        public TaskService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskDto>?> GetTasksByProjectAsync(int projectId, int userId)
        {
            var tasks = await _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedByUser)
                .Where(t => t.ProjectId == projectId)
                .ToListAsync();

            return tasks.Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Status = t.Status.ToString(),  // <- convert enum to string
                CreatedDate = t.CreatedDate,
                DueDate = t.DueDate,
                ProjectId = t.ProjectId,
                CreatedByUserId = t.CreatedByUserId,
                CreatedByUsername = t.CreatedByUser?.Username,
                AssignedUserId = t.AssignedUserId,
                AssignedUsername = t.AssignedUser?.Username,
                ProjectOwnerId = t.Project?.UserId ?? 0
            });
        }

        public async Task<TaskDto?> GetTaskByIdAsync(int taskId, int userId)
        {
            var task = await _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedByUser)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return null;

            if (task.Project.UserId != userId &&
                task.CreatedByUserId != userId &&
                task.AssignedUserId != userId)
                return null;

            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                ProjectId = task.ProjectId,
                CreatedByUserId = task.CreatedByUserId,
                CreatedByUsername = task.CreatedByUser?.Username,
                AssignedUserId = task.AssignedUserId,
                AssignedUsername = task.AssignedUser?.Username,
                CreatedDate = task.CreatedDate,
                DueDate = task.DueDate,
                ProjectOwnerId = task.Project?.UserId ?? 0
            };
        }

        public async Task<TaskDto?> CreateTaskAsync(TaskCreateDto dto, int userId)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == dto.ProjectId);
            if (project == null) return null;

            // parse string Status to enum
            var statusEnum = Enum.TryParse<TaskStatus>(dto.Status, out var s) ? s : TaskStatus.ToDo;

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = statusEnum,
                DueDate = dto.DueDate,
                ProjectId = dto.ProjectId,
                CreatedByUserId = userId,
                CreatedDate = DateTime.UtcNow
            };

            if (dto.AssignedUserId.HasValue)
            {
                bool userExists = await _context.Users.AnyAsync(u => u.Id == dto.AssignedUserId.Value);
                if (userExists)
                    task.AssignedUserId = dto.AssignedUserId.Value;
            }

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            return await GetTaskByIdAsync(task.Id, userId);
        }

        public async Task<bool> UpdateTaskAsync(int taskId, TaskUpdateDto dto, int userId)
        {
            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == taskId && t.CreatedByUserId == userId);

            if (task == null) return false;

            if (!string.IsNullOrEmpty(dto.Title)) task.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Description)) task.Description = dto.Description;

            if (!string.IsNullOrEmpty(dto.Status))
            {
                if (Enum.TryParse<TaskStatus>(dto.Status, out var s))
                    task.Status = s;
            }

            if (dto.DueDate.HasValue) task.DueDate = dto.DueDate.Value;

            if (dto.AssignedUserId.HasValue)
            {
                bool userExists = await _context.Users.AnyAsync(u => u.Id == dto.AssignedUserId.Value);
                if (userExists)
                    task.AssignedUserId = dto.AssignedUserId.Value;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AssignUserAsync(int taskId, int assignedUserId, int userId)
        {
            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId && t.CreatedByUserId == userId);
            if (task == null) return false;

            bool userExists = await _context.Users.AnyAsync(u => u.Id == assignedUserId);
            if (!userExists) return false;

            task.AssignedUserId = assignedUserId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(int taskId, string status, int userId)
        {
            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId && t.CreatedByUserId == userId);
            if (task == null) return false;

            if (Enum.TryParse<TaskStatus>(status, out var s))
                task.Status = s;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTaskAsync(int taskId, int userId)
        {
            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null || task.CreatedByUserId != userId) return false;

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}