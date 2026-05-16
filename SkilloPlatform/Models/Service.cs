using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkilloPlatform.Models;

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

// ── Project ───────────────────────────────────────────────────
