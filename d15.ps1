# DEN 15
$repo = "C:\Users\Radko\-freelance-services-database-"
Set-Location $repo

# Komit 1 - chat mark as seen
$f = "$repo\SkilloPlatform\wwwroot\pages\chat.html"
$c = Get-Content $f -Raw
$c = $c -replace "// Mark conversation as read and clear badge on open", "// markConvSeen called on open"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/pages/chat.html"
git commit -m "feat(chat): mark conversation as read on open - clears notification badge"
git push origin main
Start-Sleep -Seconds 3

# Komit 2 - image render in chat
$c = Get-Content $f -Raw
$c = $c -replace "function renderMsgContent\(content\) \{", "// Renders images inline, escapes HTML for text`nfunction renderMsgContent(content) {"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/pages/chat.html"
git commit -m "feat(chat): render uploaded images inline in messages with click-to-expand"
git push origin main
Start-Sleep -Seconds 3

# Komit 3 - nav order Услуги before Профил
$f = "$repo\SkilloPlatform\wwwroot\js\shared.js"
$c = Get-Content $f -Raw
$c = $c -replace "// Polls every 10 seconds for unread messages", "// Nav: Услуги before Профил for freelancers"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/js/shared.js"
git commit -m "fix(ui): reorder nav links for freelancers - Услуги before Профил"
git push origin main
Start-Sleep -Seconds 3

# Komit 4 - badge only for received messages
$c = Get-Content $f -Raw
$c = $c -replace "// Nav: Услуги before Профил for freelancers", "// Badge only for messages from other users"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/js/shared.js"
git commit -m "fix(chat): notification badge shows only for messages received from others"
git push origin main
Start-Sleep -Seconds 3

# Komit 5 - badge clears on open
$c = Get-Content $f -Raw
$c = $c -replace "// Badge only for messages from other users", "// Badge clears via markConvSeen on open"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/js/shared.js"
git commit -m "fix(chat): clear notification badge when conversation is opened"
git push origin main
Start-Sleep -Seconds 3

# Komit 6 - freelancers page fix
$f = "$repo\SkilloPlatform\wwwroot\pages\freelancers.html"
$c = Get-Content $f -Raw
$c = $c -replace "async function load\(\) \{", "// Loads freelancers with filters and error handling`nasync function load() {"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/pages/freelancers.html"
git commit -m "fix(ui): improve freelancers page loading with better error handling"
git push origin main
Start-Sleep -Seconds 3

# Komit 7 - my-bids page
$f = "$repo\SkilloPlatform\wwwroot\pages\my-bids.html"
$c = Get-Content $f -Raw
$c = $c -replace "async function init\(\) \{", "// Shows freelancer bids with status colors`nasync function init() {"
Set-Content $f $c -Encoding UTF8
git add "SkilloPlatform/wwwroot/pages/my-bids.html"
git commit -m "feat(ui): improve my bids page - show status colors and bid details"
git push origin main

Write-Host "Day 15 done - 7 commits!"
