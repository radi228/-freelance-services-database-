using SkilloPlatform.Controllers;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SkilloPlatform.Data;
using SkilloPlatform.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<SkilloDbContext>(opt =>
{
    string dbPath;
    if (builder.Environment.IsProduction())
    {
        // Use /data if it exists (Render persistent disk), otherwise /tmp
        var dataDir = "/data";
        if (Directory.Exists(dataDir))
            dbPath = Path.Combine(dataDir, "skillo.db");
        else
            dbPath = "/tmp/skillo.db";
        opt.UseSqlite($"Data Source={dbPath}");
    }
    else
    {
        var conn = builder.Configuration.GetConnectionString("DefaultConnection");
        opt.UseSqlServer(conn);
    }
});

var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Skillo API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SkilloDbContext>();
    try
    {
        if (app.Environment.IsProduction())
        {
            db.Database.EnsureCreated();
            // Ensure ContactMessages table exists (added later)
            db.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS ""ContactMessages"" (
                ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_ContactMessages"" PRIMARY KEY AUTOINCREMENT,
                ""Name"" TEXT NOT NULL DEFAULT '',
                ""Email"" TEXT NOT NULL DEFAULT '',
                ""Subject"" TEXT NOT NULL DEFAULT '',
                ""Message"" TEXT NOT NULL DEFAULT '',
                ""CreatedAt"" TEXT NOT NULL DEFAULT '0001-01-01T00:00:00',
                ""IsReplied"" INTEGER NOT NULL DEFAULT 0,
                ""ReplyNote"" TEXT NULL
            )");
        }
        else
            db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration error: {ex.Message}");
    }
    try
    {
        SkilloDbContext.SeedData(db);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Seed error: {ex.Message}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Skillo API v1"));
}

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.EndsWith(".html"))
            ctx.Context.Response.Headers["Content-Type"] = "text/html; charset=utf-8";
        else if (ctx.File.Name.EndsWith(".js"))
            ctx.Context.Response.Headers["Content-Type"] = "application/javascript; charset=utf-8";
        else if (ctx.File.Name.EndsWith(".css"))
            ctx.Context.Response.Headers["Content-Type"] = "text/css; charset=utf-8";
    }
});

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }
