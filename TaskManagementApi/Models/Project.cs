using System.ComponentModel.DataAnnotations;

namespace TaskManagementApi.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        
        [Required, MaxLength(100)]
        public string Name { get; set; } = "";
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int UserId { get; set; }
        
        public User? User { get; set; }

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
