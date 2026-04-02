using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SkilloPlatform.Controllers;
using SkilloPlatform.Data;
using SkilloPlatform.DTOs;
using SkilloPlatform.Models;
using SkilloPlatform.Services;
using SkilloPlatform.Tests.Helpers;
using Xunit;

namespace SkilloPlatform.Tests;

// ══════════════════════════════════════════════════════════════
//  AUTH TESTS
// ══════════════════════════════════════════════════════════════
public class AuthControllerTests
{
    private AuthController CreateController(SkilloDbContext db)
    {
        var mockTokens = new Mock<ITokenService>();
        mockTokens.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns("test_jwt_token");
        return new AuthController(db, mockTokens.Object);
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsOkWithToken()
    {
        var db  = TestDbHelper.CreateInMemoryDb();
        var req = new RegisterRequest("Иван Иванов", "ivan@test.bg", "Test1234!", "Client");

        var result = await CreateController(db).Register(req) as OkObjectResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var db = TestDbHelper.CreateWithSeedData();
        var req = new RegisterRequest("Дублиран", "client@test.bg", "Test1234!", "Client");

        var result = await CreateController(db).Register(req);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Register_WithInvalidRole_ReturnsBadRequest()
    {
        var db  = TestDbHelper.CreateInMemoryDb();
        var req = new RegisterRequest("Тест", "test@test.bg", "Test1234!", "InvalidRole");

        var result = await CreateController(db).Register(req);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Register_AsFreelancer_CreatesFreelancerProfile()
    {
        var db  = TestDbHelper.CreateInMemoryDb();
        var req = new RegisterRequest("Фрийлансър", "fl@test.bg", "Test1234!", "Freelancer");

        await CreateController(db).Register(req);

        db.FreelancerProfiles.Should().HaveCount(1);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsToken()
    {
        var db  = TestDbHelper.CreateWithSeedData();
        var req = new LoginRequest("client@test.bg", "Test1234!");

        var result = await CreateController(db).Login(req) as OkObjectResult;

        result.Should().NotBeNull();
        result!.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var db  = TestDbHelper.CreateWithSeedData();
        var req = new LoginRequest("client@test.bg", "WrongPassword!");

        var result = await CreateController(db).Login(req);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_WithBannedUser_ReturnsForbid()
    {
        var db  = TestDbHelper.CreateWithSeedData();
        var req = new LoginRequest("banned@test.bg", "Test1234!");

        var result = await CreateController(db).Login(req);

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsUnauthorized()
    {
        var db  = TestDbHelper.CreateInMemoryDb();
        var req = new LoginRequest("nobody@test.bg", "Test1234!");

        var result = await CreateController(db).Login(req);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }
}

// ══════════════════════════════════════════════════════════════
//  PROJECTS TESTS
// ══════════════════════════════════════════════════════════════
public class ProjectsControllerTests
{
    private static ProjectsController CreateController(SkilloDbContext db, int userId = 1, string role = "Client")
    {
        var controller = new ProjectsController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, role),
                }))
            }
        };
        return controller;
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyOpenProjects_ByDefault()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db).GetAll(null, null, null) as OkObjectResult;

        result.Should().NotBeNull();
        var list = result!.Value as IEnumerable<ProjectResponse>;
        list.Should().OnlyContain(p => p.Status == "Open");
    }

    [Fact]
    public async Task GetAll_FilterByCategory_ReturnsCorrectProjects()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db).GetAll("Уеб разработка", null, null) as OkObjectResult;

        var list = result!.Value as IEnumerable<ProjectResponse>;
        list.Should().OnlyContain(p => p.Category.Contains("Уеб разработка"));
    }

    [Fact]
    public async Task GetById_WithValidId_ReturnsProject()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db).GetById(1) as OkObjectResult;

        result.Should().NotBeNull();
        var project = result!.Value as ProjectResponse;
        project!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db).GetById(999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Create_WithValidData_CreatesProject()
    {
        var db  = TestDbHelper.CreateWithSeedData();
        var req = new ProjectRequest("Нов Проект", "Описание", "Уеб разработка", 1000, 3000, 30, new List<string> { "React" });

        var result = await CreateController(db, userId: 1, role: "Client").Create(req);

        result.Should().BeOfType<CreatedAtActionResult>();
        db.Projects.Count().Should().Be(4);
    }

    [Fact]
    public async Task Delete_OwnProject_ReturnsNoContent()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db, userId: 1, role: "Client").Delete(1);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_OtherUsersProject_ReturnsForbid()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db, userId: 2, role: "Client").Delete(1);

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Delete_AsAdmin_CanDeleteAnyProject()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db, userId: 3, role: "Admin").Delete(1);

        result.Should().BeOfType<NoContentResult>();
    }
}

