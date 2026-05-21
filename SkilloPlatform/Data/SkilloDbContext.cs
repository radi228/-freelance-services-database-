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
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
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
        mb.Entity<FreelancerProfile>().Property(fp => fp.HourlyRate).HasColumnType("decimal(18,2)");
        mb.Entity<Project>().Property(p => p.BudgetMin).HasPrecision(18, 2);
        mb.Entity<Project>().Property(p => p.BudgetMax).HasPrecision(18, 2);
        mb.Entity<Bid>().Property(b => b.Amount).HasPrecision(18, 2);
        mb.Entity<Payment>().Property(p => p.Amount).HasPrecision(18, 2);
    }

    public static void SeedData(SkilloDbContext db)
    {
        try { if (db.Users.Any()) return; } catch { return; }

        var hash = BCrypt.Net.BCrypt.HashPassword("Demo1234!");

        var superAdmin = new User { FullName = "Супер Администратор", Email = "superadmin@skillo.bg", PasswordHash = hash, Role = "SuperAdmin" };
        var admin      = new User { FullName = "Администратор",       Email = "admin@skillo.bg",       PasswordHash = hash, Role = "Admin" };
        var alex       = new User { FullName = "Александър Тодоров",  Email = "alex@skillo.bg",        PasswordHash = hash, Role = "Freelancer" };
        var maria      = new User { FullName = "Мария Георгиева",     Email = "maria@skillo.bg",       PasswordHash = hash, Role = "Freelancer" };
        var petar      = new User { FullName = "Петър Иванов",        Email = "petar@skillo.bg",       PasswordHash = hash, Role = "Freelancer" };
        var ivan       = new User { FullName = "Иван Стоянов",        Email = "ivan@skillo.bg",        PasswordHash = hash, Role = "Freelancer" };
        var client1    = new User { FullName = "TechStart Bulgaria",  Email = "client@techstart.bg",   PasswordHash = hash, Role = "Client" };
        var client2    = new User { FullName = "Мартин Петров",       Email = "martin@example.bg",     PasswordHash = hash, Role = "Client" };
        var client3    = new User { FullName = "СофтУни ЕООД",        Email = "hr@softuni.bg",         PasswordHash = hash, Role = "Client" };

        db.Users.AddRange(superAdmin, admin, alex, maria, petar, ivan, client1, client2, client3);
        db.SaveChanges();

        db.Categories.AddRange(
            new Category { Name = "Уеб разработка",         Icon = "💻", FreelancerCount = 312 },
            new Category { Name = "Графичен дизайн",        Icon = "🎨", FreelancerCount = 218 },
            new Category { Name = "Мобилни приложения",     Icon = "📱", FreelancerCount = 145 },
            new Category { Name = "Копирайтинг & SEO",      Icon = "✍️", FreelancerCount = 289 },
            new Category { Name = "Дигитален маркетинг",    Icon = "📊", FreelancerCount = 176 },
            new Category { Name = "Видео & Анимация",       Icon = "🎬", FreelancerCount = 98  },
            new Category { Name = "Киберсигурност",         Icon = "🔒", FreelancerCount = 67  },
            new Category { Name = "Преводи",                Icon = "🌐", FreelancerCount = 134 },
            new Category { Name = "Финанси & Счетоводство", Icon = "📈", FreelancerCount = 89  },
            new Category { Name = "AI & Автоматизация",     Icon = "🤖", FreelancerCount = 112 }
        );
        db.SaveChanges();

        var profileAlex = new FreelancerProfile { UserId = alex.Id, Title = "Full-Stack Developer", Bio = "5+ години опит в изграждане на уеб приложения с React и Node.js.", Skills = "React,Node.js,TypeScript,PostgreSQL,Docker,AWS", Category = "Уеб разработка", HourlyRate = 45, ExperienceLevel = "Senior", Location = "София, България", Website = "https://alex-dev.bg", LinkedIn = "https://linkedin.com/in/alex", GitHub = "https://github.com/alex", Languages = "Български,Английски", IsVerified = true, IsAvailable = true };
        var profileMaria = new FreelancerProfile { UserId = maria.Id, Title = "UI/UX Designer & Brand Designer", Bio = "Специализирам в създаването на красиви и интуитивни интерфейси.", Skills = "Figma,Adobe XD,Illustrator,Photoshop,Branding,Prototyping", Category = "Графичен дизайн", HourlyRate = 60, ExperienceLevel = "Senior", Location = "Пловдив, България", LinkedIn = "https://linkedin.com/in/maria", Languages = "Български,Английски,Немски", IsVerified = true, IsAvailable = true };
        var profilePetar = new FreelancerProfile { UserId = petar.Id, Title = "SEO Специалист & Копирайтър", Bio = "6+ години опит в SEO оптимизация.", Skills = "SEO,Копирайтинг,WordPress,Google Analytics,Email Marketing", Category = "Копирайтинг & SEO", HourlyRate = 25, ExperienceLevel = "Senior", Location = "Варна, България", Languages = "Български,Английски", IsVerified = false, IsAvailable = true };
        var profileIvan = new FreelancerProfile { UserId = ivan.Id, Title = "Mobile App Developer", Bio = "Разработвам мобилни приложения за iOS и Android.", Skills = "React Native,Flutter,Swift,Kotlin,Firebase", Category = "Мобилни приложения", HourlyRate = 55, ExperienceLevel = "Mid", Location = "София, България", GitHub = "https://github.com/ivan", Languages = "Български,Английски", IsVerified = true, IsAvailable = false };

        db.FreelancerProfiles.AddRange(profileAlex, profileMaria, profilePetar, profileIvan);
        db.SaveChanges();

        db.WorkExperiences.AddRange(
            new WorkExperience { FreelancerProfileId = profileAlex.Id,  Company = "SoftUni Solutions",    Position = "Senior Developer", StartDate = "2020-01", IsCurrent = true,  Description = "Водещ разработчик на SaaS платформа." },
            new WorkExperience { FreelancerProfileId = profileAlex.Id,  Company = "Telerik Academy",      Position = "Junior Developer", StartDate = "2018-06", EndDate = "2019-12", Description = "Разработка на вътрешни инструменти." },
            new WorkExperience { FreelancerProfileId = profileMaria.Id, Company = "Design Studio BG",    Position = "Lead Designer",    StartDate = "2019-03", IsCurrent = true,  Description = "Ръководя екип от 4 дизайнери." },
            new WorkExperience { FreelancerProfileId = profilePetar.Id, Company = "SEO Masters BG",      Position = "SEO Lead",         StartDate = "2018-01", IsCurrent = true,  Description = "Управление на SEO стратегии." },
            new WorkExperience { FreelancerProfileId = profileIvan.Id,  Company = "AppFactory",          Position = "Mobile Developer", StartDate = "2021-03", IsCurrent = true,  Description = "Разработка на финтех приложения." }
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
            new Service { UserId = alex.Id,  Title = "Landing Page с React",            Description = "Красив и бърз Landing Page с React и Tailwind CSS. Включва адаптивен дизайн и SEO оптимизация.", Category = "Уеб разработка",     Price = 500,  PriceType = "fixed",   DeliveryDays = 7,  Revisions = 2, IsActive = true },
            new Service { UserId = alex.Id,  Title = "Full-Stack уеб приложение",       Description = "Пълно уеб приложение с React фронтенд, Node.js API и PostgreSQL база данни.",                     Category = "Уеб разработка",     Price = 3000, PriceType = "fixed",   DeliveryDays = 30, Revisions = 3, IsActive = true },
            new Service { UserId = alex.Id,  Title = "Хостинг и DevOps настройка",      Description = "Настройка на сървър, CI/CD pipeline, SSL сертификат и мониторинг.",                               Category = "Уеб разработка",     Price = 200,  PriceType = "fixed",   DeliveryDays = 3,  Revisions = 1, IsActive = true },
            new Service { UserId = maria.Id, Title = "Дизайн на лого",                  Description = "Уникално лого с 3 концепции. Включва brand guidelines.",                                           Category = "Графичен дизайн",    Price = 350,  PriceType = "fixed",   DeliveryDays = 5,  Revisions = 5, IsActive = true },
            new Service { UserId = maria.Id, Title = "UI Kit за мобилно приложение",    Description = "Пълен UI Kit в Figma — 50+ компонента, 10+ екрана и prototype.",                                   Category = "Графичен дизайн",    Price = 1200, PriceType = "fixed",   DeliveryDays = 14, Revisions = 2, IsActive = true },
            new Service { UserId = maria.Id, Title = "Brand Identity пакет",            Description = "Пълна визуална идентичност — лого, цветова схема, типография и визитки.",                          Category = "Графичен дизайн",    Price = 800,  PriceType = "fixed",   DeliveryDays = 10, Revisions = 3, IsActive = true },
            new Service { UserId = petar.Id, Title = "SEO одит на сайт",               Description = "Пълен технически SEO одит с детайлен доклад и препоръки.",                                         Category = "Копирайтинг & SEO",  Price = 200,  PriceType = "fixed",   DeliveryDays = 3,  Revisions = 1, IsActive = true },
            new Service { UserId = petar.Id, Title = "Месечно SEO управление",          Description = "Оптимизация на ключови думи, линк билдинг и месечен отчет.",                                       Category = "Копирайтинг & SEO",  Price = 500,  PriceType = "monthly", DeliveryDays = 30, Revisions = 0, IsActive = true },
            new Service { UserId = ivan.Id,  Title = "React Native мобилно приложение", Description = "Кросплатформено приложение за iOS и Android. Включва публикуване в App Store.",                   Category = "Мобилни приложения", Price = 4000, PriceType = "fixed",   DeliveryDays = 45, Revisions = 3, IsActive = true }
        );
        db.SaveChanges();

        var proj1 = new Project { ClientId = client1.Id, Title = "Разработка на е-commerce платформа",    Description = "Търсим Next.js разработчик за онлайн магазин с Stripe и управление на инвентар.",       Category = "Уеб разработка",      BudgetMin = 3500, BudgetMax = 6000,  DeadlineDays = 45, RequiredSkills = "Next.js,Stripe,MongoDB,TypeScript",      Status = "Open" };
        var proj2 = new Project { ClientId = client1.Id, Title = "Ребрандинг на технологична компания",   Description = "Нужен ни е пълен ребрандинг — ново лого и brand guidelines за нашия B2B SaaS продукт.", Category = "Графичен дизайн",     BudgetMin = 2000, BudgetMax = 3500,  DeadlineDays = 30, RequiredSkills = "Figma,Branding,Illustration",            Status = "Open" };
        var proj3 = new Project { ClientId = client2.Id, Title = "Мобилно приложение за доставки",        Description = "Приложение за iOS и Android за куриерски доставки с real-time tracking.",              Category = "Мобилни приложения",  BudgetMin = 5000, BudgetMax = 8000,  DeadlineDays = 60, RequiredSkills = "React Native,Firebase,Google Maps",       Status = "Open" };
        var proj4 = new Project { ClientId = client2.Id, Title = "SEO оптимизация на корпоративен сайт",  Description = "SEO одит и оптимизация на сайт с 200+ страници. Целим топ 3 позиции.",                 Category = "Копирайтинг & SEO",   BudgetMin = 800,  BudgetMax = 1500,  DeadlineDays = 30, RequiredSkills = "SEO,Google Analytics,WordPress",          Status = "Open" };
        var proj5 = new Project { ClientId = client3.Id, Title = "Платформа за онлайн обучение",          Description = "LMS платформа с видео лекции, тестове, сертификати и система за плащания.",            Category = "Уеб разработка",      BudgetMin = 8000, BudgetMax = 15000, DeadlineDays = 90, RequiredSkills = "React,Node.js,PostgreSQL,Stripe",          Status = "Open" };
        var proj6 = new Project { ClientId = client3.Id, Title = "Дигитален маркетинг за SaaS продукт",   Description = "Google Ads, Facebook Ads и email маркетинг кампании за нашия SaaS.",                    Category = "Дигитален маркетинг", BudgetMin = 1000, BudgetMax = 2000,  DeadlineDays = 30, RequiredSkills = "Google Ads,Facebook Ads,Email Marketing", Status = "Open" };
        var proj7 = new Project { ClientId = client1.Id, Title = "REST API за мобилно приложение",         Description = ".NET Core REST API с JWT автентикация и Entity Framework.",                            Category = "Уеб разработка",      BudgetMin = 2000, BudgetMax = 4000,  DeadlineDays = 30, RequiredSkills = "C#,.NET,SQL Server,JWT",                  Status = "InProgress" };
        var proj8 = new Project { ClientId = client2.Id, Title = "UI дизайн за финтех приложение",         Description = "UI/UX дизайн за мобилно финтех приложение — wireframes и prototype в Figma.",         Category = "Графичен дизайн",     BudgetMin = 1500, BudgetMax = 2500,  DeadlineDays = 21, RequiredSkills = "Figma,UI Design,Prototyping",             Status = "Completed" };

        db.Projects.AddRange(proj1, proj2, proj3, proj4, proj5, proj6, proj7, proj8);
        db.SaveChanges();

        db.Bids.AddRange(
            new Bid { ProjectId = proj1.Id, FreelancerId = alex.Id,  Amount = 4500,  CoverLetter = "Имам богат опит с Next.js и Stripe. Изградил съм 3 подобни е-commerce платформи.",     DeliveryDays = 40, Status = "Pending"  },
            new Bid { ProjectId = proj1.Id, FreelancerId = ivan.Id,  Amount = 5200,  CoverLetter = "Специализирам в React/Next.js. Предлагам пълно тестване и документация.",               DeliveryDays = 45, Status = "Pending"  },
            new Bid { ProjectId = proj2.Id, FreelancerId = maria.Id, Amount = 2800,  CoverLetter = "Ребрандингът е моята специализация. Ще предложа 3 концепции.",                          DeliveryDays = 25, Status = "Pending"  },
            new Bid { ProjectId = proj3.Id, FreelancerId = ivan.Id,  Amount = 6500,  CoverLetter = "React Native е основната ми технология. Имам опит с GPS tracking.",                    DeliveryDays = 55, Status = "Pending"  },
            new Bid { ProjectId = proj4.Id, FreelancerId = petar.Id, Amount = 1200,  CoverLetter = "SEO одитите са моята специализация. Ще осигуря подробен доклад.",                      DeliveryDays = 25, Status = "Pending"  },
            new Bid { ProjectId = proj5.Id, FreelancerId = alex.Id,  Amount = 12000, CoverLetter = "LMS платформите са ми добре познати. Изградил съм подобна система.",                   DeliveryDays = 85, Status = "Pending"  },
            new Bid { ProjectId = proj7.Id, FreelancerId = alex.Id,  Amount = 3000,  CoverLetter = ".NET Core API е ежедневието ми. Ще доставя чист, документиран и тестван код.",         DeliveryDays = 28, Status = "Accepted" },
            new Bid { ProjectId = proj8.Id, FreelancerId = maria.Id, Amount = 2000,  CoverLetter = "Финтех UI е моята страст. Имам опит с compliance и accessibility стандарти.",          DeliveryDays = 18, Status = "Accepted" }
        );
        db.SaveChanges();

        db.Payments.AddRange(
            new Payment { ProjectId = proj7.Id, PayerId = client1.Id, Amount = 1500, Currency = "EUR", Method = "Stripe",    Status = "Completed", TransactionId = "stripe_ch_001",  Notes = "Авансово плащане 50%" },
            new Payment { ProjectId = proj8.Id, PayerId = client2.Id, Amount = 2000, Currency = "EUR", Method = "PayPal",    Status = "Completed", TransactionId = "paypal_ord_001", Notes = "Пълно плащане" },
            new Payment { ProjectId = proj1.Id, PayerId = client1.Id, Amount = 500,  Currency = "EUR", Method = "Simulated", Status = "Pending",   TransactionId = "sim_001",        Notes = "Депозит за резервация" }
        );
        db.SaveChanges();

        db.Reviews.AddRange(
            new Review { ReviewerId = client1.Id, RevieweeId = alex.Id,  ProjectId = proj7.Id, Rating = 5, Comment = "Александър е изключителен! Доставен код навреме, чист и добре документиран." },
            new Review { ReviewerId = client2.Id, RevieweeId = maria.Id, ProjectId = proj8.Id, Rating = 5, Comment = "Мария надмина очакванията ни. Дизайнът е невероятен!" },
            new Review { ReviewerId = alex.Id,   RevieweeId = client1.Id, ProjectId = proj7.Id, Rating = 4, Comment = "Добър клиент, ясни изисквания и бърза комуникация." }
        );
        db.SaveChanges();
    }
}
