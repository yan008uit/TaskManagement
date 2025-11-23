using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public class CommentService
    {
        private readonly ApiClient _api;

        public CommentService(ApiClient api) => _api = api;

        public async Task<List<CommentDto>> GetCommentsByTaskAsync(int taskId)
        {
            return await _api.GetAsync<List<CommentDto>>($"comment/task/{taskId}") ?? new List<CommentDto>();
        }

        public async Task<CommentDto?> AddCommentAsync(CommentCreateUpdateDto dto)
        {
            return await _api.PostAsync<CommentCreateUpdateDto, CommentDto>("comment", dto);
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            return await _api.DeleteAsync($"comment/{commentId}");
        }
    }
}