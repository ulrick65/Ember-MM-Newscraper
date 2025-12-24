# Deep Clean Script for Ember Media Manager
# Run this when you want to completely remove all build outputs
# DO NOT run this during active development - use "Clean Solution" for that

param(
    [switch]$KeepActiveConfig
)

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Ember Media Manager - Deep Clean" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# Verify VS is closed (optional - comment out if annoying)
$vsProcesses = Get-Process devenv -ErrorAction SilentlyContinue
if ($vsProcesses) {
    Write-Host "⚠️  WARNING: Visual Studio is still running!" -ForegroundColor Yellow
    Write-Host "   It's recommended to close VS before deep cleaning." -ForegroundColor Yellow
    $continue = Read-Host "   Continue anyway? (y/N)"
    if ($continue -ne 'y' -and $continue -ne 'Y') {
        Write-Host "Cancelled." -ForegroundColor Gray
        exit 0
    }
    Write-Host ""
}

# Remove solution-level build directories
Write-Host "🗑️  Removing solution-level build directories..." -ForegroundColor Yellow
$buildDirs = @(
    "EmberMM - Debug - x86",
    "EmberMM - Debug - x64",
    "EmberMM - Debug - AnyCPU",
    "EmberMM - Release - x86",
    "EmberMM - Release - x64",
    "EmberMM - Release - AnyCPU"
)

$removedCount = 0
foreach ($dir in $buildDirs) {
    if (Test-Path $dir) {
        Write-Host "   Removing: $dir" -ForegroundColor Gray
        Remove-Item -Path $dir -Recurse -Force -ErrorAction SilentlyContinue
        if (-not (Test-Path $dir)) {
            $removedCount++
        } else {
            Write-Host "   ⚠️  Could not fully remove: $dir (some files may be locked)" -ForegroundColor Yellow
        }
    }
}
Write-Host "   ✅ Removed $removedCount build folder(s)" -ForegroundColor Green
Write-Host ""

# Remove all obj folders
Write-Host "🗑️  Removing obj folders from all projects..." -ForegroundColor Yellow
$objFolders = Get-ChildItem -Path . -Recurse -Directory -Filter "obj" -ErrorAction SilentlyContinue | Where-Object { $_.FullName -notlike "*\packages\*" }
$objCount = ($objFolders | Measure-Object).Count
if ($objCount -gt 0) {
    $objFolders | ForEach-Object {
        Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }
    Write-Host "   ✅ Removed $objCount obj folder(s)" -ForegroundColor Green
} else {
    Write-Host "   ℹ️  No obj folders found" -ForegroundColor Gray
}
Write-Host ""

# Remove .vs folder (Visual Studio cache)
Write-Host "🗑️  Removing .vs folder (Visual Studio cache)..." -ForegroundColor Yellow
if (Test-Path ".vs") {
    Remove-Item -Path ".vs" -Recurse -Force -ErrorAction SilentlyContinue
    if (-not (Test-Path ".vs")) {
        Write-Host "   ✅ Removed .vs folder" -ForegroundColor Green
    } else {
        Write-Host "   ⚠️  Could not fully remove .vs folder (VS may be running)" -ForegroundColor Yellow
    }
} else {
    Write-Host "   ℹ️  No .vs folder found" -ForegroundColor Gray
}
Write-Host ""

# Optional: Remove bin folders (uncomment if desired)
Write-Host "🗑️  Removing bin folders from all projects..." -ForegroundColor Yellow
$binFolders = Get-ChildItem -Path . -Recurse -Directory -Filter "bin" -ErrorAction SilentlyContinue | Where-Object { $_.FullName -notlike "*\packages\*" }
$binCount = ($binFolders | Measure-Object).Count
if ($binCount -gt 0) {
    $binFolders | ForEach-Object {
    Remove-Item -Path $_.FullName -Recurse -Force -ErrorAction SilentlyContinue
    }
    Write-Host "   ✅ Removed $binCount bin folder(s)" -ForegroundColor Green
    } else {
    Write-Host "   ℹ️  No bin folders found" -ForegroundColor Gray
}
Write-Host ""

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  ✅ Deep Clean Complete!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Next steps:" -ForegroundColor Cyan
Write-Host "   1. Reopen Visual Studio (if closed)" -ForegroundColor White
Write-Host "   2. Restore NuGet packages (right-click solution → Restore NuGet Packages)" -ForegroundColor White
Write-Host "   3. Build → Rebuild Solution" -ForegroundColor White
Write-Host ""