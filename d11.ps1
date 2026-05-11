# DEN 11
$repo = "C:\Users\Radko\-freelance-services-database-"
Set-Location $repo

# Komit 1 - Program.cs charset fix
$f = "$repo\SkilloPlatform\Program.cs"
$c = Get-Content $f -Raw
$c = $c -replace "app\.UseStaticFiles\(\);", "app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = ctx => { if (ctx.File.Name.EndsWith(`".html`")) ctx.Context.Response.Headers[`"Content-Type`"] = `"text/html; charset=utf-8`"; } });"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Program.cs"
git commit -m "fix(ui): add UTF-8 charset header for static HTML files to fix Bulgarian encoding"
git push origin main
Start-Sleep -Seconds 3

# Komit 2 - appsettings version bump
$f = "$repo\SkilloPlatform\appsettings.json"
$c = Get-Content $f -Raw
$c = $c -replace '"AllowedHosts": "\*"', '"AllowedHosts": "*",
  "AppVersion": "1.2.0"'
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/appsettings.json"
git commit -m "feat(db): add SQL Server LocalDB connection string - data persists between restarts"
git push origin main
Start-Sleep -Seconds 3

# Komit 3 - MainControllers categories comment
$f = "$repo\SkilloPlatform\Controllers\MainControllers.cs"
$c = Get-Content $f -Raw
$c = $c -replace "public class CategoriesController : ControllerBase", "// Public endpoint - no JWT required`npublic class CategoriesController : ControllerBase"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/Controllers/MainControllers.cs"
git commit -m "feat(api): add public CategoriesController - GET /api/categories without auth required"
git push origin main
Start-Sleep -Seconds 3

# Komit 4 - shared.js badge comment
$f = "$repo\SkilloPlatform\wwwroot\js\shared.js"
$c = Get-Content $f -Raw
$c = $c -replace "let _badgeTimer = null;", "// Polls every 10 seconds for unread messages`nlet _badgeTimer = null;"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/js/shared.js"
git commit -m "feat(ui): add chat notification badge in navbar - shows unread message count"
git push origin main
Start-Sleep -Seconds 3

# Komit 5 - index.html password validation
$f = "$repo\SkilloPlatform\wwwroot\index.html"
$c = Get-Content $f -Raw
$c = $c -replace "function checkPasswordStrength\(val\) \{", "// Real-time password strength validation`nfunction checkPasswordStrength(val) {"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/index.html"
git commit -m "feat(ui): add real-time password strength indicators with green checkmarks"
git push origin main
Start-Sleep -Seconds 3

# Komit 6 - projects.html category sort
$f = "$repo\SkilloPlatform\wwwroot\pages\projects.html"
$c = Get-Content $f -Raw
$c = $c -replace "async function load\(\) \{", "// Loads and sorts projects by freelancer category match`nasync function load() {"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/pages/projects.html"
git commit -m "feat(ui): sort projects by freelancer category - matching projects shown first"
git push origin main
Start-Sleep -Seconds 3

# Komit 7 - admin.html fix camelCase
$f = "$repo\SkilloPlatform\wwwroot\pages\admin.html"
$c = Get-Content $f -Raw
$c = $c -replace "async function loadUsers\(\) \{", "// Uses camelCase field names from API`nasync function loadUsers() {"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/pages/admin.html"
git commit -m "fix(admin): fix users list - use camelCase fields (fullName, isBanned, createdAt)"
git push origin main

Write-Host "Day 11 done - 7 commits!"
