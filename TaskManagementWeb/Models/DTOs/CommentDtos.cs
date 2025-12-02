using System.ComponentModel.DataAnnotations;

namespace TaskManagementWeb.Models.DTOs
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public int TaskItemId { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CommentCreateUpdateDto
    {
        [Required(ErrorMessage = "Text is required to create a comment.")]
        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string Text { get; set; } = "";

        [Required(ErrorMessage = "TaskItemId is required to create a comment.")]
        public int TaskItemId { get; set; }
    }
}