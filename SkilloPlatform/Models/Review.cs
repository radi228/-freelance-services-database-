using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkilloPlatform.Models;

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
    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }
    public string Comment { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ── Category ──────────────────────────────────────────────────
