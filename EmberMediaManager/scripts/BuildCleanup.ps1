<#
.SYNOPSIS
    Deep clean build outputs for Ember Media Manager solution.

.DESCRIPTION
    Removes all build artifacts including bin, obj folders and solution-level build directories.
    Optionally removes the .vs folder (Visual Studio cache) with -Deep mode.
    Creates backups are NOT created - this script permanently deletes build outputs.

.PARAMETER Help
    Displays help information and usage examples.

.PARAMETER Deep
    Include .vs folder (Visual Studio cache) in cleanup. Use for complete reset.

.PARAMETER Force
    Skip Visual Studio running check and proceed without confirmation.
    Required when running as an external tool in Visual Studio.

.PARAMETER KeepActiveConfig
    Reserved for future use. Currently not implemented.

.EXAMPLE
    .\BuildCleanup.ps1 -Help
    Displays help information and usage examples.

.EXAMPLE
    .\BuildCleanup.ps1
    Performs normal cleanup (keeps .vs folder).

.EXAMPLE
    .\BuildCleanup.ps1 -Deep
    Performs deep cleanup including .vs folder.

.EXAMPLE
    .\BuildCleanup.ps1 -Force
    Runs cleanup without VS running check (for use as external tool).

.EXAMPLE
    .\BuildCleanup.ps1 -Deep -Force
    Deep cleanup from within Visual Studio without prompts.

.NOTES
    Author: Eric H. Anderson
    Version: 1.1.0
    Last Updated: 2025-12-25
    
    WARNING: This script permanently deletes build outputs.
    DO NOT run during active development - use "Clean Solution" instead.
    
    Removes:
    - EmberMM - Debug/Release - x86/x64/AnyCPU folders
    - All bin folders (except in packages)
    - All obj folders (except in packages)
    - .vs folder (only with -Deep)
#>

[CmdletBinding()]
param(
    [switch]$Help,
    [switch]$Deep,
    [switch]$Force,
    [switch]$KeepActiveConfig
)

#region Help Display

if ($Help) {
    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host " Ember Media Manager - Build Cleanup" -ForegroundColor Cyan
    Write-Host " Version 1.1.0" -ForegroundColor Cyan
    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "DESCRIPTION:" -ForegroundColor Yellow
    Write-Host "  Removes all build artifacts from the solution."
    Write-Host "  Use for deep cleaning when 'Clean Solution' isn't enough."
    Write-Host ""
    Write-Host "PARAMETERS:" -ForegroundColor Yellow
    Write-Host "  -Help    Show this help message"
    Write-Host "  -Deep    Include .vs folder in cleanup (complete reset)"
    Write-Host "  -Force   Skip VS running check (required for external tool)"
    Write-Host ""
    Write-Host "USAGE EXAMPLES:" -ForegroundColor Yellow
    Write-Host "  # Normal cleanup (keeps .vs folder)" -ForegroundColor DarkGray
    Write-Host "  .\BuildCleanup.ps1"
    Write-Host ""
    Write-Host "  # Deep cleanup (removes .vs folder)" -ForegroundColor DarkGray
    Write-Host "  .\BuildCleanup.ps1 -Deep"
    Write-Host ""
    Write-Host "  # Run from within Visual Studio as external tool" -ForegroundColor DarkGray
    Write-Host "  .\BuildCleanup.ps1 -Force"
    Write-Host ""
    Write-Host "  # Deep cleanup from within Visual Studio" -ForegroundColor DarkGray
    Write-Host "  .\BuildCleanup.ps1 -Deep -Force"
    Write-Host ""
    Write-Host "EXTERNAL TOOL SETUP:" -ForegroundColor Yellow
    Write-Host "  Tools > External Tools > Add"
    Write-Host "  Title:     Deep Clean"
    Write-Host "  Command:   powershell.exe"
    Write-Host "  Arguments: -ExecutionPolicy Bypass -File"
    Write-Host "             `"`$(SolutionDir)EmberMediaManager\scripts\BuildCleanup.ps1`" -Force"
    Write-Host ""
    Write-Host "REMOVES:" -ForegroundColor Yellow
    Write-Host "  • Solution-level build folders (EmberMM - Debug/Release - x86/x64/AnyCPU)"
    Write-Host "  • All project bin folders"
    Write-Host "  • All project obj folders"
    Write-Host "  • .vs folder (only with -Deep)"
    Write-Host ""
    Write-Host "WARNING:" -ForegroundColor Red
    Write-Host "  This permanently deletes build outputs. No backups created."
    Write-Host "  DO NOT run during active development."
    Write-Host ""
    exit 0
}

#endregion

#region Script Setup

# Navigate to solution root (script is in EmberMediaManager\scripts)
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionRoot = Split-Path -Parent (Split-Path -Parent $scriptDir)
Push-Location $solutionRoot

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Ember Media Manager - Build Cleanup" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Solution Root: $solutionRoot" -ForegroundColor DarkGray
Write-Host ""

if ($Deep) {
    Write-Host "Mode: DEEP (includes .vs cleanup)" -ForegroundColor Cyan
} else {
    Write-Host "Mode: NORMAL (keeps .vs folder)" -ForegroundColor Cyan
}
Write-Host ""

#endregion

#region VS Running Check

# Verify VS is closed (skip if -Force is specified)
if (-not $Force) {
    $vsProcesses = Get-Process devenv -ErrorAction SilentlyContinue
    if ($vsProcesses) {
        Write-Host "⚠️  WARNING: Visual Studio is still running!" -ForegroundColor Yellow
        Write-Host "   It's recommended to close VS before deep cleaning." -ForegroundColor Yellow
        $continue = Read-Host "   Continue anyway? (y/N)"
        if ($continue -ne 'y' -and $continue -ne 'Y') {
            Write-Host "Cancelled." -ForegroundColor Gray
            Pop-Location
            exit 0
        }
        Write-Host ""
    }
}

#endregion

#region Remove Solution-Level Build Directories

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

#endregion

#region Remove obj Folders

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

#endregion

#region Remove bin Folders

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

#endregion

#region Remove .vs Folder (Deep Mode Only)

if ($Deep) {
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
} else {
    Write-Host "ℹ️  Skipping .vs folder cleanup (use -Deep to include it)" -ForegroundColor Gray
    Write-Host ""
}

#endregion

#region Cleanup and Summary

# Return to original directory
Pop-Location

Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "  ✅ Build Cleanup Complete!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Next steps:" -ForegroundColor Cyan
Write-Host "   1. Build → Rebuild Solution" -ForegroundColor White
Write-Host ""

#endregion