// ══════════════════════════════════════════════════════════════
//  BIDS TESTS
// ══════════════════════════════════════════════════════════════
public class BidsControllerTests
{
    private static BidsController CreateController(SkilloDbContext db, int userId = 2, string role = "Freelancer")
    {
        var controller = new BidsController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, role),
                }))
            }
        };
        return controller;
    }

    [Fact]
    public async Task GetMine_ReturnsOnlyMyBids()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db, userId: 2).GetMine() as OkObjectResult;

        var list = result!.Value as IEnumerable<BidResponse>;
        list.Should().OnlyContain(b => b.FreelancerId == 2);
    }

    [Fact]
    public async Task Create_OnOpenProject_CreatesBid()
    {
        var db  = TestDbHelper.CreateWithSeedData();
        var req = new BidRequest(3, 2500, "Мотивационно писмо", 20);

        var result = await CreateController(db, userId: 2).Create(req);

        result.Should().BeOfType<CreatedAtActionResult>();
    }

    [Fact]
    public async Task Create_DuplicateBid_ReturnsConflict()
    {
        var db  = TestDbHelper.CreateWithSeedData();
        var req = new BidRequest(1, 1500, "Дублирана", 14); // Bid 1 already exists

        var result = await CreateController(db, userId: 2).Create(req);

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_OnClosedProject_ReturnsBadRequest()
    {
        var db  = TestDbHelper.CreateWithSeedData();
        var req = new BidRequest(2, 800, "Писмо", 7); // Project 2 is InProgress

        var result = await CreateController(db, userId: 2).Create(req);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_PendingBid_UpdatesSuccessfully()
    {
        var db  = TestDbHelper.CreateWithSeedData();
        var req = new BidUpdateRequest(2000, "Обновено писмо", 10);

        var result = await CreateController(db, userId: 2).Update(1, req);

        result.Should().BeOfType<OkObjectResult>();
        var bid = db.Bids.Find(1)!;
        bid.Amount.Should().Be(2000);
    }

    [Fact]
    public async Task Update_AcceptedBid_ReturnsBadRequest()
    {
        var db  = TestDbHelper.CreateWithSeedData();
        var req = new BidUpdateRequest(900, "Опит", 5);

        var result = await CreateController(db, userId: 2).Update(2, req); // Bid 2 is Accepted

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_PendingBid_DeletesSuccessfully()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db, userId: 2).Delete(1);

        result.Should().BeOfType<NoContentResult>();
        db.Bids.Find(1).Should().BeNull();
    }

    [Fact]
    public async Task UpdateStatus_AcceptsBid_ChangesProjectStatus()
    {
        var db  = TestDbHelper.CreateWithSeedData();
        var req = new BidStatusRequest("Accepted");

        await CreateController(db, userId: 1, role: "Client").UpdateStatus(1, req);

        var project = db.Projects.Find(1)!;
        project.Status.Should().Be("InProgress");
    }
}

// ══════════════════════════════════════════════════════════════
//  SERVICES TESTS
// ══════════════════════════════════════════════════════════════
public class ServicesControllerTests
{
    private static ServicesController CreateController(SkilloDbContext db, int userId = 2, string role = "Freelancer")
    {
        var controller = new ServicesController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, role),
                }))
            }
        };
        return controller;
    }

    [Fact]
    public async Task GetAll_ReturnsOnlyActiveServices()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db).GetAll(null, null) as OkObjectResult;

        var list = result!.Value as IEnumerable<ServiceResponse>;
        list.Should().OnlyContain(s => s.IsActive);
    }

    [Fact]
    public async Task GetMine_ReturnsAllMyServices_IncludingInactive()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db, userId: 2).GetMine() as OkObjectResult;

        var list = result!.Value as IEnumerable<ServiceResponse>;
        list!.Count().Should().Be(2); // includes inactive
    }

    [Fact]
    public async Task Create_WithValidData_CreatesService()
    {
        var db  = TestDbHelper.CreateWithSeedData();
        var req = new ServiceRequest("Нова Услуга", "Описание", "Уеб разработка", 300, "fixed", 5, 2, true);

        var result = await CreateController(db, userId: 2).Create(req);

        result.Should().BeOfType<CreatedAtActionResult>();
        db.Services.Count(s => s.UserId == 2).Should().Be(3);
    }

    [Fact]
    public async Task Update_OwnService_UpdatesSuccessfully()
    {
        var db  = TestDbHelper.CreateWithSeedData();
        var req = new ServiceRequest("Обновена Услуга", "Ново описание", "Уеб разработка", 600, "fixed", 10, 3, true);

        var result = await CreateController(db, userId: 2).Update(1, req);

        result.Should().BeOfType<OkObjectResult>();
        db.Services.Find(1)!.Title.Should().Be("Обновена Услуга");
    }

    [Fact]
    public async Task Delete_OwnService_DeletesSuccessfully()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db, userId: 2).Delete(1);

        result.Should().BeOfType<NoContentResult>();
        db.Services.Find(1).Should().BeNull();
    }

    [Fact]
    public async Task Delete_OtherUsersService_ReturnsForbid()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db, userId: 99).Delete(1);

        result.Should().BeOfType<ForbidResult>();
    }
}

