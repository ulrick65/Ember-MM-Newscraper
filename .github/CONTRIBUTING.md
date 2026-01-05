# Ember Media Manager - Development Guide

| | Document Info |
|---------------|---|
| **Version** | 2.0 |
| **Created** | December 25, 2025 |
| **Updated** | January 4, 2026 |
| **Author** | Eric H. Anderson |
| **Purpose** | Development handbook for maintaining the Ember Media Manager project |

##### [← Return to Document Index](../EmberMediaManager/docs/DocumentIndex.md)

---

## Overview

Welcome! This guide covers everything you need to know to develop and maintain Ember Media Manager.

Whether you're setting up your dev environment, adding a new scraper, or debugging an issue — you'll find it here. This is primarily a solo developer reference, but the standards apply to any contributor.

**Quick links:**
- [Quick Reference](#quick-reference) for essential project info
- [Development Workflow](#development-workflow) for day-to-day tasks
- [Debugging Tips](#debugging-tips) when things go wrong

---

## Table of Contents

- [Quick Reference](#quick-reference)
- [Development Environment Setup](#development-environment-setup)
- [Project Structure](#project-structure)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Debugging Tips](#debugging-tips)
- [Release Process](#release-process)
- [Common Issues and Solutions](#common-issues-and-solutions)
- [Important Files](#important-files)
- [Developer Notes](#developer-notes)
- [Resources](#resources)

---

## [↑](#table-of-contents) Quick Reference

| Property | Value |
|----------|-------|
| **Framework** | .NET Framework 4.8 |
| **Language** | Visual Basic .NET (VB.NET) |
| **IDE** | Visual Studio 2019+ |
| **Total Projects** | 24 (3 core + 21 addons) |
| **Coding Standards** | `.github/copilot-instructions.md` |
| **Documentation Standards** | `EmberMediaManager/docs/process-docs/DocumentationProcess.md` |

---

## [↑](#table-of-contents) Development Environment Setup

### [↑](#table-of-contents) Prerequisites

- Visual Studio 2019 or later
- .NET Framework 4.8 SDK
- Git for version control

### [↑](#table-of-contents) Initial Setup

1. Clone the repository: `git clone https://github.com/ulrick65/Ember-MM-Newscraper.git`
2. Open `Ember Media Manager.sln` in Visual Studio
3. Restore NuGet packages (right-click solution → Restore NuGet Packages)
4. Build solution to verify everything compiles

### [↑](#table-of-contents) Upstream Sync

| Remote | URL | Purpose |
|--------|-----|---------|
| origin | https://github.com/ulrick65/Ember-MM-Newscraper | Your fork |
| upstream | https://github.com/nagten/Ember-MM-Newscraper | Original repo |

To sync from upstream:

    git fetch upstream
    git checkout master
    git merge upstream/master
    git push origin master

---

## [↑](#table-of-contents) Project Structure

### [↑](#table-of-contents) Core Projects

| Project | Purpose |
|---------|---------|
| EmberMediaManager | Main application executable and UI |
| EmberAPI | Core API shared by all addons |
| KodiAPI | JSON-RPC client for Kodi communication (C#) |

### [↑](#table-of-contents) Addon Categories

| Category | Projects |
|----------|----------|
| **Data Scrapers** | TMDB, IMDB, TVDB, OMDb, Trakttv |
| **Image Scrapers** | TMDB, FanartTV, TVDB |
| **Trailer Scrapers** | TMDB, YouTube |
| **Generic Tools** | BulkRename, MediaFileManager, MovieExporter, MetadataEditor, TagManager, MediaListEditor, ContextMenu, VideoSourceMapping, Mapping |
| **Interfaces** | Kodi, Trakttv |

**Full project details:** See [ProjectIndex.md](../EmberMediaManager/docs/ProjectIndex.md)

---

## [↑](#table-of-contents) Development Workflow

### [↑](#table-of-contents) Making Changes

1. Create a feature branch: `git checkout -b feature/description`
2. Make changes following coding standards in `copilot-instructions.md`
3. Test affected addons thoroughly
4. Commit with descriptive messages: `git commit -m "Fix: TMDB scraper timeout issue"`
5. Push to origin: `git push origin feature/description`
6. Merge to master when ready

### [↑](#table-of-contents) Testing Checklist

| Item | Status |
|------|:------:|
| Affected addon compiles without errors | ⬜ |
| No warnings introduced (check Output window) | ⬜ |
| Manual testing performed with real data | ⬜ |
| Edge cases considered (missing data, network failures, etc.) | ⬜ |
| No breaking changes to addon interfaces | ⬜ |
| XML/NFO parsing validated (if applicable) | ⬜ |

### [↑](#table-of-contents) Common Tasks

**Adding a New Scraper Addon:**
1. Copy existing scraper project as template
2. Update namespace and assembly name
3. Implement required addon interfaces
4. Add project to solution
5. Test integration with core application

**Updating Scraper API Integration:**
1. Document API changes in addon README or comments
2. Update HTTP requests and parsing logic
3. Test with multiple movies/shows
4. Handle API rate limiting appropriately
5. Update error handling for new API responses

**Modifying Core Interfaces:**
1. ⚠️ **CAUTION:** Changes affect ALL addons
2. Review all 24 projects for compatibility
3. Update affected addons simultaneously
4. Test thoroughly across multiple addon types

---

## [↑](#table-of-contents) Coding Standards

### [↑](#table-of-contents) Naming Conventions

| Type | Pattern | Example |
|------|---------|---------|
| Private fields | `_camelCase` | `_dbElement`, `_cancelled` |
| Public properties | `PascalCase` | `ImageType`, `DBElement` |
| Method parameters | `camelCase` | `sender`, `scrapeType` |
| Local variables | `camelCase` | `tContentType`, `iProgress` |
| Methods | `PascalCase` | `SaveAllImages()`, `CreateListImages()` |
| Event handlers | `ControlName_EventName` | `bwImgDownload_ProgressChanged` |

### [↑](#table-of-contents) VB.NET Best Practices

- Always use `Option Strict On` and `Option Explicit On`
- Prefer explicit types over `Object`
- Use `Using` statements for IDisposable objects
- Add XML documentation to public APIs
- Use specific exception types in Try-Catch blocks

### [↑](#table-of-contents) Code Example

    ' Good: Explicit types, proper disposal, clear naming
    Using httpClient As New HttpClient()
        Dim response As HttpResponseMessage = Await httpClient.GetAsync(apiUrl)
        If response.IsSuccessStatusCode Then
            Dim jsonData As String = Await response.Content.ReadAsStringAsync()
            ' Process data
        End If
    End Using

**Full standards:** See `.github/copilot-instructions.md`

---

## [↑](#table-of-contents) Debugging Tips

### [↑](#table-of-contents) Addon Not Loading

| Check | Action |
|-------|--------|
| Output directory | Verify addon builds to correct location |
| Manifest file | Check addon manifest is present |
| Logs | Review application logs for initialization errors |
| Framework | Ensure .NET Framework 4.8 compatibility |

### [↑](#table-of-contents) Scraper API Issues

| Check | Action |
|-------|--------|
| Endpoint | Test API endpoints directly with browser/Postman |
| API changes | Check for deprecations or format changes |
| Authentication | Verify API keys still valid |
| Rate limiting | Review retry logic |

### [↑](#table-of-contents) Build Errors Across Multiple Addons

This usually indicates a core interface change:
1. Check recent commits to core libraries
2. Review shared dependency versions
3. Clean and rebuild entire solution
4. Rebuild core libraries first, then addons

---

## [↑](#table-ofcontents) Release Process

### [↑](#table-ofcontents) Pre-Release Checklist

| Item | Status |
|------|:------:|
| All projects build successfully | ⬜ |
| No compiler warnings | ⬜ |
| Version numbers updated in assemblies | ⬜ |
| ForkChangeLog.md updated with changes | ⬜ |
| Test with fresh Ember installation | ⬜ |
| Tag release in Git: `git tag v1.x.x` | ⬜ |

### [↑](#table-ofcontents) Creating Release

1. Build solution in Release configuration
2. Run `BuildCleanup.ps1 -Rebuild` to clean and rebuild
3. Package required assemblies and addons
4. Create GitHub release with tag
5. Upload packaged files
6. Update release notes

---

## [↑](#table-of-contents) Common Issues and Solutions

| Issue | Solution |
|-------|----------|
| NuGet Restore Fails | Delete `packages` folder and restore again, or check NuGet.config sources |
| Addon Interface Version Mismatch | Rebuild core libraries first, then rebuild all addons |
| Scraper Returns No Results | Check API endpoint, verify response format hasn't changed, review error logs |
| XML Parsing Errors | Validate XML structure, check encoding, handle malformed data gracefully |
| SQLite 2.0 Update Breaks Runtime | ❌ Do NOT update beyond 1.0.119 — native DLL incompatibility |

---

## [↑](#table-of-contents) Important Files

### [↑](#table-of-contents) Configuration & Standards

| File | Purpose |
|------|---------|
| `.github/copilot-instructions.md` | Coding standards and Copilot guidance |
| `EmberMediaManager/docs/process-docs/DocumentationProcess.md` | Documentation standards |
| `Ember Media Manager.sln` | Main solution file |

### [↑](#table-of-contents) Scripts

| Script | Purpose |
|--------|---------|
| `EmberMediaManager/scripts/BuildCleanup.ps1` | Clean build artifacts, optionally rebuild |
| `EmberMediaManager/scripts/UpdateAssemblyVersions.ps1` | Update versions across all projects |
| `EmberMediaManager/scripts/VersionConfig.json` | Version configuration |

### [↑](#table-of-contents) Documentation

| Document | Purpose |
|----------|---------|
| [DocumentIndex.md](../EmberMediaManager/docs/DocumentIndex.md) | Master documentation index |
| [ProjectIndex.md](../EmberMediaManager/docs/ProjectIndex.md) | All 24 projects detailed |
| [ForkChangeLog.md](../EmberMediaManager/docs/ForkChangeLog.md) | Complete change history |

---

## [↑](#table-of-contents) Developer Notes

### [↑](#table-of-contents) When Updating After Long Break

1. Check for upstream changes: `git fetch upstream`
2. Review recent commits to understand what changed
3. Rebuild entire solution to catch any breaking changes
4. Test a few scrapers to verify functionality
5. Review [ForkChangeLog.md](../EmberMediaManager/docs/ForkChangeLog.md) for recent changes

### [↑](#table-of-contents) Before Major Refactoring

1. Create a backup branch
2. Document current behavior
3. Plan changes to minimize addon impact
4. Test incrementally, not all at once
5. Update ForkChangeLog.md as you go

### [↑](#table-of-contents) API Key Management

- Store API keys securely (not in source code)
- Document where keys are stored for each scraper
- Update keys before they expire

---

## [↑](#table-of-contents) Resources

| Resource | URL |
|----------|-----|
| Repository | https://github.com/ulrick65/Ember-MM-Newscraper |
| Upstream | https://github.com/nagten/Ember-MM-Newscraper |
| VB.NET Docs | https://docs.microsoft.com/en-us/dotnet/visual-basic/ |
| .NET Framework 4.8 | https://docs.microsoft.com/en-us/dotnet/framework/ |

---

*End of file*