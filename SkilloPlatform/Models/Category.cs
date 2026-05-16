using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkilloPlatform.Models;

public class Category
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public int FreelancerCount { get; set; } = 0;
}

// ── Conversation ──────────────────────────────────────────────
