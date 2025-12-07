using TaskManagementApi.Models;
using TaskStatus = TaskManagementApi.Models.TaskStatus;

namespace TaskManagementApi.Data
{
    public static class SeedData
    {
        public static void EnsureSeedData(AppDbContext db)
        {
            db.Database.EnsureCreated();

            // If already seeded, skip
            if (db.Users.Any())
                return;

            var now = DateTime.UtcNow;

            // ---------- USERS ----------
            var users = new Dictionary<string, User>
            {
                ["yuri"] = new User { Username = "Yuri", Email = "yuri@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123!") },
                ["johanne"] = new User { Username = "Johanne", Email = "johanne@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123!") },
                ["mike"] = new User { Username = "Mike", Email = "mike@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Mike123!") },
                ["sara"] = new User { Username = "Sara", Email = "sara@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Sara123!") },
                ["emily"] = new User { Username = "Emily", Email = "emily@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Emily123!") },
                ["tom"] = new User { Username = "Tom", Email = "tom@gmail.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Tom123!") }
            };

            db.Users.AddRange(users.Values);
            db.SaveChanges();


            // ---------- PROJECTS ----------
            var projects = new Dictionary<string, Project>
            {
                ["proj1"] = new Project { Name = "Website Redesign", Description = "Make the site pretty", UserId = users["yuri"].Id },
                ["proj2"] = new Project { Name = "Mobile App", Description = "Build MVP", UserId = users["johanne"].Id },
                ["proj3"] = new Project { Name = "Internal Dashboard", Description = "Track KPIs", UserId = users["mike"].Id },
                ["proj4"] = new Project { Name = "Marketing Campaign", Description = "Launch Q1 campaign", UserId = users["emily"].Id },
                ["proj5"] = new Project { Name = "Backend Refactor", Description = "Optimize APIs", UserId = users["yuri"].Id }
            };

            db.Projects.AddRange(projects.Values);
            db.SaveChanges();


            // ---------- TASKS ----------
            var tasks = new List<TaskItem>
            {
                // Project 1
                new TaskItem { Title = "Design landing page",        Description = "Header + hero",                         ProjectId = projects["proj1"].Id, Status = TaskStatus.ToDo,        CreatedDate = now, DueDate = now.AddDays(7), CreatedByUserId = users["yuri"].Id,    AssignedUserId = users["yuri"].Id },
                new TaskItem { Title = "Create color palette",       Description = "Define primary and secondary colors",    ProjectId = projects["proj1"].Id, Status = TaskStatus.InProgress,  CreatedDate = now, DueDate = now.AddDays(5), CreatedByUserId = users["johanne"].Id, AssignedUserId = users["sara"].Id },
                new TaskItem { Title = "Responsive design check",    Description = "Test mobile and tablet layouts",        ProjectId = projects["proj1"].Id, Status = TaskStatus.ToDo,        CreatedDate = now, DueDate = now.AddDays(3), CreatedByUserId = users["mike"].Id,    AssignedUserId = users["yuri"].Id },

                // Project 2
                new TaskItem { Title = "Implement auth",             Description = "JWT login",                              ProjectId = projects["proj2"].Id, Status = TaskStatus.InProgress,  CreatedDate = now, DueDate = now.AddDays(4), CreatedByUserId = users["johanne"].Id, AssignedUserId = users["johanne"].Id },
                new TaskItem { Title = "Setup push notifications",   Description = "Use Firebase",                           ProjectId = projects["proj2"].Id, Status = TaskStatus.ToDo,        CreatedDate = now, DueDate = now.AddDays(6), CreatedByUserId = users["yuri"].Id,    AssignedUserId = users["mike"].Id },
                new TaskItem { Title = "UI testing",                 Description = "Test on Android & iOS",                  ProjectId = projects["proj2"].Id, Status = TaskStatus.ToDo,        CreatedDate = now, DueDate = now.AddDays(5), CreatedByUserId = users["sara"].Id,    AssignedUserId = users["yuri"].Id },

                // Project 3
                new TaskItem { Title = "Create KPI charts",          Description = "Bar + line charts",                      ProjectId = projects["proj3"].Id, Status = TaskStatus.Done,        CreatedDate = now, DueDate = now.AddDays(2), CreatedByUserId = users["mike"].Id,    AssignedUserId = users["sara"].Id },
                new TaskItem { Title = "Add filtering options",      Description = "Filter by date and department",          ProjectId = projects["proj3"].Id, Status = TaskStatus.ToDo,        CreatedDate = now, DueDate = now.AddDays(5), CreatedByUserId = users["sara"].Id,    AssignedUserId = users["mike"].Id },
                new TaskItem { Title = "Setup data import",          Description = "Import CSV files",                       ProjectId = projects["proj3"].Id, Status = TaskStatus.InProgress,  CreatedDate = now, DueDate = now.AddDays(7), CreatedByUserId = users["johanne"].Id, AssignedUserId = users["yuri"].Id },

                // Project 4
                new TaskItem { Title = "Create social media posts",  Description = "Facebook, Instagram, LinkedIn posts",    ProjectId = projects["proj4"].Id, Status = TaskStatus.ToDo,        CreatedDate = now, DueDate = now.AddDays(3), CreatedByUserId = users["emily"].Id,   AssignedUserId = users["sara"].Id },
                new TaskItem { Title = "Email campaign draft",       Description = "Write 3 email variants",                 ProjectId = projects["proj4"].Id, Status = TaskStatus.InProgress,  CreatedDate = now, DueDate = now.AddDays(5), CreatedByUserId = users["emily"].Id,   AssignedUserId = users["tom"].Id },
                new TaskItem { Title = "Finalize budget",            Description = "Approve spending for ads",               ProjectId = projects["proj4"].Id, Status = TaskStatus.Done,        CreatedDate = now, DueDate = now.AddDays(1), CreatedByUserId = users["sara"].Id,    AssignedUserId = users["emily"].Id },

                // Project 5
                new TaskItem { Title = "Refactor user service",      Description = "Split service into microservices",       ProjectId = projects["proj5"].Id, Status = TaskStatus.ToDo,        CreatedDate = now, DueDate = now.AddDays(7), CreatedByUserId = users["tom"].Id,    AssignedUserId = users["mike"].Id },
                new TaskItem { Title = "Optimize database queries",  Description = "Check indexes and slow queries",         ProjectId = projects["proj5"].Id, Status = TaskStatus.InProgress,  CreatedDate = now, DueDate = now.AddDays(4), CreatedByUserId = users["mike"].Id,    AssignedUserId = users["johanne"].Id },
                new TaskItem { Title = "Add API logging",            Description = "Log all incoming requests",              ProjectId = projects["proj5"].Id, Status = TaskStatus.Done,        CreatedDate = now, DueDate = now.AddDays(2), CreatedByUserId = users["tom"].Id,    AssignedUserId = users["yuri"].Id },
                new TaskItem { Title = "Write detailed documentation", Description = "Document APIs, models, edge cases",   ProjectId = projects["proj5"].Id, Status = TaskStatus.ToDo,        CreatedDate = now, DueDate = now.AddDays(10),CreatedByUserId = users["tom"].Id,    AssignedUserId = users["emily"].Id }
            };

            db.TaskItems.AddRange(tasks);
            db.SaveChanges();


            // ---------- COMMENTS ----------
            var comments = new List<Comment>
            {
                // Project 1
                new Comment { Text = "Remember mobile first",           TaskItemId = tasks[0].Id, CreatedDate = now,               UserId = users["yuri"].Id },
                new Comment { Text = "Review layout",                   TaskItemId = tasks[0].Id, CreatedDate = now.AddMinutes(5), UserId = users["yuri"].Id },

                // Project 2
                new Comment { Text = "Implement OAuth next",            TaskItemId = tasks[3].Id, CreatedDate = now, UserId = users["johanne"].Id },
                new Comment { Text = "Check push notifications",        TaskItemId = tasks[4].Id, CreatedDate = now, UserId = users["mike"].Id },
                new Comment { Text = "UI looks good",                   TaskItemId = tasks[5].Id, CreatedDate = now, UserId = users["yuri"].Id },

                // Project 3
                new Comment { Text = "Charts are ready",                TaskItemId = tasks[6].Id, CreatedDate = now, UserId = users["sara"].Id },
                new Comment { Text = "Filter options added",            TaskItemId = tasks[7].Id, CreatedDate = now, UserId = users["mike"].Id },

                // Project 4
                new Comment { Text = "Social media posts drafted",      TaskItemId = tasks[9].Id, CreatedDate = now, UserId = users["sara"].Id },
                new Comment { Text = "Email campaign reviewed",         TaskItemId = tasks[10].Id, CreatedDate = now, UserId = users["tom"].Id },
                new Comment { Text = "Budget approved",                 TaskItemId = tasks[11].Id, CreatedDate = now, UserId = users["emily"].Id },

                // Project 5
                new Comment { Text = "Refactoring plan looks good",     TaskItemId = tasks[12].Id, CreatedDate = now, UserId = users["mike"].Id },
                new Comment { Text = "Add logging to error handler",    TaskItemId = tasks[14].Id, CreatedDate = now, UserId = users["yuri"].Id },
                new Comment { Text = "Documentation needs diagrams",    TaskItemId = tasks[15].Id, CreatedDate = now, UserId = users["emily"].Id }
            };

            db.Comments.AddRange(comments);
            db.SaveChanges();
        }
    }
}