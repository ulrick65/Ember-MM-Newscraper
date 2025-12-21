# ============================================================================
# Script: Update-ModifiedAddons-SAFE.ps1
# Purpose: Increment build number for 28 addons modified by .NET 4.8 upgrade
#          Build number increments, revision resets to 0
#          SAFE: Preserves exact file formatting and line endings
# ============================================================================

$rootPath = "D:\OneDrive\MyDocs\Programming\Dev\Ember-MM-Newscraper-nagten"
$backupFolder = Join-Path $rootPath "AssemblyInfo_Backup_Addons_$(Get-Date -Format 'yyyyMMdd_HHmmss')"

# Get exact list of modified addons from Git
$modifiedAddons = @(
    "generic.EmberCore.BulkRename",
    "generic.EmberCore.ContextMenu",
    "generic.EmberCore.FilterEditor",
    "generic.EmberCore.Mapping",
    "generic.EmberCore.MediaFileManager",
    "generic.Embercore.MetadataEditor",
    "generic.EmberCore.MovieExport",
    "generic.EmberCore.TagManager",
    "generic.EmberCore.VideoSourceMapping",
    "generic.Interface.Kodi",
    "generic.Interface.Trakttv",
    "scraper.Apple.Trailer",
    "scraper.Data.OMDb",
    "scraper.Data.TVDB",
    "scraper.Davestrailerpage.Trailer",
    "scraper.FanartTV.Poster",
    "scraper.Image.TVDB",
    "scraper.IMDB.Data",
    "scraper.MoviepilotDE.Data",
    "scraper.OFDB.Data",
    "scraper.TelevisionTunes.Theme",
    "scraper.Theme.YouTube",
    "scraper.TMDB.Data",
    "scraper.TMDB.Poster",
    "scraper.TMDB.Trailer",
    "scraper.Trailer.VideobusterDE",
    "scraper.Trailer.YouTube",
    "scraper.Trakttv.Data"
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Addon Version Update Script (SAFE)" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "This will increment the BUILD number for:" -ForegroundColor Yellow
Write-Host "  - $($modifiedAddons.Count) modified addons" -ForegroundColor White
Write-Host "  - Build +1, Revision resets to 0" -ForegroundColor DarkGray
Write-Host "  - Example: 1.4.3.2 -> 1.4.4.0" -ForegroundColor DarkGray
Write-Host "  - Preserves exact file formatting" -ForegroundColor DarkGray
Write-Host "`nBackup will be created at:" -ForegroundColor Yellow
Write-Host "  $backupFolder" -ForegroundColor White
Write-Host "`n"

# Confirm before proceeding
$confirmation = Read-Host "Proceed with version updates? (Y/N)"
if ($confirmation -ne 'Y' -and $confirmation -ne 'y') {
    Write-Host "`nOperation cancelled." -ForegroundColor Red
    exit
}

# Create backup folder
New-Item -ItemType Directory -Path $backupFolder -Force | Out-Null
Write-Host "`nCreated backup folder" -ForegroundColor Green
Write-Host "`nProcessing addons...`n" -ForegroundColor Cyan

# Function to increment build number and reset revision to 0
function Increment-BuildNumber {
    param([string]$version)
    
    if ($version -match '^(\d+)\.(\d+)\.(\d+)\.(\d+)$') {
        $major = $matches[1]
        $minor = $matches[2]
        $build = [int]$matches[3] + 1
        $revision = 0  # Reset revision to 0
        return "$major.$minor.$build.$revision"
    }
    return $null
}

# Function to extract version from actual attribute declaration (line-by-line)
function Get-AssemblyVersion {
    param([string[]]$lines)
    
    foreach ($line in $lines) {
        # Skip comment lines (start with ')
        if ($line.TrimStart().StartsWith("'")) {
            continue
        }
        
        # Match the actual attribute line
        if ($line -match '<Assembly:\s*AssemblyVersion\("([^"]+)"\)>') {
            return $matches[1]
        }
    }
    return $null
}

# Process each addon
$updatedCount = 0
$skippedCount = 0
$results = @()

foreach ($addonName in $modifiedAddons) {
    $assemblyInfoPath = Join-Path $rootPath "Addons\$addonName\My Project\AssemblyInfo.vb"
    
    if (Test-Path $assemblyInfoPath) {
        # Read as array of lines (preserves line endings)
        $lines = Get-Content -Path $assemblyInfoPath
        
        # Extract current version from actual attribute (skipping comments)
        $oldVersion = Get-AssemblyVersion $lines
        
        if ($oldVersion) {
            $newVersion = Increment-BuildNumber $oldVersion
            
            if ($newVersion) {
                # Create backup
                $backupPath = Join-Path $backupFolder "Addons\$addonName\My Project"
                New-Item -ItemType Directory -Path $backupPath -Force | Out-Null
                Copy-Item -Path $assemblyInfoPath -Destination $backupPath -Force
                
                # Process line-by-line and replace only non-comment lines
                $updatedLines = foreach ($line in $lines) {
                    if ($line.TrimStart().StartsWith("'")) {
                        # Keep comment lines unchanged
                        $line
                    }
                    elseif ($line -match '<Assembly:\s*AssemblyVersion\("' + [regex]::Escape($oldVersion) + '"\)>') {
                        # Replace AssemblyVersion
                        $line -replace [regex]::Escape("`"$oldVersion`""), "`"$newVersion`""
                    }
                    elseif ($line -match '<Assembly:\s*AssemblyFileVersion\("' + [regex]::Escape($oldVersion) + '"\)>') {
                        # Replace AssemblyFileVersion
                        $line -replace [regex]::Escape("`"$oldVersion`""), "`"$newVersion`""
                    }
                    else {
                        # Keep all other lines unchanged
                        $line
                    }
                }
                
                # Write updated content (preserves line endings automatically)
                Set-Content -Path $assemblyInfoPath -Value $updatedLines -Encoding UTF8
                
                Write-Host "[UPDATED] " -ForegroundColor Green -NoNewline
                Write-Host $addonName.PadRight(45) -NoNewline
                Write-Host " $oldVersion -> $newVersion" -ForegroundColor DarkGray
                
                $results += [PSCustomObject]@{
                    Addon = $addonName
                    OldVersion = $oldVersion
                    NewVersion = $newVersion
                    Status = "Updated"
                }
                $updatedCount++
            } else {
                Write-Host "[SKIPPED] " -ForegroundColor Yellow -NoNewline
                Write-Host $addonName.PadRight(45) -NoNewline
                Write-Host " (Invalid version format: $oldVersion)" -ForegroundColor DarkGray
                $skippedCount++
            }
        } else {
            Write-Host "[SKIPPED] " -ForegroundColor Yellow -NoNewline
            Write-Host $addonName.PadRight(45) -NoNewline
            Write-Host " (No version found)" -ForegroundColor DarkGray
            $skippedCount++
        }
    } else {
        Write-Host "[ERROR]   " -ForegroundColor Red -NoNewline
        Write-Host $addonName.PadRight(45) -NoNewline
        Write-Host " (AssemblyInfo.vb not found!)" -ForegroundColor DarkGray
        $skippedCount++
    }
}

# Summary
Write-Host "`n============================================" -ForegroundColor Cyan
Write-Host "Update Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Addons Updated:  " -NoNewline -ForegroundColor Yellow
Write-Host $updatedCount -ForegroundColor White
Write-Host "Addons Skipped:  " -NoNewline -ForegroundColor Yellow
Write-Host $skippedCount -ForegroundColor White
Write-Host "Backup Location: " -NoNewline -ForegroundColor Yellow
Write-Host $backupFolder -ForegroundColor White
Write-Host "============================================`n" -ForegroundColor Cyan

# Export results to CSV for record-keeping
if ($results.Count -gt 0) {
    $resultsFile = Join-Path $rootPath "Version_Updates_$(Get-Date -Format 'yyyyMMdd_HHmmss').csv"
    $results | Export-Csv -Path $resultsFile -NoTypeInformation
    Write-Host "Detailed results saved to:" -ForegroundColor Cyan
    Write-Host "  $resultsFile`n" -ForegroundColor White
}