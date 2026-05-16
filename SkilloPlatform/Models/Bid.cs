using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkilloPlatform.Models;

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

// ── Payment ───────────────────────────────────────────────────
