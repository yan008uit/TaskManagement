using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public class TaskService
    {
        private readonly ApiClient _api;

        public TaskService(ApiClient api) => _api = api;

        // Get all tasks for a project
        public async Task<List<TaskDto>> GetTasksByProjectAsync(int projectId)
            => await _api.GetAsync<List<TaskDto>>($"task/project/{projectId}") ?? new();

        // Get single task
        public async Task<TaskDto?> GetTaskAsync(int id)
            => await _api.GetAsync<TaskDto>($"task/{id}");

        // Create a task
        public async Task<TaskDto?> CreateTaskAsync(TaskCreateUpdateDto dto)
            => await _api.PostAsync<TaskCreateUpdateDto, TaskDto>("task", dto);

        // Update a task
        public async Task<bool> UpdateTaskAsync(int id, TaskCreateUpdateDto dto)
            => await _api.PutAsync($"task/{id}", dto);

        // Delete a task
        public async Task<bool> DeleteTaskAsync(int id)
            => await _api.DeleteAsync($"task/{id}");

        // Assign single user to a task
        public async Task<bool> AssignUserAsync(int taskId, int userId)
        {
            var dto = new TaskAssignUserDto { UserId = userId };
            return await _api.PatchAsync($"task/{taskId}/assign", dto);
        }

        // Update task status
        public async Task<bool> UpdateStatusAsync(int taskId, string status)
        {
            var dto = new { Status = status };
            return await _api.PatchAsync($"task/{taskId}/status", dto);
        }
    }
}