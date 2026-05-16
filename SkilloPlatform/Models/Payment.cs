using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkilloPlatform.Models;

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

// ── Review ────────────────────────────────────────────────────
