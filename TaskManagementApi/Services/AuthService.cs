using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagementApi.Data;
using TaskManagementApi.Models;
using TaskManagementApi.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // --------------------
    // Register a new user
    // --------------------
    /// <summary>
    /// Registers a new user with username, email, and password.
    /// Returns null if username or email already exists.
    /// Password is hashed before storing.
    /// </summary>
    public async Task<User?> Register(string username, string email, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Username == username || u.Email == email))
            return null;

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    // --------------------
    // Login an existing user
    // --------------------
    /// <summary>
    /// Authenticates a user by username and password.
    /// Returns a JWT token if successful, otherwise null.
    /// </summary>
    public async Task<string?> Login(string username, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return null;

        return GenerateJwtToken(user);
    }

    // --------------------
    // Generate JWT token
    // --------------------
    /// <summary>
    /// Creates a JWT token containing user ID and username claims.
    /// Token expires in 8 hours.
    /// </summary>
    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // User ID
            new Claim(ClaimTypes.Name, user.Username),               // Username
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Subject
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username) // Unique username
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],        // Token issuer
            audience: _config["Jwt:Audience"],    // Token audience
            claims: claims,                       // Claims
            expires: DateTime.UtcNow.AddHours(8), // Expiration
            signingCredentials: creds             // Signing credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}