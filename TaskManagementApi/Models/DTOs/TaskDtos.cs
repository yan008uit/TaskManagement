using System.ComponentModel.DataAnnotations;
using TaskManagementApi.Models;

namespace TaskManagementApi.Models.DTOs
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public TaskStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class TaskCreateDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.ToDo;

        [Required(ErrorMessage = "Due date is required.")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Project ID is required to create a task.")]
        public int ProjectId { get; set; }

        public int? AssignedUserId { get; set; }
    }

    public class TaskUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TaskStatus? Status { get; set; }
        public DateTime? DueDate { get; set; }
        public int? AssignedUserId { get; set; }
    }

    public class TaskSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public TaskStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class TaskDetailsDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? DueDate { get; set; }

        public int ProjectId { get; set; }
        public string? ProjectName { get; set; }

        public int? AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }
        public string? AssignedUserEmail { get; set; }
    }
    
    public class UpdateStatusDto
    {
        [Required(ErrorMessage = "Status is required.")]
        public TaskStatus Status { get; set; }
    }
}