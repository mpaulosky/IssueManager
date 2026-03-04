#!/usr/bin/env pwsh
# =======================================================
# Copyright (c) 2026. All rights reserved.
# File Name :     Add-CopyrightHeaders.ps1
# Company :       mpaulosky
# Author :        Matthew Paulosky
# Solution Name : IssueManager
# Project Name :  Scripts
# =======================================================

<#
.SYNOPSIS
	Adds standardized copyright headers to all C# files missing them.

.DESCRIPTION
	This script scans all .cs files in src/ and tests/ directories and adds
	a standardized copyright header to files that don't already have one.

	The header format:
	// =======================================================
	// Copyright (c) 2026. All rights reserved.
	// File Name :     {filename}
	// Company :       mpaulosky
	// Author :        Matthew Paulosky
	// Solution Name : IssueManager
	// Project Name :  {project-name}
	// =======================================================

.PARAMETER DryRun
	If specified, only reports what would be changed without making modifications.

.PARAMETER Fix
	If specified, updates existing headers to fix incorrect Project Names or other issues.

.EXAMPLE
	.\Add-CopyrightHeaders.ps1 -DryRun
	Shows what files would be updated without making changes.

.EXAMPLE
	.\Add-CopyrightHeaders.ps1
	Adds copyright headers to all files missing them.

.EXAMPLE
	.\Add-CopyrightHeaders.ps1 -Fix
	Fixes incorrect Project Names in existing headers.
#>

