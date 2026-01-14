using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Skillo.Data;
using Skillo.Models;

namespace Skillo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.Username))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Username, email and password are required"
                    });
                }

                if (request.Username.Length < 3)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Username must be at least 3 characters"
                    });
                }

                if (request.Password != request.ConfirmPassword)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Passwords do not match"
                    });
                }

                if (request.Password.Length < 8)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Password must be at least 8 characters long"
                    });
                }

                // Require at least one uppercase letter and one number
                if (!Regex.IsMatch(request.Password, "[A-Z]") || !Regex.IsMatch(request.Password, "\\d"))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Password must contain at least one uppercase letter and one number"
                    });
                }

                // Check if user already exists with this email
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                // Check if username is already taken
                var existingUsername = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == request.Username);
                
                if (existingUsername != null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Username already exists"
                    });
                }

                // Create new user
                var user = new User
                {
                    Email = request.Email,
                    PasswordHash = HashPassword(request.Password),
                    Username = request.Username,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Return response without password
                var responseUser = new UserLoginResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    AccountType = "offering",  // Default account type (determined at login)
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Registration successful",
                    User = responseUser
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during registration"
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Message = "Username and password are required"
                    });
                }

                // Find user by username
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

                if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    });
                }

                if (!user.IsActive)
                {
                    return Unauthorized(new AuthResponse
                    {
                        Success = false,
                        Message = "Account is inactive"
                    });
                }

                // Determine which account to log into based on the request
                string accountType = request.AccountType ?? "offering";  // Default to offering if not specified

                // Return response with username and account type
                var responseUser = new UserLoginResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    AccountType = accountType,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ProfileImage = user.ProfileImage,
                    Bio = user.Bio
                };

                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Login successful",
                    User = responseUser
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Message = "An error occurred during login"
                });
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }
    }
}
