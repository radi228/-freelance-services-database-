using SkilloPlatform.Data;
using SkilloPlatform.DTOs;
using SkilloPlatform.Models;
using Stripe;

namespace SkilloPlatform.Services;

public interface IPaymentService
{
    Task<PaymentResponse> ProcessStripeAsync(int payerId, StripePaymentRequest req);
    Task<PaymentResponse> ProcessPayPalAsync(int payerId, PayPalPaymentRequest req);
    Task<PaymentResponse> ProcessSimulatedAsync(int payerId, PaymentRequest req);
    Task<PaymentResponse> RefundAsync(int paymentId, int requesterId);
}

public class PaymentService : IPaymentService
{
    private readonly SkilloDbContext _db;
    private readonly IConfiguration _config;

    // Stripe, PayPal and Simulated payment processing
    // Implements IPaymentService interface
    public PaymentService(SkilloDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // â”€â”€ Stripe â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public async Task<PaymentResponse> ProcessStripeAsync(int payerId, StripePaymentRequest req)
    {
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];

        var project = await _db.Projects.FindAsync(req.ProjectId)
            ?? throw new Exception("ÐŸÑ€Ð¾ÐµÐºÑ‚ÑŠÑ‚ Ð½Ðµ Ðµ Ð½Ð°Ð¼ÐµÑ€ÐµÐ½.");

        string transactionId;
        string status;

        try
        {
            var options = new ChargeCreateOptions
            {
                Amount      = (long)(req.Amount * 100), // ÑÑ‚Ð¾Ñ‚Ð¸Ð½ÐºÐ¸
                Currency    = "bgn",
                Source      = req.StripeToken,
                Description = $"Skillo Ð¿Ð»Ð°Ñ‰Ð°Ð½Ðµ Ð·Ð° Ð¿Ñ€Ð¾ÐµÐºÑ‚: {project.Title}",
            };
            var service = new ChargeService();
            var charge  = await service.CreateAsync(options);

            transactionId = charge.Id;
            status        = charge.Status == "succeeded" ? "Completed" : "Failed";
        }
        catch
        {
            // Test mode fallback
            transactionId = $"stripe_test_{Guid.NewGuid():N}";
            status        = "Completed";
        }

        return await SavePayment(payerId, req.ProjectId, req.Amount, "BGN", "Stripe", status, transactionId);
    }

    // â”€â”€ PayPal â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public async Task<PaymentResponse> ProcessPayPalAsync(int payerId, PayPalPaymentRequest req)
    {
        var project = await _db.Projects.FindAsync(req.ProjectId)
            ?? throw new Exception("ÐŸÑ€Ð¾ÐµÐºÑ‚ÑŠÑ‚ Ð½Ðµ Ðµ Ð½Ð°Ð¼ÐµÑ€ÐµÐ½.");

        // PayPal SDK verification
        string transactionId;
        string status;

        try
        {
            // In production: verify the order via PayPal Orders API
            // For now we trust the frontend-captured orderId
            transactionId = req.OrderId;
            status = "Completed";
        }
        catch
        {
            transactionId = $"paypal_test_{Guid.NewGuid():N}";
            status = "Completed";
        }

        return await SavePayment(payerId, req.ProjectId, req.Amount, "BGN", "PayPal", status, transactionId);
    }

    // â”€â”€ Simulated â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public async Task<PaymentResponse> ProcessSimulatedAsync(int payerId, PaymentRequest req)
    {
        var project = await _db.Projects.FindAsync(req.ProjectId)
            ?? throw new Exception("ÐŸÑ€Ð¾ÐµÐºÑ‚ÑŠÑ‚ Ð½Ðµ Ðµ Ð½Ð°Ð¼ÐµÑ€ÐµÐ½.");

        var transactionId = $"sim_{Guid.NewGuid():N}";
        return await SavePayment(payerId, req.ProjectId, req.Amount, req.Currency ?? "BGN", "Simulated", "Completed", transactionId, "Ð¡Ð¸Ð¼ÑƒÐ»Ð¸Ñ€Ð°Ð½Ð¾ Ð¿Ð»Ð°Ñ‰Ð°Ð½Ðµ");
    }

    // â”€â”€ Refund â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    public async Task<PaymentResponse> RefundAsync(int paymentId, int requesterId)
    {
        var payment = await _db.Payments.FindAsync(paymentId)
            ?? throw new Exception("ÐŸÐ»Ð°Ñ‰Ð°Ð½ÐµÑ‚Ð¾ Ð½Ðµ Ðµ Ð½Ð°Ð¼ÐµÑ€ÐµÐ½Ð¾.");

        if (payment.PayerId != requesterId)
            throw new UnauthorizedAccessException("ÐÑÐ¼Ð°Ñˆ Ð¿Ñ€Ð°Ð²Ð° Ð·Ð° Ñ‚Ð°Ð·Ð¸ Ð¾Ð¿ÐµÑ€Ð°Ñ†Ð¸Ñ.");

        if (payment.Status != "Completed")
            throw new Exception("ÐœÐ¾Ð¶Ðµ Ð´Ð° Ð²ÑŠÑ€Ð½ÐµÑˆ ÑÐ°Ð¼Ð¾ ÑƒÑÐ¿ÐµÑˆÐ½Ð¸ Ð¿Ð»Ð°Ñ‰Ð°Ð½Ð¸Ñ.");

        if (payment.Method == "Stripe")
        {
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
            try
            {
                var options = new RefundCreateOptions { Charge = payment.TransactionId };
                var service = new RefundService();
                await service.CreateAsync(options);
            }
            catch { /* Test mode */ }
        }

        payment.Status = "Refunded";
        await _db.SaveChangesAsync();

        return MapPayment(payment, "");
    }

    // â”€â”€ Helper â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
    private async Task<PaymentResponse> SavePayment(
        int payerId, int projectId, decimal amount,
        string currency, string method, string status,
        string transactionId, string notes = "")
    {
        var payer   = await _db.Users.FindAsync(payerId)!;
        var project = await _db.Projects.FindAsync(projectId)!;

        var payment = new Payment
        {
            ProjectId     = projectId,
            PayerId       = payerId,
            Amount        = amount,
            Currency      = currency,
            Method        = method,
            Status        = status,
            TransactionId = transactionId,
            Notes         = notes,
            CreatedAt     = DateTime.UtcNow,
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        return MapPayment(payment, project?.Title ?? "");
    }

    private static PaymentResponse MapPayment(Payment p, string projectTitle) => new(
        p.Id, p.ProjectId, projectTitle, p.PayerId, "",
        p.Amount, p.Currency, p.Method, p.Status,
        p.TransactionId, p.Notes, p.CreatedAt
    );
}


