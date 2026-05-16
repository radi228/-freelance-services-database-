using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkilloPlatform.Models;

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

// ── FreelancerProfile ─────────────────────────────────────────
