using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public class CommentService
    {
        private readonly ApiClient _api;

        public CommentService(ApiClient api) => _api = api;

        private void LogError(string action, Exception ex, int? id = null)
        {
            Console.WriteLine($"[CommentService] Error during '{action}'{(id.HasValue ? $" (ID: {id})" : "")}: {ex.Message}");
        }

        // All comments for a task
        public async Task<List<CommentDto>> GetCommentsByTaskAsync(int taskId)
        {
            try
            {
                return await _api.GetAsync<List<CommentDto>>($"comment/task/{taskId}") ?? new();
            }
            catch (Exception ex)
            {
                LogError("GetCommentsByTask", ex, taskId);
                return new();
            }
        }

        //  Add a comment
        public async Task<CommentDto?> AddCommentAsync(CommentCreateUpdateDto dto)
        {
            try
            {
                return await _api.PostAsync<CommentCreateUpdateDto, CommentDto>("comment", dto);
            }
            catch (Exception ex)
            {
                LogError("AddComment", ex);
                return null;
            }
        }

        // Delete a comment
        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            try
            {
                return await _api.DeleteAsync($"comment/{commentId}");
            }
            catch (Exception ex)
            {
                LogError("DeleteComment", ex, commentId);
                return false;
            }
        }
    }
}