# BL-FR-005: Migrate to .NET 8 LTS

| Field | Value |
|-------|-------|
| **ID** | BL-FR-005 |
| **Created** | January 14, 2026 |
| **Updated** | January 14, 2026 |
| **Category** | Feature Requests (FR) |
| **Priority** | Low |
| **Effort** | 30-60 hours |
| **Status** | 📋 Open |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Summary

Migrate Ember Media Manager from .NET Framework 4.8 to .NET 8 LTS to enable modern features like Entity Framework Core, improved performance, and long-term support.

---

## Table of Contents

- [Motivation](#motivation)
- [Prerequisites](#prerequisites)
- [Migration Phases](#migration-phases)
- [Breaking Changes](#breaking-changes)
- [Entity Framework Core](#entity-framework-core)
- [Testing Strategy](#testing-strategy)
- [Rollback Plan](#rollback-plan)
- [Related Files](#related-files)
- [Notes](#notes)
- [Change History](#change-history)

---

## [↑](#table-of-contents) Motivation

### Why Migrate?

| Benefit | Description |
|---------|-------------|
| **Entity Framework Core** | Modern ORM with better performance, LINQ improvements, and migrations |
| **Performance** | .NET 8 has significant runtime performance improvements |
| **Long-term Support** | .NET Framework 4.8 is in maintenance mode; .NET 8 LTS supported until 2026 |
| **Modern Language Features** | C# 12 / VB.NET improvements |
| **Cross-platform Potential** | Future option to run on Linux/macOS |
| **NuGet Ecosystem** | Many packages are .NET Core/.NET 5+ only |

### Why .NET 8 LTS?

- Long-term support (3 years)
- Stable and production-ready
- Entity Framework Core 8.0 included
- WinForms fully supported on Windows

---

## [↑](#table-of-contents) Prerequisites

The following must be completed **before** attempting migration:

| Prerequisite | Status | Notes |
|--------------|--------|-------|
| Replace BinaryFormatter in CloneDeep | ✅ Complete | [BL-CC-001](BL-CC-001-ReplaceBinaryFormatter.md) — Required because BinaryFormatter is obsolete in .NET 5+ |
| Audit `My.*` namespace usage | ⬜ Not started | [BL-CC-002](BL-CC-002-AuditMyNamespaceUsage.md) — `My.Computer`, `My.Settings`, etc. need replacements |
| Review NuGet packages for .NET 8 compatibility | ⬜ Not started | [BL-CC-003](BL-CC-003-NuGetCompatibilityReview.md) — Some packages may need updates or replacements |
| Convert to SDK-style projects | ⬜ Not started | [BL-CC-004](BL-CC-004-ConvertToSDKStyleProjects.md) — Required for .NET 8; simplifies project files |

---

## [↑](#table-of-contents) Migration Phases

### Phase 1: Preparation (8-12 hours)

1. **Audit VB.NET-specific features**
   - `My.Computer.FileSystem` → `System.IO`
   - `My.Settings` → Custom settings class or `IConfiguration`
   - `My.Application` → Manual startup handling
   - `My.Resources` → Standard resource handling

2. **Audit deprecated APIs**
   - `BinaryFormatter` — ✅ Already replaced
   - `WebClient` → `HttpClient`
   - `WebRequest` → `HttpClient`

3. **Create compatibility shim library** (optional)
   - Wrapper methods to ease transition
   - Can be removed after migration complete

### Phase 2: Project Conversion (4-8 hours)

1. **Convert to SDK-style project format**
   - Simpler `.vbproj` / `.csproj` files
   - Use .NET Upgrade Assistant tool

2. **Update target framework**

        <TargetFramework>net8.0-windows</TargetFramework>

3. **Update NuGet packages**
   - Newtonsoft.Json — Compatible
   - NLog — Compatible
   - System.Data.SQLite → Microsoft.Data.Sqlite (recommended)

### Phase 3: WinForms Compatibility (4-8 hours)

1. **Verify all forms load correctly**
   - Designer compatibility
   - Third-party controls (if any)

2. **Test visual rendering**
   - DPI scaling may differ
   - Font rendering changes

3. **Update resource handling**
   - Embedded resources syntax may change

### Phase 4: Database Migration (8-16 hours)

1. **Replace SQLite wrapper**
   - Current: `System.Data.SQLite`
   - Recommended: `Microsoft.Data.Sqlite` (lighter, maintained by Microsoft)

2. **Implement Entity Framework Core** (optional but recommended)
   - Create `DbContext` class
   - Define entity classes
   - Set up migrations

3. **Data migration testing**
   - Existing databases must work
   - Test with production-size databases

### Phase 5: Addon Testing (4-8 hours)

1. **Test each addon loads**
2. **Test scraper functionality**
3. **Test Kodi integration**

---

## [↑](#table-of-contents) Breaking Changes

### VB.NET Specific

| Feature | .NET Framework 4.8 | .NET 8 Replacement |
|---------|--------------------|--------------------|
| `My.Computer.FileSystem` | Built-in | `System.IO.File`, `System.IO.Directory` |
| `My.Settings` | Built-in | `IConfiguration` or custom class |
| `My.Application.Info` | Built-in | `Assembly.GetExecutingAssembly()` |
| `My.Resources` | Built-in | Standard `ResourceManager` |

### General .NET

| Feature | .NET Framework 4.8 | .NET 8 Replacement |
|---------|--------------------|--------------------|
| `BinaryFormatter` | Available (obsolete) | JSON/XML serialization — ✅ Done |
| `WebClient` | Available | `HttpClient` |
| `WebRequest` | Available | `HttpClient` |
| `System.Data.SQLite` | NuGet | `Microsoft.Data.Sqlite` |

### Removed/Changed APIs

    ' These will cause compile errors:
    My.Computer.FileSystem.CopyFile(source, dest)
    My.Settings.SomeSetting
    My.Application.Info.Version

---

## [↑](#table-of-contents) Entity Framework Core

### Why EF Core?

- Type-safe queries with LINQ
- Change tracking
- Migrations for schema changes
- Better testability (can mock DbContext)
- Performance improvements over raw ADO.NET for complex queries

### Implementation Approach

1. **Create DbContext**

        Public Class EmberDbContext
            Inherits DbContext
            
            Public Property Movies As DbSet(Of Movie)
            Public Property TVShows As DbSet(Of TVShow)
            Public Property MovieSets As DbSet(Of MovieSet)
            
            Protected Overrides Sub OnConfiguring(optionsBuilder As DbContextOptionsBuilder)
                optionsBuilder.UseSqlite($"Data Source={Master.DB.MyVideosDBPath}")
            End Sub
        End Class

2. **Create Entity Classes**
   - Map existing database tables
   - Define relationships

3. **Gradual Migration**
   - Keep existing `Database` class working
   - Add EF Core alongside
   - Migrate queries incrementally

### EF Core Packages Required

    Microsoft.EntityFrameworkCore.Sqlite
    Microsoft.EntityFrameworkCore.Design (dev only)

---

## [↑](#table-of-contents) Testing Strategy

### Unit Testing

| Area | Test Focus |
|------|------------|
| Database | CRUD operations, migrations |
| Scrapers | API calls still work |
| File I/O | Path handling, file operations |
| Settings | Load/save settings |

### Integration Testing

| Area | Test Focus |
|------|------------|
| Full scrape cycle | Movies, TV Shows, MovieSets |
| Image downloading | All image types |
| NFO generation | All NFO formats |
| Kodi sync | If Kodi interface enabled |

### Regression Testing

- Run through all major features
- Test with existing production database
- Verify addon loading

---

## [↑](#table-of-contents) Rollback Plan

1. **Keep .NET Framework 4.8 branch**
   - Create `release/net48-final` branch before migration
   - Can release hotfixes if needed

2. **Database compatibility**
   - EF Core migrations should be backward-compatible
   - Test opening migrated DB with old version

3. **Parallel builds during transition**
   - Maintain both versions temporarily
   - Allow users to choose

---

## [↑](#table-of-contents) Related Files

| File | Relevance |
|------|-----------|
| `EmberAPI\clsDatabase.vb` | Main database class — will need updates |
| `EmberAPI\clsCommon.vb` | Contains `My.*` usage to replace — see [BL-CC-002](BL-CC-002-AuditMyNamespaceUsage.md) |
| `EmberMediaManager\My Project\Settings.settings` | Will need migration — see [BL-CC-002](BL-CC-002-AuditMyNamespaceUsage.md) |
| All `.vbproj` / `.csproj` files | Need SDK-style conversion — see [BL-CC-004](BL-CC-004-ConvertToSDKStyleProjects.md) |

### Related Backlog Documents

| Document | Purpose |
|----------|---------|
| [BL-CC-001](BL-CC-001-ReplaceBinaryFormatter.md) | ✅ BinaryFormatter replacement (complete) |
| [BL-CC-002](BL-CC-002-AuditMyNamespaceUsage.md) | Audit and replace `My.*` namespace usage |
| [BL-CC-003](BL-CC-003-NuGetCompatibilityReview.md) | Review NuGet package compatibility |
| [BL-CC-004](BL-CC-004-ConvertToSDKStyleProjects.md) | Convert to SDK-style project files |

---

## [↑](#table-of-contents) Notes

### Tools

- **[.NET Upgrade Assistant](https://docs.microsoft.com/en-us/dotnet/core/porting/upgrade-assistant-overview)** — Microsoft tool to help with migration
- **[try-convert](https://github.com/dotnet/try-convert)** — Converts project files to SDK-style

### References

- [Porting from .NET Framework to .NET](https://docs.microsoft.com/en-us/dotnet/core/porting/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [WinForms on .NET](https://docs.microsoft.com/en-us/dotnet/desktop/winforms/)

### Risks

| Risk | Mitigation |
|------|------------|
| Breaking existing installations | Thorough testing, rollback plan |
| Addon compatibility | Test all addons, provide migration guide |
| Database corruption | Backup before migration, test extensively |
| Performance regression | Benchmark before/after |

---

## [↑](#table-of-contents) Change History

| Date | Description |
|------|-------------|
| January 14, 2026 | Created |

---

*End of file*