using Microsoft.EntityFrameworkCore;
using SkilloPlatform.Data;
using SkilloPlatform.Models;

namespace SkilloPlatform.Tests.Helpers;

public static class TestDbHelper
{
    public static SkilloDbContext CreateInMemoryDb(string dbName = "")
    {
        var name    = string.IsNullOrEmpty(dbName) ? Guid.NewGuid().ToString() : dbName;
        var options = new DbContextOptionsBuilder<SkilloDbContext>()
            .UseInMemoryDatabase(name)
            .Options;

        var db = new SkilloDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }

    public static SkilloDbContext CreateWithSeedData()
    {
        var db = CreateInMemoryDb();

        // Users
        db.Users.AddRange(
            new User { Id = 1, FullName = "Тест Клиент",      Email = "client@test.bg",     PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234!"), Role = "Client" },
            new User { Id = 2, FullName = "Тест Фрийлансър",  Email = "freelancer@test.bg",  PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234!"), Role = "Freelancer" },
            new User { Id = 3, FullName = "Тест Админ",       Email = "admin@test.bg",       PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234!"), Role = "Admin" },
            new User { Id = 4, FullName = "Супер Админ",      Email = "super@test.bg",       PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234!"), Role = "SuperAdmin" },
            new User { Id = 5, FullName = "Блокиран Потребит", Email = "banned@test.bg",      PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test1234!"), Role = "Client", IsBanned = true }
        );

        // FreelancerProfile
        db.FreelancerProfiles.Add(new FreelancerProfile
        {
            Id = 1, UserId = 2,
            Title = "Full-Stack Developer",
            Bio = "Тест биография",
            Skills = "React,Node.js,C#",
            Category = "Уеб разработка",
            HourlyRate = 45,
            ExperienceLevel = "Senior",
            IsVerified = true,
            IsAvailable = true,
        });

        // Projects
        db.Projects.AddRange(
            new Project { Id = 1, ClientId = 1, Title = "Тест Проект 1", Category = "Уеб разработка", BudgetMin = 1000, BudgetMax = 3000, Status = "Open" },
            new Project { Id = 2, ClientId = 1, Title = "Тест Проект 2", Category = "Графичен дизайн", BudgetMin = 500, BudgetMax = 1500, Status = "InProgress" },
            new Project { Id = 3, ClientId = 1, Title = "Тест Проект 3", Category = "Уеб разработка", BudgetMin = 2000, BudgetMax = 5000, Status = "Open" }
        );

        // Bids
        db.Bids.AddRange(
            new Bid { Id = 1, ProjectId = 1, FreelancerId = 2, Amount = 1500, CoverLetter = "Тест писмо", DeliveryDays = 14, Status = "Pending" },
            new Bid { Id = 2, ProjectId = 2, FreelancerId = 2, Amount = 800,  CoverLetter = "Друго писмо", DeliveryDays = 7,  Status = "Accepted" }
        );

        // Services
        db.Services.AddRange(
            new Service { Id = 1, UserId = 2, Title = "Тест Услуга 1", Category = "Уеб разработка", Price = 500, PriceType = "fixed", DeliveryDays = 7, IsActive = true },
            new Service { Id = 2, UserId = 2, Title = "Тест Услуга 2", Category = "Уеб разработка", Price = 1000, PriceType = "hourly", DeliveryDays = 14, IsActive = false }
        );

        // Payments
        db.Payments.AddRange(
            new Payment { Id = 1, ProjectId = 1, PayerId = 1, Amount = 1500, Currency = "BGN", Method = "Stripe",    Status = "Completed", TransactionId = "stripe_test_001" },
            new Payment { Id = 2, ProjectId = 1, PayerId = 1, Amount = 800,  Currency = "BGN", Method = "PayPal",    Status = "Completed", TransactionId = "paypal_test_001" },
            new Payment { Id = 3, ProjectId = 2, PayerId = 1, Amount = 500,  Currency = "BGN", Method = "Simulated", Status = "Pending",   TransactionId = "sim_001" }
        );

        // Reviews
        db.Reviews.Add(new Review
        {
            Id = 1, ReviewerId = 1, RevieweeId = 2, ProjectId = 2,
            Rating = 5, Comment = "Отлична работа!"
        });

        // Categories
        db.Categories.AddRange(
            new Category { Id = 1, Name = "Уеб разработка",  Icon = "💻" },
            new Category { Id = 2, Name = "Графичен дизайн", Icon = "🎨" }
        );

        db.SaveChanges();
        return db;
    }
}
