using TaskManagementApi.Models;
using TaskStatus = TaskManagementApi.Models.TaskStatus;

namespace TaskManagementApi.Data
{
    public static class SeedData
    {
        public static void EnsureSeedData(AppDbContext db)
        {
            db.Database.EnsureCreated();

            if (db.Users.Any())
                return;

            // Users
            var yuri = new User { Username = "Yuri", Email = "yuri@gmail.com" };
            yuri.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123!");
            var johanne = new User { Username = "Johanne", Email = "johanne@gmail.com" };
            johanne.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123");
            var mike = new User { Username = "Mike", Email = "mike@gmail.com" };
            mike.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Mike123!");
            var sara = new User { Username = "Sara", Email = "sara@gmail.com" };
            sara.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Sara123!");

            db.Users.AddRange(yuri, johanne, mike, sara);
            db.SaveChanges();

            // Projects
            var proj1 = new Project { Name = "Website Redesign", Description = "Make the site pretty", UserId = yuri.Id };
            var proj2 = new Project { Name = "Mobile App", Description = "Build MVP", UserId = johanne.Id };
            var proj3 = new Project { Name = "Internal Dashboard", Description = "Track KPIs", UserId = mike.Id };
            db.Projects.AddRange(proj1, proj2, proj3);
            db.SaveChanges();

            // Tasks
            var tasks = new List<TaskItem>
            {
                // Project 1
                new TaskItem
                {
                    Title = "Design landing page",
                    Description = "Header + hero",
                    ProjectId = proj1.Id,
                    Status = TaskStatus.ToDo,
                    CreatedDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(7),
                    CreatedByUserId = yuri.Id,
                    AssignedUserId = yuri.Id
                },
                new TaskItem
                {
                    Title = "Create color palette",
                    Description = "Define primary and secondary colors",
                    ProjectId = proj1.Id,
                    Status = TaskStatus.InProgress,
                    CreatedDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(5),
                    CreatedByUserId = johanne.Id,
                    AssignedUserId = sara.Id
                },
                new TaskItem
                {
                    Title = "Responsive design check",
                    Description = "Test mobile and tablet layouts",
                    ProjectId = proj1.Id,
                    Status = TaskStatus.ToDo,
                    CreatedDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(3),
                    CreatedByUserId = mike.Id,
                    AssignedUserId = null
                },

                // Project 2
                new TaskItem
                {
                    Title = "Implement auth",
                    Description = "JWT login",
                    ProjectId = proj2.Id,
                    Status = TaskStatus.InProgress,
                    CreatedDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(4),
                    CreatedByUserId = johanne.Id,
                    AssignedUserId = johanne.Id
                },
                new TaskItem
                {
                    Title = "Setup push notifications",
                    Description = "Use Firebase",
                    ProjectId = proj2.Id,
                    Status = TaskStatus.ToDo,
                    CreatedDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(6),
                    CreatedByUserId = yuri.Id,
                    AssignedUserId = mike.Id
                },

                // Project 3
                new TaskItem
                {
                    Title = "Create KPI charts",
                    Description = "Bar + line charts",
                    ProjectId = proj3.Id,
                    Status = TaskStatus.Done,
                    CreatedDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(2),
                    CreatedByUserId = mike.Id,
                    AssignedUserId = sara.Id
                },
                new TaskItem
                {
                    Title = "Add filtering options",
                    Description = "Filter by date and department",
                    ProjectId = proj3.Id,
                    Status = TaskStatus.ToDo,
                    CreatedDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(5),
                    CreatedByUserId = sara.Id,
                    AssignedUserId = mike.Id
                },
                new TaskItem
                {
                    Title = "Setup data import",
                    Description = "Import CSV files",
                    ProjectId = proj3.Id,
                    Status = TaskStatus.InProgress,
                    CreatedDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(7),
                    CreatedByUserId = yuri.Id,
                    AssignedUserId = null
                }
            };

            db.TaskItems.AddRange(tasks);
            db.SaveChanges();

            // Comments
            var comments = new List<Comment>
            {
                new Comment { Text = "Remember mobile first", TaskItemId = tasks[0].Id, UserId = yuri.Id, CreatedDate = DateTime.UtcNow },
                new Comment { Text = "Don't forget accessibility", TaskItemId = tasks[0].Id, UserId = johanne.Id, CreatedDate = DateTime.UtcNow.AddMinutes(5) },
                new Comment { Text = "Implement OAuth next", TaskItemId = tasks[3].Id, UserId = johanne.Id, CreatedDate = DateTime.UtcNow },
                new Comment { Text = "Review security protocols", TaskItemId = tasks[3].Id, UserId = yuri.Id, CreatedDate = DateTime.UtcNow.AddMinutes(10) },
                new Comment { Text = "We need to finalize the sprint goals", TaskItemId = tasks[5].Id, UserId = yuri.Id, CreatedDate = DateTime.UtcNow },
                new Comment { Text = "I will prepare the roadmap", TaskItemId = tasks[5].Id, UserId = johanne.Id, CreatedDate = DateTime.UtcNow.AddMinutes(5) },
                new Comment { Text = "Check chart library compatibility", TaskItemId = tasks[5].Id, UserId = sara.Id, CreatedDate = DateTime.UtcNow.AddMinutes(15) }
            };

            db.Comments.AddRange(comments);
            db.SaveChanges();
        }
    }
}