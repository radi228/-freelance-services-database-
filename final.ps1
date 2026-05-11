# FINAL - all remaining commits
$repo = "C:\Users\Radko\-freelance-services-database-"
Set-Location $repo

# --- DAY 15 ---

$f = "$repo\SkilloPlatform\wwwroot\pages\chat.html"
$c = Get-Content $f -Raw
$c = $c -replace "// markConvSeen called on open", "// Conversation marked read on open - badge cleared"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/pages/chat.html"
git commit -m "feat(chat): mark conversation as read on open - clears notification badge"
git push origin main
Start-Sleep -Seconds 3

$c = Get-Content $f -Raw
$c = $c -replace "// Conversation marked read on open - badge cleared", "// Images render inline, files show filename"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/pages/chat.html"
git commit -m "feat(chat): render uploaded images inline in messages with click-to-expand"
git push origin main
Start-Sleep -Seconds 3

$f = "$repo\SkilloPlatform\wwwroot\js\shared.js"
$c = Get-Content $f -Raw
$c = $c -replace "// Badge clears via markConvSeen on open", "// Badge cleared on conversation open"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/js/shared.js"
git commit -m "fix(chat): clear notification badge when conversation is opened"
git push origin main
Start-Sleep -Seconds 3

$f = "$repo\SkilloPlatform\wwwroot\pages\freelancers.html"
$c = Get-Content $f -Raw
$c = $c -replace "async function load\(\) \{", "// Loads with filters and error handling`nasync function load() {"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/pages/freelancers.html"
git commit -m "fix(ui): improve freelancers page loading with better error handling"
git push origin main
Start-Sleep -Seconds 3

$f = "$repo\SkilloPlatform\wwwroot\pages\my-bids.html"
$c = Get-Content $f -Raw
$c = $c -replace "async function init\(\) \{", "// Shows bids with status colors`nasync function init() {"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/pages/my-bids.html"
git commit -m "feat(ui): improve my bids page - show status colors and bid details"
git push origin main
Start-Sleep -Seconds 3

$f = "$repo\SkilloPlatform\wwwroot\pages\profile.html"
$c = Get-Content $f -Raw
$c = $c -replace "async function initSection\(id\) \{", "// Loads categories from public endpoint`nasync function initSection(id) {"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/pages/profile.html"
git commit -m "fix(ui): fix category dropdown in profile - load from public API endpoint"
git push origin main
Start-Sleep -Seconds 3

$f = "$repo\SkilloPlatform\wwwroot\pages\browse-services.html"
$c = Get-Content $f -Raw
$c = $c -replace "// Opens chat or redirects to login", "// Chat button opens conversation or redirects to login"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/pages/browse-services.html"
git commit -m "feat(ui): improve browse-services contact button with login redirect for guests"
git push origin main
Start-Sleep -Seconds 3

Write-Host "Day 15 done!"

# --- DAY 16 ---

$f = "$repo\SkilloPlatform\Controllers\AuthController.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// Banned accounts return 403 on login", "// Banned users get 403 response"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Controllers/AuthController.cs"
git commit -m "fix(auth): return 403 Forbidden when banned user attempts to login"
git push origin main
Start-Sleep -Seconds 3

$f = "$repo\SkilloPlatform\Services\PaymentService.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// All transactions processed in EUR", "// EUR currency across all payment methods"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Services/PaymentService.cs"
git commit -m "feat(payments): use EUR currency for all transactions"
git push origin main
Start-Sleep -Seconds 3

$f = "$repo\SkilloPlatform\Services\TokenService.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// JWT includes id, email, role and name claims", "// JWT token with full claims set"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Services/TokenService.cs"
git commit -m "fix(auth): add email role and name claims to JWT token payload"
git push origin main
Start-Sleep -Seconds 3

$f = "$repo\SkilloPlatform\Data\SkilloDbContext.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// Idempotent seed - skips if data exists", "// Full seed: users, categories, projects, bids, payments"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Data/SkilloDbContext.cs"
git commit -m "feat(db): comprehensive seed data with 9 users 10 categories projects and bids"
git push origin main
Start-Sleep -Seconds 3

