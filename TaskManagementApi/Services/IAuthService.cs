using TaskManagementApi.Models;

namespace TaskManagementApi.Services
{
    public interface IAuthService
    {
        Task<User?> Register(string username, string email, string password);
        Task<string?> Login(string username, string password);
    }
}