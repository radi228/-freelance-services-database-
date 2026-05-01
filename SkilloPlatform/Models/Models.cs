using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkilloPlatform.Models;

// â”€â”€ User â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class User
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string FullName { get; set; } = "";

    [Required, MaxLength(200)]
    public string Email { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    // Client | Freelancer | Admin | SuperAdmin
    public string Role { get; set; } = "Client";

    public string Avatar { get; set; } = "";
    public bool IsBanned { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public FreelancerProfile? FreelancerProfile { get; set; }
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public ICollection<Review> ReviewsGiven { get; set; } = new List<Review>();
    public ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Service> Services { get; set; } = new List<Service>();
}

// â”€â”€ FreelancerProfile â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class FreelancerProfile
{
    public int Id { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string Title { get; set; } = "";
    public string Bio { get; set; } = "";
    public string Skills { get; set; } = "";      // comma-separated
    public string Category { get; set; } = "";
    public decimal HourlyRate { get; set; } = 0;
    public string ExperienceLevel { get; set; } = "Mid"; // Junior | Mid | Senior
    public string Location { get; set; } = "";
    public string Website { get; set; } = "";
    public string LinkedIn { get; set; } = "";
    public string GitHub { get; set; } = "";
    public string Languages { get; set; } = "";   // comma-separated
    public bool IsVerified { get; set; } = false;
    public bool IsAvailable { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<WorkExperience> WorkExperiences { get; set; } = new List<WorkExperience>();
    public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
}

// â”€â”€ WorkExperience â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class WorkExperience
{
    public int Id { get; set; }

    [ForeignKey("FreelancerProfile")]
    public int FreelancerProfileId { get; set; }
    public FreelancerProfile FreelancerProfile { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Company { get; set; } = "";

    [Required, MaxLength(200)]
    public string Position { get; set; } = "";

    public string StartDate { get; set; } = "";
    public string EndDate { get; set; } = "";
    public bool IsCurrent { get; set; } = false;
    public string Description { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// â”€â”€ Certificate â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class Certificate
{
    public int Id { get; set; }

    [ForeignKey("FreelancerProfile")]
    public int FreelancerProfileId { get; set; }
    public FreelancerProfile FreelancerProfile { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Name { get; set; } = "";

    [Required, MaxLength(200)]
    public string Issuer { get; set; } = "";

    public string IssueDate { get; set; } = "";
    public string ExpiryDate { get; set; } = "";
    public string Credential { get; set; } = "";
    public string FileUrl { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// â”€â”€ Service â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class Service
{
    public int Id { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [Required, MaxLength(300)]
    public string Title { get; set; } = "";

    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; } = 0;
    public string PriceType { get; set; } = "fixed"; // fixed | hourly | monthly
    public int DeliveryDays { get; set; } = 7;
    public int Revisions { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// â”€â”€ Project â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class Project
{
    public int Id { get; set; }

    [ForeignKey("Client")]
    public int ClientId { get; set; }
    public User Client { get; set; } = null!;

    [Required, MaxLength(300)]
    public string Title { get; set; } = "";

    public string Description { get; set; } = "";
    public string Category { get; set; } = "";   // comma-separated (multi-category)
    public decimal BudgetMin { get; set; } = 0;
    public decimal BudgetMax { get; set; } = 0;
    public int DeadlineDays { get; set; } = 30;
    public string RequiredSkills { get; set; } = ""; // comma-separated
    public string Status { get; set; } = "Open";     // Open | InProgress | Completed | Closed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Bid> Bids { get; set; } = new List<Bid>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

// â”€â”€ Bid â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class Bid
{
    public int Id { get; set; }

    [ForeignKey("Project")]
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [ForeignKey("Freelancer")]
    public int FreelancerId { get; set; }
    public User Freelancer { get; set; } = null!;

    public decimal Amount { get; set; }
    public string CoverLetter { get; set; } = "";
    public int DeliveryDays { get; set; } = 30;
    public string Status { get; set; } = "Pending"; // Pending | Accepted | Rejected
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// â”€â”€ Payment â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class Payment
{
    public int Id { get; set; }

    [ForeignKey("Project")]
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [ForeignKey("Payer")]
    public int PayerId { get; set; }
    public User Payer { get; set; } = null!;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "BGN";
    public string Method { get; set; } = "Stripe"; // Stripe | PayPal | Simulated
    public string Status { get; set; } = "Pending"; // Pending | Completed | Failed | Refunded
    public string TransactionId { get; set; } = "";
    public string Notes { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// â”€â”€ Review â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class Review
{
    public int Id { get; set; }

    [ForeignKey("Reviewer")]
    public int ReviewerId { get; set; }
    public User Reviewer { get; set; } = null!;

    [ForeignKey("Reviewee")]
    public int RevieweeId { get; set; }
    public User Reviewee { get; set; } = null!;

    [ForeignKey("Project")]
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; }
    public string Comment { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// â”€â”€ Category â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public int FreelancerCount { get; set; } = 0;
}

// â”€â”€ Conversation â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class Conversation
{
    public int Id { get; set; }

    [ForeignKey("ParticipantOne")]
    public int ParticipantOneId { get; set; }
    public User ParticipantOne { get; set; } = null!;

    [ForeignKey("ParticipantTwo")]
    public int ParticipantTwoId { get; set; }
    public User ParticipantTwo { get; set; } = null!;

    public bool IsSupport { get; set; } = false;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

// â”€â”€ ChatMessage â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public class ChatMessage
{
    public int Id { get; set; }

    [ForeignKey("Conversation")]
    public int ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;

    [ForeignKey("Sender")]
    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;

    [Required]
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

