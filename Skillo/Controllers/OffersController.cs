using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skillo.Data;
using Skillo.Models;

namespace Skillo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OffersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OffersController> _logger;

        public OffersController(ApplicationDbContext context, ILogger<OffersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/offers - Get all active offers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OfferResponse>>> GetAllOffers()
        {
            try
            {
                var offers = await _context.Offers
                    .Where(o => o.IsActive)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                var response = offers.Select(o => new OfferResponse
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    Title = o.Title,
                    Description = o.Description,
                    Category = o.Category,
                    Price = o.Price,
                    Location = o.Location,
                    ImageUrl = o.ImageUrl,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching offers");
                return StatusCode(500, new { message = "An error occurred while fetching offers" });
            }
        }

        // GET: api/offers/{id} - Get a specific offer
        [HttpGet("{id}")]
        public async Task<ActionResult<OfferResponse>> GetOffer(int id)
        {
            try
            {
                var offer = await _context.Offers.FindAsync(id);

                if (offer == null)
                    return NotFound(new { message = "Offer not found" });

                return Ok(new OfferResponse
                {
                    Id = offer.Id,
                    UserId = offer.UserId,
                    Title = offer.Title,
                    Description = offer.Description,
                    Category = offer.Category,
                    Price = offer.Price,
                    Location = offer.Location,
                    ImageUrl = offer.ImageUrl,
                    CreatedAt = offer.CreatedAt,
                    UpdatedAt = offer.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching offer");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        // GET: api/offers/user/{userId} - Get all offers by a specific user
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<OfferResponse>>> GetUserOffers(int userId)
        {
            try
            {
                var offers = await _context.Offers
                    .Where(o => o.UserId == userId && o.IsActive)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync();

                var response = offers.Select(o => new OfferResponse
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    Title = o.Title,
                    Description = o.Description,
                    Category = o.Category,
                    Price = o.Price,
                    Location = o.Location,
                    ImageUrl = o.ImageUrl,
                    CreatedAt = o.CreatedAt,
                    UpdatedAt = o.UpdatedAt
                }).ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user offers");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        // POST: api/offers - Create a new offer
        [HttpPost]
        public async Task<ActionResult<OfferResponse>> CreateOffer([FromBody] CreateOfferRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Description))
                {
                    return BadRequest(new { message = "Title and description are required" });
                }

                if (request.Price < 0)
                {
                    return BadRequest(new { message = "Price must be greater than or equal to 0" });
                }

                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                {
                    return BadRequest(new { message = "User not found" });
                }

                var offer = new Offer
                {
                    UserId = request.UserId,
                    Title = request.Title.Trim(),
                    Description = request.Description.Trim(),
                    Category = request.Category?.Trim() ?? string.Empty,
                    Price = request.Price,
                    Location = request.Location?.Trim(),
                    ImageUrl = request.ImageUrl?.Trim(),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Offers.Add(offer);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetOffer), new { id = offer.Id }, new OfferResponse
                {
                    Id = offer.Id,
                    UserId = offer.UserId,
                    Title = offer.Title,
                    Description = offer.Description,
                    Category = offer.Category,
                    Price = offer.Price,
                    Location = offer.Location,
                    ImageUrl = offer.ImageUrl,
                    CreatedAt = offer.CreatedAt,
                    UpdatedAt = offer.UpdatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating offer");
                return StatusCode(500, new { message = "An error occurred while creating the offer" });
            }
        }

        // PUT: api/offers/{id} - Update an offer
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOffer(int id, [FromBody] UpdateOfferRequest request)
        {
            try
            {
                var offer = await _context.Offers.FindAsync(id);

                if (offer == null)
                    return NotFound(new { message = "Offer not found" });

                if (offer.UserId != request.UserId)
                    return Forbid();

                if (!string.IsNullOrWhiteSpace(request.Title))
                    offer.Title = request.Title.Trim();

                if (!string.IsNullOrWhiteSpace(request.Description))
                    offer.Description = request.Description.Trim();

                if (!string.IsNullOrWhiteSpace(request.Category))
                    offer.Category = request.Category.Trim();

                if (request.Price.HasValue && request.Price >= 0)
                    offer.Price = request.Price.Value;

                if (request.Location != null)
                    offer.Location = request.Location.Trim();

                if (request.ImageUrl != null)
                    offer.ImageUrl = request.ImageUrl.Trim();

                if (request.IsActive.HasValue)
                    offer.IsActive = request.IsActive.Value;

                offer.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Offer updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating offer");
                return StatusCode(500, new { message = "An error occurred while updating the offer" });
            }
        }

        // DELETE: api/offers/{id} - Delete an offer
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOffer(int id, [FromBody] DeleteOfferRequest request)
        {
            try
            {
                var offer = await _context.Offers.FindAsync(id);

                if (offer == null)
                    return NotFound(new { message = "Offer not found" });

                if (offer.UserId != request.UserId)
                    return Forbid();

                _context.Offers.Remove(offer);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Offer deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting offer");
                return StatusCode(500, new { message = "An error occurred while deleting the offer" });
            }
        }
    }

    // Request/Response Models
    public class CreateOfferRequest
    {
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Category { get; set; }
        public decimal Price { get; set; }
        public string? Location { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class UpdateOfferRequest
    {
        public int UserId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public decimal? Price { get; set; }
        public string? Location { get; set; }
        public string? ImageUrl { get; set; }
        public bool? IsActive { get; set; }
    }

    public class DeleteOfferRequest
    {
        public int UserId { get; set; }
    }

    public class OfferResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Location { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
