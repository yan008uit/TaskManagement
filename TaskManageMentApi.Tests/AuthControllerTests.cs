using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagementApi.Controllers;
using TaskManagementApi.Models;
using TaskManagementApi.Models.DTOs;
using TaskManagementApi.Services;

namespace TaskManagementApi.Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AuthController(_authServiceMock.Object);
        }

        // ------------------- REGISTER TESTS -------------------

        // Success case: User registers successfully
        [Fact]
        public async Task Register_ReturnsOk_WhenUserRegisteredSuccessfully()
        {
            var request = new RegisterRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = "hashedpassword"
            };

            _authServiceMock
                .Setup(s => s.Register(request.Username, request.Email, request.Password))
                .ReturnsAsync(user);

            var result = await _controller.Register(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<RegisterResponse>(okResult.Value);
            Assert.Equal("User registered successfully", response.Message);
            Assert.Equal(request.Username, response.Username);
        }

        // Conflict case: User already exists (username/email taken)
        [Fact]
        public async Task Register_ReturnsConflict_WhenUserAlreadyExists()
        {
            var request = new RegisterRequest
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "password123"
            };

            _authServiceMock
                .Setup(s => s.Register(request.Username, request.Email, request.Password))
                .ReturnsAsync((User?)null);

            var result = await _controller.Register(request);

            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(conflictResult.Value);
            Assert.Equal("Username or email is already in use.", response.Message);
        }

        // Invalid input case: ModelState is invalid
        [Fact]
        public async Task Register_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            var request = new RegisterRequest { Username = "", Email = "", Password = "" };
            _controller.ModelState.AddModelError("Username", "Required");

            var result = await _controller.Register(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequest.Value);
        }

        // ------------------- LOGIN TESTS -------------------

        // Success case: Login with valid credentials
        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsAreValid()
        {
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "password123"
            };

            _authServiceMock
                .Setup(s => s.Login(request.Username, request.Password))
                .ReturnsAsync("fake-jwt-token");

            var result = await _controller.Login(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<AuthResponse>(okResult.Value);
            Assert.Equal("fake-jwt-token", response.Token);
        }

        // Unauthorized case: Login with invalid credentials
        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenCredentialsAreInvalid()
        {
            var request = new LoginRequest
            {
                Username = "testuser",
                Password = "wrongpassword"
            };

            _authServiceMock
                .Setup(s => s.Login(request.Username, request.Password))
                .ReturnsAsync((string?)null);

            var result = await _controller.Login(request);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var response = Assert.IsType<ErrorResponse>(unauthorizedResult.Value);
            Assert.Equal("Invalid username or password.", response.Message);
        }

        // Invalid input case: ModelState is invalid
        [Fact]
        public async Task Login_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            var request = new LoginRequest { Username = "", Password = "" };
            _controller.ModelState.AddModelError("Username", "Required");

            var result = await _controller.Login(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequest.Value);
        }
    }
}