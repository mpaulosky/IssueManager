#!/usr/bin/env pwsh
# cleanup-merged-branches.ps1
# Deletes local branches whose remote tracking branch is gone (PR merged/closed)

param(
	[switch]$DryRun,
	[switch]$Force
)

Write-Host "🔄 Fetching from origin with prune..." -ForegroundColor Cyan
git fetch --prune

Write-Host "`n🔍 Scanning for orphaned local branches..." -ForegroundColor Cyan

$gone = git branch -vv | Where-Object { $_ -match ': gone\]' } | ForEach-Object {
	($_ -split '\s+' | Where-Object { $_ })[0] -replace '^\*', '' 
} | Where-Object { $_ -ne 'main' -and $_ -ne 'develop' }

if (-not $gone) {
	Write-Host "✅ No orphaned branches found." -ForegroundColor Green
	exit 0
}

Write-Host "`n📋 Orphaned branches to remove:" -ForegroundColor Yellow
$gone | ForEach-Object { Write-Host "  - $_" }

if ($DryRun) {
	Write-Host "`n⚠️  Dry run — no branches deleted." -ForegroundColor Yellow
	exit 0
}

$deleted = @()
$skipped = @()

foreach ($branch in $gone) {
	$result = git branch -d $branch 2>&1
	if ($LASTEXITCODE -eq 0) {
		$deleted += $branch
		Write-Host "  ✅ Deleted: $branch" -ForegroundColor Green
	} elseif ($Force) {
		git branch -D $branch 2>&1 | Out-Null
		$deleted += $branch
		Write-Host "  ✅ Force-deleted: $branch" -ForegroundColor Yellow
	} else {
		$skipped += $branch
		Write-Host "  ⚠️  Skipped (unmerged work?): $branch  — use -Force to override" -ForegroundColor Red
	}
}

Write-Host "`n📊 Summary:" -ForegroundColor Cyan
Write-Host "  Deleted: $($deleted.Count) | Skipped: $($skipped.Count)"
