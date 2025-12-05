using TaskManagementWeb.Models.DTOs;

namespace TaskManagementWeb.Services
{
    public class CommentService
    {
        private readonly ApiClient _api;

        // Inject the shared API client used for all HTTP calls.
        public CommentService(ApiClient api) => _api = api;

        // Basic internal logger for debugging and contextual error tracking.
        private void LogError(string action, Exception ex, int? id = null)
        {
            Console.WriteLine($"[CommentService] Error during '{action}'{(id.HasValue ? $" (ID: {id})" : "")}: {ex.Message}");
        }

        // Retrieves all comments associated with a specific task.
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

        // Creates a new comment for a task and returns the created item.
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

        // Removes a comment by its unique ID.
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