using Microsoft.EntityFrameworkCore;
using SkilloPlatform.Models;

namespace SkilloPlatform.Data;

public class SkilloDbContext : DbContext
{
    public SkilloDbContext(DbContextOptions<SkilloDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<FreelancerProfile> FreelancerProfiles => Set<FreelancerProfile>();
    public DbSet<WorkExperience> WorkExperiences => Set<WorkExperience>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Bid> Bids => Set<Bid>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Bid>()
            .HasOne(b => b.Project).WithMany(p => p.Bids)
            .HasForeignKey(b => b.ProjectId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<Bid>()
            .HasOne(b => b.Freelancer).WithMany(u => u.Bids)
            .HasForeignKey(b => b.FreelancerId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Review>()
            .HasOne(r => r.Reviewer).WithMany(u => u.ReviewsGiven)
            .HasForeignKey(r => r.ReviewerId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Review>()
            .HasOne(r => r.Reviewee).WithMany(u => u.ReviewsReceived)
            .HasForeignKey(r => r.RevieweeId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Review>()
            .HasOne(r => r.Project).WithMany()
            .HasForeignKey(r => r.ProjectId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Payment>()
            .HasOne(p => p.Payer).WithMany(u => u.Payments)
            .HasForeignKey(p => p.PayerId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Payment>()
            .HasOne(p => p.Project).WithMany(pr => pr.Payments)
            .HasForeignKey(p => p.ProjectId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Project>()
            .HasOne(p => p.Client).WithMany(u => u.Projects)
            .HasForeignKey(p => p.ClientId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<FreelancerProfile>()
            .HasOne(fp => fp.User).WithOne(u => u.FreelancerProfile)
            .HasForeignKey<FreelancerProfile>(fp => fp.UserId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<WorkExperience>()
            .HasOne(w => w.FreelancerProfile).WithMany(fp => fp.WorkExperiences)
            .HasForeignKey(w => w.FreelancerProfileId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<Certificate>()
            .HasOne(c => c.FreelancerProfile).WithMany(fp => fp.Certificates)
            .HasForeignKey(c => c.FreelancerProfileId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<Service>()
            .HasOne(s => s.User).WithMany(u => u.Services)
            .HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Conversation>()
            .HasOne(c => c.ParticipantOne).WithMany()
            .HasForeignKey(c => c.ParticipantOneId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<Conversation>()
            .HasOne(c => c.ParticipantTwo).WithMany()
            .HasForeignKey(c => c.ParticipantTwoId).OnDelete(DeleteBehavior.Restrict);
        mb.Entity<ChatMessage>()
            .HasOne(m => m.Conversation).WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId).OnDelete(DeleteBehavior.Cascade);
        mb.Entity<ChatMessage>()
            .HasOne(m => m.Sender).WithMany()
            .HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict);

        mb.Entity<User>().HasIndex(u => u.Email).IsUnique();
        mb.Entity<Bid>().HasIndex(b => new { b.ProjectId, b.FreelancerId }).IsUnique();
        mb.Entity<Review>().HasIndex(r => new { r.ReviewerId, r.RevieweeId, r.ProjectId }).IsUnique();

        mb.Entity<Service>().Property(s => s.Price).HasPrecision(18, 2);
        mb.Entity<Project>().Property(p => p.BudgetMin).HasPrecision(18, 2);
        mb.Entity<Project>().Property(p => p.BudgetMax).HasPrecision(18, 2);
        mb.Entity<Bid>().Property(b => b.Amount).HasPrecision(18, 2);
        mb.Entity<Payment>().Property(p => p.Amount).HasPrecision(18, 2);
    }

    // Loads demo data on first run - idempotent check
    public static void SeedData(SkilloDbContext db)
    {
        if (db.Users.Any()) return;

        var hash = BCrypt.Net.BCrypt.HashPassword("Demo1234!");

        var superAdmin = new User { FullName = "Ð¡ÑƒÐ¿ÐµÑ€ ÐÐ´Ð¼Ð¸Ð½Ð¸ÑÑ‚Ñ€Ð°Ñ‚Ð¾Ñ€", Email = "superadmin@skillo.bg", PasswordHash = hash, Role = "SuperAdmin" };
        var admin      = new User { FullName = "ÐÐ´Ð¼Ð¸Ð½Ð¸ÑÑ‚Ñ€Ð°Ñ‚Ð¾Ñ€",       Email = "admin@skillo.bg",       PasswordHash = hash, Role = "Admin" };
        var alex       = new User { FullName = "ÐÐ»ÐµÐºÑÐ°Ð½Ð´ÑŠÑ€ Ð¢Ð¾Ð´Ð¾Ñ€Ð¾Ð²",  Email = "alex@skillo.bg",        PasswordHash = hash, Role = "Freelancer" };
        var maria      = new User { FullName = "ÐœÐ°Ñ€Ð¸Ñ Ð“ÐµÐ¾Ñ€Ð³Ð¸ÐµÐ²Ð°",     Email = "maria@skillo.bg",       PasswordHash = hash, Role = "Freelancer" };
        var petar      = new User { FullName = "ÐŸÐµÑ‚ÑŠÑ€ Ð˜Ð²Ð°Ð½Ð¾Ð²",        Email = "petar@skillo.bg",       PasswordHash = hash, Role = "Freelancer" };
        var ivan       = new User { FullName = "Ð˜Ð²Ð°Ð½ Ð¡Ñ‚Ð¾ÑÐ½Ð¾Ð²",        Email = "ivan@skillo.bg",        PasswordHash = hash, Role = "Freelancer" };
        var client1    = new User { FullName = "TechStart Bulgaria",  Email = "client@techstart.bg",   PasswordHash = hash, Role = "Client" };
        var client2    = new User { FullName = "ÐœÐ°Ñ€Ñ‚Ð¸Ð½ ÐŸÐµÑ‚Ñ€Ð¾Ð²",       Email = "martin@example.bg",     PasswordHash = hash, Role = "Client" };
        var client3    = new User { FullName = "Ð¡Ð¾Ñ„Ñ‚Ð£Ð½Ð¸ Ð•ÐžÐžÐ”",        Email = "hr@softuni.bg",         PasswordHash = hash, Role = "Client" };

        db.Users.AddRange(superAdmin, admin, alex, maria, petar, ivan, client1, client2, client3);
        db.SaveChanges();

        db.Categories.AddRange(
            new Category { Name = "Ð£ÐµÐ± Ñ€Ð°Ð·Ñ€Ð°Ð±Ð¾Ñ‚ÐºÐ°",         Icon = "ðŸ’»", FreelancerCount = 312 },
            new Category { Name = "Ð“Ñ€Ð°Ñ„Ð¸Ñ‡ÐµÐ½ Ð´Ð¸Ð·Ð°Ð¹Ð½",        Icon = "ðŸŽ¨", FreelancerCount = 218 },
            new Category { Name = "ÐœÐ¾Ð±Ð¸Ð»Ð½Ð¸ Ð¿Ñ€Ð¸Ð»Ð¾Ð¶ÐµÐ½Ð¸Ñ",     Icon = "ðŸ“±", FreelancerCount = 145 },
            new Category { Name = "ÐšÐ¾Ð¿Ð¸Ñ€Ð°Ð¹Ñ‚Ð¸Ð½Ð³ & SEO",      Icon = "âœï¸", FreelancerCount = 289 },
            new Category { Name = "Ð”Ð¸Ð³Ð¸Ñ‚Ð°Ð»ÐµÐ½ Ð¼Ð°Ñ€ÐºÐµÑ‚Ð¸Ð½Ð³",    Icon = "ðŸ“Š", FreelancerCount = 176 },
            new Category { Name = "Ð’Ð¸Ð´ÐµÐ¾ & ÐÐ½Ð¸Ð¼Ð°Ñ†Ð¸Ñ",       Icon = "ðŸŽ¬", FreelancerCount = 98  },
            new Category { Name = "ÐšÐ¸Ð±ÐµÑ€ÑÐ¸Ð³ÑƒÑ€Ð½Ð¾ÑÑ‚",         Icon = "ðŸ”’", FreelancerCount = 67  },
            new Category { Name = "ÐŸÑ€ÐµÐ²Ð¾Ð´Ð¸",                Icon = "ðŸŒ", FreelancerCount = 134 },
            new Category { Name = "Ð¤Ð¸Ð½Ð°Ð½ÑÐ¸ & Ð¡Ñ‡ÐµÑ‚Ð¾Ð²Ð¾Ð´ÑÑ‚Ð²Ð¾", Icon = "ðŸ“ˆ", FreelancerCount = 89  },
            new Category { Name = "AI & ÐÐ²Ñ‚Ð¾Ð¼Ð°Ñ‚Ð¸Ð·Ð°Ñ†Ð¸Ñ",     Icon = "ðŸ¤–", FreelancerCount = 112 }
        );
        db.SaveChanges();

        var profileAlex = new FreelancerProfile { UserId = alex.Id, Title = "Full-Stack Developer", Bio = "5+ Ð³Ð¾Ð´Ð¸Ð½Ð¸ Ð¾Ð¿Ð¸Ñ‚ Ð² Ð¸Ð·Ð³Ñ€Ð°Ð¶Ð´Ð°Ð½Ðµ Ð½Ð° ÑƒÐµÐ± Ð¿Ñ€Ð¸Ð»Ð¾Ð¶ÐµÐ½Ð¸Ñ Ñ React Ð¸ Node.js.", Skills = "React,Node.js,TypeScript,PostgreSQL,Docker,AWS", Category = "Ð£ÐµÐ± Ñ€Ð°Ð·Ñ€Ð°Ð±Ð¾Ñ‚ÐºÐ°", HourlyRate = 45, ExperienceLevel = "Senior", Location = "Ð¡Ð¾Ñ„Ð¸Ñ, Ð‘ÑŠÐ»Ð³Ð°Ñ€Ð¸Ñ", Website = "https://alex-dev.bg", LinkedIn = "https://linkedin.com/in/alex", GitHub = "https://github.com/alex", Languages = "Ð‘ÑŠÐ»Ð³Ð°Ñ€ÑÐºÐ¸,ÐÐ½Ð³Ð»Ð¸Ð¹ÑÐºÐ¸", IsVerified = true, IsAvailable = true };
        var profileMaria = new FreelancerProfile { UserId = maria.Id, Title = "UI/UX Designer & Brand Designer", Bio = "Ð¡Ð¿ÐµÑ†Ð¸Ð°Ð»Ð¸Ð·Ð¸Ñ€Ð°Ð¼ Ð² ÑÑŠÐ·Ð´Ð°Ð²Ð°Ð½ÐµÑ‚Ð¾ Ð½Ð° ÐºÑ€Ð°ÑÐ¸Ð²Ð¸ Ð¸ Ð¸Ð½Ñ‚ÑƒÐ¸Ñ‚Ð¸Ð²Ð½Ð¸ Ð¸Ð½Ñ‚ÐµÑ€Ñ„ÐµÐ¹ÑÐ¸.", Skills = "Figma,Adobe XD,Illustrator,Photoshop,Branding,Prototyping", Category = "Ð“Ñ€Ð°Ñ„Ð¸Ñ‡ÐµÐ½ Ð´Ð¸Ð·Ð°Ð¹Ð½", HourlyRate = 60, ExperienceLevel = "Senior", Location = "ÐŸÐ»Ð¾Ð²Ð´Ð¸Ð², Ð‘ÑŠÐ»Ð³Ð°Ñ€Ð¸Ñ", LinkedIn = "https://linkedin.com/in/maria", Languages = "Ð‘ÑŠÐ»Ð³Ð°Ñ€ÑÐºÐ¸,ÐÐ½Ð³Ð»Ð¸Ð¹ÑÐºÐ¸,ÐÐµÐ¼ÑÐºÐ¸", IsVerified = true, IsAvailable = true };
        var profilePetar = new FreelancerProfile { UserId = petar.Id, Title = "SEO Ð¡Ð¿ÐµÑ†Ð¸Ð°Ð»Ð¸ÑÑ‚ & ÐšÐ¾Ð¿Ð¸Ñ€Ð°Ð¹Ñ‚ÑŠÑ€", Bio = "6+ Ð³Ð¾Ð´Ð¸Ð½Ð¸ Ð¾Ð¿Ð¸Ñ‚ Ð² SEO Ð¾Ð¿Ñ‚Ð¸Ð¼Ð¸Ð·Ð°Ñ†Ð¸Ñ.", Skills = "SEO,ÐšÐ¾Ð¿Ð¸Ñ€Ð°Ð¹Ñ‚Ð¸Ð½Ð³,WordPress,Google Analytics,Email Marketing", Category = "ÐšÐ¾Ð¿Ð¸Ñ€Ð°Ð¹Ñ‚Ð¸Ð½Ð³ & SEO", HourlyRate = 25, ExperienceLevel = "Senior", Location = "Ð’Ð°Ñ€Ð½Ð°, Ð‘ÑŠÐ»Ð³Ð°Ñ€Ð¸Ñ", Languages = "Ð‘ÑŠÐ»Ð³Ð°Ñ€ÑÐºÐ¸,ÐÐ½Ð³Ð»Ð¸Ð¹ÑÐºÐ¸", IsVerified = false, IsAvailable = true };
        var profileIvan = new FreelancerProfile { UserId = ivan.Id, Title = "Mobile App Developer", Bio = "Ð Ð°Ð·Ñ€Ð°Ð±Ð¾Ñ‚Ð²Ð°Ð¼ Ð¼Ð¾Ð±Ð¸Ð»Ð½Ð¸ Ð¿Ñ€Ð¸Ð»Ð¾Ð¶ÐµÐ½Ð¸Ñ Ð·Ð° iOS Ð¸ Android.", Skills = "React Native,Flutter,Swift,Kotlin,Firebase", Category = "ÐœÐ¾Ð±Ð¸Ð»Ð½Ð¸ Ð¿Ñ€Ð¸Ð»Ð¾Ð¶ÐµÐ½Ð¸Ñ", HourlyRate = 55, ExperienceLevel = "Mid", Location = "Ð¡Ð¾Ñ„Ð¸Ñ, Ð‘ÑŠÐ»Ð³Ð°Ñ€Ð¸Ñ", GitHub = "https://github.com/ivan", Languages = "Ð‘ÑŠÐ»Ð³Ð°Ñ€ÑÐºÐ¸,ÐÐ½Ð³Ð»Ð¸Ð¹ÑÐºÐ¸", IsVerified = true, IsAvailable = false };

        db.FreelancerProfiles.AddRange(profileAlex, profileMaria, profilePetar, profileIvan);
        db.SaveChanges();

        db.WorkExperiences.AddRange(
            new WorkExperience { FreelancerProfileId = profileAlex.Id,  Company = "SoftUni Solutions",    Position = "Senior Developer", StartDate = "2020-01", IsCurrent = true,  Description = "Ð’Ð¾Ð´ÐµÑ‰ Ñ€Ð°Ð·Ñ€Ð°Ð±Ð¾Ñ‚Ñ‡Ð¸Ðº Ð½Ð° SaaS Ð¿Ð»Ð°Ñ‚Ñ„Ð¾Ñ€Ð¼Ð°." },
            new WorkExperience { FreelancerProfileId = profileAlex.Id,  Company = "Telerik Academy",      Position = "Junior Developer", StartDate = "2018-06", EndDate = "2019-12", Description = "Ð Ð°Ð·Ñ€Ð°Ð±Ð¾Ñ‚ÐºÐ° Ð½Ð° Ð²ÑŠÑ‚Ñ€ÐµÑˆÐ½Ð¸ Ð¸Ð½ÑÑ‚Ñ€ÑƒÐ¼ÐµÐ½Ñ‚Ð¸." },
            new WorkExperience { FreelancerProfileId = profileMaria.Id, Company = "Design Studio BG",    Position = "Lead Designer",    StartDate = "2019-03", IsCurrent = true,  Description = "Ð ÑŠÐºÐ¾Ð²Ð¾Ð´Ñ ÐµÐºÐ¸Ð¿ Ð¾Ñ‚ 4 Ð´Ð¸Ð·Ð°Ð¹Ð½ÐµÑ€Ð¸." },
            new WorkExperience { FreelancerProfileId = profilePetar.Id, Company = "SEO Masters BG",      Position = "SEO Lead",         StartDate = "2018-01", IsCurrent = true,  Description = "Ð£Ð¿Ñ€Ð°Ð²Ð»ÐµÐ½Ð¸Ðµ Ð½Ð° SEO ÑÑ‚Ñ€Ð°Ñ‚ÐµÐ³Ð¸Ð¸." },
            new WorkExperience { FreelancerProfileId = profileIvan.Id,  Company = "AppFactory",          Position = "Mobile Developer", StartDate = "2021-03", IsCurrent = true,  Description = "Ð Ð°Ð·Ñ€Ð°Ð±Ð¾Ñ‚ÐºÐ° Ð½Ð° Ñ„Ð¸Ð½Ñ‚ÐµÑ… Ð¿Ñ€Ð¸Ð»Ð¾Ð¶ÐµÐ½Ð¸Ñ." }
        );
        db.SaveChanges();

        db.Certificates.AddRange(
            new Certificate { FreelancerProfileId = profileAlex.Id,  Name = "AWS Certified Developer",        Issuer = "Amazon Web Services", IssueDate = "2022-05" },
            new Certificate { FreelancerProfileId = profileAlex.Id,  Name = "Meta React Developer",           Issuer = "Meta",                IssueDate = "2021-11" },
            new Certificate { FreelancerProfileId = profileMaria.Id, Name = "Google UX Design Certificate",   Issuer = "Google",              IssueDate = "2021-08" },
            new Certificate { FreelancerProfileId = profileMaria.Id, Name = "Adobe Certified Professional",   Issuer = "Adobe",               IssueDate = "2020-03" },
            new Certificate { FreelancerProfileId = profilePetar.Id, Name = "Google Analytics Certification", Issuer = "Google",              IssueDate = "2022-01" },
            new Certificate { FreelancerProfileId = profileIvan.Id,  Name = "Flutter Certified Developer",    Issuer = "Google",              IssueDate = "2023-06" }
        );
        db.SaveChanges();

        db.Services.AddRange(
            new Service { UserId = alex.Id,  Title = "Landing Page Ñ React",            Description = "ÐšÑ€Ð°ÑÐ¸Ð² Ð¸ Ð±ÑŠÑ€Ð· Landing Page Ñ React Ð¸ Tailwind CSS. Ð’ÐºÐ»ÑŽÑ‡Ð²Ð° Ð°Ð´Ð°Ð¿Ñ‚Ð¸Ð²ÐµÐ½ Ð´Ð¸Ð·Ð°Ð¹Ð½ Ð¸ SEO Ð¾Ð¿Ñ‚Ð¸Ð¼Ð¸Ð·Ð°Ñ†Ð¸Ñ.", Category = "Ð£ÐµÐ± Ñ€Ð°Ð·Ñ€Ð°Ð±Ð¾Ñ‚ÐºÐ°",     Price = 500,  PriceType = "fixed",   DeliveryDays = 7,  Revisions = 2, IsActive = true },
            new Service { UserId = alex.Id,  Title = "Full-Stack ÑƒÐµÐ± Ð¿Ñ€Ð¸Ð»Ð¾Ð¶ÐµÐ½Ð¸Ðµ",       Description = "ÐŸÑŠÐ»Ð½Ð¾ ÑƒÐµÐ± Ð¿Ñ€Ð¸Ð»Ð¾Ð¶ÐµÐ½Ð¸Ðµ Ñ React Ñ„Ñ€Ð¾Ð½Ñ‚ÐµÐ½Ð´, Node.js API Ð¸ PostgreSQL Ð±Ð°Ð·Ð° Ð´Ð°Ð½Ð½Ð¸.",                     Category = "Ð£ÐµÐ± Ñ€Ð°Ð·Ñ€Ð°Ð±Ð¾Ñ‚ÐºÐ°",     Price = 3000, PriceType = "fixed",   DeliveryDays = 30, Revisions = 3, IsActive = true },
            new Service { UserId = alex.Id,  Title = "Ð¥Ð¾ÑÑ‚Ð¸Ð½Ð³ Ð¸ DevOps Ð½Ð°ÑÑ‚Ñ€Ð¾Ð¹ÐºÐ°",      Description = "ÐÐ°ÑÑ‚Ñ€Ð¾Ð¹ÐºÐ° Ð½Ð° ÑÑŠÑ€Ð²ÑŠÑ€, CI/CD pipeline, SSL ÑÐµÑ€Ñ‚Ð¸Ñ„Ð¸ÐºÐ°Ñ‚ Ð¸ Ð¼Ð¾Ð½Ð¸Ñ‚Ð¾Ñ€Ð¸Ð½Ð³.",                               Category = "Ð£ÐµÐ± Ñ€Ð°Ð·Ñ€Ð°Ð±Ð¾Ñ‚ÐºÐ°",     Price = 200,  PriceType = "fixed",   DeliveryDays = 3,  Revisions = 1, IsActive = true },
            new Service { UserId = maria.Id, Title = "Ð”Ð¸Ð·Ð°Ð¹Ð½ Ð½Ð° Ð»Ð¾Ð³Ð¾",                  Description = "Ð£Ð½Ð¸ÐºÐ°Ð»Ð½Ð¾ Ð»Ð¾Ð³Ð¾ Ñ 3 ÐºÐ¾Ð½Ñ†ÐµÐ¿Ñ†Ð¸Ð¸. Ð’ÐºÐ»ÑŽÑ‡Ð²Ð° brand guidelines.",                                           Category = "Ð“Ñ€Ð°Ñ„Ð¸Ñ‡ÐµÐ½ Ð´Ð¸Ð·Ð°Ð¹Ð½",    Price = 350,  PriceType = "fixed",   DeliveryDays = 5,  Revisions = 5, IsActive = true },
            new Service { UserId = maria.Id, Title = "UI Kit Ð·Ð° Ð¼Ð¾Ð±Ð¸Ð»Ð½Ð¾ Ð¿Ñ€Ð¸Ð»Ð¾Ð¶ÐµÐ½Ð¸Ðµ",    Description = "ÐŸÑŠÐ»ÐµÐ½ UI Kit Ð² Figma â€” 50+ ÐºÐ¾Ð¼Ð¿Ð¾Ð½ÐµÐ½Ñ‚Ð°, 10+ ÐµÐºÑ€Ð°Ð½Ð° Ð¸ prototype.",                                   Category = "Ð“Ñ€Ð°Ñ„Ð¸Ñ‡ÐµÐ½ Ð´Ð¸Ð·Ð°Ð¹Ð½",    Price = 1200, PriceType = "fixed",   DeliveryDays = 14, Revisions = 2, IsActive = true },
            new Service { UserId = maria.Id, Title = "Brand Identity Ð¿Ð°ÐºÐµÑ‚",            Description = "ÐŸÑŠÐ»Ð½Ð° Ð²Ð¸Ð·ÑƒÐ°Ð»Ð½Ð° Ð¸Ð´ÐµÐ½Ñ‚Ð¸Ñ‡Ð½Ð¾ÑÑ‚ â€” Ð»Ð¾Ð³Ð¾, Ñ†Ð²ÐµÑ‚Ð¾Ð²Ð° ÑÑ…ÐµÐ¼Ð°, Ñ‚Ð¸Ð¿Ð¾Ð³Ñ€Ð°Ñ„Ð¸Ñ Ð¸ Ð²Ð¸Ð·Ð¸Ñ‚ÐºÐ¸.",                          Category = "Ð“Ñ€Ð°Ñ„Ð¸Ñ‡ÐµÐ½ Ð´Ð¸Ð·Ð°Ð¹Ð½",    Price = 800,  PriceType = "fixed",   DeliveryDays = 10, Revisions = 3, IsActive = true },
            new Service { UserId = petar.Id, Title = "SEO Ð¾Ð´Ð¸Ñ‚ Ð½Ð° ÑÐ°Ð¹Ñ‚",               Description = "ÐŸÑŠÐ»ÐµÐ½ Ñ‚ÐµÑ…Ð½Ð¸Ñ‡ÐµÑÐºÐ¸ SEO Ð¾Ð´Ð¸Ñ‚ Ñ Ð´ÐµÑ‚Ð°Ð¹Ð»ÐµÐ½ Ð´Ð¾ÐºÐ»Ð°Ð´ Ð¸ Ð¿Ñ€ÐµÐ¿Ð¾Ñ€ÑŠÐºÐ¸.",                                         Category = "ÐšÐ¾Ð¿Ð¸Ñ€Ð°Ð¹Ñ‚Ð¸Ð½Ð³ & SEO",  Price = 200,  PriceType = "fixed",   DeliveryDays = 3,  Revisions = 1, IsActive = true },
            new Service { UserId = petar.Id, Title = "ÐœÐµÑÐµÑ‡Ð½Ð¾ SEO ÑƒÐ¿Ñ€Ð°Ð²Ð»ÐµÐ½Ð¸Ðµ",          Description = "ÐžÐ¿Ñ‚Ð¸Ð¼Ð¸Ð·Ð°Ñ†Ð¸Ñ Ð½Ð° ÐºÐ»ÑŽÑ‡Ð¾Ð²Ð¸ Ð´ÑƒÐ¼Ð¸, Ð»Ð¸Ð½Ðº Ð±Ð¸Ð»Ð´Ð¸Ð½Ð³ Ð¸ Ð¼ÐµÑÐµÑ‡ÐµÐ½ Ð¾Ñ‚Ñ‡ÐµÑ‚.",                                       Category = "ÐšÐ¾Ð¿Ð¸Ñ€Ð°Ð¹Ñ‚Ð¸Ð½Ð³ & SEO",  Price = 500,  PriceType = "monthly", DeliveryDays = 30, Revisions = 0, IsActive = true },
            new Service { UserId = ivan.Id,  Title = "React Native Ð¼Ð¾Ð±Ð¸Ð»Ð½Ð¾ Ð¿Ñ€Ð¸Ð»Ð¾Ð¶ÐµÐ½Ð¸Ðµ", Description = "ÐšÑ€Ð¾ÑÐ¿Ð»Ð°Ñ‚Ñ„Ð¾Ñ€Ð¼ÐµÐ½Ð¾ Ð¿Ñ€Ð¸Ð»Ð¾Ð¶ÐµÐ½Ð¸Ðµ Ð·Ð° iOS Ð¸ Android. Ð’ÐºÐ»ÑŽÑ‡Ð²Ð° Ð¿ÑƒÐ±Ð»Ð¸ÐºÑƒÐ²Ð°Ð½Ðµ Ð² App Store.",                   Category = "ÐœÐ¾Ð±Ð¸Ð»Ð½Ð¸ Ð¿Ñ€Ð¸Ð»Ð¾Ð¶ÐµÐ½Ð¸Ñ", Price = 4000, PriceType = "fixed",   DeliveryDays = 45, Revisions = 3, IsActive = true }
        );
        db.SaveChanges();

        var proj1 = new Project { ClientId = client1.Id, Title = "Ð Ð°Ð·Ñ€Ð°Ð±Ð¾Ñ‚ÐºÐ° Ð½Ð° Ðµ-commerce Ð¿Ð»Ð°Ñ‚Ñ„Ð¾Ñ€Ð¼Ð°",    Description = "Ð¢ÑŠÑ€ÑÐ¸Ð¼ Next.js Ñ€Ð°Ð·Ñ€Ð°Ð±Ð¾Ñ‚Ñ‡Ð¸Ðº Ð·Ð° Ð¾Ð½Ð»Ð°Ð¹Ð½ Ð¼Ð°Ð³Ð°Ð·Ð¸Ð½ Ñ Stripe Ð¸ ÑƒÐ¿Ñ€Ð°Ð²Ð»ÐµÐ½Ð¸Ðµ Ð½Ð° Ð¸Ð½Ð²ÐµÐ½Ñ‚Ð°Ñ€.",       Category = "Ð£ÐµÐ± Ñ€Ð°Ð·Ñ€Ð°Ð±Ð¾Ñ‚ÐºÐ°",      BudgetMin = 3500, BudgetMax = 6000,  DeadlineDays = 45, RequiredSkills = "Next.js,Stripe,MongoDB,TypeScript",      Status = "Open" };
        var proj2 = new Project { ClientId = client1.Id, Title = "Ð ÐµÐ±Ñ€Ð°Ð½Ð´Ð¸Ð½Ð³ Ð½Ð° Ñ‚ÐµÑ…Ð½Ð¾Ð»Ð¾Ð³Ð¸Ñ‡Ð½Ð° ÐºÐ¾Ð¼Ð¿Ð°Ð½Ð¸Ñ",   Description = "ÐÑƒÐ¶ÐµÐ½ Ð½Ð¸ Ðµ Ð¿ÑŠÐ»ÐµÐ½ Ñ€ÐµÐ±Ñ€Ð°Ð½Ð´Ð¸Ð½Ð³ â€” Ð½Ð¾Ð²Ð¾ Ð»Ð¾Ð³Ð¾ Ð¸ brand guidelines Ð·Ð° Ð½Ð°ÑˆÐ¸Ñ B2B SaaS Ð¿Ñ€Ð¾Ð´ÑƒÐºÑ‚.", Category = "Ð“Ñ€Ð°Ñ„Ð¸Ñ‡ÐµÐ½ Ð´Ð¸Ð·Ð°Ð¹Ð½",     BudgetMin = 2000, BudgetMax = 3500,  DeadlineDays = 30, RequiredSkills = "Figma,Branding,Illustration",            Status = "Open" };
        var proj3 = new Project { ClientId = client2.Id, Title = "ÐœÐ¾Ð±Ð¸Ð»Ð½Ð¾ Ð¿Ñ€Ð¸Ð»Ð¾Ð¶ÐµÐ½Ð¸Ðµ Ð·Ð° Ð´Ð¾ÑÑ‚Ð°Ð²ÐºÐ¸",        Description = "ÐŸÑ€Ð¸Ð»Ð¾Ð¶ÐµÐ½Ð¸Ðµ Ð·Ð° iOS Ð¸ Android Ð·Ð° ÐºÑƒÑ€Ð¸ÐµÑ€ÑÐºÐ¸ Ð´Ð¾ÑÑ‚Ð°Ð²ÐºÐ¸ Ñ real-time tracking.",              Category = "ÐœÐ¾Ð±Ð¸Ð»Ð½Ð¸ Ð¿Ñ€Ð¸Ð»Ð¾Ð¶ÐµÐ½Ð¸Ñ",  BudgetMin = 5000, BudgetMax = 8000,  DeadlineDays = 60, RequiredSkills = "React Native,Firebase,Google Maps",       Status = "Open" };
        var proj4 = new Project { ClientId = client2.Id, Title = "SEO Ð¾Ð¿Ñ‚Ð¸Ð¼Ð¸Ð·Ð°Ñ†Ð¸Ñ Ð½Ð° ÐºÐ¾Ñ€Ð¿Ð¾Ñ€Ð°Ñ‚Ð¸Ð²ÐµÐ½ ÑÐ°Ð¹Ñ‚",  Description = "SEO Ð¾Ð´Ð¸Ñ‚ Ð¸ Ð¾Ð¿Ñ‚Ð¸Ð¼Ð¸Ð·Ð°Ñ†Ð¸Ñ Ð½Ð° ÑÐ°Ð¹Ñ‚ Ñ 200+ ÑÑ‚Ñ€Ð°Ð½Ð¸Ñ†Ð¸. Ð¦ÐµÐ»Ð¸Ð¼ Ñ‚Ð¾Ð¿ 3 Ð¿Ð¾Ð·Ð¸Ñ†Ð¸Ð¸.",                 Category = "ÐšÐ¾Ð¿Ð¸Ñ€Ð°Ð¹Ñ‚Ð¸Ð½Ð³ & SEO",   BudgetMin = 800,  BudgetMax = 1500,  DeadlineDays = 30, RequiredSkills = "SEO,Google Analytics,WordPress",          Status = "Open" };
        var proj5 = new Project { ClientId = client3.Id, Title = "ÐŸÐ»Ð°Ñ‚Ñ„Ð¾Ñ€Ð¼Ð° Ð·Ð° Ð¾Ð½Ð»Ð°Ð¹Ð½ Ð¾Ð±ÑƒÑ‡ÐµÐ½Ð¸Ðµ",          Description = "LMS Ð¿Ð»Ð°Ñ‚Ñ„Ð¾Ñ€Ð¼Ð° Ñ Ð²Ð¸Ð´ÐµÐ¾ Ð»ÐµÐºÑ†Ð¸Ð¸, Ñ‚ÐµÑÑ‚Ð¾Ð²Ðµ, ÑÐµÑ€Ñ‚Ð¸Ñ„Ð¸ÐºÐ°Ñ‚Ð¸ Ð¸ ÑÐ¸ÑÑ‚ÐµÐ¼Ð° Ð·Ð° Ð¿Ð»Ð°Ñ‰Ð°Ð½Ð¸Ñ.",            Category = "Ð£ÐµÐ± Ñ€Ð°Ð·Ñ€Ð°Ð±Ð¾Ñ‚ÐºÐ°",      BudgetMin = 8000, BudgetMax = 15000, DeadlineDays = 90, RequiredSkills = "React,Node.js,PostgreSQL,Stripe",          Status = "Open" };
        var proj6 = new Project { ClientId = client3.Id, Title = "Ð”Ð¸Ð³Ð¸Ñ‚Ð°Ð»ÐµÐ½ Ð¼Ð°Ñ€ÐºÐµÑ‚Ð¸Ð½Ð³ Ð·Ð° SaaS Ð¿Ñ€Ð¾Ð´ÑƒÐºÑ‚",   Description = "Google Ads, Facebook Ads Ð¸ email Ð¼Ð°Ñ€ÐºÐµÑ‚Ð¸Ð½Ð³ ÐºÐ°Ð¼Ð¿Ð°Ð½Ð¸Ð¸ Ð·Ð° Ð½Ð°ÑˆÐ¸Ñ SaaS.",                    Category = "Ð”Ð¸Ð³Ð¸Ñ‚Ð°Ð»ÐµÐ½ Ð¼Ð°Ñ€ÐºÐµÑ‚Ð¸Ð½Ð³", BudgetMin = 1000, BudgetMax = 2000,  DeadlineDays = 30, RequiredSkills = "Google Ads,Facebook Ads,Email Marketing", Status = "Open" };
        var proj7 = new Project { ClientId = client1.Id, Title = "REST API Ð·Ð° Ð¼Ð¾Ð±Ð¸Ð»Ð½Ð¾ Ð¿Ñ€Ð¸Ð»Ð¾Ð¶ÐµÐ½Ð¸Ðµ",         Description = ".NET Core REST API Ñ JWT Ð°Ð²Ñ‚ÐµÐ½Ñ‚Ð¸ÐºÐ°Ñ†Ð¸Ñ Ð¸ Entity Framework.",                            Category = "Ð£ÐµÐ± Ñ€Ð°Ð·Ñ€Ð°Ð±Ð¾Ñ‚ÐºÐ°",      BudgetMin = 2000, BudgetMax = 4000,  DeadlineDays = 30, RequiredSkills = "C#,.NET,SQL Server,JWT",                  Status = "InProgress" };
        var proj8 = new Project { ClientId = client2.Id, Title = "UI Ð´Ð¸Ð·Ð°Ð¹Ð½ Ð·Ð° Ñ„Ð¸Ð½Ñ‚ÐµÑ… Ð¿Ñ€Ð¸Ð»Ð¾Ð¶ÐµÐ½Ð¸Ðµ",         Description = "UI/UX Ð´Ð¸Ð·Ð°Ð¹Ð½ Ð·Ð° Ð¼Ð¾Ð±Ð¸Ð»Ð½Ð¾ Ñ„Ð¸Ð½Ñ‚ÐµÑ… Ð¿Ñ€Ð¸Ð»Ð¾Ð¶ÐµÐ½Ð¸Ðµ â€” wireframes Ð¸ prototype Ð² Figma.",         Category = "Ð“Ñ€Ð°Ñ„Ð¸Ñ‡ÐµÐ½ Ð´Ð¸Ð·Ð°Ð¹Ð½",     BudgetMin = 1500, BudgetMax = 2500,  DeadlineDays = 21, RequiredSkills = "Figma,UI Design,Prototyping",             Status = "Completed" };

        db.Projects.AddRange(proj1, proj2, proj3, proj4, proj5, proj6, proj7, proj8);
        db.SaveChanges();

        db.Bids.AddRange(
            new Bid { ProjectId = proj1.Id, FreelancerId = alex.Id,  Amount = 4500,  CoverLetter = "Ð˜Ð¼Ð°Ð¼ Ð±Ð¾Ð³Ð°Ñ‚ Ð¾Ð¿Ð¸Ñ‚ Ñ Next.js Ð¸ Stripe. Ð˜Ð·Ð³Ñ€Ð°Ð´Ð¸Ð» ÑÑŠÐ¼ 3 Ð¿Ð¾Ð´Ð¾Ð±Ð½Ð¸ Ðµ-commerce Ð¿Ð»Ð°Ñ‚Ñ„Ð¾Ñ€Ð¼Ð¸.",     DeliveryDays = 40, Status = "Pending"  },
            new Bid { ProjectId = proj1.Id, FreelancerId = ivan.Id,  Amount = 5200,  CoverLetter = "Ð¡Ð¿ÐµÑ†Ð¸Ð°Ð»Ð¸Ð·Ð¸Ñ€Ð°Ð¼ Ð² React/Next.js. ÐŸÑ€ÐµÐ´Ð»Ð°Ð³Ð°Ð¼ Ð¿ÑŠÐ»Ð½Ð¾ Ñ‚ÐµÑÑ‚Ð²Ð°Ð½Ðµ Ð¸ Ð´Ð¾ÐºÑƒÐ¼ÐµÐ½Ñ‚Ð°Ñ†Ð¸Ñ.",               DeliveryDays = 45, Status = "Pending"  },
            new Bid { ProjectId = proj2.Id, FreelancerId = maria.Id, Amount = 2800,  CoverLetter = "Ð ÐµÐ±Ñ€Ð°Ð½Ð´Ð¸Ð½Ð³ÑŠÑ‚ Ðµ Ð¼Ð¾ÑÑ‚Ð° ÑÐ¿ÐµÑ†Ð¸Ð°Ð»Ð¸Ð·Ð°Ñ†Ð¸Ñ. Ð©Ðµ Ð¿Ñ€ÐµÐ´Ð»Ð¾Ð¶Ð° 3 ÐºÐ¾Ð½Ñ†ÐµÐ¿Ñ†Ð¸Ð¸.",                          DeliveryDays = 25, Status = "Pending"  },
            new Bid { ProjectId = proj3.Id, FreelancerId = ivan.Id,  Amount = 6500,  CoverLetter = "React Native Ðµ Ð¾ÑÐ½Ð¾Ð²Ð½Ð°Ñ‚Ð° Ð¼Ð¸ Ñ‚ÐµÑ…Ð½Ð¾Ð»Ð¾Ð³Ð¸Ñ. Ð˜Ð¼Ð°Ð¼ Ð¾Ð¿Ð¸Ñ‚ Ñ GPS tracking.",                    DeliveryDays = 55, Status = "Pending"  },
            new Bid { ProjectId = proj4.Id, FreelancerId = petar.Id, Amount = 1200,  CoverLetter = "SEO Ð¾Ð´Ð¸Ñ‚Ð¸Ñ‚Ðµ ÑÐ° Ð¼Ð¾ÑÑ‚Ð° ÑÐ¿ÐµÑ†Ð¸Ð°Ð»Ð¸Ð·Ð°Ñ†Ð¸Ñ. Ð©Ðµ Ð¾ÑÐ¸Ð³ÑƒÑ€Ñ Ð¿Ð¾Ð´Ñ€Ð¾Ð±ÐµÐ½ Ð´Ð¾ÐºÐ»Ð°Ð´.",                      DeliveryDays = 25, Status = "Pending"  },
            new Bid { ProjectId = proj5.Id, FreelancerId = alex.Id,  Amount = 12000, CoverLetter = "LMS Ð¿Ð»Ð°Ñ‚Ñ„Ð¾Ñ€Ð¼Ð¸Ñ‚Ðµ ÑÐ° Ð¼Ð¸ Ð´Ð¾Ð±Ñ€Ðµ Ð¿Ð¾Ð·Ð½Ð°Ñ‚Ð¸. Ð˜Ð·Ð³Ñ€Ð°Ð´Ð¸Ð» ÑÑŠÐ¼ Ð¿Ð¾Ð´Ð¾Ð±Ð½Ð° ÑÐ¸ÑÑ‚ÐµÐ¼Ð°.",                   DeliveryDays = 85, Status = "Pending"  },
            new Bid { ProjectId = proj7.Id, FreelancerId = alex.Id,  Amount = 3000,  CoverLetter = ".NET Core API Ðµ ÐµÐ¶ÐµÐ´Ð½ÐµÐ²Ð¸ÐµÑ‚Ð¾ Ð¼Ð¸. Ð©Ðµ Ð´Ð¾ÑÑ‚Ð°Ð²Ñ Ñ‡Ð¸ÑÑ‚, Ð´Ð¾ÐºÑƒÐ¼ÐµÐ½Ñ‚Ð¸Ñ€Ð°Ð½ Ð¸ Ñ‚ÐµÑÑ‚Ð²Ð°Ð½ ÐºÐ¾Ð´.",         DeliveryDays = 28, Status = "Accepted" },
            new Bid { ProjectId = proj8.Id, FreelancerId = maria.Id, Amount = 2000,  CoverLetter = "Ð¤Ð¸Ð½Ñ‚ÐµÑ… UI Ðµ Ð¼Ð¾ÑÑ‚Ð° ÑÑ‚Ñ€Ð°ÑÑ‚. Ð˜Ð¼Ð°Ð¼ Ð¾Ð¿Ð¸Ñ‚ Ñ compliance Ð¸ accessibility ÑÑ‚Ð°Ð½Ð´Ð°Ñ€Ñ‚Ð¸.",          DeliveryDays = 18, Status = "Accepted" }
        );
        db.SaveChanges();

        db.Payments.AddRange(
            new Payment { ProjectId = proj7.Id, PayerId = client1.Id, Amount = 1500, Currency = "EUR", Method = "Stripe",    Status = "Completed", TransactionId = "stripe_ch_001",  Notes = "ÐÐ²Ð°Ð½ÑÐ¾Ð²Ð¾ Ð¿Ð»Ð°Ñ‰Ð°Ð½Ðµ 50%" },
            new Payment { ProjectId = proj8.Id, PayerId = client2.Id, Amount = 2000, Currency = "EUR", Method = "PayPal",    Status = "Completed", TransactionId = "paypal_ord_001", Notes = "ÐŸÑŠÐ»Ð½Ð¾ Ð¿Ð»Ð°Ñ‰Ð°Ð½Ðµ" },
            new Payment { ProjectId = proj1.Id, PayerId = client1.Id, Amount = 500,  Currency = "EUR", Method = "Simulated", Status = "Pending",   TransactionId = "sim_001",        Notes = "Ð”ÐµÐ¿Ð¾Ð·Ð¸Ñ‚ Ð·Ð° Ñ€ÐµÐ·ÐµÑ€Ð²Ð°Ñ†Ð¸Ñ" }
        );
        db.SaveChanges();

        db.Reviews.AddRange(
            new Review { ReviewerId = client1.Id, RevieweeId = alex.Id,  ProjectId = proj7.Id, Rating = 5, Comment = "ÐÐ»ÐµÐºÑÐ°Ð½Ð´ÑŠÑ€ Ðµ Ð¸Ð·ÐºÐ»ÑŽÑ‡Ð¸Ñ‚ÐµÐ»ÐµÐ½! Ð”Ð¾ÑÑ‚Ð°Ð²ÐµÐ½ ÐºÐ¾Ð´ Ð½Ð°Ð²Ñ€ÐµÐ¼Ðµ, Ñ‡Ð¸ÑÑ‚ Ð¸ Ð´Ð¾Ð±Ñ€Ðµ Ð´Ð¾ÐºÑƒÐ¼ÐµÐ½Ñ‚Ð¸Ñ€Ð°Ð½." },
            new Review { ReviewerId = client2.Id, RevieweeId = maria.Id, ProjectId = proj8.Id, Rating = 5, Comment = "ÐœÐ°Ñ€Ð¸Ñ Ð½Ð°Ð´Ð¼Ð¸Ð½Ð° Ð¾Ñ‡Ð°ÐºÐ²Ð°Ð½Ð¸ÑÑ‚Ð° Ð½Ð¸. Ð”Ð¸Ð·Ð°Ð¹Ð½ÑŠÑ‚ Ðµ Ð½ÐµÐ²ÐµÑ€Ð¾ÑÑ‚ÐµÐ½!" },
            new Review { ReviewerId = alex.Id,   RevieweeId = client1.Id, ProjectId = proj7.Id, Rating = 4, Comment = "Ð”Ð¾Ð±ÑŠÑ€ ÐºÐ»Ð¸ÐµÐ½Ñ‚, ÑÑÐ½Ð¸ Ð¸Ð·Ð¸ÑÐºÐ²Ð°Ð½Ð¸Ñ Ð¸ Ð±ÑŠÑ€Ð·Ð° ÐºÐ¾Ð¼ÑƒÐ½Ð¸ÐºÐ°Ñ†Ð¸Ñ." }
        );
        db.SaveChanges();
    }
}



