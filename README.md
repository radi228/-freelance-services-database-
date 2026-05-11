# Skillo - Web Freelance Platform v1.2

**Diploma project** by Radoslav Ivaylov Vodenov | Class 12B
**Specialty:** 4810301 - Applied Programming | **Session:** May-June 2026

## Technologies
- Backend: ASP.NET Core 8 Web API
- Database: EF Core 8 + SQL Server LocalDB
- Auth: JWT Bearer + BCrypt
- Payments: Stripe + PayPal + Simulated
- Chat: SignalR + REST + file upload
- Tests: xUnit + Moq (40+ tests)
- Deploy: Render.com

## Start
```bash
cd SkilloPlatform
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
$env:ASPNETCORE_ENVIRONMENT="Development"; dotnet run
```
Demo Acccounts:
Freelancer: alex@skillo.bg|Demo1234!
User: client@techstart.bg|Demo1234!
Admin : admin@skillo.bg|Demo1234!
Super Admin: superadmin@skillo.bg|Demo1234!
