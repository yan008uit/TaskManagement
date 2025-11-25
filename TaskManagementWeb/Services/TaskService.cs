using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public class TaskService
    {
        private readonly ApiClient _api;

        public TaskService(ApiClient api) => _api = api;

        private void LogError(string action, Exception ex, int? id = null)
        {
            Console.WriteLine($"[TaskService] Error during '{action}'{(id.HasValue ? $" (ID: {id})" : "")}: {ex.Message}");
        }

        // Get all tasks for a project
        public async Task<List<TaskDto>> GetTasksByProjectAsync(int projectId)
        {
            try
            {
                return await _api.GetAsync<List<TaskDto>>($"task/project/{projectId}") ?? new();
            }
            catch (Exception ex)
            {
                LogError("GetTasksByProject", ex, projectId);
                return new();
            }
        }

        // Get a task
        public async Task<TaskDto?> GetTaskAsync(int id)
        {
            try
            {
                return await _api.GetAsync<TaskDto>($"task/{id}");
            }
            catch (Exception ex)
            {
                LogError("GetTask", ex, id);
                return null;
            }
        }

        // Create task
        public async Task<TaskDto?> CreateTaskAsync(TaskCreateUpdateDto dto)
        {
            try
            {
                return await _api.PostAsync<TaskCreateUpdateDto, TaskDto>("task", dto);
            }
            catch (Exception ex)
            {
                LogError("CreateTask", ex);
                return null;
            }
        }

        // Update task
        public async Task<bool> UpdateTaskAsync(int id, TaskCreateUpdateDto dto)
        {
            try
            {
                return await _api.PutAsync($"task/{id}", dto);
            }
            catch (Exception ex)
            {
                LogError("UpdateTask", ex, id);
                return false;
            }
        }

        // Delete task
        public async Task<bool> DeleteTaskAsync(int id)
        {
            try
            {
                return await _api.DeleteAsync($"task/{id}");
            }
            catch (Exception ex)
            {
                LogError("DeleteTask", ex, id);
                return false;
            }
        }

        // Assign single user
        public async Task<bool> AssignUserAsync(int taskId, int userId)
        {
            try
            {
                var dto = new TaskAssignUserDto { UserId = userId };
                return await _api.PatchAsync($"task/{taskId}/assign", dto);
            }
            catch (Exception ex)
            {
                LogError("AssignUser", ex, taskId);
                return false;
            }
        }

        //  Update status
        public async Task<bool> UpdateStatusAsync(int taskId, string status)
        {
            try
            {
                var dto = new { Status = status };
                return await _api.PatchAsync($"task/{taskId}/status", dto);
            }
            catch (Exception ex)
            {
                LogError("UpdateStatus", ex, taskId);
                return false;
            }
        }
    }
}