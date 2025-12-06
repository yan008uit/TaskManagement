using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Models.DTOs;
using TaskManagementApi.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    // --------------------
    // Get all users
    // --------------------
    /// <summary>
    /// Retrieves all users from the database.
    /// Returns a list of UserDto containing only Id and Username.
    /// </summary>
    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username
            }).ToListAsync();
    }
}