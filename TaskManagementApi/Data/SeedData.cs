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

            // --- USERS ---
            var yuri = new User { Username = "Yuri", Email = "yuri@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123!") };
            var johanne = new User { Username = "Johanne", Email = "johanne@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123") };
            var mike = new User { Username = "Mike", Email = "mike@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Mike123!") };
            var sara = new User { Username = "Sara", Email = "sara@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Sara123!") };
            var emily = new User { Username = "Emily", Email = "emily@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Emily123!") };
            var tom = new User { Username = "Tom", Email = "tom@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Tom123!") };

            db.Users.AddRange(yuri, johanne, mike, sara, emily, tom);
            db.SaveChanges();

            // --- PROJECTS ---
            var proj1 = new Project { Name = "Website Redesign", Description = "Make the site pretty", UserId = yuri.Id };
            var proj2 = new Project { Name = "Mobile App", Description = "Build MVP", UserId = johanne.Id };
            var proj3 = new Project { Name = "Internal Dashboard", Description = "Track KPIs", UserId = mike.Id };
            var proj4 = new Project { Name = "Marketing Campaign", Description = "Launch Q1 campaign", UserId = emily.Id };
            var proj5 = new Project { Name = "Backend Refactor", Description = "Optimize APIs", UserId = yuri.Id };

            db.Projects.AddRange(proj1, proj2, proj3, proj4, proj5);
            db.SaveChanges();

            // --- TASKS ---
            var tasks = new List<TaskItem>
            {
                // Project 1
                new TaskItem { Title = "Design landing page", Description = "Header + hero", ProjectId = proj1.Id, Status = TaskStatus.ToDo, CreatedDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(7), CreatedByUserId = yuri.Id, AssignedUserId = yuri.Id },
                new TaskItem { Title = "Create color palette", Description = "Define primary and secondary colors", ProjectId = proj1.Id, Status = TaskStatus.InProgress, CreatedDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(5), CreatedByUserId = johanne.Id, AssignedUserId = sara.Id },
                new TaskItem { Title = "Responsive design check", Description = "Test mobile and tablet layouts", ProjectId = proj1.Id, Status = TaskStatus.ToDo, CreatedDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(3), CreatedByUserId = mike.Id, AssignedUserId = yuri.Id },

                // Project 2
                new TaskItem { Title = "Implement auth", Description = "JWT login", ProjectId = proj2.Id, Status = TaskStatus.InProgress, CreatedDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(4), CreatedByUserId = johanne.Id, AssignedUserId = johanne.Id },
                new TaskItem { Title = "Setup push notifications", Description = "Use Firebase", ProjectId = proj2.Id, Status = TaskStatus.ToDo, CreatedDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(6), CreatedByUserId = yuri.Id, AssignedUserId = mike.Id },
                new TaskItem { Title = "UI testing", Description = "Test on Android & iOS", ProjectId = proj2.Id, Status = TaskStatus.ToDo, CreatedDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(5), CreatedByUserId = sara.Id, AssignedUserId = yuri.Id },

                // Project 3
                new TaskItem { Title = "Create KPI charts", Description = "Bar + line charts", ProjectId = proj3.Id, Status = TaskStatus.Done, CreatedDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(2), CreatedByUserId = mike.Id, AssignedUserId = sara.Id },
                new TaskItem { Title = "Add filtering options", Description = "Filter by date and department", ProjectId = proj3.Id, Status = TaskStatus.ToDo, CreatedDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(5), CreatedByUserId = sara.Id, AssignedUserId = mike.Id },
                new TaskItem { Title = "Setup data import", Description = "Import CSV files", ProjectId = proj3.Id, Status = TaskStatus.InProgress, CreatedDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(7), CreatedByUserId = johanne.Id, AssignedUserId = yuri.Id },

                // Project 4
                new TaskItem { Title = "Create social media posts", Description = "Facebook, Instagram, LinkedIn posts", ProjectId = proj4.Id, Status = TaskStatus.ToDo, CreatedDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(3), CreatedByUserId = emily.Id, AssignedUserId = sara.Id },
                new TaskItem { Title = "Email campaign draft", Description = "Write 3 email variants", ProjectId = proj4.Id, Status = TaskStatus.InProgress, CreatedDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(5), CreatedByUserId = emily.Id, AssignedUserId = tom.Id },
                new TaskItem { Title = "Finalize budget", Description = "Approve spending for ads", ProjectId = proj4.Id, Status = TaskStatus.Done, CreatedDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(1), CreatedByUserId = sara.Id, AssignedUserId = emily.Id },

                // Project 5
                new TaskItem { Title = "Refactor user service", Description = "Split service into microservices", ProjectId = proj5.Id, Status = TaskStatus.ToDo, CreatedDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(7), CreatedByUserId = tom.Id, AssignedUserId = mike.Id },
                new TaskItem { Title = "Optimize database queries", Description = "Check indexes and slow queries", ProjectId = proj5.Id, Status = TaskStatus.InProgress, CreatedDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(4), CreatedByUserId = mike.Id, AssignedUserId = johanne.Id },
                new TaskItem { Title = "Add API logging", Description = "Log all incoming requests and responses", ProjectId = proj5.Id, Status = TaskStatus.Done, CreatedDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(2), CreatedByUserId = tom.Id, AssignedUserId = yuri.Id },
                new TaskItem { Title = "Write detailed documentation", Description = "Document APIs, data models, edge cases. Include diagrams and examples.", ProjectId = proj5.Id, Status = TaskStatus.ToDo, CreatedDate = DateTime.UtcNow, DueDate = DateTime.UtcNow.AddDays(10), CreatedByUserId = tom.Id, AssignedUserId = emily.Id }
            };

            db.TaskItems.AddRange(tasks);
            db.SaveChanges();

            // --- COMMENTS (only allowed users) ---
            var comments = new List<Comment>
            {
                // Project 1
                new Comment { Text = "Remember mobile first", TaskItemId = tasks[0].Id, UserId = yuri.Id, CreatedDate = DateTime.UtcNow },
                new Comment { Text = "Review layout", TaskItemId = tasks[0].Id, UserId = yuri.Id, CreatedDate = DateTime.UtcNow.AddMinutes(5) },

                // Project 2
                new Comment { Text = "Implement OAuth next", TaskItemId = tasks[3].Id, UserId = johanne.Id, CreatedDate = DateTime.UtcNow },
                new Comment { Text = "Check push notifications", TaskItemId = tasks[4].Id, UserId = mike.Id, CreatedDate = DateTime.UtcNow },
                new Comment { Text = "UI looks good", TaskItemId = tasks[5].Id, UserId = yuri.Id, CreatedDate = DateTime.UtcNow },

                // Project 3
                new Comment { Text = "Charts are ready", TaskItemId = tasks[6].Id, UserId = sara.Id, CreatedDate = DateTime.UtcNow },
                new Comment { Text = "Filter options added", TaskItemId = tasks[7].Id, UserId = mike.Id, CreatedDate = DateTime.UtcNow },

                // Project 4
                new Comment { Text = "Social media posts drafted", TaskItemId = tasks[9].Id, UserId = sara.Id, CreatedDate = DateTime.UtcNow },
                new Comment { Text = "Email campaign reviewed", TaskItemId = tasks[10].Id, UserId = tom.Id, CreatedDate = DateTime.UtcNow },
                new Comment { Text = "Budget approved", TaskItemId = tasks[11].Id, UserId = emily.Id, CreatedDate = DateTime.UtcNow },

                // Project 5
                new Comment { Text = "Refactoring plan looks good", TaskItemId = tasks[12].Id, UserId = mike.Id, CreatedDate = DateTime.UtcNow },
                new Comment { Text = "Add logging to error handler", TaskItemId = tasks[14].Id, UserId = yuri.Id, CreatedDate = DateTime.UtcNow },
                new Comment { Text = "Documentation needs diagrams", TaskItemId = tasks[15].Id, UserId = emily.Id, CreatedDate = DateTime.UtcNow }
            };

            db.Comments.AddRange(comments);
            db.SaveChanges();
        }
    }
}