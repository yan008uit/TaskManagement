using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TaskManagementApi.Controllers;
using TaskManagementApi.Models.DTOs;
using TaskManagementApi.Services;

namespace TaskManagementApi.Tests
{
    public class CommentControllerTests
    {
        private readonly Mock<ICommentService> _mockService;
        private readonly CommentController _controller;

        public CommentControllerTests()
        {
            _mockService = new Mock<ICommentService>();
            _controller = new CommentController(_mockService.Object);

            // Mock user with ID = 1
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        // ------------------- GET COMMENTS -------------------

        // Success case: Comments exist for a task
        [Fact]
        public async Task GetComments_ReturnsOk_WhenCommentsExist()
        {
            var comments = new List<CommentDto>
            {
                new CommentDto { Id = 1, Text = "Test comment", TaskItemId = 1 }
            };

            _mockService.Setup(s => s.GetCommentsByTaskAsync(1, 1))
                        .ReturnsAsync(comments);

            var result = await _controller.GetComments(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedComments = Assert.IsType<List<CommentDto>>(okResult.Value);
            Assert.Single(returnedComments);
            Assert.Equal("Test comment", returnedComments[0].Text);
        }

        // NotFound case: No comments or no access to task
        [Fact]
        public async Task GetComments_ReturnsNotFound_WhenTaskNotFoundOrNoAccess()
        {
            _mockService.Setup(s => s.GetCommentsByTaskAsync(99, 1))
                        .ReturnsAsync((List<CommentDto>?)null);

            var result = await _controller.GetComments(99);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Task not found, or you don't have access to this task.", notFound.Value);
        }

        // ------------------- CREATE COMMENT -------------------

        // Success case: Comment created
        [Fact]
        public async Task CreateComment_ReturnsCreated_WhenSuccessful()
        {
            var dto = new CommentCreateUpdateDto { Text = "New Comment", TaskItemId = 1 };
            var createdComment = new CommentDto { Id = 10, Text = "New Comment", TaskItemId = 1 };

            _mockService.Setup(s => s.CreateCommentAsync(dto, 1))
                        .ReturnsAsync(createdComment);

            var result = await _controller.CreateComment(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var comment = Assert.IsType<CommentDto>(createdResult.Value);
            Assert.Equal(10, comment.Id);
            Assert.Equal("New Comment", comment.Text);
        }

        // BadRequest case: Creation failed
        [Fact]
        public async Task CreateComment_ReturnsBadRequest_WhenCreationFails()
        {
            var dto = new CommentCreateUpdateDto { Text = "Invalid", TaskItemId = 99 };

            _mockService.Setup(s => s.CreateCommentAsync(dto, 1))
                        .ReturnsAsync((CommentDto?)null);

            var result = await _controller.CreateComment(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Task not found or access denied.", badRequest.Value);
        }

        // ------------------- DELETE COMMENT -------------------

        // Success case: Comment deleted
        [Fact]
        public async Task DeleteComment_ReturnsOk_WhenCommentDeleted()
        {
            _mockService.Setup(s => s.DeleteCommentAsync(1, 1))
                        .ReturnsAsync(true);

            var result = await _controller.DeleteComment(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Comment deleted.", okResult.Value);
        }

        // NotFound case: Comment does not exist or no access
        [Fact]
        public async Task DeleteComment_ReturnsNotFound_WhenCommentNotFound()
        {
            _mockService.Setup(s => s.DeleteCommentAsync(99, 1))
                        .ReturnsAsync(false);

            var result = await _controller.DeleteComment(99);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Comment not found or access denied.", notFound.Value);
        }
    }
}