// ══════════════════════════════════════════════════════════════
//  PAYMENTS TESTS
// ══════════════════════════════════════════════════════════════
public class PaymentsControllerTests
{
    private static PaymentsController CreateController(SkilloDbContext db, int userId = 1, string role = "Client")
    {
        var mockPayments = new Mock<IPaymentService>();

        mockPayments.Setup(p => p.ProcessSimulatedAsync(It.IsAny<int>(), It.IsAny<PaymentRequest>()))
            .ReturnsAsync(new PaymentResponse(10, 1, "Проект", userId, "Клиент", 1000, "BGN", "Simulated", "Completed", "sim_test", "", DateTime.UtcNow));

        mockPayments.Setup(p => p.ProcessStripeAsync(It.IsAny<int>(), It.IsAny<StripePaymentRequest>()))
            .ReturnsAsync(new PaymentResponse(11, 1, "Проект", userId, "Клиент", 1000, "BGN", "Stripe", "Completed", "stripe_test", "", DateTime.UtcNow));

        mockPayments.Setup(p => p.ProcessPayPalAsync(It.IsAny<int>(), It.IsAny<PayPalPaymentRequest>()))
            .ReturnsAsync(new PaymentResponse(12, 1, "Проект", userId, "Клиент", 1000, "BGN", "PayPal", "Completed", "paypal_test", "", DateTime.UtcNow));

        mockPayments.Setup(p => p.RefundAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new PaymentResponse(1, 1, "Проект", userId, "Клиент", 1500, "BGN", "Stripe", "Refunded", "stripe_test_001", "", DateTime.UtcNow));

        var controller = new PaymentsController(db, mockPayments.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, role),
                }))
            }
        };
        return controller;
    }

    [Fact]
    public async Task GetMine_ReturnsMyPayments()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db, userId: 1).GetMine() as OkObjectResult;

        result.Should().NotBeNull();
        var list = result!.Value as IEnumerable<PaymentResponse>;
        list.Should().OnlyContain(p => p.PayerId == 1);
    }

    [Fact]
    public async Task PaySimulated_WithValidProject_ReturnsOk()
    {
        var db  = TestDbHelper.CreateWithSeedData();
        var req = new PaymentRequest(1, 1000, "BGN", "Simulated");

        var result = await CreateController(db, userId: 1).PaySimulated(req) as OkObjectResult;

        result.Should().NotBeNull();
        var payment = result!.Value as PaymentResponse;
        payment!.Method.Should().Be("Simulated");
    }

    [Fact]
    public async Task PayStripe_ReturnsCompletedPayment()
    {
        var db  = TestDbHelper.CreateWithSeedData();
        var req = new StripePaymentRequest(1, 1000, "tok_test_visa");

        var result = await CreateController(db, userId: 1).PayWithStripe(req) as OkObjectResult;

        var payment = result!.Value as PaymentResponse;
        payment!.Method.Should().Be("Stripe");
        payment.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task PayPayPal_ReturnsCompletedPayment()
    {
        var db  = TestDbHelper.CreateWithSeedData();
        var req = new PayPalPaymentRequest(1, 1000, "ORDER_ID_TEST");

        var result = await CreateController(db, userId: 1).PayWithPayPal(req) as OkObjectResult;

        var payment = result!.Value as PaymentResponse;
        payment!.Method.Should().Be("PayPal");
    }

    [Fact]
    public async Task Refund_CompletedPayment_ReturnsRefunded()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db, userId: 1).Refund(1) as OkObjectResult;

        var payment = result!.Value as PaymentResponse;
        payment!.Status.Should().Be("Refunded");
    }
}

