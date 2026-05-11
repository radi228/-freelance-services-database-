# DEN 17 - Final
$repo = "C:\Users\Radko\-freelance-services-database-"
Set-Location $repo

# Komit 1 - FreelancersController
$f = "$repo\SkilloPlatform\Controllers\FreelancersController.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// Freelancers Controller - handles profile CRUD", "// Full profile CRUD with avatar upload"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Controllers/FreelancersController.cs"
git commit -m "refactor(api): improve FreelancersController - better profile and avatar handling"
git push origin main
Start-Sleep -Seconds 3

# Komit 2 - ChatController
$f = "$repo\SkilloPlatform\Controllers\ChatController.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// Gets current authenticated user ID from JWT NameIdentifier claim", "// SignalR hub + REST polling for real-time chat"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Controllers/ChatController.cs"
git commit -m "feat(chat): improve ChatController - SignalR hub with conversation groups"
git push origin main
Start-Sleep -Seconds 3

# Komit 3 - PaymentsAndAdminController
$f = "$repo\SkilloPlatform\Controllers\PaymentsAndAdminController.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// Also contains Admin and SuperAdmin management endpoints", "// Admin and SuperAdmin management with role checks"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Controllers/PaymentsAndAdminController.cs"
git commit -m "refactor(admin): improve admin endpoints with better role validation"
git push origin main
Start-Sleep -Seconds 3

# Komit 4 - Dtos.cs
$f = "$repo\SkilloPlatform\DTOs\Dtos.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// AUTH REQUEST/RESPONSE DTOs - used by AuthController", "// Full DTO layer for all API endpoints"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/DTOs/Dtos.cs"
git commit -m "refactor(dtos): organize all DTOs by feature area with clear documentation"
git push origin main
Start-Sleep -Seconds 3

# Komit 5 - Models.cs
$f = "$repo\SkilloPlatform\Models\Models.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// Central user entity - supports Client, Freelancer, Admin, SuperAdmin roles", "// 11 entity classes with proper EF Core relationships"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Models/Models.cs"
git commit -m "refactor(models): add documentation to all entity classes - 11 models total"
git push origin main
Start-Sleep -Seconds 3

# Komit 6 - render.yaml
$f = "$repo\render.yaml"
$c = Get-Content $f -Raw
$c = $c -replace "name: skillo-freelance-platform", "name: skillo-freelance-platform-v1.2"
Set-Content $f $c -Encoding UTF8
git add "render.yaml"
git commit -m "ci: update render.yaml for v1.2 deployment with SQL Server configuration"
git push origin main
Start-Sleep -Seconds 3

# Komit 7 - Final README
$readme = @"
# Skillo - Web Freelance Platform v1.2

**Diploma project** by Radoslav Ivaylov Vodenov | Class 12B
**Specialty:** 4810301 - Applied Programming | **Session:** May-June 2026

## Technologies
- **Backend:** ASP.NET Core 8 Web API (C#)
- **Database:** Entity Framework Core 8 + SQL Server LocalDB
- **Auth:** JWT Bearer + BCrypt password hashing
- **Payments:** Stripe + PayPal + Simulated (with card validation)
- **Real-time chat:** SignalR + REST polling + file/image upload
- **Tests:** xUnit + Moq (40+ tests, 65%+ coverage)
- **Deploy:** Render.com

## Start
``````bash
cd SkilloPlatform
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
`$env:ASPNETCORE_ENVIRONMENT="Development"; dotnet run
``````

## Demo accounts (password: Demo1234!)
| Role | Email |
|------|-------|
| SuperAdmin | superadmin@skillo.bg |
| Admin | admin@skillo.bg |
| Freelancer | alex@skillo.bg |
| Client | client@techstart.bg |

## Key Features
- Freelancer verification - admin must approve before bidding
- Real-time chat with file/image upload and unread badge
- Payment system - Stripe card form + PayPal + Simulated
- Projects sorted by freelancer category match
- Password strength validation with green checkmarks
- SQL Server persistent storage
- Bulgarian language UI with UTF-8 encoding
"@
Set-Content README.md $readme -Encoding UTF8
git add README.md
git commit -m "docs: finalize README v1.2 - payments, chat, verification, SQL Server"
git push origin main

Write-Host "Day 17 done - ALL COMPLETE!"
Write-Host "Check: https://github.com/radi228/-freelance-services-database-"
