using TaskManagementWeb.Models;

namespace TaskManagementWeb.Services
{
    public class CommentService
    {
        private readonly ApiClient _api;

        public CommentService(ApiClient api) => _api = api;

        public async Task<List<CommentDto>> GetCommentsAsync(int taskId)
            => await _api.GetAsync<List<CommentDto>>($"comment/task/{taskId}") ?? new();

        public async Task<CommentDto?> CreateCommentAsync(CommentCreateUpdateDto dto)
            => await _api.PostAsync<CommentCreateUpdateDto, CommentDto>("comment", dto);

        public async Task<bool> DeleteCommentAsync(int id)
            => await _api.DeleteAsync($"comment/{id}");
    }
}