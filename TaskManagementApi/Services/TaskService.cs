using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Models;
using TaskManagementApi.Models.DTOs;
using TaskStatus = TaskManagementApi.Models.TaskStatus;

namespace TaskManagementApi.Services
{
    public class TaskService : ITaskService
    {
        private readonly AppDbContext _context;

        public TaskService(AppDbContext context)
        {
            _context = context;
        }

        // --------------------
        // Get tasks by project
        // --------------------
        /// <summary>
        /// Returns all tasks for a given project.
        /// Includes project, assigned user, and creator info.
        /// </summary>
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
                AssignedUsername = t.AssignedUser?.Username,
                ProjectOwnerId = t.Project?.UserId ?? 0
            });
        }

        // --------------------
        // Get a single task by ID
        // --------------------
        /// <summary>
        /// Returns a single task if the user is the project owner, creator, or assigned user.
        /// </summary>
        public async Task<TaskDto?> GetTaskByIdAsync(int taskId, int userId)
        {
            var task = await _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedByUser)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return null;

            // Access control: user must be owner, creator, or assignee
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

        // --------------------
        // Get detailed task info
        // --------------------
        /// <summary>
        /// Returns a task with detailed info, including comments.
        /// Access allowed if user is project owner, creator, or assignee.
        /// </summary>
        public async Task<TaskDetailsDto?> GetTaskDetailsByIdAsync(int taskId, int userId)
        {
            var task = await _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null) return null;

            // Access control
            if (task.Project.UserId != userId &&
                task.CreatedByUserId != userId &&
                task.AssignedUserId != userId)
                return null;

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
                Comments = task.Comments.Select(c => new CommentDto
                {
                    Id = c.Id,
                    Text = c.Text,
                    CreatedDate = c.CreatedDate,
                    UserId = c.UserId,
                    Username = c.User?.Username
                }).ToList()
            };
        }

        // --------------------
        // Create a new task
        // --------------------
        /// <summary>
        /// Creates a new task under a project. The current user becomes the creator.
        /// Optionally assigns a user if provided.
        /// </summary>
        public async Task<TaskDto?> CreateTaskAsync(TaskCreateDto dto, int userId)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == dto.ProjectId);
            if (project == null) return null;

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

            // Assign user if valid
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

        // --------------------
        // Update a task
        // --------------------
        /// <summary>
        /// Updates a task. Creator can edit title, description, due date, and assigned user.
        /// Both creator and assigned user can update status.
        /// </summary>
        public async Task<bool> UpdateTaskAsync(int taskId, TaskUpdateDto dto, int userId)
        {
            var task = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == taskId &&
                    (t.CreatedByUserId == userId || t.AssignedUserId == userId));

            if (task == null) return false;

            bool isCreator = task.CreatedByUserId == userId;

            // Only creator can edit core details
            if (isCreator)
            {
                if (!string.IsNullOrEmpty(dto.Title)) task.Title = dto.Title;
                if (!string.IsNullOrEmpty(dto.Description)) task.Description = dto.Description;
                if (dto.DueDate.HasValue) task.DueDate = dto.DueDate.Value;

                if (dto.AssignedUserId.HasValue)
                {
                    bool userExists = await _context.Users.AnyAsync(u => u.Id == dto.AssignedUserId.Value);
                    if (userExists)
                        task.AssignedUserId = dto.AssignedUserId.Value;
                }
            }

            // Both creator and assignee can update status
            if (!string.IsNullOrEmpty(dto.Status))
            {
                if (Enum.TryParse<TaskStatus>(dto.Status, out var s))
                    task.Status = s;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        // --------------------
        // Assign a user to a task
        // --------------------
        /// <summary>
        /// Assigns a user to a task. Only the creator of the task can assign.
        /// </summary>
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

        // --------------------
        // Update task status
        // --------------------
        /// <summary>
        /// Updates the status of a task. Only creator can update status via this method.
        /// </summary>
        public async Task<bool> UpdateStatusAsync(int taskId, string status, int userId)
        {
            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId && t.CreatedByUserId == userId);
            if (task == null) return false;

            if (Enum.TryParse<TaskStatus>(status, out var s))
                task.Status = s;

            await _context.SaveChangesAsync();
            return true;
        }

        // --------------------
        // Delete a task
        // --------------------
        /// <summary>
        /// Deletes a task. Only creator or assigned user can delete.
        /// </summary>
        public async Task<bool> DeleteTaskAsync(int taskId, int userId)
        {
            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null) return false;

            if (task.CreatedByUserId != userId && task.AssignedUserId != userId)
                return false;

            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}