[CmdletBinding()]
param(
	[switch]$DryRun,
	[switch]$Fix
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Navigate to repo root
$repoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $repoRoot

Write-Host "Scanning for C# files..." -ForegroundColor Cyan

# Get all C# files (excluding obj/bin directories)
$allFiles = Get-ChildItem -Path "src", "tests" -Filter "*.cs" -Recurse -File |
	Where-Object { $_.FullName -notmatch "\\obj\\|\\bin\\" }

Write-Host "   Found $($allFiles.Count) C# files" -ForegroundColor Gray

# Function to determine project name from file path
function Get-ProjectName {
	param([string]$FilePath)

	$relativePath = $FilePath -replace [regex]::Escape($repoRoot), ""
	$relativePath = $relativePath.TrimStart("\", "/")

	# Map directory paths to project names
	if ($relativePath -match "^src\\Api\\") { return "Api" }
	if ($relativePath -match "^src\\AppHost\\") { return "AppHost" }
	if ($relativePath -match "^src\\Web\\") { return "Web" }
	if ($relativePath -match "^src\\Shared\\") { return "Shared" }
	if ($relativePath -match "^src\\ServiceDefaults\\") { return "ServiceDefaults" }
	if ($relativePath -match "^tests\\Unit\.Tests\\") { return "Unit.Tests" }
	if ($relativePath -match "^tests\\Integration\.Tests\\") { return "Integration.Tests" }
	if ($relativePath -match "^tests\\Blazor\.Tests\\") { return "Blazor.Tests" }
	if ($relativePath -match "^tests\\Architecture\.Tests\\") { return "Architecture.Tests" }
	if ($relativePath -match "^tests\\Aspire\\") { return "Aspire.Tests" }
	if ($relativePath -match "^scripts\\") { return "Scripts" }

	# Default fallback
	return "Unknown"
}

# Function to check if file has a copyright header
function Test-HasCopyrightHeader {
	param([string]$FilePath)

	$content = Get-Content -Path $FilePath -Raw

	# Check for any form of copyright header
	return $content -match "(?m)^//.*Copyright.*\(c\).*\d{4}"
}

# Function to extract existing project name from header
function Get-ExistingProjectName {
	param([string]$FilePath)

	$lines = Get-Content -Path $FilePath -Head 15
	foreach ($line in $lines) {
		if ($line -match "^//\s*Project Name\s*:\s*(.+)$") {
			return $matches[1].Trim()
		}
	}
	return $null
}

# Function to create copyright header
function Get-CopyrightHeader {
	param(
		[string]$FileName,
		[string]$ProjectName
	)

	return @"
// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     $FileName
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  $ProjectName
// =======================================================

"@
}

# Statistics
$stats = @{
	Total = $allFiles.Count
	HasHeader = 0
	NeedsHeader = 0
	NeedsFix = 0
	Fixed = 0
	Added = 0
}

$filesToAdd = @()
$filesToFix = @()

Write-Host ""
Write-Host "Analyzing files..." -ForegroundColor Cyan

foreach ($file in $allFiles) {
	$hasHeader = Test-HasCopyrightHeader -FilePath $file.FullName

	if ($hasHeader) {
		$stats.HasHeader++

		if ($Fix) {
			$expectedProject = Get-ProjectName -FilePath $file.FullName
			$existingProject = Get-ExistingProjectName -FilePath $file.FullName

			if ($existingProject -and $existingProject -ne $expectedProject) {
				# Special case: accept both "Unit Tests" and "Unit.Tests" as valid
				if ($expectedProject -eq "Unit.Tests" -and $existingProject -eq "Unit Tests") {
					# Both are acceptable, skip
				}
				else {
					$stats.NeedsFix++
					$filesToFix += [PSCustomObject]@{
						Path = $file.FullName
						RelativePath = $file.FullName -replace [regex]::Escape($repoRoot + "\"), ""
						FileName = $file.Name
						ExpectedProject = $expectedProject
						ExistingProject = $existingProject
					}
				}
			}
		}
	}
	else {
		$stats.NeedsHeader++
		$projectName = Get-ProjectName -FilePath $file.FullName
		$filesToAdd += [PSCustomObject]@{
			Path = $file.FullName
			RelativePath = $file.FullName -replace [regex]::Escape($repoRoot + "\"), ""
			FileName = $file.Name
			ProjectName = $projectName
		}
	}
}

# Display statistics
Write-Host ""
Write-Host "Statistics:" -ForegroundColor Cyan
Write-Host "   Total files:          $($stats.Total)" -ForegroundColor Gray
Write-Host "   With headers:         $($stats.HasHeader) ($([math]::Round($stats.HasHeader / $stats.Total * 100, 1))%%)" -ForegroundColor Green
Write-Host "   Missing headers:      $($stats.NeedsHeader) ($([math]::Round($stats.NeedsHeader / $stats.Total * 100, 1))%%)" -ForegroundColor Yellow
if ($Fix) {
	Write-Host "   Need fixing:          $($stats.NeedsFix)" -ForegroundColor Magenta
}

# Display files needing headers
if ($filesToAdd.Count -gt 0) {
	Write-Host ""
	Write-Host "Files missing copyright headers:" -ForegroundColor Yellow
	foreach ($file in $filesToAdd) {
		Write-Host "   - $($file.RelativePath) [Project: $($file.ProjectName)]" -ForegroundColor Gray
	}
}

# Display files needing fixes
if ($Fix -and $filesToFix.Count -gt 0) {
	Write-Host ""
	Write-Host "Files with incorrect Project Name:" -ForegroundColor Magenta
	foreach ($file in $filesToFix) {
		Write-Host "   - $($file.RelativePath)" -ForegroundColor Gray
		Write-Host "     Expected: '$($file.ExpectedProject)', Found: '$($file.ExistingProject)'" -ForegroundColor DarkGray
	}
}

# If dry run, exit here
if ($DryRun) {
	Write-Host ""
	Write-Host "Dry run complete. Use without -DryRun to apply changes." -ForegroundColor Green
	exit 0
}

# Add headers to files
if ($filesToAdd.Count -gt 0) {
	Write-Host ""
	Write-Host "Adding copyright headers..." -ForegroundColor Cyan

	foreach ($file in $filesToAdd) {
		try {
			$content = Get-Content -Path $file.Path -Raw
			$header = Get-CopyrightHeader -FileName $file.FileName -ProjectName $file.ProjectName
			$newContent = $header + $content

			Set-Content -Path $file.Path -Value $newContent -NoNewline
			$stats.Added++
			Write-Host "   [OK] $($file.RelativePath)" -ForegroundColor Green
		}
		catch {
			Write-Host "   [ERROR] $($file.RelativePath): $($_.Exception.Message)" -ForegroundColor Red
		}
	}
}

# Fix headers
if ($Fix -and $filesToFix.Count -gt 0) {
	Write-Host ""
	Write-Host "Fixing incorrect headers..." -ForegroundColor Cyan

	foreach ($file in $filesToFix) {
		try {
			$lines = Get-Content -Path $file.Path
			$newLines = @()
			$inHeader = $false
			$headerEnd = 0

			# Find header end
			for ($i = 0; $i -lt $lines.Count; $i++) {
				if ($lines[$i] -match "^//\s*=+\s*$" -and $i -gt 0) {
					$headerEnd = $i
					break
				}
			}

			if ($headerEnd -gt 0) {
				# Create new header
				$newHeader = Get-CopyrightHeader -FileName $file.FileName -ProjectName $file.ExpectedProject
				$newHeaderLines = $newHeader.Split("`n") | Where-Object { $_ -ne "" }

				# Replace old header with new header
				$newContent = ($newHeaderLines -join "`n") + "`n" + (($lines[($headerEnd + 1)..($lines.Count - 1)]) -join "`n")

				Set-Content -Path $file.Path -Value $newContent -NoNewline
				$stats.Fixed++
				Write-Host "   [OK] $($file.RelativePath)" -ForegroundColor Green
			}
		}
		catch {
			Write-Host "   [ERROR] $($file.RelativePath): $($_.Exception.Message)" -ForegroundColor Red
		}
	}
}

# Final summary
Write-Host ""
Write-Host "Complete!" -ForegroundColor Green
if ($stats.Added -gt 0) {
	Write-Host "   Added headers to $($stats.Added) file(s)" -ForegroundColor Green
}
if ($stats.Fixed -gt 0) {
	Write-Host "   Fixed headers in $($stats.Fixed) file(s)" -ForegroundColor Green
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "   1. Review the changes: git diff" -ForegroundColor Gray
Write-Host "   2. Run pre-push hook to validate: bash scripts/hooks/pre-push" -ForegroundColor Gray
Write-Host "   3. Commit the changes: git add . && git commit -m 'chore: add copyright headers'" -ForegroundColor Gray

exit 0
