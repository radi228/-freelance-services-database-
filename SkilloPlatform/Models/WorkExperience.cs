using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkilloPlatform.Models;

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

// ── Certificate ───────────────────────────────────────────────
