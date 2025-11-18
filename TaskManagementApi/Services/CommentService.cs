using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Data;
using TaskManagementApi.Models;
using TaskManagementApi.Models.DTOs;

namespace TaskManagementApi.Services
{
    public class CommentService
    {
        private readonly AppDbContext _context;

        public CommentService(AppDbContext context)
        {
            _context = context;
        }

        // Get all comments for a task
        public async Task<IEnumerable<CommentDto>?> GetCommentsByTaskAsync(int taskId, int userId)
        {
            // Checks that the task exist and belongs to the user
            var task = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t =>
                    t.Id == taskId &&
                    t.Project != null &&
                    t.Project.UserId == userId);

            // Returns null if task does not exist
            if (task == null)
                return null;

            // If task exists all comments related to the task for user is retrieved
            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.TaskItemId == taskId)
                .ToListAsync();

            // Returns comments as a DTO
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

        // Creates a new comment
        public async Task<CommentDto?> CreateCommentAsync(CommentCreateUpdateDto dto, int userId)
        {
            // Checks that the task exist and belongs to the user
            var task = await _context.TaskItems
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t =>
                    t.Id == dto.TaskItemId &&
                    t.Project != null &&
                    t.Project.UserId == userId);

            // Returns null if task does not exist or user doesnt have access
            if (task == null)
                return null;

            // If task exists a new comment is created
            var comment = new Comment
            {
                Text = dto.Text,
                TaskItemId = dto.TaskItemId,
                UserId = userId,
                CreatedDate = DateTime.UtcNow
            };

            // Saves comment to database
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Retrives usedname to add to DTO
            var user = await _context.Users.FindAsync(userId);

            // Returns the comment as a DTO
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

        // Deletes a comment
        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            // Retrives comment, its related task and its related project
            var comment = await _context.Comments
                .Include(c => c.TaskItem)
                .ThenInclude(t => t.Project)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            // Returns false is comment does not exist
            if (comment == null)
                return false;

            // Determines if user is allowed to delete comment
            bool commentOwner = comment.UserId == userId;
            bool projectOwner = comment.TaskItem?.Project?.UserId == userId;

            if (!commentOwner && !projectOwner)
                return false;

            // Deletes the comment and saves changes
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}