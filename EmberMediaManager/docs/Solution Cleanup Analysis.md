# Solution Cleanup Analysis Report

**Generated:** December 22, 2025  
**Solution:** Ember-MM-Newscraper  
**Target Framework:** .NET Framework 4.8

---

## Executive Summary

This report provides a comprehensive analysis of the Ember-MM-Newscraper solution, identifying:
- Duplicate NuGet package dependencies with version mismatches
- Files containing commented-out or potentially dead code
- Recommendations for cleanup and standardization

**Total Projects Analyzed:** 31 (4 core + 27 addons)

---

## Table of Contents

1. [Duplicate Dependencies](#duplicate-dependencies)
2. [Consistent Packages](#consistent-packages)
3. [Package Distribution by Project](#package-distribution-by-project)
4. [Files with Commented Code](#files-with-commented-code)
5. [Orphaned Files Detection](#orphaned-files-detection)
6. [Recommended Actions](#recommended-actions)
7. [Summary Statistics](#summary-statistics)

---

## Duplicate Dependencies

### Critical Issue: EntityFramework Version Mismatch

**Problem:** Different projects are using different versions of EntityFramework, which can cause runtime conflicts and compatibility issues.

| Package | Version | Used By |
|---------|---------|---------|
| EntityFramework | 6.1.3 | EmberAPI, EmberMediaManager, generic.EmberCore.BulkRename, generic.EmberCore.MovieExport, generic.Interface.Trakttv |
| EntityFramework | 6.0.0 | generic.Interface.Kodi ⚠️ MISMATCH |

**Impact:** Version 6.0.0 is outdated and may have compatibility issues with projects using 6.1.3.

**Resolution Required:** Update `Addons\generic.Interface.Kodi\packages.config` to use EntityFramework 6.1.3.

---

## Consistent Packages

The following packages maintain consistent versions across all projects (no action needed):

### NLog 4.2.3
Used consistently across all projects:
- EmberAPI
- EmberMediaManager
- Trakttv
- All addon projects requiring logging

**Status:** ✅ No issues

### Newtonsoft.Json 13.0.4
Used consistently across projects requiring JSON serialization:
- EmberAPI
- EmberMediaManager
- KodiAPI
- scraper.TMDB.Data
- scraper.IMDB.Data
- scraper.Data.OMDb
- generic.Interface.Trakttv

**Status:** ✅ No issues

### System.Data.SQLite 1.0.108.0
All SQLite-related packages maintain version 1.0.108.0:
- System.Data.SQLite
- System.Data.SQLite.Core
- System.Data.SQLite.EF6
- System.Data.SQLite.Linq

**Status:** ✅ No issues

---

## Package Distribution by Project

### Core Projects

#### EmberAPI
- EntityFramework 6.1.3
- Newtonsoft.Json 13.0.4
- NLog 4.2.3
- System.Data.SQLite 1.0.108.0 (all variants)
- VideoLibrary 3.1.2
- NETStandard.Library 1.6.1
- Multiple System.* packages (Collections, IO, Linq, Net.Http, Reflection, Runtime, Threading, Xml)

#### EmberMediaManager
- EntityFramework 6.1.3
- Newtonsoft.Json 13.0.4
- NLog 4.2.3
- System.Data.SQLite 1.0.108.0 (all variants)
- NETStandard.Library 1.6.1
- Multiple System.* packages (same as EmberAPI)

#### KodiAPI
- Newtonsoft.Json 13.0.4

#### Trakttv
- NLog 4.2.3

### Addon Projects

#### generic.EmberCore.BulkRename
- EntityFramework 6.1.3
- NLog 4.2.3
- System.Data.SQLite 1.0.108.0 (all variants)

#### generic.EmberCore.MovieExport
- EntityFramework 6.1.3
- NLog 4.2.3
- System.Data.SQLite 1.0.108.0 (all variants)

#### generic.Embercore.MetadataEditor
- NLog 4.2.3

#### generic.Interface.Kodi
- EntityFramework 6.0.0 ⚠️
- NLog 4.2.3
- System.Data.SQLite 1.0.108.0 (all variants)

#### generic.Interface.Trakttv
- EntityFramework 6.1.3
- Newtonsoft.Json 13.0.4
- NLog 4.2.3
- TraktApiSharp 0.11.0

#### scraper.TMDB.Data
- Newtonsoft.Json 13.0.4
- NLog 4.2.3
- System.Net.Http 4.3.4
- TMDbLib 1.8.1

#### scraper.IMDB.Data
- HtmlAgilityPack 1.11.42
- Newtonsoft.Json 13.0.4
- NLog 4.2.3

#### scraper.Data.OMDb
- MovieCollection.OpenMovieDatabase 2.0.1
- Newtonsoft.Json 13.0.4
- NLog 4.2.3

#### scraper.Data.TVDB
- NLog 4.2.3

#### scraper.Trailer.YouTube
- NLog 4.2.3

#### scraper.EmberCore.XML
- SharpZipLib 0.86.0 ⚠️ (Very old version)

---

## Files with Commented Code

The following files contain significant blocks of commented-out code that may indicate legacy/unused functionality:

### EmberAPI Project

**File:** `EmberAPI\clsAPICommon.vb`
- Lines 711-744: Commented code block
- Lines 1218-1237: Commented code block
- **Recommendation:** Review and remove if no longer needed

**File:** `EmberAPI\clsAPIMediaContainers.vb`
- Lines 1319-1356: Commented code block
- Lines 4927-4970: Commented code block
- **Recommendation:** Review and remove if no longer needed

**File:** `EmberAPI\clsAPISettings.vb`
- Lines 477-491: Commented code block
- Lines 1657-1673: Commented code block
- **Recommendation:** Review and remove if no longer needed

### Trakttv Project

**File:** `Trakttv\TraktAPI.cs`
- Lines 821-830: Commented code block
- Lines 854-894: Commented code block
- **Recommendation:** Review and remove if no longer needed

**File:** `Trakttv\Model\Comment\TraktComment.cs`
- Lines 2-38: Commented code block
- **Recommendation:** Review and remove if no longer needed

### KodiAPI Project

**File:** `KodiAPI\XBMCRPC\Methods\Playlist.cs`
- Lines 602-619: Commented code block
- **Recommendation:** Review and remove if no longer needed

**File:** `KodiAPI\XBMCRPC\Methods\VideoLibrary.cs`
- Lines 861-883: Commented code block
- Lines 1270-1292: Commented code block
- **Recommendation:** Review and remove if no longer needed

### Addon Projects

**File:** `Addons\scraper.EmberCore.XML\XMLScraper\Utilities\UrlInfo.vb`
- Lines 153-187: Commented code block
- **Recommendation:** Review and remove if no longer needed

**File:** `EmberMediaManager\dlgClearOrReplace.vb`
- Lines 26-63: Commented code block
- **Recommendation:** Review and remove if no longer needed

### HTML Templates

**Location:** `Addons\generic.EmberCore.MovieExport\Templates\`
- Multiple template files contain commented-out JavaScript/HTML sections
- Files affected:
  - `template modernWallCard\English_(en_US).html`
  - `template modernWallCard\German_(de_DE).html`
  - `template6\English_(en_US).html`
  - `template4\English_(en_US).html`
- **Recommendation:** Review templates and clean up unused code

---

## Orphaned Files Detection

### Manual Detection Required

To identify files that exist in the file system but are not included in any project file, use the PowerShell script described below.

#### PowerShell Script: Find-OrphanedFiles.ps1

**Purpose:** Identify code files not referenced in any project file

**Location:** Save as `Find-OrphanedFiles.ps1` in the solution root directory

**Script Content:** Create a PowerShell file with the following logic:
- Find all VB and C# files (*.vb, *.cs, *.designer.vb, *.designer.cs) excluding bin/obj directories
- Find all project files (*.vbproj, *.csproj)
- Compare code files against project file contents
- Report files not referenced in any project
- Export results to CSV for further analysis

**Key Variables:**
- `$allFiles` - All code files found recursively (excluding build directories)
- `$projectFiles` - All project files in the solution
- `$orphanedFiles` - Array of files not found in any project file

**Output:**
- Console display showing total files scanned, included files, and orphaned count
- CSV file: `OrphanedFiles_[timestamp].csv` with columns: FileName, FullPath, Size(KB), LastModified

**Usage Instructions:**
1. Open PowerShell in the solution root directory
2. Run: `.\Find-OrphanedFiles.ps1`
3. Review console output for summary
4. Open the generated CSV file for detailed analysis
5. Verify files before deletion (check source control history)

### Additional Search Patterns

Files matching these patterns may indicate backup/temporary files:
- Files ending with `.backup`, `.old`, `.bak`
- Files starting with `Copy of`, `Backup of`
- Files containing `temp`, `test` in the name
- Designer files without corresponding form files

**Manual Review Recommended:**
- Check source control history before deleting
- Verify no external tools reference these files
- Consider archiving instead of deleting if uncertain

---

## Recommended Actions

### Priority 1: Fix Version Mismatch (CRITICAL)

**Action:** Update EntityFramework version in generic.Interface.Kodi

**File to Modify:** `Addons\generic.Interface.Kodi\packages.config`

**Change Required:**
- OLD: `<package id="EntityFramework" version="6.0.0" targetFramework="net45" />`
- NEW: `<package id="EntityFramework" version="6.1.3" targetFramework="net45" />`

**Steps:**
1. Open `Addons\generic.Interface.Kodi\packages.config`
2. Locate the EntityFramework package line
3. Change version from "6.0.0" to "6.1.3"
4. Save the file
5. Right-click the project in Solution Explorer
6. Select "Manage NuGet Packages"
7. Update EntityFramework to 6.1.3
8. Rebuild the project to verify no breaking changes

**Verification:**
- Build should succeed without errors
- No runtime binding redirect conflicts
- All Entity Framework functionality works as expected

### Priority 2: Remove Commented Code (MEDIUM)

**Action:** Review and clean up commented code blocks

**Approach:**
1. Open each file listed in the "Files with Commented Code" section
2. Review the commented code to determine if it's:
   - Legacy code no longer needed (REMOVE)
   - Code kept for reference/documentation (CONVERT TO COMMENTS)
   - Code that might be needed later (MOVE TO ARCHIVE BRANCH)
3. Delete confirmed unnecessary commented code
4. Add clear comments explaining why code is preserved if kept

**Files to Review (in order):**
1. EmberAPI files (clsAPICommon.vb, clsAPIMediaContainers.vb, clsAPISettings.vb)
2. Trakttv files (TraktAPI.cs, TraktComment.cs)
3. KodiAPI files (Playlist.cs, VideoLibrary.cs)
4. Addon files (UrlInfo.vb, dlgClearOrReplace.vb)
5. HTML templates (review all template directories)

**Best Practice:**
- Use source control to review history of commented code
- If code was commented out more than 6 months ago, likely safe to remove
- Document removal in commit message for traceability

### Priority 3: Find and Remove Orphaned Files (MEDIUM)

**Action:** Execute orphaned files detection script

**Steps:**
1. Create the PowerShell script as described in the "Orphaned Files Detection" section
2. Run the script: `.\Find-OrphanedFiles.ps1`
3. Review the generated CSV file
4. For each orphaned file:
   - Check Git history: `git log --all --full-history -- path/to/file`
   - Verify no external references (search solution)
   - If truly unused, delete the file
5. Commit deletions with clear message: "Remove orphaned files identified in cleanup analysis"

**Safety Checks:**
- Always review file contents before deletion
- Check if file is referenced in documentation
- Verify no build scripts or external tools reference the file
- Consider creating an archive branch before deletion

### Priority 4: Upgrade Outdated Packages (LOW)

**Action:** Consider upgrading very old packages

**Packages to Review:**

**SharpZipLib 0.86.0** (in scraper.EmberCore.XML)
- Current version: 0.86.0 (Very old - released ~2010)
- Latest stable: 1.4.2 (as of 2024)
- **Recommendation:** Test upgrade to latest version
- **Risk:** High - API may have breaking changes
- **Benefit:** Security fixes, performance improvements, bug fixes

**Steps to Upgrade:**
1. Review SharpZipLib changelog for breaking changes
2. Create a feature branch for the upgrade
3. Update package version in packages.config
4. Build and test scraper.EmberCore.XML thoroughly
5. Test all XML scraping functionality
6. If successful, merge; if issues found, document compatibility problems

### Priority 5: Additional Cleanup (LOW)

**Actions:**

**Remove Unused References:**
1. For each project, right-click in Solution Explorer
2. Select "Remove Unused References" (if available in your Visual Studio version)
3. Review suggested removals before confirming
4. Rebuild to ensure no breaking changes

**Run Code Cleanup:**
1. Select all projects in Solution Explorer
2. Right-click → "Analyze and Code Cleanup" → "Run Code Cleanup"
3. Apply consistent formatting across the solution
4. Commit formatting changes separately from functional changes

**Review TODO/FIXME Comments:**
1. Search solution for "TODO", "FIXME", "HACK", "UNDONE"
2. Create issues/work items for valid TODOs
3. Remove resolved TODOs
4. Update outdated comments

---

## Summary Statistics

### Project Breakdown
- **Total Projects:** 31
- **Core Projects:** 4 (EmberAPI, EmberMediaManager, KodiAPI, Trakttv)
- **Addon Projects:** 27

### Dependency Issues
- **Critical Version Mismatches:** 1 (EntityFramework in generic.Interface.Kodi)
- **Outdated Packages:** 1 (SharpZipLib 0.86.0)
- **Consistent Packages:** 3 (NLog, Newtonsoft.Json, System.Data.SQLite)

### Code Quality Issues
- **Files with Commented Code:** 15+ files identified
- **Orphaned Files:** Requires script execution to determine
- **HTML Templates Needing Cleanup:** 4+ template directories

### Risk Assessment

| Issue | Priority | Risk Level | Effort | Impact |
|-------|----------|------------|--------|--------|
| EntityFramework Version Mismatch | Critical | High | Low | High |
| Commented Code Cleanup | Medium | Low | Medium | Medium |
| Orphaned Files | Medium | Low | Low | Low |
| SharpZipLib Upgrade | Low | Medium | High | Medium |
| General Code Cleanup | Low | Low | Low | Low |

---

## Next Steps

1. **Immediate Action (Today):**
   - Fix EntityFramework version mismatch in generic.Interface.Kodi
   - Run full solution build to verify no conflicts

2. **This Week:**
   - Create and run orphaned files detection script
   - Begin reviewing commented code in EmberAPI files
   - Document findings and create cleanup backlog

3. **This Month:**
   - Complete commented code cleanup across all projects
   - Remove confirmed orphaned files
   - Run Visual Studio code cleanup tools

4. **Future Consideration:**
   - Plan SharpZipLib upgrade (separate feature branch)
   - Establish code quality standards to prevent future issues
   - Set up automated dependency version checking

---

## Appendix: Package Version Reference

### Current Versions in Use

| Package | Version | Projects Using |
|---------|---------|----------------|
| EntityFramework | 6.1.3 | 5 projects |
| EntityFramework | 6.0.0 | 1 project (NEEDS UPDATE) |
| Newtonsoft.Json | 13.0.4 | 7 projects |
| NLog | 4.2.3 | 14+ projects |
| System.Data.SQLite | 1.0.108.0 | 6 projects |
| HtmlAgilityPack | 1.11.42 | 1 project |
| TMDbLib | 1.8.1 | 1 project |
| TraktApiSharp | 0.11.0 | 1 project |
| SharpZipLib | 0.86.0 | 1 project |
| VideoLibrary | 3.1.2 | 1 project |
| NETStandard.Library | 1.6.1 | 2 projects |

---

**Report End**

*This analysis was generated on December 22, 2025. Re-run analysis after implementing cleanup actions to verify improvements.*