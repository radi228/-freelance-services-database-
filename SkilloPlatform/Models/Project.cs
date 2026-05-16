using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkilloPlatform.Models;

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

// ── Bid ───────────────────────────────────────────────────────
