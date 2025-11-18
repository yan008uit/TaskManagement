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
            var yuri = new User
            {
                Username = "Yuri", 
                Email = "yuri@gmail.com"
            };
            yuri.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass123!");

            var johanne = new User
            {
                Username = "Johanne", 
                Email = "johanne@gmail.com"
            };
            johanne.PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test123");

            db.Users.AddRange(yuri, johanne);
            db.SaveChanges();

            // Projects
            var proj1 = new Project
            {
                Name = "Website Redesign", 
                Description = "Make the site pretty", 
                UserId = yuri.Id
            };
            var proj2 = new Project
            {
                Name = "Mobile App", 
                Description = "Build MVP", 
                UserId = johanne.Id
            };

            db.Projects.AddRange(proj1, proj2);
            db.SaveChanges();

            // Tasks
            var task1 = new TaskItem
            {
                Title = "Design landing page",
                Description = "Header + hero",
                ProjectId = proj1.Id,
                AssignedUserId = yuri.Id,
                Status = TaskStatus.ToDo,
                CreatedDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7)
            };
            
            var task2 = new TaskItem
            {
                Title = "Implement auth",
                Description = "JWT login",
                ProjectId = proj2.Id,
                AssignedUserId = johanne.Id,
                Status = TaskStatus.InProgress,
                CreatedDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(3)
            };

            db.TaskItems.AddRange(task1, task2);
            db.SaveChanges();

            // Comments
            var comment1 = new Comment
            {
                Text = "Remember mobile first",
                TaskItemId = task1.Id,
                UserId = yuri.Id,
                CreatedDate = DateTime.UtcNow
            };

            var comment2 = new Comment
            {
                Text = "Don't forget accessibility",
                TaskItemId = task1.Id,
                UserId = johanne.Id,
                CreatedDate = DateTime.UtcNow.AddMinutes(5)
            };

            var comment3 = new Comment
            {
                Text = "Implement OAuth next",
                TaskItemId = task2.Id,
                UserId = johanne.Id,
                CreatedDate = DateTime.UtcNow
            };

            var comment4 = new Comment
            {
                Text = "Review security protocols",
                TaskItemId = task2.Id,
                UserId = yuri.Id,
                CreatedDate = DateTime.UtcNow.AddMinutes(10)
            };

            db.Comments.AddRange(comment1, comment2, comment3, comment4);
            db.SaveChanges();
        }
    }
}