using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Models;
using TaskManagementApi.Models.DTOs;

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
            // Checks if project exists 
            bool projectExists = await _context.Projects
                .AnyAsync(p => p.Id == projectId && p.UserId == userId);
            
            // Returns null if projects doesnt exist
            if (!projectExists)
                return null;
            
            // Retrieves task relevant for the given project id
            var tasks = await _context.TaskItems
                .Include(t => t.Project)
                .Where(t => t.ProjectId == projectId && t.Project != null && t.Project.UserId == userId)                
                .ToListAsync();

            // Returns task as a DTO
            return tasks.Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status,
                CreatedDate = t.CreatedDate,
                DueDate = t.DueDate
            });
        }

        // Get detailed task info
        public async Task<TaskDetailsDto?> GetTaskByIdAsync(int id, int userId)
        {
            // Uses taskId to retrieve task, but only if user has access rights
            var task = await _context.TaskItems
                .Include(t => t.Project)
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == id && t.Project != null && t.Project.UserId == userId);
            
            // If task does not exist null is returned
            if (task == null) 
                return null;

            // Returns all details related to a task usind taskDetail DTO
            return new TaskDetailsDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                CreatedDate = task.CreatedDate,
                DueDate = task.DueDate,
                ProjectId = task.ProjectId,
                ProjectName = task.Project?.Name,
                AssignedUserId = task.AssignedUserId,
                AssignedUserName = task.AssignedUser?.Username,
                AssignedUserEmail = task.AssignedUser?.Email
            };
        }

        // Creates a new task
        public async Task<TaskDetailsDto?> CreateTaskAsync(TaskCreateDto dto, int userId)
        {
            // Checks if project exist and user has access
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == dto.ProjectId && p.UserId == userId);

            // Returns null if either condition does not exist
            if (project == null) 
                return null;

            // Validates assigned user
            if (dto.AssignedUserId.HasValue)
            {
                var userExists = await _context.Users.AnyAsync(u => u.Id == dto.AssignedUserId.Value);
                if (!userExists) 
                    return null;
            }

            // Creates a new task 
            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                CreatedDate = DateTime.UtcNow,
                DueDate = dto.DueDate,
                ProjectId = dto.ProjectId,
                AssignedUserId = dto.AssignedUserId
            };

            // Saves the task
            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync();

            // Return the new task as a details DTO
            return new TaskDetailsDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                DueDate = task.DueDate,
                ProjectId = task.ProjectId,
                ProjectName = project.Name,
                AssignedUserId = task.AssignedUserId
            };
        }

        // Update a task
        public async Task<bool> UpdateTaskAsync(int id, TaskUpdateDto dto, int userId)
        {
            // Checks if task exist and user has access
            var task = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id && t.Project != null && t.Project.UserId == userId);            
            
            // Returns false if user doesnt have access or task doesnt exist
            if (task == null) 
                return false;
            
            // Updates only the fields where user gives input
            if (dto.Title != null)
                task.Title = dto.Title;
            
            if (dto.Description != null)
                task.Description = dto.Description;
            
            if (dto.Status.HasValue)
                task.Status = dto.Status.Value;
            
            if (dto.DueDate.HasValue)
                task.DueDate = dto.DueDate.Value;

            if (dto.AssignedUserId.HasValue)
                task.AssignedUserId = dto.AssignedUserId;

            // Saves changes and returns true
            await _context.SaveChangesAsync();
            return true;
        }

        // Delete
        public async Task<bool> DeleteTaskAsync(int id, int userId)
        {
            // Checks that the task exist and that user has access
            var task = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id && t.Project != null && t.Project.UserId == userId);
            
            // returns false if user doesnt have access or task doesnt exist
            if (task == null) 
                return false;

            // Deletes task from database
            _context.TaskItems.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }

        // Update status
        public async Task<bool> UpdateStatusAsync(int id, Models.TaskStatus status, int userId)
        {
            // Retrieves the task and checks if user has access
            var task = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id && t.Project != null && t.Project.UserId == userId);
            
            if (task == null) 
                return false;

            // Updates task status
            task.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        // Assign task to another user
        public async Task<bool> AssignTaskAsync(int id, int assignedUserId, int userId)
        {
            // Checks that the task exist and that user has access
            var task = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id && t.Project != null && t.Project.UserId == userId);
            
            if (task == null) 
                return false;

            // Checks that assigned user exists
            var assignedUser = await _context.Users.FindAsync(assignedUserId);
            
            if (assignedUser == null) 
                return false;

            // Assigns the task to a user
            task.AssignedUserId = assignedUserId;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}