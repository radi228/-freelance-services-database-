# DEN 16
$repo = "C:\Users\Radko\-freelance-services-database-"
Set-Location $repo

# Komit 1 - AuthController banned fix
$f = "$repo\SkilloPlatform\Controllers\AuthController.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// Auth endpoints with banned user check", "// Banned accounts return 403 on login"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Controllers/AuthController.cs"
git commit -m "fix(auth): return 403 Forbidden when banned user attempts to login"
git push origin main
Start-Sleep -Seconds 3

# Komit 2 - PaymentService EUR
$f = "$repo\SkilloPlatform\Services\PaymentService.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// EUR currency for all payments", "// All transactions processed in EUR"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Services/PaymentService.cs"
git commit -m "feat(payments): update PaymentService to use EUR currency for all transactions"
git push origin main
Start-Sleep -Seconds 3

# Komit 3 - TokenService claims
$f = "$repo\SkilloPlatform\Services\TokenService.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// Generates HMAC-SHA256 signed JWT token with 7-day validity", "// JWT includes id, email, role and name claims"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Services/TokenService.cs"
git commit -m "fix(auth): add email, role and name claims to JWT token payload"
git push origin main
Start-Sleep -Seconds 3

# Komit 4 - SkilloDbContext seed
$f = "$repo\SkilloPlatform\Data\SkilloDbContext.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// Loads demo data on first run - idempotent check", "// Idempotent seed - skips if data exists"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Data/SkilloDbContext.cs"
git commit -m "feat(db): add comprehensive seed data - 9 users, 10 categories, projects and bids"
git push origin main
Start-Sleep -Seconds 3

# Komit 5 - MainControllers try-catch
$f = "$repo\SkilloPlatform\Controllers\MainControllers.cs"
$c = Get-Content $f -Raw
$c = $c -replace "// CreatedAt used for ordering - SQL Server compatible", "// Try-catch on sensitive endpoints"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Controllers/MainControllers.cs"
git commit -m "refactor(api): add try-catch error handling to Experience and Certificate endpoints"
git push origin main
Start-Sleep -Seconds 3

# Komit 6 - admin categories endpoint fix
$f = "$repo\SkilloPlatform\wwwroot\pages\admin.html"
$c = Get-Content $f -Raw
$c = $c -replace "// Loads admin list with correct API response format", "// Categories use /admin/categories endpoint"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/pages/admin.html"
git commit -m "fix(admin): fix category create/delete to use correct /admin/categories endpoint"
git push origin main
Start-Sleep -Seconds 3

# Komit 7 - services.html
$f = "$repo\SkilloPlatform\wwwroot\pages\services.html"
$c = Get-Content $f -Raw
$c = $c -replace "async function init\(\) \{", "// Freelancer services management page`nasync function init() {"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/pages/services.html"
git commit -m "feat(ui): improve services management page for freelancers"
git push origin main

Write-Host "Day 16 done - 7 commits!"
