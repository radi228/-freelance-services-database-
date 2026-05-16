using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkilloPlatform.Models;

public class ChatMessage
{
    public int Id { get; set; }

    [ForeignKey("Conversation")]
    public int ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;

    [ForeignKey("Sender")]
    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;

    [Required]
    public string Content { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
