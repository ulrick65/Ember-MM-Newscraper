<#
.SYNOPSIS
    Updates assembly versions for Ember Media Manager projects.

.DESCRIPTION
    Reads version configuration from VersionConfig.json and updates AssemblyInfo files.
    Supports both VB.NET and C# projects. Creates backups and exports results.

.PARAMETER Help
    Displays help information and usage examples.

.PARAMETER WhatIf
    Shows what changes would be made without actually modifying files.

.PARAMETER IncrementBuild
    Increments the build number for all projects instead of using config versions.
    Example: 1.12.0.0 -> 1.12.1.0

.PARAMETER Category
    Update only specific category: Core, GenericAddons, DataScrapers, ImageScrapers, TrailerScrapers, ThemeScrapers, All

.PARAMETER IncludeDeprecated
    Include deprecated projects in the update. By default, deprecated projects are skipped.

.PARAMETER NoBackup
    Skip creating backup files. Use with caution.

.PARAMETER ConfigPath
    Path to the version configuration JSON file. Defaults to VersionConfig.json in script directory.

.EXAMPLE
    .\Update-AssemblyVersions.ps1 -Help
    Displays help information and usage examples.

.EXAMPLE
    .\Update-AssemblyVersions.ps1 -WhatIf
    Shows what changes would be made without modifying files.

.EXAMPLE
    .\Update-AssemblyVersions.ps1 -Category GenericAddons
    Updates only generic addon projects.

.EXAMPLE
    .\Update-AssemblyVersions.ps1 -IncrementBuild -Category All
    Increments build number for all active projects.

.EXAMPLE
    .\Update-AssemblyVersions.ps1 -IncludeDeprecated
    Updates all projects including deprecated ones.

.NOTES
    Author: Eric H. Anderson
    Version: 2.1.0
    Last Updated: 2025-12-25
    
    Configuration: VersionConfig.json in the scripts directory
    Results: Saved to EmberMediaManager\docs\VersionUpdates_*.csv
    Backups: Created in solution root as AssemblyInfo_Backup_*
#>

[CmdletBinding(SupportsShouldProcess)]
param(
    [switch]$Help,
    
    [switch]$IncrementBuild,
    
    [ValidateSet("Core", "GenericAddons", "DataScrapers", "ImageScrapers", "TrailerScrapers", "ThemeScrapers", "All")]
    [string]$Category = "All",
    
    [switch]$IncludeDeprecated,
    
    [switch]$NoBackup,
    
    [string]$ConfigPath
)

#region Help Display
# Shows usage information when -Help is specified

if ($Help) {
    Write-Host ""
    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host " Ember Media Manager - Assembly Version Updater" -ForegroundColor Cyan
    Write-Host " Version 2.1.0" -ForegroundColor Cyan
    Write-Host "============================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "DESCRIPTION:" -ForegroundColor Yellow
    Write-Host "  Updates assembly versions across all projects in the solution."
    Write-Host "  Reads target versions from VersionConfig.json."
    Write-Host ""
    Write-Host "PARAMETERS:" -ForegroundColor Yellow
    Write-Host "  -Help              Show this help message"
    Write-Host "  -WhatIf            Preview changes without modifying files"
    Write-Host "  -IncrementBuild    Auto-increment build number (ignores config versions)"
    Write-Host "  -Category          Filter by category:"
    Write-Host "                       Core, GenericAddons, DataScrapers,"
    Write-Host "                       ImageScrapers, TrailerScrapers, ThemeScrapers, All"
    Write-Host "  -IncludeDeprecated Include deprecated projects (skipped by default)"
    Write-Host "  -NoBackup          Skip backup creation"
    Write-Host "  -ConfigPath        Custom path to VersionConfig.json"
    Write-Host ""
    Write-Host "USAGE EXAMPLES:" -ForegroundColor Yellow
    Write-Host "  # Preview all changes" -ForegroundColor DarkGray
    Write-Host "  .\Update-AssemblyVersions.ps1 -WhatIf"
    Write-Host ""
    Write-Host "  # Update all active projects to config versions" -ForegroundColor DarkGray
    Write-Host "  .\Update-AssemblyVersions.ps1"
    Write-Host ""
    Write-Host "  # Increment build number for all projects" -ForegroundColor DarkGray
    Write-Host "  .\Update-AssemblyVersions.ps1 -IncrementBuild -IncludeDeprecated"
    Write-Host ""
    Write-Host "  # Update only scrapers" -ForegroundColor DarkGray
    Write-Host "  .\Update-AssemblyVersions.ps1 -Category DataScrapers"
    Write-Host ""
    Write-Host "FILES:" -ForegroundColor Yellow
    Write-Host "  Config:   EmberMediaManager\scripts\VersionConfig.json"
    Write-Host "  Results:  EmberMediaManager\docs\VersionUpdates_*.csv"
    Write-Host "  Backups:  <SolutionRoot>\AssemblyInfo_Backup_*"
    Write-Host ""
    Write-Host "WORKFLOW:" -ForegroundColor Yellow
    Write-Host "  1. Edit VersionConfig.json with new target versions"
    Write-Host "  2. Run with -WhatIf to preview changes"
    Write-Host "  3. Run without -WhatIf to apply changes"
    Write-Host "  4. Rebuild solution to verify"
    Write-Host ""
    exit 0
}

