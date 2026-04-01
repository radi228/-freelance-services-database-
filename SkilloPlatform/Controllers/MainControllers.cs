using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkilloPlatform.Data;
using SkilloPlatform.DTOs;
using SkilloPlatform.Models;

namespace SkilloPlatform.Controllers;

// ══════════════════════════════════════════════════════════════
//  PROJECTS
// ══════════════════════════════════════════════════════════════
[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly SkilloDbContext _db;
    public ProjectsController(SkilloDbContext db) => _db = db;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? category,
        [FromQuery] string? status,
        [FromQuery] string? search)
    {
        var query = _db.Projects
            .Include(p => p.Client)
            .Include(p => p.Bids)
            .AsQueryable();

        query = string.IsNullOrEmpty(status)
            ? query.Where(p => p.Status == "Open")
            : query.Where(p => p.Status == status);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.Category.Contains(category));

        if (!string.IsNullOrEmpty(search))
            query = query.Where(p =>
                p.Title.Contains(search) || p.Description.Contains(search));

        var list = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        return Ok(list.Select(Map));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _db.Projects
            .Include(p => p.Client)
            .Include(p => p.Bids)
            .FirstOrDefaultAsync(p => p.Id == id);
        return p is null ? NotFound() : Ok(Map(p));
    }

    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> Create(ProjectRequest req)
    {
        var project = new Project
        {
            ClientId      = UserId,
            Title         = req.Title,
            Description   = req.Description ?? "",
            Category      = req.Category,
            BudgetMin     = req.BudgetMin,
            BudgetMax     = req.BudgetMax,
            DeadlineDays  = req.DeadlineDays > 0 ? req.DeadlineDays : 30,
            RequiredSkills= string.Join(",", req.RequiredSkills ?? new List<string>()),
        };
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        var full = await _db.Projects.Include(p => p.Client).Include(p => p.Bids).FirstAsync(p => p.Id == project.Id);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, Map(full));
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await _db.Projects.FindAsync(id);
        if (p is null) return NotFound();

        var role = User.FindFirstValue(ClaimTypes.Role)!;
        if (p.ClientId != UserId && role != "Admin" && role != "SuperAdmin")
            return Forbid();

        _db.Projects.Remove(p);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static ProjectResponse Map(Project p) => new(
        p.Id, p.ClientId, p.Client?.FullName ?? "",
        p.Title, p.Description, p.Category,
        p.BudgetMin, p.BudgetMax, p.DeadlineDays,
        p.RequiredSkills.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
        p.Status, p.CreatedAt, p.Bids?.Count ?? 0
    );
}

// ══════════════════════════════════════════════════════════════
//  BIDS
// ══════════════════════════════════════════════════════════════
[ApiController]
[Route("api/bids")]
public class BidsController : ControllerBase
{
    private readonly SkilloDbContext _db;
    public BidsController(SkilloDbContext db) => _db = db;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("mine")]
    [Authorize(Roles = "Freelancer")]
    public async Task<IActionResult> GetMine()
    {
        var bids = await _db.Bids
            .Include(b => b.Project)
            .Include(b => b.Freelancer).ThenInclude(u => u.FreelancerProfile)
            .Where(b => b.FreelancerId == UserId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
        return Ok(bids.Select(Map));
    }

    [HttpGet("project/{projectId:int}")]
    [Authorize]
    public async Task<IActionResult> GetByProject(int projectId)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project is null) return NotFound();

        var role = User.FindFirstValue(ClaimTypes.Role)!;
        if (project.ClientId != UserId && role != "Admin" && role != "SuperAdmin")
            return Forbid();

        var bids = await _db.Bids
            .Include(b => b.Project)
            .Include(b => b.Freelancer).ThenInclude(u => u.FreelancerProfile)
            .Where(b => b.ProjectId == projectId)
            .ToListAsync();
        return Ok(bids.Select(Map));
    }

    [HttpPost]
    [Authorize(Roles = "Freelancer")]
    public async Task<IActionResult> Create(BidRequest req)
    {
        var project = await _db.Projects.FindAsync(req.ProjectId);
        if (project is null) return NotFound(new { message = "Проектът не е намерен." });
        if (project.Status != "Open") return BadRequest(new { message = "Проектът не приема оферти." });

        if (await _db.Bids.AnyAsync(b => b.ProjectId == req.ProjectId && b.FreelancerId == UserId))
            return Conflict(new { message = "Вече си подал оферта за този проект." });

        var bid = new Bid
        {
            ProjectId    = req.ProjectId,
            FreelancerId = UserId,
            Amount       = req.Amount,
            CoverLetter  = req.CoverLetter ?? "",
            DeliveryDays = req.DeliveryDays,
        };
        _db.Bids.Add(bid);
        await _db.SaveChangesAsync();

        var full = await _db.Bids
            .Include(b => b.Project)
            .Include(b => b.Freelancer).ThenInclude(u => u.FreelancerProfile)
            .FirstAsync(b => b.Id == bid.Id);
        return CreatedAtAction(nameof(GetMine), Map(full));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Freelancer")]
    public async Task<IActionResult> Update(int id, BidUpdateRequest req)
    {
        var bid = await _db.Bids.FindAsync(id);
        if (bid is null) return NotFound();
        if (bid.FreelancerId != UserId) return Forbid();
        if (bid.Status != "Pending") return BadRequest(new { message = "Може да редактираш само чакащи оферти." });

        bid.Amount       = req.Amount;
        bid.CoverLetter  = req.CoverLetter ?? bid.CoverLetter;
        bid.DeliveryDays = req.DeliveryDays;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Офертата е обновена." });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Freelancer")]
    public async Task<IActionResult> Delete(int id)
    {
        var bid = await _db.Bids.FindAsync(id);
        if (bid is null) return NotFound();
        if (bid.FreelancerId != UserId) return Forbid();
        if (bid.Status != "Pending") return BadRequest(new { message = "Може да изтриеш само чакащи оферти." });

        _db.Bids.Remove(bid);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Client")]
    public async Task<IActionResult> UpdateStatus(int id, BidStatusRequest req)
    {
        if (!new[] { "Accepted", "Rejected" }.Contains(req.Status))
            return BadRequest(new { message = "Невалиден статус." });

        var bid = await _db.Bids.Include(b => b.Project).FirstOrDefaultAsync(b => b.Id == id);
        if (bid is null) return NotFound();
        if (bid.Project.ClientId != UserId) return Forbid();

        bid.Status = req.Status;
        if (req.Status == "Accepted")
            bid.Project.Status = "InProgress";

        await _db.SaveChangesAsync();
        return Ok(new { message = "Статусът е обновен." });
    }

    private static BidResponse Map(Bid b) => new(
        b.Id, b.ProjectId, b.Project?.Title ?? "",
        b.FreelancerId, b.Freelancer?.FullName ?? "",
        b.Freelancer?.FreelancerProfile?.Title ?? "",
        b.Amount, b.CoverLetter, b.DeliveryDays, b.Status, b.CreatedAt
    );
}

