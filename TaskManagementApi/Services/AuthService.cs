using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagementApi.Data;
using TaskManagementApi.Models;

namespace TaskManagementApi.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<User?> Register(string username, string email, string password)
        {
            // Checks if username and email already exists
            if (await _context.Users.AnyAsync(u => u.Username == username || u.Email == email))
                return null;
            
            // Creates a new user
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            // Saves information in the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            return user;
        }

        public async Task<string?> Login(string username, string password)
        {
            // Checks if username exist and if password is correct.
            // Retunrs null if username or password is wrong
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            // Generates a token with a successful login
            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            // Loads key from config and signing credentials using key and HMACSHA256
            var key = new SymmetricSecurityKey(
    Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
);

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Claims included in JWT for users identity 
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),

                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username)
            };
            
            // Creates a JWT token
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"], 
                claims: claims, 
                expires: DateTime.UtcNow.AddHours(8), 
                signingCredentials: creds
                );
            
            // Returns token as a JWTT string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}