#endregion

#region Script Setup
# Initialize paths and variables used throughout the script

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionRoot = Split-Path -Parent (Split-Path -Parent $scriptDir)
$docsDir = Join-Path $solutionRoot "EmberMediaManager\docs"

# Default config path if not specified
if (-not $ConfigPath) {
    $ConfigPath = Join-Path $scriptDir "VersionConfig.json"
}

# Timestamp for unique backup folder and results file names
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

#endregion

#region Functions

function Write-Banner {
    # Displays a formatted section header
    param([string]$Title)
    
    $line = "=" * 60
    Write-Host ""
    Write-Host $line -ForegroundColor Cyan
    Write-Host " $Title" -ForegroundColor Cyan
    Write-Host $line -ForegroundColor Cyan
    Write-Host ""
}

function Write-Status {
    # Displays a color-coded status message
    # Status types: OK (green), SKIP (yellow), ERROR (red), WHATIF (cyan), INFO (white), BACKUP (gray)
    param(
        [string]$Status,
        [string]$Message,
        [string]$Detail = ""
    )
    
    $statusColors = @{
        "OK"      = "Green"
        "SKIP"    = "Yellow"
        "ERROR"   = "Red"
        "WHATIF"  = "Cyan"
        "INFO"    = "White"
        "BACKUP"  = "DarkGray"
    }
    
    $color = $statusColors[$Status]
    if (-not $color) { $color = "White" }
    
    Write-Host "  [$Status] " -ForegroundColor $color -NoNewline
    Write-Host $Message -NoNewline
    if ($Detail) {
        Write-Host " - $Detail" -ForegroundColor DarkGray
    } else {
        Write-Host ""
    }
}

function Get-VersionFromFile {
    # Extracts the AssemblyVersion from file content
    # Skips commented lines to avoid picking up old versions in comments
    param(
        [string[]]$Lines,
        [string]$FileType  # "vb" or "cs"
    )
    
    foreach ($line in $Lines) {
        # Skip comment lines
        if ($FileType -eq "vb" -and $line.TrimStart().StartsWith("'")) { continue }
        if ($FileType -eq "cs" -and $line.TrimStart().StartsWith("//")) { continue }
        
        # Match version attribute based on file type
        if ($FileType -eq "vb") {
            if ($line -match '<Assembly:\s*AssemblyVersion\("([^"]+)"\)>') {
                return $matches[1]
            }
        }
        elseif ($FileType -eq "cs") {
            if ($line -match '\[assembly:\s*AssemblyVersion\("([^"]+)"\)\]') {
                return $matches[1]
            }
        }
    }
    return $null
}

function Get-IncrementedVersion {
    # Increments the build number (third part) and resets revision to 0
    # Example: 1.12.0.0 -> 1.12.1.0
    param([string]$Version)
    
    if ($Version -match '^(\d+)\.(\d+)\.(\d+)\.(\d+)$') {
        $major = $matches[1]
        $minor = $matches[2]
        $build = [int]$matches[3] + 1
        $revision = 0
        return "$major.$minor.$build.$revision"
    }
    return $null
}

