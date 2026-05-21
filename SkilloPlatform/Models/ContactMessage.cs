using System;

namespace SkilloPlatform.Models;

public class ContactMessage
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Subject { get; set; } = "";
    public string Message { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsReplied { get; set; } = false;
    public string? ReplyNote { get; set; }
}