// ══════════════════════════════════════════════════════════════
//  ADMIN TESTS
// ══════════════════════════════════════════════════════════════
public class AdminControllerTests
{
    private static AdminController CreateController(SkilloDbContext db, int userId = 3, string role = "Admin")
    {
        var mockTokens = new Mock<ITokenService>();
        mockTokens.Setup(t => t.GenerateToken(It.IsAny<User>())).Returns("test_token");

        var controller = new AdminController(db, mockTokens.Object);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, role),
                }))
            }
        };
        return controller;
    }

    [Fact]
    public async Task GetStats_ReturnsCorrectCounts()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db, role: "SuperAdmin").GetStats() as OkObjectResult;

        result.Should().NotBeNull();
        var stats = result!.Value as AdminStatsResponse;
        stats!.TotalProjects.Should().Be(3);
        stats.OpenProjects.Should().Be(2);
    }

    [Fact]
    public async Task GetUsers_Admin_CannotSeeOtherAdmins()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db, userId: 3, role: "Admin").GetUsers(null, null) as OkObjectResult;

        var list = (result!.Value as IEnumerable<dynamic>)!.ToList();
        list.Should().NotContain(u => ((dynamic)u).Role == "Admin" || ((dynamic)u).Role == "SuperAdmin");
    }

    [Fact]
    public async Task BanUser_AsAdmin_BansClientSuccessfully()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db, userId: 3, role: "Admin").BanUser(1, new BanRequest(true));

        result.Should().BeOfType<OkObjectResult>();
        db.Users.Find(1)!.IsBanned.Should().BeTrue();
    }

    [Fact]
    public async Task BanUser_CannotBanSuperAdmin_ReturnsForbid()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db, userId: 3, role: "Admin").BanUser(4, new BanRequest(true));

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task DeleteUser_AsSuperAdmin_DeletesSuccessfully()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db, userId: 4, role: "SuperAdmin").DeleteUser(1);

        result.Should().BeOfType<NoContentResult>();
        db.Users.Find(1).Should().BeNull();
    }

    [Fact]
    public async Task DeleteUser_CannotDeleteSuperAdmin_ReturnsForbid()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db, userId: 4, role: "SuperAdmin").DeleteUser(4);

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task ChangeRole_ToFreelancer_CreatesProfile()
    {
        var db = TestDbHelper.CreateWithSeedData();

        await CreateController(db, userId: 4, role: "SuperAdmin").ChangeRole(1, new ChangeRoleRequest("Freelancer"));

        db.Users.Find(1)!.Role.Should().Be("Freelancer");
        db.FreelancerProfiles.Should().Contain(fp => fp.UserId == 1);
    }

    [Fact]
    public async Task VerifyFreelancer_SetsVerifiedTrue()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db, role: "Admin").VerifyFreelancer(2, new VerifyRequest(true));

        result.Should().BeOfType<OkObjectResult>();
        db.FreelancerProfiles.First(fp => fp.UserId == 2).IsVerified.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAdmin_AsSuperAdmin_CreatesAdminUser()
    {
        var db  = TestDbHelper.CreateWithSeedData();
        var req = new CreateAdminRequest("Нов Админ", "newadmin@test.bg", "Admin1234!");

        await CreateController(db, userId: 4, role: "SuperAdmin").CreateAdmin(req);

        db.Users.Should().Contain(u => u.Email == "newadmin@test.bg" && u.Role == "Admin");
    }
}

// ══════════════════════════════════════════════════════════════
//  REVIEWS TESTS
// ══════════════════════════════════════════════════════════════
public class ReviewsControllerTests
{
    private static ReviewsController CreateController(SkilloDbContext db, int userId = 1)
    {
        var controller = new ReviewsController(db);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                }))
            }
        };
        return controller;
    }

    [Fact]
    public async Task GetByUser_ReturnsReviewsForUser()
    {
        var db = TestDbHelper.CreateWithSeedData();

        var result = await CreateController(db).GetByUser(2) as OkObjectResult;

        var list = result!.Value as IEnumerable<ReviewResponse>;
        list.Should().OnlyContain(r => r.RevieweeId == 2);
    }

    [Fact]
    public async Task Create_WithValidData_CreatesReview()
    {
        var db  = TestDbHelper.CreateWithSeedData();
        var req = new ReviewRequest(2, 1, 4, "Добра работа");

        var result = await CreateController(db, userId: 1).Create(req);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_DuplicateReview_ReturnsConflict()
    {
        var db  = TestDbHelper.CreateWithSeedData();
        var req = new ReviewRequest(2, 2, 3, "Дублиран"); // Review 1 already exists for reviewer 1, reviewee 2, project 2

        var result = await CreateController(db, userId: 1).Create(req);

        result.Should().BeOfType<ConflictObjectResult>();
    }
}
