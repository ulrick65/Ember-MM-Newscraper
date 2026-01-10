<#
.SYNOPSIS
    Deep clean build outputs for Ember Media Manager solution.

.DESCRIPTION
    Removes all build artifacts including bin, obj folders and solution-level build directories.
    Optionally removes the .vs folder (Visual Studio cache) with -Deep mode.
    Optionally rebuilds the solution after cleanup with -Rebuild mode.
    Creates backups are NOT created - this script permanently deletes build outputs.

.PARAMETER Help
    Displays help information and usage examples.

.PARAMETER Deep
    Include .vs folder (Visual Studio cache) in cleanup. Use for complete reset.

.PARAMETER Force
    Skip Visual Studio running check and proceed without confirmation.
    Required when running as an external tool in Visual Studio.

.PARAMETER Rebuild
    After cleanup, automatically rebuild the solution using MSBuild.
    Defaults to Debug configuration and x64 platform.

.PARAMETER Configuration
    Build configuration to use with -Rebuild. Default: Debug

.PARAMETER Platform
    Build platform to use with -Rebuild. Default: x64

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
    .\BuildCleanup.ps1 -Rebuild
    Performs cleanup then rebuilds the solution (Debug x64).

.EXAMPLE
    .\BuildCleanup.ps1 -Deep -Rebuild -Configuration Release -Platform x86
    Deep cleanup then rebuild Release x86.

.EXAMPLE
    .\BuildCleanup.ps1 -Deep -Force -Rebuild
    Deep cleanup and rebuild from within Visual Studio without prompts.

.NOTES
    ============================================================
    Document Info
    ============================================================
    Version:      1.2.1
    Created:      December 25, 2025
    Updated:      January 2, 2026
    Author:       Eric H. Anderson
    Purpose:      Deep clean and optionally rebuild Ember Media Manager
    ============================================================
    
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
    [switch]$Rebuild,
    [string]$Configuration = "Debug",
    [string]$Platform = "x64",
    [switch]$KeepActiveConfig
)

#region Help Display

