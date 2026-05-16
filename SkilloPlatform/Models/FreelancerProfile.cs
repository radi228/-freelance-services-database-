using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkilloPlatform.Models;

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

// ── WorkExperience ────────────────────────────────────────────
