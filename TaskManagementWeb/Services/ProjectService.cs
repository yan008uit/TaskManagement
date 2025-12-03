using TaskManagementWeb.Models;
using TaskManagementWeb.Models.DTOs;

namespace TaskManagementWeb.Services
{
    public class ProjectService
    {
        private readonly ApiClient _api;

        public ProjectService(ApiClient api) => _api = api;

        private void LogError(string action, Exception ex, int? id = null)
        {
            Console.WriteLine($"[ProjectService] Error during '{action}'{(id.HasValue ? $" (ID: {id})" : "")}: {ex.Message}");
        }

        // Get all projects visible to the user (owns or has tasks)
        public async Task<List<ProjectDto>> GetVisibleProjectsAsync()
        {
            try
            {
                return await _api.GetAsync<List<ProjectDto>>("project/visible") ?? new();
            }
            catch (Exception ex)
            {
                LogError("GetVisibleProjects", ex);
                return new();
            }
        }

        // Get a project
        public async Task<ProjectDto?> GetProjectAsync(int id)
        {
            try
            {
                return await _api.GetAsync<ProjectDto>($"project/{id}");
            }
            catch (Exception ex)
            {
                LogError("GetProject", ex, id);
                return null;
            }
        }

        // Create project
        public async Task<ProjectDto?> CreateProjectAsync(ProjectCreateDto dto)
        {
            try
            {
                return await _api.PostAsync<ProjectCreateDto, ProjectDto>("project", dto);
            }
            catch (Exception ex)
            {
                LogError("CreateProject", ex);
                return null;
            }
        }

        // Update project
        public async Task<bool> UpdateProjectAsync(int id, ProjectUpdateDto dto)
        {
            try
            {
                return await _api.PutAsync($"project/{id}", dto);
            }
            catch (Exception ex)
            {
                LogError("UpdateProject", ex, id);
                return false;
            }
        }

        // Delete project
        public async Task<bool> DeleteProjectAsync(int id)
        {
            try
            {
                return await _api.DeleteAsync($"project/{id}");
            }
            catch (Exception ex)
            {
                LogError("DeleteProject", ex, id);
                return false;
            }
        }
    }
}