// ══════════════════════════════════════════════════════════════
//  REVIEWS
// ══════════════════════════════════════════════════════════════
[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly SkilloDbContext _db;
    public ReviewsController(SkilloDbContext db) => _db = db;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId)
    {
        var reviews = await _db.Reviews
            .Include(r => r.Reviewer)
            .Where(r => r.RevieweeId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return Ok(reviews.Select(r => new ReviewResponse(
            r.Id, r.ReviewerId, r.Reviewer.FullName,
            r.RevieweeId, r.ProjectId, r.Rating, r.Comment, r.CreatedAt
        )));
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(ReviewRequest req)
    {
        if (await _db.Reviews.AnyAsync(r =>
                r.ReviewerId == UserId &&
                r.RevieweeId == req.RevieweeId &&
                r.ProjectId  == req.ProjectId))
            return Conflict(new { message = "Вече си оставил отзив." });

        _db.Reviews.Add(new Review
        {
            ReviewerId = UserId,
            RevieweeId = req.RevieweeId,
            ProjectId  = req.ProjectId,
            Rating     = req.Rating,
            Comment    = req.Comment ?? "",
        });
        await _db.SaveChangesAsync();
        return Ok(new { message = "Отзивът е добавен." });
    }
}

// ══════════════════════════════════════════════════════════════
//  SERVICES
// ══════════════════════════════════════════════════════════════
[ApiController]
[Route("api/services")]
public class ServicesController : ControllerBase
{
    private readonly SkilloDbContext _db;
    public ServicesController(SkilloDbContext db) => _db = db;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? category, [FromQuery] string? search)
    {
        var query = _db.Services.Include(s => s.User)
            .Where(s => s.IsActive && !s.User.IsBanned);
        if (!string.IsNullOrEmpty(category)) query = query.Where(s => s.Category == category);
        if (!string.IsNullOrEmpty(search))   query = query.Where(s => s.Title.Contains(search) || s.Description.Contains(search));
        var list = await query.OrderByDescending(s => s.CreatedAt).ToListAsync();
        return Ok(list.Select(Map));
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Freelancer")]
    public async Task<IActionResult> GetMine()
    {
        var list = await _db.Services.Include(s => s.User)
            .Where(s => s.UserId == UserId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
        return Ok(list.Select(Map));
    }

    [HttpPost]
    [Authorize(Roles = "Freelancer")]
    public async Task<IActionResult> Create(ServiceRequest req)
    {
        var svc = new Service
        {
            UserId      = UserId,
            Title       = req.Title,
            Description = req.Description ?? "",
            Category    = req.Category ?? "",
            Price       = req.Price,
            PriceType   = req.PriceType ?? "fixed",
            DeliveryDays= req.DeliveryDays,
            Revisions   = req.Revisions,
            IsActive    = req.IsActive,
        };
        _db.Services.Add(svc);
        await _db.SaveChangesAsync();
        var full = await _db.Services.Include(s => s.User).FirstAsync(s => s.Id == svc.Id);
        return CreatedAtAction(nameof(GetMine), Map(full));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Freelancer")]
    public async Task<IActionResult> Update(int id, ServiceRequest req)
    {
        var svc = await _db.Services.FindAsync(id);
        if (svc is null || svc.UserId != UserId) return Forbid();
        svc.Title = req.Title; svc.Description = req.Description ?? "";
        svc.Category = req.Category ?? ""; svc.Price = req.Price;
        svc.PriceType = req.PriceType ?? "fixed"; svc.DeliveryDays = req.DeliveryDays;
        svc.Revisions = req.Revisions; svc.IsActive = req.IsActive;
        await _db.SaveChangesAsync();
        return Ok(new { message = "Услугата е обновена." });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Freelancer")]
    public async Task<IActionResult> Delete(int id)
    {
        var svc = await _db.Services.FindAsync(id);
        if (svc is null || svc.UserId != UserId) return Forbid();
        _db.Services.Remove(svc);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static ServiceResponse Map(Service s) => new(
        s.Id, s.UserId, s.User?.FullName ?? "", s.User?.Avatar ?? "",
        s.Title, s.Description, s.Category,
        s.Price, s.PriceType, s.DeliveryDays, s.Revisions, s.IsActive, s.CreatedAt
    );
}

// ══════════════════════════════════════════════════════════════
//  WORK EXPERIENCE
// ══════════════════════════════════════════════════════════════
[ApiController]
[Route("api/experience")]
[Authorize]
public class ExperienceController : ControllerBase
{
    private readonly SkilloDbContext _db;
    public ExperienceController(SkilloDbContext db) => _db = db;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var profile = await _db.FreelancerProfiles.FirstOrDefaultAsync(fp => fp.UserId == UserId);
        if (profile is null) return NotFound();
        var list = await _db.WorkExperiences.Where(w => w.FreelancerProfileId == profile.Id).OrderByDescending(w => w.StartDate).ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<IActionResult> Create(WorkExperienceRequest req)
    {
        var profile = await _db.FreelancerProfiles.FirstOrDefaultAsync(fp => fp.UserId == UserId);
        if (profile is null) return NotFound();
        var item = new WorkExperience { FreelancerProfileId = profile.Id, Company = req.Company, Position = req.Position, StartDate = req.StartDate, EndDate = req.EndDate ?? "", IsCurrent = req.IsCurrent, Description = req.Description ?? "" };
        _db.WorkExperiences.Add(item);
        await _db.SaveChangesAsync();
        return Ok(item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, WorkExperienceRequest req)
    {
        var item    = await _db.WorkExperiences.Include(w => w.FreelancerProfile).FirstOrDefaultAsync(w => w.Id == id);
        if (item is null || item.FreelancerProfile.UserId != UserId) return Forbid();
        item.Company = req.Company; item.Position = req.Position;
        item.StartDate = req.StartDate; item.EndDate = req.EndDate ?? "";
        item.IsCurrent = req.IsCurrent; item.Description = req.Description ?? "";
        await _db.SaveChangesAsync();
        return Ok(new { message = "Обновено." });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.WorkExperiences.Include(w => w.FreelancerProfile).FirstOrDefaultAsync(w => w.Id == id);
        if (item is null || item.FreelancerProfile.UserId != UserId) return Forbid();
        _db.WorkExperiences.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

// ══════════════════════════════════════════════════════════════
//  CERTIFICATES
// ══════════════════════════════════════════════════════════════
[ApiController]
[Route("api/certificates")]
[Authorize]
public class CertificatesController : ControllerBase
{
    private readonly SkilloDbContext _db;
    public CertificatesController(SkilloDbContext db) => _db = db;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetMine()
    {
        var profile = await _db.FreelancerProfiles.FirstOrDefaultAsync(fp => fp.UserId == UserId);
        if (profile is null) return NotFound();
        return Ok(await _db.Certificates.Where(c => c.FreelancerProfileId == profile.Id).OrderByDescending(c => c.IssueDate).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CertificateRequest req, IFormFile? file)
    {
        var profile = await _db.FreelancerProfiles.FirstOrDefaultAsync(fp => fp.UserId == UserId);
        if (profile is null) return NotFound();

        string fileUrl = "";
        if (file is { Length: > 0 })
        {
            var dir = Path.Combine("wwwroot", "uploads");
            Directory.CreateDirectory(dir);
            var fname = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            using var s = System.IO.File.Create(Path.Combine(dir, fname));
            await file.CopyToAsync(s);
            fileUrl = $"/uploads/{fname}";
        }

        var cert = new Certificate { FreelancerProfileId = profile.Id, Name = req.Name, Issuer = req.Issuer, IssueDate = req.IssueDate, ExpiryDate = req.ExpiryDate ?? "", Credential = req.Credential ?? "", FileUrl = fileUrl };
        _db.Certificates.Add(cert);
        await _db.SaveChangesAsync();
        return Ok(cert);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cert = await _db.Certificates.Include(c => c.FreelancerProfile).FirstOrDefaultAsync(c => c.Id == id);
        if (cert is null || cert.FreelancerProfile.UserId != UserId) return Forbid();
        _db.Certificates.Remove(cert);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
