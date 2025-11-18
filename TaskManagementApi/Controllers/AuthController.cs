using Microsoft.AspNetCore.Mvc;
using TaskManagementApi.Models.DTOs;
using TaskManagementApi.Services;

namespace TaskManagementApi.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <response code = "200">Successfully registered user.</response>
        /// <response code = "400">Invalid input data.</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("All fields are required.");

            var user = await _authService.Register(request.Username, request.Email, request.Password);
            if (user == null)
                return BadRequest("Username or email is already in use.");

            return Ok(new
            {
                message = "User registered successfully",
                username = user.Username
            });
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <response code = "200">Login successful, and a token has been returned.</response>
        /// <response code = "400">Invalid input data.</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and password are required.");

            var token = await _authService.Login(request.Username, request.Password);
            if (token == null)
                return Unauthorized("Invalid username or password.");

            return Ok(new { token });
        }
    }
}