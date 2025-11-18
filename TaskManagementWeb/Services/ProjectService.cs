using TaskManagementWeb.Models;
using TaskManagementWeb.Services;
using System.Net.Http;

namespace TaskManagementWeb.Services
{
    public class ProjectService
    {
        private readonly ApiClient _api;

        public ProjectService(ApiClient api)
        {
            _api = api;
        }

        public async Task<List<ProjectDto>> GetProjectsAsync()
        {
            try
            {
                var projects = await _api.GetAsync<List<ProjectDto>>("project");
                return projects ?? new List<ProjectDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching projects: {ex.Message}");
                return new List<ProjectDto>();
            }
        }

        public async Task<ProjectDto?> GetProjectAsync(int id)
        {
            try
            {
                return await _api.GetAsync<ProjectDto>($"project/{id}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error fetching project {id}: {ex.Message}");
                return null;
            }
        }

        public async Task<ProjectDto?> CreateProjectAsync(ProjectCreateDto dto)
        {
            try
            {
                return await _api.PostAsync<ProjectCreateDto, ProjectDto>("project", dto);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error creating project: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateProjectAsync(int id, ProjectCreateDto dto)
        {
            try
            {
                return await _api.PutAsync($"project/{id}", dto);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error updating project {id}: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            try
            {
                return await _api.DeleteAsync($"project/{id}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error deleting project {id}: {ex.Message}");
                return false;
            }
        }
    }
}