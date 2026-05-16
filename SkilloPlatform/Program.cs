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
    var conn = builder.Configuration.GetConnectionString("DefaultConnection");
    if (builder.Environment.IsProduction())
        opt.UseSqlite(conn);
    else
        opt.UseSqlServer(conn);
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
        db.Database.Migrate();
        SkilloDbContext.SeedData(db);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Migration/Seed error: {ex.Message}");
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
