using TaskManagementApi.Models.DTOs;

namespace TaskManagementApi.Services
{
    public interface IProjectService
    {
        Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(int userId);
        Task<List<ProjectDto>> GetVisibleProjectsAsync(int userId);
        Task<ProjectDto?> GetProjectByIdAsync(int id, int userId);
        Task<ProjectDto> CreateProjectAsync(ProjectCreateDto dto, int userId);
        Task<bool> UpdateProjectAsync(int id, ProjectUpdateDto dto, int userId);
        Task<bool> DeleteProjectAsync(int id, int userId);
    }
}