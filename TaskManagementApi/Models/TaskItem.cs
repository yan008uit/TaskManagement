using System.ComponentModel.DataAnnotations;

namespace TaskManagementApi.Models
{
    public enum TaskStatus
    {
        ToDo,
        InProgress,
        Done
    }

    public class TaskItem
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public TaskStatus Status { get; set; } = TaskStatus.ToDo;

        public DateTime? DueDate { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        [Required]
        public int CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }
        public int? AssignedUserId { get; set; }
        public User? AssignedUser { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}