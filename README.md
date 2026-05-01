# Skillo - Web Freelance Platform

**Diploma project** by Radoslav Ivaylov Vodenov | Class 12B
**Specialty:** 4810301 - Applied Programming | **Session:** May-June 2026

## Description
Skillo is a full-featured freelance services web platform connecting clients with freelancers.

## Technologies
- Backend: ASP.NET Core 8 Web API (C#)
- Database: Entity Framework Core 8 + SQLite
- Auth: JWT Bearer + BCrypt
- Payments: Stripe + PayPal + Simulated
- Real-time chat: SignalR + REST polling
- Tests: xUnit + Moq (40+ tests, 65%+ coverage)
- Deploy: Render.com

## Start
```bash
cd SkilloPlatform
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
$env:ASPNETCORE_ENVIRONMENT="Development"; dotnet run
```

## Demo accounts (password: Demo1234!)
- SuperAdmin: superadmin@skillo.bg
- Admin: admin@skillo.bg
- Freelancer: alex@skillo.bg
- Client: client@techstart.bg

## Run tests
```bash
cd SkilloPlatform.Tests
dotnet test
```
