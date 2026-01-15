# BL-CC-003: Review NuGet Packages for .NET 8 Compatibility

| Field | Value |
|-------|-------|
| **ID** | BL-CC-003 |
| **Created** | January 14, 2026 |
| **Updated** | January 14, 2026 |
| **Category** | Code Cleanup (CC) |
| **Priority** | Low |
| **Effort** | 2-4 hours |
| **Status** | 📋 Open |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Summary

Review all NuGet packages used by Ember Media Manager and its addons for .NET 8 compatibility. Identify packages that need updates or replacements before migration.

---

## Table of Contents

- [Problem Description](#problem-description)
- [Current Packages](#current-packages)
- [Compatibility Assessment](#compatibility-assessment)
- [Implementation Details](#implementation-details)
- [Related Files](#related-files)
- [Notes](#notes)
- [Change History](#change-history)

---

## [↑](#table-of-contents) Problem Description

Some NuGet packages used in .NET Framework projects may not be compatible with .NET 8. Before migration, we need to:

1. Inventory all packages currently in use
2. Check each package's .NET 8 compatibility
3. Identify replacements for incompatible packages
4. Plan upgrade order (some packages may have dependencies)

---

## [↑](#table-of-contents) Current Packages

### Core Projects

| Package | Current Version | Project | .NET 8 Compatible? |
|---------|-----------------|---------|-------------------|
| Newtonsoft.Json | TBD | Multiple | ✅ Yes |
| NLog | TBD | Multiple | ✅ Yes |
| System.Data.SQLite | TBD | EmberAPI | ⚠️ Replace recommended |
| HtmlAgilityPack | TBD | Scrapers | ✅ Yes |

### Addon Projects

| Package | Current Version | Addon | .NET 8 Compatible? |
|---------|-----------------|-------|-------------------|
| TMDbLib | TBD | scraper.Data.TMDB | ✅ Check version |
| TraktApiSharp | TBD | scraper.Data.Trakttv | ⚠️ Check version |

*(Run audit to complete this table)*

---

## [↑](#table-of-contents) Compatibility Assessment

### Known Replacements Needed

| Current Package | Replacement | Reason |
|-----------------|-------------|--------|
| `System.Data.SQLite` | `Microsoft.Data.Sqlite` | Microsoft-maintained, lighter, better .NET Core support |

### Packages to Verify

Run this PowerShell to extract all package references:

    Get-ChildItem -Path "C:\Dev\Ember-MM-Newscraper" -Include "packages.config","*.csproj","*.vbproj" -Recurse |
        Select-String -Pattern 'PackageReference|package id=' |
        Select-Object Path, Line |
        Format-Table -AutoSize -Wrap

### Checking Compatibility

For each package:
1. Visit [nuget.org](https://www.nuget.org)
2. Search for the package
3. Check "Dependencies" tab for `.NET 8` or `.NET Standard 2.0` support
4. If not supported, search for alternatives

---

## [↑](#table-of-contents) Implementation Details

### SQLite Migration

**Current:** `System.Data.SQLite`

    Dim conn As New SQLiteConnection(connectionString)

**After:** `Microsoft.Data.Sqlite`

    Dim conn As New SqliteConnection(connectionString)

Key differences:
- Namespace changes from `System.Data.SQLite` to `Microsoft.Data.Sqlite`
- Class names change (e.g., `SQLiteConnection` → `SqliteConnection`)
- Some advanced features may differ

### Package Update Order

1. Update packages that have no breaking changes first
2. Then update packages requiring code changes
3. SQLite migration should be done alongside EF Core implementation

---

## [↑](#table-of-contents) Related Files

| File | Relevance |
|------|-----------|
| `EmberAPI\packages.config` | Core API packages |
| `EmberMediaManager\packages.config` | Main application packages |
| `Addons\*\packages.config` | Addon packages |
| All `.vbproj` / `.csproj` files | May contain PackageReference entries |

---

## [↑](#table-of-contents) Notes

### Tools

- **[NuGet Package Explorer](https://github.com/NuGetPackageExplorer/NuGetPackageExplorer)** — View package contents and dependencies
- **[dotnet-outdated](https://github.com/dotnet-outdated/dotnet-outdated)** — CLI tool to check for outdated packages (after SDK-style conversion)

### Prerequisite For

- [BL-FR-005: Migrate to .NET 8 LTS](BL-FR-005-DotNet8Migration.md)

---

## [↑](#table-of-contents) Change History

| Date | Description |
|------|-------------|
| January 14, 2026 | Created |

---

*End of file*