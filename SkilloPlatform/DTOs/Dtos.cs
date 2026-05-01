using System.ComponentModel.DataAnnotations;

namespace SkilloPlatform.DTOs;

// â”€â”€ Auth â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public record RegisterRequest(
    [Required] string FullName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] string Role  // Client | Freelancer
);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponse(
    string Token,
    int UserId,
    string FullName,
    string Email,
    string Role,
    string Avatar
);

// â”€â”€ User / Profile â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public record UpdateUserRequest(
    [Required] string FullName
);

public record FreelancerProfileRequest(
    string Title,
    string Bio,
    List<string> Skills,
    string Category,
    decimal HourlyRate,
    string ExperienceLevel,
    string Location,
    string Website,
    string LinkedIn,
    string GitHub,
    List<string> Languages,
    bool IsAvailable
);

public record FreelancerProfileResponse(
    int Id,
    int UserId,
    string FullName,
    string Email,
    string Avatar,
    string Title,
    string Bio,
    List<string> Skills,
    string Category,
    decimal HourlyRate,
    string ExperienceLevel,
    string Location,
    string Website,
    string LinkedIn,
    string GitHub,
    List<string> Languages,
    bool IsVerified,
    bool IsAvailable,
    double AverageRating,
    int ReviewCount
);

// â”€â”€ Work Experience â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public record WorkExperienceRequest(
    [Required] string Company,
    [Required] string Position,
    [Required] string StartDate,
    string? EndDate,
    bool IsCurrent,
    string? Description
);

// â”€â”€ Certificate â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public record CertificateRequest(
    [Required] string Name,
    [Required] string Issuer,
    [Required] string IssueDate,
    string? ExpiryDate,
    string? Credential
);

// â”€â”€ Service â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public record ServiceRequest(
    [Required] string Title,
    string? Description,
    string? Category,
    [Range(0, 1000000)] decimal Price,
    string PriceType,
    [Range(1, 365)] int DeliveryDays,
    [Range(0, 100)] int Revisions,
    bool IsActive
);

public record ServiceResponse(
    int Id,
    int UserId,
    string FreelancerName,
    string Avatar,
    string Title,
    string Description,
    string Category,
    decimal Price,
    string PriceType,
    int DeliveryDays,
    int Revisions,
    bool IsActive,
    DateTime CreatedAt
);

// â”€â”€ Project â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public record ProjectRequest(
    [Required] string Title,
    string? Description,
    [Required] string Category,
    decimal BudgetMin,
    decimal BudgetMax,
    int DeadlineDays,
    List<string>? RequiredSkills
);

public record ProjectResponse(
    int Id,
    int ClientId,
    string ClientName,
    string Title,
    string Description,
    string Category,
    decimal BudgetMin,
    decimal BudgetMax,
    int DeadlineDays,
    List<string> RequiredSkills,
    string Status,
    DateTime CreatedAt,
    int BidCount
);

// â”€â”€ Bid â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public record BidRequest(
    [Required] int ProjectId,
    [Range(1, 10000000)] decimal Amount,
    string? CoverLetter,
    [Range(1, 365)] int DeliveryDays
);

public record BidUpdateRequest(
    [Range(1, 10000000)] decimal Amount,
    string? CoverLetter,
    [Range(1, 365)] int DeliveryDays
);

public record BidStatusRequest(
    [Required] string Status  // Accepted | Rejected
);

public record BidResponse(
    int Id,
    int ProjectId,
    string ProjectTitle,
    int FreelancerId,
    string FreelancerName,
    string FreelancerTitle,
    decimal Amount,
    string CoverLetter,
    int DeliveryDays,
    string Status,
    DateTime CreatedAt
);

// â”€â”€ Payment â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public record PaymentRequest(
    [Required] int ProjectId,
    [Range(1, 10000000)] decimal Amount,
    string Currency,
    string Method  // Stripe | PayPal | Simulated
);

public record PaymentResponse(
    int Id,
    int ProjectId,
    string ProjectTitle,
    int PayerId,
    string PayerName,
    decimal Amount,
    string Currency,
    string Method,
    string Status,
    string TransactionId,
    string Notes,
    DateTime CreatedAt
);

public record StripePaymentRequest(
    [Required] int ProjectId,
    [Range(1, 10000000)] decimal Amount,
    [Required] string StripeToken  // from Stripe.js
);

public record PayPalPaymentRequest(
    [Required] int ProjectId,
    [Range(1, 10000000)] decimal Amount,
    [Required] string OrderId  // from PayPal SDK
);

// â”€â”€ Review â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public record ReviewRequest(
    [Required] int RevieweeId,
    [Required] int ProjectId,
    [Range(1, 5)] int Rating,
    string? Comment
);

public record ReviewResponse(
    int Id,
    int ReviewerId,
    string ReviewerName,
    int RevieweeId,
    int ProjectId,
    int Rating,
    string Comment,
    DateTime CreatedAt
);

// â”€â”€ Admin â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
public record AdminStatsResponse(
    int TotalUsers,
    int Freelancers,
    int Clients,
    int TotalProjects,
    int OpenProjects,
    int TotalBids,
    int TotalServices,
    int TotalReviews,
    int TotalPayments,
    decimal TotalRevenue,
    int BannedUsers
);

public record BanRequest(bool Banned);
public record VerifyRequest(bool Verified);
public record ChangeRoleRequest([Required] string Role);
public record CreateAdminRequest(
    [Required] string FullName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password
);

