using TaskManagementApi.Models.DTOs;

namespace TaskManagementApi.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskDto>?> GetTasksByProjectAsync(int projectId, int userId);
        Task<TaskDto?> GetTaskByIdAsync(int taskId, int userId);
        Task<TaskDetailsDto?> GetTaskDetailsByIdAsync(int taskId, int userId);
        Task<TaskDto?> CreateTaskAsync(TaskCreateDto dto, int userId);
        Task<bool> UpdateTaskAsync(int taskId, TaskUpdateDto dto, int userId);
        Task<bool> AssignUserAsync(int taskId, int assignedUserId, int userId);
        Task<bool> UpdateStatusAsync(int taskId, string status, int userId);
        Task<bool> DeleteTaskAsync(int taskId, int userId);
    }
}