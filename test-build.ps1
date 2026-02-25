#!/usr/bin/env pwsh
Set-Location E:\github\IssueManager
Write-Host "Starting build..."
dotnet build --configuration Release /p:TreatWarningsAsErrors=true
Write-Host "Build exit code: $LASTEXITCODE"
