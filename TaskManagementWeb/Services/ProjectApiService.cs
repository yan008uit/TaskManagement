using TaskManagementWeb.Models.DTOs;

namespace TaskManagementWeb.Services
{
    public class ProjectApiService
    {
        private readonly ApiClient _api;

        // Cached list of users to avoid repeated fetching
        private List<UserDto> _users = new();
        private bool _usersLoaded = false;

        public ProjectApiService(ApiClient api) => _api = api;

        // Internal error logger
        private void LogError(string action, Exception ex, int? id = null)
        {
            Console.WriteLine($"[ProjectApiService] Error during '{action}'{(id.HasValue ? $" (ID: {id})" : "")}: {ex.Message}");
        }

        // Load all users once per session
        private async Task LoadUsersAsync()
        {
            if (_usersLoaded) return;

            try
            {
                _users = await _api.GetAsync<List<UserDto>>("user") ?? new List<UserDto>();
            }
            catch (Exception ex)
            {
                LogError("LoadUsers", ex);
                _users = new List<UserDto>();
            }

            _usersLoaded = true;
        }

        // Public method to get all users
        public async Task<List<UserDto>> GetUsersAsync()
        {
            await LoadUsersAsync();
            return _users;
        }

        // Get all projects visible to the current user
        public async Task<List<ProjectDto>> GetVisibleProjectsAsync()
        {
            try
            {
                return await _api.GetAsync<List<ProjectDto>>("project/visible") ?? new List<ProjectDto>();
            }
            catch (Exception ex)
            {
                LogError("GetVisibleProjects", ex);
                return new List<ProjectDto>();
            }
        }

        // Get a single project by ID
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

        // Create a new project
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

        // Update existing project
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

        // Delete a project
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