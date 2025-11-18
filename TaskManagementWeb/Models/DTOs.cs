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
        public string Status { get; set; } = "Pending";
        public int ProjectId { get; set; }
        public int? AssignedUserId { get; set; }
    }

    public class TaskCreateUpdateDto
    {
        public int ProjectId { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public string Status { get; set; } = "Pending";
    }

    public class CommentDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = "";
        public int TaskItemId { get; set; }
        public string? AuthorName { get; set; }
    }

    public class CommentCreateUpdateDto
    {
        public int TaskItemId { get; set; }
        public string Content { get; set; } = "";
    }
}