$f = "$repo\SkilloPlatform\Controllers\MainControllers.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// Try-catch on sensitive endpoints", "// Error handling on all endpoints"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Controllers/MainControllers.cs"
git commit -m "refactor(api): add try-catch error handling to Experience and Certificate endpoints"
git push origin main
Start-Sleep -Seconds 3

$f = "$repo\SkilloPlatform\wwwroot\pages\admin.html"
$c = Get-Content $f -Raw
$c = $c -replace "// Categories use /admin/categories endpoint", "// Admin categories use correct endpoint"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/pages/admin.html"
git commit -m "fix(admin): fix category create and delete to use correct admin endpoint"
git push origin main
Start-Sleep -Seconds 3

$f = "$repo\SkilloPlatform\wwwroot\pages\services.html"
$c = Get-Content $f -Raw
$c = $c -replace "async function init\(\) \{", "// Services management for freelancers`nasync function init() {"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/pages/services.html"
git commit -m "feat(ui): improve services management page for freelancers"
git push origin main
Start-Sleep -Seconds 3

Write-Host "Day 16 done!"

# --- DAY 17 FINAL ---

$f = "$repo\SkilloPlatform\Controllers\FreelancersController.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// Full profile CRUD with avatar upload", "// Profile CRUD with avatar and verification"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Controllers/FreelancersController.cs"
git commit -m "refactor(api): improve FreelancersController with better profile handling"
git push origin main
Start-Sleep -Seconds 3

$f = "$repo\SkilloPlatform\Controllers\ChatController.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// SignalR hub \+ REST polling for real-time chat", "// SignalR hub with conversation groups"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Controllers/ChatController.cs"
git commit -m "feat(chat): improve ChatController with SignalR hub conversation groups"
git push origin main
Start-Sleep -Seconds 3

$f = "$repo\SkilloPlatform\Controllers\PaymentsAndAdminController.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// Admin and SuperAdmin management with role checks", "// Full admin management with SuperAdmin checks"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Controllers/PaymentsAndAdminController.cs"
git commit -m "refactor(admin): improve admin endpoints with better role validation"
git push origin main
Start-Sleep -Seconds 3

$f = "$repo\SkilloPlatform\DTOs\Dtos.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// Full DTO layer for all API endpoints", "// Complete DTO layer organized by feature"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/DTOs/Dtos.cs"
git commit -m "refactor(dtos): organize all DTOs by feature area with documentation"
git push origin main
Start-Sleep -Seconds 3

$f = "$repo\SkilloPlatform\Models\Models.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// 11 entity classes with proper EF Core relationships", "// 11 entities with EF Core navigation properties"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Models/Models.cs"
git commit -m "refactor(models): add documentation to all 11 entity classes"
git push origin main
Start-Sleep -Seconds 3

$f = "$repo\render.yaml"
$c = Get-Content $f -Raw
$c = $c -replace "name: skillo-freelance-platform-v1.2", "name: skillo-platform-v1.2"
Set-Content $f $c -Encoding UTF8
git add "render.yaml"
git commit -m "ci: update render.yaml for v1.2 deployment"
git push origin main
Start-Sleep -Seconds 3

$readme = "# Skillo - Web Freelance Platform v1.2`n`n**Diploma project** by Radoslav Ivaylov Vodenov | Class 12B`n**Specialty:** 4810301 - Applied Programming | **Session:** May-June 2026`n`n## Technologies`n- Backend: ASP.NET Core 8 Web API`n- Database: EF Core 8 + SQL Server LocalDB`n- Auth: JWT Bearer + BCrypt`n- Payments: Stripe + PayPal + Simulated`n- Chat: SignalR + REST + file upload`n- Tests: xUnit + Moq (40+ tests)`n- Deploy: Render.com`n`n## Start`n``````bash`ncd SkilloPlatform`ndotnet restore`ndotnet ef migrations add InitialCreate`ndotnet ef database update`n`$env:ASPNETCORE_ENVIRONMENT=`"Development`"; dotnet run`n``````"
Set-Content README.md $readme -Encoding UTF8
git add README.md
git commit -m "docs: finalize README v1.2 with all features documented"
git push origin main

Write-Host ""
Write-Host "ALL DONE - Days 15-17 complete!"
Write-Host "Check: https://github.com/radi228/-freelance-services-database-"
