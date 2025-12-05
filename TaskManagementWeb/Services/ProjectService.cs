using TaskManagementWeb.Models.DTOs;

namespace TaskManagementWeb.Services
{
    public class ProjectService
    {
        private readonly ApiClient _api;

        // Cached list of users to avoid repeatedly fetching them.
        private List<UserDto> _users = new();
        private bool _usersLoaded = false;

        public ProjectService(ApiClient api) => _api = api;

        // Internal helper for consistent error logging.
        private void LogError(string action, Exception ex, int? id = null)
        {
            Console.WriteLine($"[ProjectService] Error during '{action}'{(id.HasValue ? $" (ID: {id})" : "")}: {ex.Message}");
        }

        // Loads all users once per application session.
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

        // Retrieves all users.
        public async Task<List<UserDto>> GetUsersAsync()
        {
            await LoadUsersAsync();
            return _users;
        }

        // Retrieves all projects.
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

        // Retrieves a single project by its ID.
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

        // Request to create a new project.
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

        // Updates an existing project with new data.
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

        // Deletes a project.
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