<#
.SYNOPSIS
    Generates a Markdown blog post summarizing a commit's (or commit range's) changes and stages
    it for the next push.

.DESCRIPTION
    Reads a commit's message and diff (HEAD by default) — or, when given a range like
    `HEAD~3..HEAD`, the combined commit log and diff across that range — and writes a dated
    Markdown post to docs/blogs/, then runs `git add` on it so it is included in the same push
    as the code change it documents.

.PARAMETER Ref
    The git ref/commit to summarize, or a range (`A..B`). Defaults to HEAD.

.PARAMETER Title
    Optional title for the post. Defaults to the commit's subject line (single ref) or a
    generated summary (range).

.EXAMPLE
    .\scripts\New-BlogPost.ps1

.EXAMPLE
    .\scripts\New-BlogPost.ps1 -Ref HEAD~1 -Title "Consolidated Mongo repositories"

.EXAMPLE
    .\scripts\New-BlogPost.ps1 -Ref HEAD~3..HEAD -Title "This week's refactors"
#>

param(
	[string]$Ref = "HEAD",
	[string]$Title
)

$ErrorActionPreference = "Stop"

$repoRoot = git rev-parse --show-toplevel
if (-not $repoRoot) {
	throw "Not inside a git repository."
}

function Get-Slug([string]$text) {
	$slug = $text.ToLowerInvariant()
	$slug = $slug -replace "[^a-z0-9\s-]", ""
	$slug = $slug -replace "\s+", "-"
	$slug = $slug -replace "-+", "-"
	return $slug.Trim("-")
}

$isRange = $Ref -match "\.\."

if ($isRange) {
	$parts = $Ref -split "\.\.\.?", 2
	$fromRef = $parts[0]
	$toRef = if ($parts[1]) { $parts[1] } else { "HEAD" }

	$fromHash = git rev-parse --short $fromRef
	$toHash = git rev-parse --short $toRef
	$commitHash = "$fromHash..$toHash"
	$commitAuthor = git log -1 --format=%an $toRef
	$commitDate = git log -1 --format=%cI $toRef
	$commitCount = (git rev-list --count $Ref).Trim()
	$commitLog = (git log --format="- %s (%h)" $Ref) -join "`n"
	$diffStat = (git diff --stat $Ref) -join "`n"
	$diff = (git diff $Ref) -join "`n"

	if (-not $Title) {
		$Title = "$commitCount commits: $fromHash..$toHash"
	}

	$bodySection = "## Commits`n`n$commitLog`n`n"
} else {
	$commitHash = git rev-parse --short $Ref
	$commitSubject = git log -1 --format=%s $Ref
	$commitBody = (git log -1 --format=%b $Ref) -join "`n"
	$commitAuthor = git log -1 --format="%an" $Ref
	$commitDate = git log -1 --format=%cI $Ref
	$diffStat = (git diff-tree --no-commit-id --stat -r $Ref) -join "`n"
	$diff = (git diff-tree -p --no-commit-id -r $Ref) -join "`n"

	if (-not $Title) {
		$Title = $commitSubject
	}

	$bodySection = if ($commitBody.Trim()) { "## Summary`n`n$($commitBody.Trim())`n`n" } else { "## Summary`n`n$commitSubject`n`n" }
}

$date = [DateTimeOffset]::Parse($commitDate)
$dateStamp = $date.ToString("yyyy-MM-dd")

$slug = Get-Slug $Title
if (-not $slug) {
	$slug = $commitHash -replace "[^a-z0-9]", "-"
}

$blogsDir = Join-Path $repoRoot "docs/blogs"
New-Item -ItemType Directory -Path $blogsDir -Force | Out-Null

$fileName = "$dateStamp-$slug.md"
$filePath = Join-Path $blogsDir $fileName

$content = @"
---
title: "$Title"
date: $dateStamp
commit: $commitHash
author: $commitAuthor
---

# $Title

$bodySection## Files Changed

``````
$diffStat
``````

## Diff

``````diff
$diff
``````
"@

Set-Content -Path $filePath -Value $content -NoNewline
git -C $repoRoot add $filePath

Write-Host "Blog post created and staged: docs/blogs/$fileName"
