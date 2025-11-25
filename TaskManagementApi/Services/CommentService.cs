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

        // Creates a new comment
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

        // Deletes a comment
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