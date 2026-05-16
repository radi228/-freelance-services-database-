using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkilloPlatform.Models;

public class Conversation
{
    public int Id { get; set; }

    [ForeignKey("ParticipantOne")]
    public int ParticipantOneId { get; set; }
    public User ParticipantOne { get; set; } = null!;

    [ForeignKey("ParticipantTwo")]
    public int ParticipantTwoId { get; set; }
    public User ParticipantTwo { get; set; } = null!;

    public bool IsSupport { get; set; } = false;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

// ── ChatMessage ───────────────────────────────────────────────
