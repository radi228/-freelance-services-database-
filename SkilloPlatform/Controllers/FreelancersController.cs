using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkilloPlatform.Data;
using SkilloPlatform.DTOs;
using SkilloPlatform.Models;

namespace SkilloPlatform.Controllers;

[ApiController]
[Route("api/freelancers")]
public class FreelancersController : ControllerBase
{
    private readonly SkilloDbContext _db;

    // Dependency injection
    public FreelancersController(SkilloDbContext db) => _db = db;

    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /api/freelancers
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? category,
        [FromQuery] string? skill,
        [FromQuery] string? search)
    {
        var query = _db.FreelancerProfiles
            .Include(fp => fp.User)
            .Where(fp => fp.IsAvailable && !fp.User.IsBanned);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(fp => fp.Category == category);

        if (!string.IsNullOrEmpty(skill))
            query = query.Where(fp => fp.Skills.Contains(skill));

        if (!string.IsNullOrEmpty(search))
            query = query.Where(fp =>
                fp.Title.Contains(search) ||
                fp.Bio.Contains(search)   ||
                fp.Skills.Contains(search)||
                fp.User.FullName.Contains(search));

        var profiles = await query.ToListAsync();
        return Ok(profiles.Select(p => MapProfile(p)));
    }

    // GET /api/freelancers/me/full
    [HttpGet("me/full")]
    [Authorize(Roles = "Freelancer")]
    public async Task<IActionResult> GetMyFull()
    {
        var profile = await _db.FreelancerProfiles
            .Include(fp => fp.User)
            .Include(fp => fp.WorkExperiences)
            .Include(fp => fp.Certificates)
            .FirstOrDefaultAsync(fp => fp.UserId == UserId);

        if (profile is null) return NotFound(new { message = "ÐŸÑ€Ð¾Ñ„Ð¸Ð»ÑŠÑ‚ Ð½Ðµ Ðµ Ð½Ð°Ð¼ÐµÑ€ÐµÐ½." });

        var services = await _db.Services.Where(s => s.UserId == UserId && s.IsActive).ToListAsync();
        var reviews  = await GetReviews(UserId);

        var prof = MapProfile(profile);
        return Ok(new
        {
            // Profile fields (flat)
            prof.Id, prof.UserId, prof.FullName, prof.Email, prof.Avatar,
            prof.Title, prof.Bio, prof.Skills, prof.Category,
            prof.HourlyRate, prof.ExperienceLevel, prof.Location,
            prof.Website, prof.LinkedIn, prof.GitHub, prof.Languages,
            prof.IsVerified, prof.IsAvailable,
            prof.AverageRating, prof.ReviewCount,
            // Arrays
            WorkExperiences = profile.WorkExperiences.Select(e => new {
                e.Id, e.Company, e.Position, e.StartDate, e.EndDate, e.IsCurrent, e.Description
            }),
            Certificates = profile.Certificates.Select(cert => new {
                cert.Id, cert.Name, cert.Issuer, cert.IssueDate,
                cert.ExpiryDate, cert.Credential, cert.FileUrl
            }),
            Services = services.Select(s => new {
                s.Id, s.UserId, s.Title, s.Description, s.Category,
                s.Price, s.PriceType, s.DeliveryDays, s.Revisions, s.IsActive
            }),
            Reviews = reviews,
        });
    }

    // GET /api/freelancers/{userId}
    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetById(int userId)
    {
        var profile = await _db.FreelancerProfiles
            .Include(fp => fp.User)
            .Include(fp => fp.WorkExperiences)
            .Include(fp => fp.Certificates)
            .FirstOrDefaultAsync(fp => fp.UserId == userId);

        if (profile is null) return NotFound(new { message = "ÐÐµ Ðµ Ð½Ð°Ð¼ÐµÑ€ÐµÐ½." });

        var services = await _db.Services.Where(s => s.UserId == userId && s.IsActive).ToListAsync();
        var reviews  = await GetReviews(userId);

        var prof = MapProfile(profile);
        return Ok(new
        {
            // Profile fields (flat)
            prof.Id, prof.UserId, prof.FullName, prof.Email, prof.Avatar,
            prof.Title, prof.Bio, prof.Skills, prof.Category,
            prof.HourlyRate, prof.ExperienceLevel, prof.Location,
            prof.Website, prof.LinkedIn, prof.GitHub, prof.Languages,
            prof.IsVerified, prof.IsAvailable,
            prof.AverageRating, prof.ReviewCount,
            // Arrays
            WorkExperiences = profile.WorkExperiences.Select(e => new {
                e.Id, e.Company, e.Position, e.StartDate, e.EndDate, e.IsCurrent, e.Description
            }),
            Certificates = profile.Certificates.Select(cert => new {
                cert.Id, cert.Name, cert.Issuer, cert.IssueDate,
                cert.ExpiryDate, cert.Credential, cert.FileUrl
            }),
            Services = services.Select(s => new {
                s.Id, s.UserId, s.Title, s.Description, s.Category,
                s.Price, s.PriceType, s.DeliveryDays, s.Revisions, s.IsActive
            }),
            Reviews = reviews,
        });
    }

    // PUT /api/freelancers/me
    [HttpPut("me")]
    [Authorize(Roles = "Freelancer")]
    public async Task<IActionResult> UpdateMe(FreelancerProfileRequest req)
    {
        var profile = await _db.FreelancerProfiles.FirstOrDefaultAsync(fp => fp.UserId == UserId);
        if (profile is null) return NotFound();

        profile.Title           = req.Title;
        profile.Bio             = req.Bio;
        profile.Skills          = string.Join(",", req.Skills ?? new List<string>());
        profile.Category        = req.Category;
        profile.HourlyRate      = req.HourlyRate;
        profile.ExperienceLevel = req.ExperienceLevel;
        profile.Location        = req.Location;
        profile.Website         = req.Website;
        profile.LinkedIn        = req.LinkedIn;
        profile.GitHub          = req.GitHub;
        profile.Languages       = string.Join(",", req.Languages ?? new List<string>());
        profile.IsAvailable     = req.IsAvailable;
        profile.UpdatedAt       = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { message = "ÐŸÑ€Ð¾Ñ„Ð¸Ð»ÑŠÑ‚ Ðµ Ð¾Ð±Ð½Ð¾Ð²ÐµÐ½." });
    }

    // POST /api/freelancers/avatar
    [HttpPost("avatar")]
    [Authorize]
    public async Task<IActionResult> UploadAvatar(IFormFile avatar)
    {
        if (avatar is null || avatar.Length == 0)
            return BadRequest(new { message = "ÐÐµ Ðµ ÐºÐ°Ñ‡ÐµÐ½ Ñ„Ð°Ð¹Ð»." });

        if (avatar.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Ð¤Ð°Ð¹Ð»ÑŠÑ‚ Ðµ Ð¿Ñ€ÐµÐºÐ°Ð»ÐµÐ½Ð¾ Ð³Ð¾Ð»ÑÐ¼ (Ð¼Ð°ÐºÑ. 5MB)." });

        var uploadsDir = Path.Combine("wwwroot", "uploads");
        Directory.CreateDirectory(uploadsDir);

        var ext      = Path.GetExtension(avatar.FileName);
        var filename = $"{Guid.NewGuid():N}{ext}";
        var filepath = Path.Combine(uploadsDir, filename);

        using var stream = System.IO.File.Create(filepath);
        await avatar.CopyToAsync(stream);

        var url  = $"/uploads/{filename}";
        var user = await _db.Users.FindAsync(UserId)!;
        user!.Avatar = url;
        await _db.SaveChangesAsync();

        return Ok(new { avatar = url });
    }

    // â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private async Task<List<object>> GetReviews(int userId)
    {
        var reviews = await _db.Reviews
            .Include(r => r.Reviewer)
            .Where(r => r.RevieweeId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return reviews.Select(r => (object)new
        {
            r.Id, r.ReviewerId,
            ReviewerName = r.Reviewer.FullName,
            r.RevieweeId, r.ProjectId,
            r.Rating, r.Comment, r.CreatedAt,
        }).ToList();
    }

    private FreelancerProfileResponse MapProfile(FreelancerProfile p)
    {
        var reviews    = _db.Reviews.Where(r => r.RevieweeId == p.UserId).ToList();
        var avgRating  = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;

        return new FreelancerProfileResponse(
            p.Id, p.UserId, p.User.FullName, p.User.Email, p.User.Avatar,
            p.Title, p.Bio,
            p.Skills.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
            p.Category, p.HourlyRate, p.ExperienceLevel,
            p.Location, p.Website, p.LinkedIn, p.GitHub,
            p.Languages.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
            p.IsVerified, p.IsAvailable,
            Math.Round(avgRating, 1), reviews.Count
        );
    }
}

