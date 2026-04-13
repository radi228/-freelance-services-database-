using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkilloPlatform.Data;
using SkilloPlatform.DTOs;
using SkilloPlatform.Models;
using SkilloPlatform.Services;

namespace SkilloPlatform.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly SkilloDbContext _db;
    private readonly ITokenService   _tokens;

    public AuthController(SkilloDbContext db, ITokenService tokens)
    {
        _db     = db;
        _tokens = tokens;
    }

    // Register endpoint
    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest req)
    {
        if (!new[] { "Client", "Freelancer" }.Contains(req.Role))
            return BadRequest(new { message = "Ð Ð¾Ð»ÑÑ‚Ð° Ñ‚Ñ€ÑÐ±Ð²Ð° Ð´Ð° Ðµ Client Ð¸Ð»Ð¸ Freelancer." });

        if (await _db.Users.AnyAsync(u => u.Email == req.Email))
            return Conflict(new { message = "Ð˜Ð¼ÐµÐ¹Ð»ÑŠÑ‚ Ð²ÐµÑ‡Ðµ Ðµ Ñ€ÐµÐ³Ð¸ÑÑ‚Ñ€Ð¸Ñ€Ð°Ð½." });

        var user = new User
        {
            FullName     = req.FullName,
            Email        = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            Role         = req.Role,
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        if (req.Role == "Freelancer")
        {
            _db.FreelancerProfiles.Add(new FreelancerProfile { UserId = user.Id });
            await _db.SaveChangesAsync();
        }

        var token = _tokens.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Id, user.FullName, user.Email, user.Role, user.Avatar));
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest req)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == req.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Ð“Ñ€ÐµÑˆÐµÐ½ Ð¸Ð¼ÐµÐ¹Ð» Ð¸Ð»Ð¸ Ð¿Ð°Ñ€Ð¾Ð»Ð°." });

        if (user.IsBanned)
            return Forbid();

        var token = _tokens.GenerateToken(user);
        return Ok(new AuthResponse(token, user.Id, user.FullName, user.Email, user.Role, user.Avatar));
    }
}

