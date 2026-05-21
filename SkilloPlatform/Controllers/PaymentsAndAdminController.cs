using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkilloPlatform.Data;
using SkilloPlatform.DTOs;
using SkilloPlatform.Models;
using SkilloPlatform.Services;

namespace SkilloPlatform.Controllers;

// ══════════════════════════════════════════════════════════════
//  PAYMENTS
// ══════════════════════════════════════════════════════════════
[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly SkilloDbContext _db;
    private readonly IPaymentService _payments;

    public PaymentsController(SkilloDbContext db, IPaymentService payments)
    {
        _db       = db;
        _payments = payments;
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /api/payments/mine — payment history for logged-in user
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var payments = await _db.Payments
            .Include(p => p.Project)
            .Include(p => p.Payer)
            .Where(p => p.PayerId == UserId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(payments.Select(Map));
    }

    // GET /api/payments/project/{id} — payments for a specific project
    [HttpGet("project/{projectId:int}")]
    public async Task<IActionResult> GetByProject(int projectId)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project is null) return NotFound();

        var role = User.FindFirstValue(ClaimTypes.Role)!;
        if (project.ClientId != UserId && role != "Admin" && role != "SuperAdmin")
            return Forbid();

        var payments = await _db.Payments
            .Include(p => p.Project)
            .Include(p => p.Payer)
            .Where(p => p.ProjectId == projectId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(payments.Select(Map));
    }

    // POST /api/payments/stripe
    [HttpPost("stripe")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> PayWithStripe([FromBody] SimplePaymentRequest req)
    {
        var simReq = new PaymentRequest(req.ProjectId, req.Amount, "EUR", "Stripe");
        var result = await _payments.ProcessSimulatedAsync(UserId, simReq);
        return Ok(result);
    }

    // POST /api/payments/paypal
    [HttpPost("paypal")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> PayWithPayPal([FromBody] SimplePaymentRequest req)
    {
        var simReq = new PaymentRequest(req.ProjectId, req.Amount, "EUR", "PayPal");
        var result = await _payments.ProcessSimulatedAsync(UserId, simReq);
        return Ok(result);
    }

    // POST /api/payments/simulated
    [HttpPost("simulated")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> PaySimulated([FromBody] SimplePaymentRequest req)
    {
        var simReq = new PaymentRequest(req.ProjectId, req.Amount, "EUR", "Simulated");
        var result = await _payments.ProcessSimulatedAsync(UserId, simReq);
        return Ok(result);
    }

    // POST /api/payments/{id}/refund
    [HttpPost("{id:int}/refund")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Refund(int id)
    {
        try
        {
            var result = await _payments.RefundAsync(id, UserId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private static PaymentResponse Map(Payment p) => new(
        p.Id, p.ProjectId, p.Project?.Title ?? "",
        p.PayerId, p.Payer?.FullName ?? "",
        p.Amount, p.Currency, p.Method,
        p.Status, p.TransactionId, p.Notes, p.CreatedAt
    );
}

// ══════════════════════════════════════════════════════════════
//  ADMIN
// ══════════════════════════════════════════════════════════════
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AdminController : ControllerBase
{
    private readonly SkilloDbContext _db;
    private readonly ITokenService   _tokens;

    public AdminController(SkilloDbContext db, ITokenService tokens)
    {
        _db     = db;
        _tokens = tokens;
    }

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool IsSuperAdmin => User.IsInRole("SuperAdmin");

    // GET /api/admin/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var totalRevenue = await _db.Payments
            .Where(p => p.Status == "Completed")
            .Select(p => p.Amount)
            .DefaultIfEmpty(0)
            .SumAsync();

        return Ok(new AdminStatsResponse(
            TotalUsers:    await _db.Users.CountAsync(u => u.Role != "Admin" && u.Role != "SuperAdmin"),
            Freelancers:   await _db.Users.CountAsync(u => u.Role == "Freelancer"),
            Clients:       await _db.Users.CountAsync(u => u.Role == "Client"),
            TotalProjects: await _db.Projects.CountAsync(),
            OpenProjects:  await _db.Projects.CountAsync(p => p.Status == "Open"),
            TotalBids:     await _db.Bids.CountAsync(),
            TotalServices: await _db.Services.CountAsync(),
            TotalReviews:  await _db.Reviews.CountAsync(),
            TotalPayments: await _db.Payments.CountAsync(),
            TotalRevenue:  totalRevenue,
            BannedUsers:   await _db.Users.CountAsync(u => u.IsBanned)
        ));
    }

    // GET /api/admin/users
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? role)
    {
        var query = _db.Users.AsQueryable();

        // Admin cannot see SuperAdmin accounts
        query = IsSuperAdmin
            ? query.Where(u => true)
            : query.Where(u => u.Role != "SuperAdmin" && u.Role != "Admin");

        if (!string.IsNullOrEmpty(role))   query = query.Where(u => u.Role == role);
        if (!string.IsNullOrEmpty(search)) query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));

        var list = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
        return Ok(list.Select(u => new { u.Id, u.FullName, u.Email, u.Role, u.Avatar, u.IsBanned, u.CreatedAt }));
    }

    // PATCH /api/admin/users/{id}/ban
    [HttpPatch("users/{id:int}/ban")]
    public async Task<IActionResult> BanUser(int id, BanRequest req)
    {
        var target = await _db.Users.FindAsync(id);
        if (target is null) return NotFound();
        if (target.Role == "SuperAdmin") return Forbid();
        if (!IsSuperAdmin && (target.Role == "Admin" || target.Role == "SuperAdmin")) return Forbid();

        target.IsBanned = req.Banned;
        await _db.SaveChangesAsync();
        return Ok(new { message = req.Banned ? "Акаунтът е блокиран." : "Акаунтът е разблокиран." });
    }

    // PATCH /api/admin/freelancers/{userId}/verify
    [HttpPatch("freelancers/{userId:int}/verify")]
    public async Task<IActionResult> VerifyFreelancer(int userId, VerifyRequest req)
    {
        var profile = await _db.FreelancerProfiles.FirstOrDefaultAsync(fp => fp.UserId == userId);
        if (profile is null) return NotFound();
        profile.IsVerified = req.Verified;
        await _db.SaveChangesAsync();
        return Ok(new { message = req.Verified ? "Верифициран." : "Верификацията е премахната." });
    }

    // DELETE /api/admin/users/{id} — SuperAdmin only
    [HttpDelete("users/{id:int}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var target = await _db.Users.FindAsync(id);
        if (target is null) return NotFound();
        if (target.Role == "SuperAdmin") return Forbid();
        _db.Users.Remove(target);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // PATCH /api/admin/users/{id}/role — SuperAdmin only
    [HttpPatch("users/{id:int}/role")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ChangeRole(int id, ChangeRoleRequest req)
    {
        if (!new[] { "Client", "Freelancer", "Admin" }.Contains(req.Role))
            return BadRequest(new { message = "Невалидна роля." });

        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        user.Role = req.Role;

        if (req.Role == "Freelancer" && !await _db.FreelancerProfiles.AnyAsync(fp => fp.UserId == id))
            _db.FreelancerProfiles.Add(new FreelancerProfile { UserId = id });

        await _db.SaveChangesAsync();
        return Ok(new { message = "Ролята е сменена." });
    }

    // POST /api/admin/create-admin — SuperAdmin only
    [HttpPost("create-admin")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateAdmin(CreateAdminRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return Conflict(new { message = "Имейлът вече съществува." });

        var user = new User
        {
            FullName     = req.FullName,
            Email        = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role         = "Admin",
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(new { user.Id, user.FullName, user.Email, user.Role });
    }

    // GET /api/admin/projects
    [HttpGet("projects")]
    public async Task<IActionResult> GetProjects()
    {
        var list = await _db.Projects
            .Include(p => p.Client)
            .Include(p => p.Bids)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(list.Select(p => new
        {
            p.Id, p.ClientId, ClientName = p.Client.FullName,
            p.Title, p.Category, p.Status,
            p.BudgetMin, p.BudgetMax, BidCount = p.Bids.Count, p.CreatedAt,
        }));
    }

    // DELETE /api/admin/projects/{id}
    [HttpDelete("projects/{id:int}")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        var p = await _db.Projects.FindAsync(id);
        if (p is null) return NotFound();
        _db.Projects.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // GET /api/admin/payments
    [HttpGet("payments")]
    public async Task<IActionResult> GetPayments()
    {
        var list = await _db.Payments
            .Include(p => p.Project)
            .Include(p => p.Payer)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return Ok(list.Select(p => new
        {
            p.Id, p.ProjectId, ProjectTitle = p.Project?.Title ?? "",
            p.PayerId, PayerName = p.Payer?.FullName ?? "",
            p.Amount, p.Currency, p.Method, p.Status, p.TransactionId, p.CreatedAt,
        }));
    }

    // GET /api/admin/services
    [HttpGet("services")]
    public async Task<IActionResult> GetServices()
    {
        var list = await _db.Services.Include(s => s.User).OrderByDescending(s => s.CreatedAt).ToListAsync();
        return Ok(list.Select(s => new { s.Id, s.UserId, FreelancerName = s.User.FullName, s.Title, s.Category, s.Price, s.IsActive }));
    }

    // DELETE /api/admin/services/{id}
    [HttpDelete("services/{id:int}")]
    public async Task<IActionResult> DeleteService(int id)
    {
        var s = await _db.Services.FindAsync(id);
        if (s is null) return NotFound();
        _db.Services.Remove(s);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // GET /api/admin/categories
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories() =>
        Ok(await _db.Categories.OrderBy(c => c.Name).ToListAsync());

    // POST /api/admin/categories — Admin + SuperAdmin
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] Category req)
    {
        _db.Categories.Add(new Category { Name = req.Name, Icon = req.Icon });
        await _db.SaveChangesAsync();
        return Ok(new { message = "Категорията е добавена." });
    }

    // DELETE /api/admin/categories/{id} — SuperAdmin only
    [HttpDelete("categories/{id:int}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var c = await _db.Categories.FindAsync(id);
        if (c is null) return NotFound();
        _db.Categories.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
