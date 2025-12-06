using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagementApi.Models.DTOs;
using TaskManagementApi.Services;

namespace TaskManagementApi.Tests
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _mockService;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockService = new Mock<IUserService>();
            _controller = new UserController(_mockService.Object);
        }

        // ------------------- GET USERS -------------------

        // Success case: Returns a list of users
        [Fact]
        public async Task GetUsers_ReturnsOk_WithUsers()
        {
            var users = new List<UserDto>
            {
                new UserDto { Id = 1, Username = "user1"},
                new UserDto { Id = 2, Username = "user2"}
            };

            _mockService.Setup(s => s.GetAllUsersAsync())
                        .ReturnsAsync(users);

            var result = await _controller.GetUsers();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUsers = Assert.IsType<List<UserDto>>(okResult.Value);
            Assert.Equal(2, returnedUsers.Count);
        }

        // Edge case: No users exist, returns empty list
        [Fact]
        public async Task GetUsers_ReturnsOk_WithEmptyList_WhenNoUsersExist()
        {
            _mockService.Setup(s => s.GetAllUsersAsync())
                        .ReturnsAsync(new List<UserDto>());

            var result = await _controller.GetUsers();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedUsers = Assert.IsType<List<UserDto>>(okResult.Value);
            Assert.Empty(returnedUsers);
        }
    }
}