function Update-AssemblyInfoFile {
    # Updates both AssemblyVersion and AssemblyFileVersion in the file
    # Returns a hashtable with Success, Skipped, OldVersion, NewVersion, and Message
    param(
        [string]$FilePath,
        [string]$NewVersion,
        [string]$FileType,
        [switch]$WhatIf
    )
    
    $lines = Get-Content -Path $FilePath
    $currentVersion = Get-VersionFromFile -Lines $lines -FileType $FileType
    
    if (-not $currentVersion) {
        return @{ Success = $false; Message = "Could not find version" }
    }
    
    # Skip if already at target version
    if ($currentVersion -eq $NewVersion) {
        return @{ Success = $true; Skipped = $true; Message = "Already at version $NewVersion" }
    }
    
    # Process each line, updating version attributes while preserving comments
    $updatedLines = foreach ($line in $lines) {
        # Keep comment lines unchanged
        if (($FileType -eq "vb" -and $line.TrimStart().StartsWith("'")) -or
            ($FileType -eq "cs" -and $line.TrimStart().StartsWith("//"))) {
            $line
            continue
        }
        
        # Update VB.NET version attributes
        if ($FileType -eq "vb") {
            if ($line -match '<Assembly:\s*AssemblyVersion\("' + [regex]::Escape($currentVersion) + '"\)>') {
                $line -replace [regex]::Escape("`"$currentVersion`""), "`"$NewVersion`""
            }
            elseif ($line -match '<Assembly:\s*AssemblyFileVersion\("' + [regex]::Escape($currentVersion) + '"\)>') {
                $line -replace [regex]::Escape("`"$currentVersion`""), "`"$NewVersion`""
            }
            else {
                $line
            }
        }
        # Update C# version attributes
        elseif ($FileType -eq "cs") {
            if ($line -match '\[assembly:\s*AssemblyVersion\("' + [regex]::Escape($currentVersion) + '"\)\]') {
                $line -replace [regex]::Escape("`"$currentVersion`""), "`"$NewVersion`""
            }
            elseif ($line -match '\[assembly:\s*AssemblyFileVersion\("' + [regex]::Escape($currentVersion) + '"\)\]') {
                $line -replace [regex]::Escape("`"$currentVersion`""), "`"$NewVersion`""
            }
            else {
                $line
            }
        }
    }
    
    # Write changes unless in WhatIf mode
    if (-not $WhatIf) {
        Set-Content -Path $FilePath -Value $updatedLines -Encoding UTF8
    }
    
    return @{
        Success = $true
        Skipped = $false
        OldVersion = $currentVersion
        NewVersion = $NewVersion
    }
}

function New-BackupDirectory {
    # Creates a timestamped backup directory in the solution root
    param([string]$SolutionRoot)
    
    $backupDir = Join-Path $SolutionRoot "AssemblyInfo_Backup_$timestamp"
    New-Item -ItemType Directory -Path $backupDir -Force | Out-Null
    return $backupDir
}

function Backup-AssemblyInfo {
    # Copies an AssemblyInfo file to the backup directory, preserving folder structure
    param(
        [string]$SourcePath,
        [string]$BackupDir,
        [string]$SolutionRoot
    )
    
    # Calculate relative path to maintain folder structure in backup
    $relativePath = $SourcePath.Replace($SolutionRoot, "").TrimStart("\")
    $backupPath = Join-Path $BackupDir $relativePath
    $backupFolder = Split-Path -Parent $backupPath
    
    if (-not (Test-Path $backupFolder)) {
        New-Item -ItemType Directory -Path $backupFolder -Force | Out-Null
    }
    
    Copy-Item -Path $SourcePath -Destination $backupPath -Force
    return $backupPath
}

#endregion

#region Main Script

Write-Banner "Ember Media Manager - Assembly Version Updater v2.1"

# Load and validate configuration file
if (-not (Test-Path $ConfigPath)) {
    Write-Host "ERROR: Configuration file not found: $ConfigPath" -ForegroundColor Red
    Write-Host "Create VersionConfig.json in the scripts directory." -ForegroundColor Yellow
    Write-Host "Run with -Help for more information." -ForegroundColor Yellow
    exit 1
}

Write-Status "INFO" "Loading configuration" $ConfigPath
$config = Get-Content $ConfigPath -Raw | ConvertFrom-Json

# Display current settings
Write-Host ""
Write-Host "  Solution Root:      $solutionRoot" -ForegroundColor DarkGray
Write-Host "  Category:           $Category" -ForegroundColor DarkGray
Write-Host "  Include Deprecated: $IncludeDeprecated" -ForegroundColor DarkGray
Write-Host "  Increment Build:    $IncrementBuild" -ForegroundColor DarkGray
Write-Host "  WhatIf Mode:        $WhatIfPreference" -ForegroundColor DarkGray
Write-Host ""

# Map parameter category names to JSON property names
$categoryMap = @{
    "Core"            = "core"
    "GenericAddons"   = "genericAddons"
    "DataScrapers"    = "dataScrapers"
    "ImageScrapers"   = "imageScrapers"
    "TrailerScrapers" = "trailerScrapers"
    "ThemeScrapers"   = "themeScrapers"
}

# Collect projects based on selected category
$projectsToProcess = @()
if ($Category -eq "All") {
    foreach ($cat in $categoryMap.Values) {
        $projectsToProcess += $config.projects.$cat
    }
}
else {
    $catKey = $categoryMap[$Category]
    $projectsToProcess += $config.projects.$catKey
}

# Filter out deprecated projects unless explicitly included
if (-not $IncludeDeprecated) {
    $projectsToProcess = $projectsToProcess | Where-Object { $_.status -ne "deprecated" }
}

Write-Host "  Projects to process: $($projectsToProcess.Count)" -ForegroundColor Green
Write-Host ""

# Create backup directory (skip in WhatIf mode)
$backupDir = $null
if (-not $NoBackup -and -not $WhatIfPreference) {
    $backupDir = New-BackupDirectory -SolutionRoot $solutionRoot
    Write-Status "BACKUP" "Backup directory created" $backupDir
    Write-Host ""
}

# Process each project
$results = @()
$updatedCount = 0
$skippedCount = 0
$errorCount = 0

Write-Host "Processing projects..." -ForegroundColor Cyan
Write-Host ""

foreach ($project in $projectsToProcess) {
    $fullPath = Join-Path $solutionRoot $project.path
    $projectName = $project.name
    
    # Verify file exists
    if (-not (Test-Path $fullPath)) {
        Write-Status "ERROR" $projectName "File not found"
        $errorCount++
        continue
    }
    
    # Determine target version (from config or auto-increment)
    if ($IncrementBuild) {
        $lines = Get-Content -Path $fullPath
        $currentVersion = Get-VersionFromFile -Lines $lines -FileType $project.type
        $targetVersion = Get-IncrementedVersion -Version $currentVersion
        if (-not $targetVersion) {
            Write-Status "ERROR" $projectName "Could not increment version"
            $errorCount++
            continue
        }
    }
    else {
        $targetVersion = $project.version
    }
    
    # Create backup before modifying
    if ($backupDir -and -not $WhatIfPreference) {
        Backup-AssemblyInfo -SourcePath $fullPath -BackupDir $backupDir -SolutionRoot $solutionRoot | Out-Null
    }
    
    # Update the file
    $result = Update-AssemblyInfoFile -FilePath $fullPath -NewVersion $targetVersion -FileType $project.type -WhatIf:$WhatIfPreference
    
    # Handle result
    if ($result.Success) {
        if ($result.Skipped) {
            Write-Status "SKIP" $projectName $result.Message
            $skippedCount++
        }
        else {
            if ($WhatIfPreference) {
                Write-Status "WHATIF" $projectName "$($result.OldVersion) -> $($result.NewVersion)"
            }
            else {
                Write-Status "OK" $projectName "$($result.OldVersion) -> $($result.NewVersion)"
            }
            $updatedCount++
            
            # Track results for CSV export
            $results += [PSCustomObject]@{
                Project = $projectName
                Path = $project.path
                OldVersion = $result.OldVersion
                NewVersion = $result.NewVersion
                Status = $project.status
                Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
            }
        }
    }
    else {
        Write-Status "ERROR" $projectName $result.Message
        $errorCount++
    }
}

# Display summary
Write-Banner "Summary"

Write-Host "  Updated:  $updatedCount" -ForegroundColor Green
Write-Host "  Skipped:  $skippedCount" -ForegroundColor Yellow
Write-Host "  Errors:   $errorCount" -ForegroundColor Red
Write-Host ""

if ($backupDir) {
    Write-Host "  Backup:   $backupDir" -ForegroundColor DarkGray
}

# Export results to CSV in docs folder
if ($results.Count -gt 0 -and -not $WhatIfPreference) {
    if (-not (Test-Path $docsDir)) {
        New-Item -ItemType Directory -Path $docsDir -Force | Out-Null
    }
    $resultsFile = Join-Path $docsDir "VersionUpdates_$timestamp.csv"
    $results | Export-Csv -Path $resultsFile -NoTypeInformation
    Write-Host "  Results:  $resultsFile" -ForegroundColor DarkGray
}

Write-Host ""

if ($WhatIfPreference) {
    Write-Host "  [WHATIF] No files were modified. Run without -WhatIf to apply changes." -ForegroundColor Yellow
}
else {
    Write-Host "  Version updates complete! Remember to rebuild the solution." -ForegroundColor Green
}

Write-Host ""

#endregion