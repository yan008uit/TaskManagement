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
        /// <response code="200">Successfully registered user.</response>
        /// <response code="400">Invalid input data.</response>
        /// <response code="409">Username or email already exists.</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _authService.Register(request.Username, request.Email, request.Password);
            if (user == null)
                return Conflict(new { message = "Username or email is already in use." });

            return Ok(new
            {
                message = "User registered successfully",
                username = user.Username
            });
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        /// <response code="200">Login successful, and a token has been returned.</response>
        /// <response code="400">Invalid input data.</response>
        /// <response code="401">Invalid username or password.</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var token = await _authService.Login(request.Username, request.Password);
            if (token == null)
                return Unauthorized(new { message = "Invalid username or password." });

            return Ok(new { token });
        }
    }
}