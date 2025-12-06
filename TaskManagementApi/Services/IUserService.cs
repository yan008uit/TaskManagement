using TaskManagementApi.Models.DTOs;

namespace TaskManagementApi.Services
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllUsersAsync();
    }
}