using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SkilloPlatform.Data;
using SkilloPlatform.Models;

namespace SkilloPlatform.Controllers;

public class ChatHub : Hub
{
    private readonly SkilloDbContext _db;
    public ChatHub(SkilloDbContext db) { _db = db; }

    public async Task JoinConversation(int conversationId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, $"conv_{conversationId}");

    public async Task SendMessage(int conversationId, string content)
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim is null) return;
        int userId = int.Parse(userIdClaim);
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return;
        var conv = await _db.Conversations.FindAsync(conversationId);
        if (conv is null || (conv.ParticipantOneId != userId && conv.ParticipantTwoId != userId)) return;

        var msg = new ChatMessage { ConversationId = conversationId, SenderId = userId, Content = content.Trim(), CreatedAt = DateTime.UtcNow };
        _db.ChatMessages.Add(msg);
        conv.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await Clients.Group($"conv_{conversationId}").SendAsync("ReceiveMessage", new
        {
            id = msg.Id, conversationId, senderId = userId,
            senderName = user.FullName, senderAvatar = user.Avatar,
            content = msg.Content, createdAt = msg.CreatedAt, isOwn = false,
        });
    }
}

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly SkilloDbContext      _db;
    private readonly IHubContext<ChatHub> _hub;

    public ChatController(SkilloDbContext db, IHubContext<ChatHub> hub) { _db = db; _hub = hub; }

    // Gets current authenticated user ID from JWT NameIdentifier claim
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations()
    {
        var convs = await _db.Conversations
            .Include(c => c.ParticipantOne).Include(c => c.ParticipantTwo)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .Where(c => c.ParticipantOneId == UserId || c.ParticipantTwoId == UserId)
            .OrderByDescending(c => c.UpdatedAt).ToListAsync();

        return Ok(convs.Select(c => {
            var other = c.ParticipantOneId == UserId ? c.ParticipantTwo : c.ParticipantOne;
            var last  = c.Messages.FirstOrDefault();
            return new { id=c.Id, otherUserId=other.Id, otherUserName=other.FullName,
                otherAvatar=other.Avatar, otherRole=other.Role,
                lastMessage=last?.Content??"", lastMessageAt=last?.CreatedAt,
                isSupport=c.IsSupport, updatedAt=c.UpdatedAt };
        }));
    }

    [HttpGet("conversations/{id:int}/messages")]
    public async Task<IActionResult> GetMessages(int id, [FromQuery] int page = 1)
    {
        var conv = await _db.Conversations.FindAsync(id);
        if (conv is null) return NotFound();
        if (conv.ParticipantOneId != UserId && conv.ParticipantTwoId != UserId)
        {
            var role = User.FindFirstValue(ClaimTypes.Role);
            if (role != "Admin" && role != "SuperAdmin") return Forbid();
        }
        var msgs = await _db.ChatMessages.Include(m => m.Sender)
            .Where(m => m.ConversationId == id).OrderBy(m => m.CreatedAt)
            .Skip((page-1)*50).Take(50).ToListAsync();
        return Ok(msgs.Select(m => new { id=m.Id, conversationId=m.ConversationId,
            senderId=m.SenderId, senderName=m.Sender.FullName, senderAvatar=m.Sender.Avatar,
            content=m.Content, createdAt=m.CreatedAt, isOwn=m.SenderId==UserId }));
    }

    [HttpPost("conversations")]
    public async Task<IActionResult> StartConversation([FromBody] StartConvRequest req)
    {
        if (req.OtherUserId == UserId) return BadRequest(new { message="ÐÐµ Ð¼Ð¾Ð¶Ðµ Ð´Ð° Ð¿Ð¸ÑˆÐµÑˆ Ð½Ð° ÑÐµÐ±Ðµ ÑÐ¸." });
        var other = await _db.Users.FindAsync(req.OtherUserId);
        if (other is null) return NotFound(new { message="ÐŸÐ¾Ñ‚Ñ€ÐµÐ±Ð¸Ñ‚ÐµÐ»ÑÑ‚ Ð½Ðµ Ðµ Ð½Ð°Ð¼ÐµÑ€ÐµÐ½." });

        var existing = await _db.Conversations.FirstOrDefaultAsync(c =>
            (c.ParticipantOneId==UserId && c.ParticipantTwoId==req.OtherUserId) ||
            (c.ParticipantOneId==req.OtherUserId && c.ParticipantTwoId==UserId));
        if (existing is not null) return Ok(new { id=existing.Id, isNew=false });

        var me = await _db.Users.FindAsync(UserId);
        var conv = new Conversation {
            ParticipantOneId=UserId, ParticipantTwoId=req.OtherUserId,
            IsSupport = me!.Role is "Admin" or "SuperAdmin" || other.Role is "Admin" or "SuperAdmin",
            UpdatedAt=DateTime.UtcNow };
        _db.Conversations.Add(conv);
        await _db.SaveChangesAsync();
        return Ok(new { id=conv.Id, isNew=true });
    }

    [HttpPost("conversations/{id:int}/send")]
    public async Task<IActionResult> SendMsg(int id, [FromBody] SendMsgRequest req)
    {
        var conv = await _db.Conversations.FindAsync(id);
        if (conv is null) return NotFound();
        if (conv.ParticipantOneId != UserId && conv.ParticipantTwoId != UserId) return Forbid();
        if (string.IsNullOrWhiteSpace(req.Content)) return BadRequest(new { message="Ð¡ÑŠÐ¾Ð±Ñ‰ÐµÐ½Ð¸ÐµÑ‚Ð¾ Ðµ Ð¿Ñ€Ð°Ð·Ð½Ð¾." });

        var msg = new ChatMessage { ConversationId=id, SenderId=UserId, Content=req.Content.Trim(), CreatedAt=DateTime.UtcNow };
        _db.ChatMessages.Add(msg);
        conv.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var user = await _db.Users.FindAsync(UserId);
        await _hub.Clients.Group($"conv_{id}").SendAsync("ReceiveMessage", new {
            id=msg.Id, conversationId=id, senderId=UserId,
            senderName=user!.FullName, senderAvatar=user.Avatar,
            content=msg.Content, createdAt=msg.CreatedAt, isOwn=false });
        return Ok(new { id=msg.Id });
    }

    [HttpGet("search-users")]
    public async Task<IActionResult> SearchUsers([FromQuery] string q)
    {
        if (string.IsNullOrEmpty(q) || q.Length < 3) return Ok(Array.Empty<object>());
        var users = await _db.Users
            .Where(u => !u.IsBanned && u.Id != UserId && (u.Email.Contains(q) || u.FullName.Contains(q)))
            .Take(8).Select(u => new { u.Id, u.FullName, u.Email, u.Role, u.Avatar }).ToListAsync();
        return Ok(users);
    }

    [HttpGet("find-admin")]
    public async Task<IActionResult> FindAdmin()
    {
        var admin = await _db.Users.Where(u => u.Role=="Admin"||u.Role=="SuperAdmin")
            .Select(u => new { u.Id, u.FullName, u.Email, u.Role }).FirstOrDefaultAsync();
        if (admin is null) return NotFound(new { message="ÐÑÐ¼Ð° Ð°Ð´Ð¼Ð¸Ð½Ð¸ÑÑ‚Ñ€Ð°Ñ‚Ð¾Ñ€." });
        return Ok(admin);
    }

    [HttpGet("support")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetSupport()
    {
        var convs = await _db.Conversations
            .Include(c => c.ParticipantOne).Include(c => c.ParticipantTwo)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .Where(c => c.IsSupport).OrderByDescending(c => c.UpdatedAt).ToListAsync();
        return Ok(convs.Select(c => {
            var u = c.ParticipantOne.Role is "Admin" or "SuperAdmin" ? c.ParticipantTwo : c.ParticipantOne;
            var last = c.Messages.FirstOrDefault();
            return new { id=c.Id, userId=u.Id, userName=u.FullName, userAvatar=u.Avatar, userRole=u.Role, lastMessage=last?.Content??"", updatedAt=c.UpdatedAt };
        }));
    }
}

public record StartConvRequest(int OtherUserId);
public record SendMsgRequest(string Content);



