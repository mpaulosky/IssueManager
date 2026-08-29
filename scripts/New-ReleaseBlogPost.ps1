<#
.SYNOPSIS
    Generates a Markdown blog post documenting a GitHub release and stages it.

.DESCRIPTION
    Fetches a release's notes via the GitHub CLI (`gh`) and writes a dated Markdown post to
    docs/blogs/, then runs `git add` on it. Requires `gh` to be authenticated against this repo.

.PARAMETER Tag
    The release tag to document (e.g. v0.0.19). Defaults to the latest release.

.PARAMETER All
    Generate a post for every release in the repository (skips ones that already have a post).

.EXAMPLE
    .\scripts\New-ReleaseBlogPost.ps1 -Tag v0.0.19

.EXAMPLE
    .\scripts\New-ReleaseBlogPost.ps1 -All
#>

param(
	[string]$Tag,
	[switch]$All
)

$ErrorActionPreference = "Stop"

$repoRoot = git rev-parse --show-toplevel
if (-not $repoRoot) {
	throw "Not inside a git repository."
}

$blogsDir = Join-Path $repoRoot "docs/blogs"
New-Item -ItemType Directory -Path $blogsDir -Force | Out-Null

function New-ReleasePost([string]$releaseTag, [string]$releaseName, [string]$publishedAt, [string]$body) {
	$date = [DateTimeOffset]::Parse($publishedAt)
	$dateStamp = $date.ToString("yyyy-MM-dd")
	$slug = $releaseTag.ToLowerInvariant() -replace "[^a-z0-9.]", "-"
	$fileName = "$dateStamp-release-$slug.md"
	$filePath = Join-Path $blogsDir $fileName

	if (Test-Path $filePath) {
		Write-Host "Skipping $releaseTag - post already exists: docs/blogs/$fileName"
		return
	}

	$title = if ($releaseName) { $releaseName } else { $releaseTag }

	$content = @"
---
title: "$title"
date: $dateStamp
release: $releaseTag
---

# $title

$body
"@

	Set-Content -Path $filePath -Value $content -NoNewline
	git -C $repoRoot add $filePath
	Write-Host "Release post created and staged: docs/blogs/$fileName"
}

if ($All) {
	$releases = gh release list --limit 200 --json tagName | ConvertFrom-Json
	foreach ($r in $releases) {
		$details = gh release view $r.tagName --json tagName,name,publishedAt,body | ConvertFrom-Json
		New-ReleasePost -releaseTag $details.tagName -releaseName $details.name -publishedAt $details.publishedAt -body $details.body
	}
	return
}

if (-not $Tag) {
	$details = gh release view --json tagName,name,publishedAt,body | ConvertFrom-Json
} else {
	$details = gh release view $Tag --json tagName,name,publishedAt,body | ConvertFrom-Json
}

New-ReleasePost -releaseTag $details.tagName -releaseName $details.name -publishedAt $details.publishedAt -body $details.body
