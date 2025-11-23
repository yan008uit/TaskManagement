namespace TaskManagementWeb.Models
{

    public class LoginResponse
    {
        public string? Token { get; set; }
    }

    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<TaskSummaryDto> Tasks { get; set; } = new();
    }

    public class ProjectCreateDto
    {
        public string Name { get; set; } = "";
        public string? Description { get; set; }
    }

    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }

        public string Status { get; set; } = "ToDo";

        public int ProjectId { get; set; }
        public int? AssignedUserId { get; set; }

        public DateTime CreatedDate { get; set; }
        public DateTime? DueDate { get; set; }

        public List<CommentDto> Comments { get; set; } = new();
    }

    public class TaskCreateUpdateDto
    {
        public int ProjectId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }

        public string Status { get; set; } = "ToDo";
        public DateTime? DueDate { get; set; }
    }

    public class TaskSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Status { get; set; } = "ToDo";

        public DateTime CreatedDate { get; set; }
        public DateTime? DueDate { get; set; }
    }

    public class CommentDto
    {
        public int Id { get; set; }
        public string Text { get; set; } = "";
        public int TaskItemId { get; set; }
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CommentCreateUpdateDto
    {
        public int TaskItemId { get; set; }
        public string Text { get; set; } = "";
    }

}