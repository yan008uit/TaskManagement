using System.ComponentModel.DataAnnotations;

namespace TaskManagementApi.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(500)]
        public string Text { get; set; } = "";

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int TaskItemId { get; set; }

        public TaskItem? TaskItem { get; set; }

        [Required]
        public int UserId { get; set; }

        public User? User { get; set; }
    }
}
