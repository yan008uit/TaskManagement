using TaskManagementWeb.Models.DTOs;

namespace TaskManagementWeb.Services
{
    public class TaskService
    {
        private readonly ApiClient _api;

        public TaskService(ApiClient api) => _api = api;

        // Centralized error logging for consistent output and easier debugging.
        private void LogError(string action, Exception ex, int? id = null)
        {
            Console.WriteLine($"[TaskService] Error during '{action}'{(id.HasValue ? $" (ID: {id})" : "")}: {ex.Message}");
        }

        // Retrieves all tasks that belong to a specific project.
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

        // Fetches a single task by its ID.
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

        // Retrieves a task including comments, project info, assigned user, etc.
        public async Task<TaskDetailsDto?> GetTaskDetailAsync(int id)
        {
            try
            {
                return await _api.GetAsync<TaskDetailsDto>($"task/{id}/details");
            }
            catch (Exception ex)
            {
                LogError("GetTaskDetail", ex, id);
                return null;
            }
        }

        // Creates a new task and returns the created record.
        public async Task<TaskDto?> CreateTaskAsync(TaskCreateDto dto)
        {
            try
            {
                return await _api.PostAsync<TaskCreateDto, TaskDto>("task", dto);
            }
            catch (Exception ex)
            {
                LogError("CreateTask", ex);
                return null;
            }
        }

        // Updates the basic attributes of an existing task.
        public async Task<bool> UpdateTaskAsync(int id, TaskUpdateDto dto)
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

        // Deletes a task.
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

        // Assigns a single user to the task.
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

        // Updates only the status field.
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