if ($Help) {
    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host " Ember Media Manager - Build Cleanup" -ForegroundColor Cyan
    Write-Host " Version 1.2.1" -ForegroundColor Cyan
    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "DESCRIPTION:" -ForegroundColor Yellow
    Write-Host "  Removes all build artifacts from the solution."
    Write-Host "  Use for deep cleaning when 'Clean Solution' isn't enough."
    Write-Host "  Optionally rebuilds the solution after cleanup."
    Write-Host ""
    Write-Host "PARAMETERS:" -ForegroundColor Yellow
    Write-Host "  -Help          Show this help message"
    Write-Host "  -Deep          Include .vs folder in cleanup (complete reset)"
    Write-Host "  -Force         Skip VS running check (required for external tool)"
    Write-Host "  -Rebuild       Rebuild solution after cleanup"
    Write-Host "  -Configuration Build configuration (default: Debug)"
    Write-Host "  -Platform      Build platform (default: x64)"
    Write-Host ""
    Write-Host "USAGE EXAMPLES:" -ForegroundColor Yellow
    Write-Host "  # Normal cleanup (keeps .vs folder)" -ForegroundColor DarkGray
    Write-Host "  .\BuildCleanup.ps1"
    Write-Host ""
    Write-Host "  # Deep cleanup (removes .vs folder)" -ForegroundColor DarkGray
    Write-Host "  .\BuildCleanup.ps1 -Deep"
    Write-Host ""
    Write-Host "  # Cleanup and rebuild (Debug x64)" -ForegroundColor DarkGray
    Write-Host "  .\BuildCleanup.ps1 -Rebuild"
    Write-Host ""
    Write-Host "  # Deep cleanup and rebuild Release x86" -ForegroundColor DarkGray
    Write-Host "  .\BuildCleanup.ps1 -Deep -Rebuild -Configuration Release -Platform x86"
    Write-Host ""
    Write-Host "  # Run from within Visual Studio as external tool" -ForegroundColor DarkGray
    Write-Host "  .\BuildCleanup.ps1 -Force -Rebuild"
    Write-Host ""
    Write-Host "EXTERNAL TOOL SETUP:" -ForegroundColor Yellow
    Write-Host "  Tools > External Tools > Add"
    Write-Host "  Title:     Deep Clean & Rebuild"
    Write-Host "  Command:   powershell.exe"
    Write-Host "  Arguments: -ExecutionPolicy Bypass -File"
    Write-Host "             `"`$(SolutionDir)EmberMediaManager\scripts\BuildCleanup.ps1`" -Force -Rebuild"
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
Write-Host "  Ember Media Manager - Build Cleanup v1.2.1" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "Solution Root: $solutionRoot" -ForegroundColor DarkGray
Write-Host ""

$modeDesc = "NORMAL"
if ($Deep) { $modeDesc = "DEEP (includes .vs cleanup)" }
if ($Rebuild) { $modeDesc += " + REBUILD ($Configuration|$Platform)" }
Write-Host "Mode: $modeDesc" -ForegroundColor Cyan
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

#region Rebuild Solution

if ($Rebuild) {
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  🔨 Rebuilding Solution..." -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Configuration: $Configuration" -ForegroundColor Gray
    Write-Host "Platform:      $Platform" -ForegroundColor Gray
    Write-Host ""

    # Find MSBuild using vswhere (the official way to locate VS installations)
    $msbuild = $null
    
    # Try vswhere first (works for all VS editions including Preview/Insiders)
    $vswherePath = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswherePath) {
        Write-Host "   Searching for MSBuild using vswhere..." -ForegroundColor Gray
        $msbuild = & $vswherePath -latest -prerelease -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | Select-Object -First 1
    }

    # Fallback to known paths if vswhere didn't find anything
    if (-not $msbuild) {
        Write-Host "   Searching known MSBuild paths..." -ForegroundColor Gray
        $msbuildPaths = @(
            # VS 2022 Preview/Insiders
            "${env:ProgramFiles}\Microsoft Visual Studio\2022\Preview\MSBuild\Current\Bin\MSBuild.exe",
            # VS 2022 Standard editions
            "${env:ProgramFiles}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
            "${env:ProgramFiles}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
            "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
            # VS 2019 editions
            "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
            "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe",
            "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
            # Build Tools (standalone MSBuild)
            "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
            "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
        )

        foreach ($path in $msbuildPaths) {
            if (Test-Path $path) {
                $msbuild = $path
                break
            }
        }
    }

    if (-not $msbuild) {
        Write-Host "❌ ERROR: Could not find MSBuild.exe" -ForegroundColor Red
        Write-Host "   Please ensure Visual Studio or Build Tools is installed." -ForegroundColor Red
        Write-Host ""
        Write-Host "   Searched locations:" -ForegroundColor Gray
        Write-Host "   - vswhere.exe (for all VS editions including Preview)" -ForegroundColor Gray
        Write-Host "   - VS 2022/2019 Enterprise, Professional, Community, Preview" -ForegroundColor Gray
        Write-Host "   - Build Tools 2022/2019" -ForegroundColor Gray
        Pop-Location
        exit 1
    }

    Write-Host "MSBuild:       $msbuild" -ForegroundColor Gray
    Write-Host ""

    # Find solution file
    $solutionFile = Get-ChildItem -Path . -Filter "*.sln" | Select-Object -First 1
    if (-not $solutionFile) {
        Write-Host "❌ ERROR: Could not find solution file in $solutionRoot" -ForegroundColor Red
        Pop-Location
        exit 1
    }

    Write-Host "Solution:      $($solutionFile.Name)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Starting build..." -ForegroundColor Yellow
    Write-Host ""

    # Run MSBuild with normal verbosity to capture project summary
    $buildArgs = @(
        $solutionFile.FullName,
        "/t:Rebuild",
        "/p:Configuration=$Configuration",
        "/p:Platform=$Platform",
        "/m",
        "/v:normal",
        "/nologo",
        "/consoleloggerparameters:Summary"
    )

    & $msbuild @buildArgs
    $buildExitCode = $LASTEXITCODE

    Write-Host ""
    if ($buildExitCode -eq 0) {
        Write-Host "========== Rebuild All: Build succeeded ==========" -ForegroundColor Green
    } else {
        Write-Host "========== Rebuild All: Build FAILED ==========" -ForegroundColor Red
        Pop-Location
        exit $buildExitCode
    }
}

#endregion

# Return to original directory
Pop-Location
