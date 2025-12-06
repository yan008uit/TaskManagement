using TaskManagementApi.Models.DTOs;

namespace TaskManagementApi.Services
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentDto>?> GetCommentsByTaskAsync(int taskId, int userId);
        Task<CommentDto?> CreateCommentAsync(CommentCreateUpdateDto dto, int userId);
        Task<bool> DeleteCommentAsync(int commentId, int userId);
    }
}