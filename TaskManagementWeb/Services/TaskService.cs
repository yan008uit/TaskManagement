using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public class TaskService
    {
        private readonly ApiClient _api;

        public TaskService(ApiClient api) => _api = api;

        public async Task<List<TaskDto>> GetTasksByProjectAsync(int projectId)
            => await _api.GetAsync<List<TaskDto>>($"task/project/{projectId}") ?? new();

        public async Task<TaskDto?> GetTaskAsync(int id)
            => await _api.GetAsync<TaskDto>($"task/{id}");

        public async Task<TaskDto?> CreateTaskAsync(TaskCreateUpdateDto dto)
            => await _api.PostAsync<TaskCreateUpdateDto, TaskDto>("task", dto);

        public async Task<bool> UpdateTaskAsync(int id, TaskCreateUpdateDto dto)
            => await _api.PutAsync($"task/{id}", dto);

        public async Task<bool> DeleteTaskAsync(int id)
            => await _api.DeleteAsync($"task/{id}");
    }
}