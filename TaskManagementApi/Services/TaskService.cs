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

        // Get all tasks for a project
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
                Status = t.Status.ToString(),
                CreatedDate = t.CreatedDate,
                DueDate = t.DueDate,
                ProjectId = t.ProjectId,
                CreatedByUserId = t.CreatedByUserId,
                CreatedByUsername = t.CreatedByUser?.Username,
                AssignedUserId = t.AssignedUserId,
                AssignedUsername = t.AssignedUser?.Username
            });
        }

        // Get detailed task info
        public async Task<TaskDetailsDto?> GetTaskByIdAsync(int taskId, int userId)
        {
            var task = await _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.Comments)
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedByUser)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return null;

            return new TaskDetailsDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status.ToString(),
                CreatedDate = task.CreatedDate,
                DueDate = task.DueDate,
                ProjectId = task.ProjectId,
                ProjectName = task.Project?.Name,
                CreatedByUserId = task.CreatedByUserId,
                CreatedByUsername = task.CreatedByUser?.Username,
                AssignedUserId = task.AssignedUserId,
                AssignedUsername = task.AssignedUser?.Username,
                AssignedUserEmail = task.AssignedUser?.Email,
                Comments = task.Comments
                               .Select(c => new CommentDto
                               {
                                   Id = c.Id,
                                   Text = c.Text,
                                   TaskItemId = c.TaskItemId,
                                   UserId = c.UserId,
                                   Username = c.User?.Username,
                                   CreatedDate = c.CreatedDate
                               })
                               .ToList()
            };
        }

        // Create a new task
        public async Task<TaskDetailsDto?> CreateTaskAsync(TaskCreateDto dto, int userId)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId);
            if (project == null) return null;

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                DueDate = dto.DueDate,
                ProjectId = dto.ProjectId,
                CreatedByUserId = userId,
                CreatedDate = DateTime.UtcNow
            };

            // Assign single user
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

        // Update task (only creator can update)
        public async Task<bool> UpdateTaskAsync(int taskId, TaskUpdateDto dto, int userId)
        {
            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == taskId && t.CreatedByUserId == userId);

            if (task == null) return false;

            if (!string.IsNullOrEmpty(dto.Title)) task.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Description)) task.Description = dto.Description;
            if (dto.Status.HasValue) task.Status = dto.Status.Value;
            if (dto.DueDate.HasValue) task.DueDate = dto.DueDate.Value;

            // Update assigned user
            if (dto.AssignedUserId.HasValue)
            {
                bool userExists = await _context.Users.AnyAsync(u => u.Id == dto.AssignedUserId.Value);
                if (userExists)
                    task.AssignedUserId = dto.AssignedUserId.Value;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // Assign a single user (only creator can assign)
        public async Task<bool> AssignUserAsync(int taskId, int assignedUserId, int userId)
        {
            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == taskId && t.CreatedByUserId == userId);

            if (task == null) return false;

            bool userExists = await _context.Users.AnyAsync(u => u.Id == assignedUserId);
            if (!userExists) return false;

            task.AssignedUserId = assignedUserId;
            await _context.SaveChangesAsync();
            return true;
        }

        // Update task status (creator only)
        public async Task<bool> UpdateStatusAsync(int taskId, TaskStatus status, int userId)
        {
            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == taskId && t.CreatedByUserId == userId);

            if (task == null) return false;

            task.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        // Delete task (creator only)
        public async Task<bool> DeleteTaskAsync(int taskId, int userId)
        {
            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null || task.CreatedByUserId != userId) return false;

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}