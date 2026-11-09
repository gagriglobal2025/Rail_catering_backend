# ============================================================================
# RailCat Platform - Production Publish Script
# Packages all 4 microservices into ready-to-deploy IIS folders with web.config
# ============================================================================

param (
    [string]$Service = "all",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$rootDir = $PSScriptRoot

$services = @(
    @{ Name = "users";      Project = "$rootDir\API\RailCat.Users.Api\RailCat.Users.Api.csproj";           OutDir = "$rootDir\API\RailCat.Users.Api\bin\$Configuration\net8.0\publish" },
    @{ Name = "catalog";    Project = "$rootDir\API\RailCat.Catalog.Api\RailCat.Catalog.Api.csproj";       OutDir = "$rootDir\API\RailCat.Catalog.Api\bin\$Configuration\net8.0\publish" },
    @{ Name = "orders";     Project = "$rootDir\API\RailCat.Orders.Api\RailCat.Orders.Api.csproj";         OutDir = "$rootDir\API\RailCat.Orders.Api\bin\$Configuration\net8.0\publish" },
    @{ Name = "settlement"; Project = "$rootDir\API\RailCat.Settlement.Api\RailCat.Settlement.Api.csproj"; OutDir = "$rootDir\API\RailCat.Settlement.Api\bin\$Configuration\net8.0\publish" }
)

Write-Host "==============================================================" -ForegroundColor Cyan
Write-Host "RailCat Production Publish: Configuration = $Configuration" -ForegroundColor Cyan
Write-Host "==============================================================" -ForegroundColor Cyan

foreach ($svc in $services) {
    if ($Service -ne "all" -and $Service -ne $svc.Name) {
        continue
    }

    Write-Host "`n---> Publishing [$($svc.Name)]..." -ForegroundColor Yellow
    if (Test-Path $svc.OutDir) {
        Remove-Item -Recurse -Force $svc.OutDir
    }

    dotnet publish $svc.Project -c $Configuration -o $svc.OutDir
    
    if (Test-Path "$($svc.OutDir)\web.config") {
        Write-Host "     [OK] Published successfully with web.config: $($svc.OutDir)" -ForegroundColor Green
    } else {
        Write-Host "     [WARN] Published without web.config: $($svc.OutDir)" -ForegroundColor DarkYellow
    }
}

Write-Host "`n==============================================================" -ForegroundColor Green
Write-Host "ALL REQUESTED SERVICES PUBLISHED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "Ready to copy to IIS physical folders." -ForegroundColor Green
Write-Host "==============================================================" -ForegroundColor Green

