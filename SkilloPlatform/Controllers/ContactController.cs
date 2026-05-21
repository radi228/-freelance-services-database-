using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkilloPlatform.Data;
using SkilloPlatform.Models;

namespace SkilloPlatform.Controllers;

[ApiController]
[Route("api/contact")]
public class ContactController : ControllerBase
{
    private readonly SkilloDbContext _db;
    public ContactController(SkilloDbContext db) { _db = db; }

    // POST /api/contact - anyone can send
    [HttpPost]
    public async Task<IActionResult> Send([FromBody] ContactRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Message))
            return BadRequest(new { message = "Попълни всички полета." });

        var msg = new ContactMessage
        {
            Name = req.Name,
            Email = req.Email,
            Subject = req.Subject ?? "Без тема",
            Message = req.Message,
            CreatedAt = DateTime.UtcNow,
            IsReplied = false
        };
        _db.ContactMessages.Add(msg);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Съобщението е изпратено!" });
    }

    // GET /api/contact - Admin only
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetAll()
    {
        var msgs = await _db.ContactMessages
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new {
                m.Id, m.Name, m.Email, m.Subject,
                m.Message, m.CreatedAt, m.IsReplied, m.ReplyNote
            })
            .ToListAsync();
        return Ok(msgs);
    }

    // PATCH /api/contact/{id}/replied - Admin marks as replied
    [HttpPatch("{id:int}/replied")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> MarkReplied(int id, [FromBody] ReplyNoteRequest req)
    {
        var msg = await _db.ContactMessages.FindAsync(id);
        if (msg is null) return NotFound();
        msg.IsReplied = true;
        msg.ReplyNote = req.Note;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Маркирано като отговорено." });
    }
}

public record ContactRequest(string Name, string Email, string? Subject, string Message);
public record ReplyNoteRequest(string? Note);
