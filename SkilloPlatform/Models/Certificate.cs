using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkilloPlatform.Models;

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

// ── Service ───────────────────────────────────────────────────
