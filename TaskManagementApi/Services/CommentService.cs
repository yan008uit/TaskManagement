using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Models;
using TaskManagementApi.Models.DTOs;

namespace TaskManagementApi.Services
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _context;

        public CommentService(AppDbContext context)
        {
            _context = context;
        }

        // --------------------
        // Get all comments for a specific task
        // --------------------
        /// <summary>
        /// Retrieves all comments associated with a given task.
        /// Only returns comments if the user has access to the task (creator, assigned, or project owner).
        /// </summary>
        /// <param name="taskId">The ID of the task.</param>
        /// <param name="userId">The ID of the requesting user.</param>
        /// <returns>List of CommentDto or null if the task is not accessible.</returns>
        public async Task<IEnumerable<CommentDto>?> GetCommentsByTaskAsync(int taskId, int userId)
        {
            var task = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t =>
                    t.Id == taskId &&
                    t.Project != null &&
                    (t.Project.UserId == userId || t.CreatedByUserId == userId || t.AssignedUserId == userId));

            if (task == null)
                return null;

            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.TaskItemId == taskId)
                .ToListAsync();

            return comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Text = c.Text,
                TaskItemId = c.TaskItemId,
                UserId = c.UserId,
                Username = c.User?.Username,
                CreatedDate = c.CreatedDate
            });
        }

        // --------------------
        // Create a new comment
        // --------------------
        /// <summary>
        /// Creates a new comment on a task if the user has access.
        /// </summary>
        /// <param name="dto">CommentCreateUpdateDto containing TaskItemId and text.</param>
        /// <param name="userId">ID of the user creating the comment.</param>
        /// <returns>The created CommentDto or null if task not accessible.</returns>
        public async Task<CommentDto?> CreateCommentAsync(CommentCreateUpdateDto dto, int userId)
        {
            var task = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t =>
                    t.Id == dto.TaskItemId &&
                    t.Project != null &&
                    (t.Project.UserId == userId || t.CreatedByUserId == userId || t.AssignedUserId == userId));

            if (task == null)
                return null;

            var comment = new Comment
            {
                Text = dto.Text,
                TaskItemId = dto.TaskItemId,
                UserId = userId,
                CreatedDate = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var user = await _context.Users.FindAsync(userId);

            return new CommentDto
            {
                Id = comment.Id,
                Text = comment.Text,
                TaskItemId = comment.TaskItemId,
                UserId = comment.UserId,
                Username = user?.Username,
                CreatedDate = comment.CreatedDate
            };
        }

        // --------------------
        // Delete a comment
        // --------------------
        /// <summary>
        /// Deletes a comment if the requesting user is the author.
        /// </summary>
        /// <param name="commentId">ID of the comment to delete.</param>
        /// <param name="userId">ID of the user attempting to delete the comment.</param>
        /// <returns>True if deleted, false otherwise.</returns>
        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                return false;

            if (comment.UserId != userId)
                return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}