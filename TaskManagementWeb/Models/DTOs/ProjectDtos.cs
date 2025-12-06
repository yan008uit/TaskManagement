using System.ComponentModel.DataAnnotations;

namespace TaskManagementWeb.Models.DTOs
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime CreatedDate { get; set; }
        public int UserId { get; set; }
        public List<TaskSummaryDto> Tasks { get; set; } = new();
    }

    public class ProjectCreateDto
    {
        [Required(ErrorMessage = "Project Name is required")]
        [StringLength(100, ErrorMessage = "Project name cannot exceed 100 characters")]
        public string Name { get; set; } = "";

        [StringLength(500, ErrorMessage = "Project description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }
    }
    public class ProjectUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = "";

        [StringLength(500)]
        public string? Description { get; set; }
    }
}