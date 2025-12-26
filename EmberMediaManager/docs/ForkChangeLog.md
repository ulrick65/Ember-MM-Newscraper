# Ember Media Manager - Fork Changelog

## Document Purpose

This document tracks all changes made to this fork of Ember Media Manager, starting from the upstream source (nagten/Ember-MM-Newscraper). It serves as:

- A historical record of changes since forking
- A version tracking system for all projects in the solution
- A reference for future development decisions
- A guide for understanding divergence from upstream

---

## Document Information

| Property | Value |
|----------|-------|
| **Document Version** | 1.0.3 |
| **Created** | December 25, 2025 |
| **Last Updated** | December 26, 2025 |
| **Author** | Eric H. Anderson |
---

## Fork Information

| Property | Value |
|----------|-------|
| **Upstream Repository** | [nagten/Ember-MM-Newscraper](https://github.com/nagten/Ember-MM-Newscraper) |
| **Fork Repository** | [ulrick65/Ember-MM-Newscraper](https://github.com/ulrick65/Ember-MM-Newsscraper) |
| **Fork Date** | December 21, 2025 |
| **Upstream Version at Fork** | 1.11.1.0 |
| **Current Fork Version** | 1.12.1.0 |
| **Target Framework** | .NET Framework 4.8 (upgraded from 4.5) |

---

## Version History Summary

| Version | Date | Description |
|---------|------|-------------|
| 1.11.1.0 | Pre-fork | Upstream nagten version at time of fork |
| 1.11.1.7 | Dec 21, 2025 | Upstream patch pulled (IMDB writer comparison fix) |
| 1.12.0.0 | Dec 22, 2025 | Framework upgrade to .NET 4.8, package updates, all addons versioned |
| 1.12.1.0 | Dec 23-26, 2025 | Bug fixes, cleanup, genre mapping fix, documentation, code organization |

---

## Project Version Registry

This section tracks the current version of every project in the solution. Versions should be incremented when changes are made to that project.

### Version Status Legend

| Status | Description |
|--------|-------------|
| Code Modified | Actual code logic changes made |
| Framework Updated | .NET 4.8 upgrade and package updates only |
| Deprecated | Slated for removal, non-functional or not needed |
| Removed | Deleted from solution |

### Core Projects

| Project | Assembly Name | Current Version | Upstream Version | Change Type |
|---------|---------------|-----------------|------------------|-------------|
| EmberMediaManager | Ember Media Manager.exe | 1.12.1.0 | 1.11.1.0 | Code Modified |
| EmberAPI | EmberAPI.dll | 1.12.1.1 | 1.11.1.0 | Code Modified |
| KodiAPI | KodiAPI.dll | 1.10.1.0 | 1.10.0 | Framework Updated |

### Generic Addons

| Project | Assembly Name | Current Version | Upstream Version | Change Type |
|---------|---------------|-----------------|------------------|-------------|
| generic.EmberCore.BulkRename | generic.EmberCore.BulkRename.dll | 1.12.0.0 | 1.11.1.0 | Framework Updated |
| generic.EmberCore.ContextMenu | generic.EmberCore.ContextMenu.dll | 1.12.0.0 | 1.10.1.0 | Framework Updated |
| generic.EmberCore.FilterEditor | generic.EmberCore.FilterEditor.dll | 1.12.0.0 | 1.10.1.0 | Framework Updated |
| generic.EmberCore.Mapping | generic.EmberCore.Mapping.dll | 1.12.0.0 | 1.11.1.0 | Framework Updated |
| generic.EmberCore.MediaFileManager | generic.EmberCore.MediaFileManager.dll | 1.12.0.0 | 1.10.1.0 | Framework Updated |
| generic.EmberCore.MetadataEditor | generic.EmberCore.MetadataEditor.dll | 1.12.0.0 | 1.10.1.0 | Framework Updated |
| generic.EmberCore.MovieExport | generic.EmberCore.MovieExporter.dll | 1.12.0.0 | 1.11.1.0 | Framework Updated |
| generic.EmberCore.TagManager | generic.EmberCore.TagManager.dll | 1.12.0.0 | 1.11.1.0 | Framework Updated |
| generic.EmberCore.VideoSourceMapping | generic.EmberCore.MediaSources.dll | 1.12.0.0 | 1.10.1.0 | Framework Updated |
| generic.Interface.Kodi | generic.Interface.Kodi.dll | 1.11.1.0 | 1.11.0.0 | Code Modified |
| generic.Interface.Trakttv | generic.EmberCore.Trakt.dll | 1.12.0.0 | 1.11.1.0 | Framework Updated |

### Data Scrapers

| Project | Assembly Name | Current Version | Upstream Version | Change Type |
|---------|---------------|-----------------|------------------|-------------|
| scraper.IMDB.Data | scraper.Data.IMDB.dll | 1.11.2.7 | 1.11.2.0 | Code Modified |
| scraper.TMDB.Data | scraper.Data.TMDB.dll | 1.12.0.0 | 1.11.2.0 | Framework Updated |
| scraper.Data.TVDB | scraper.Data.TVDB.dll | 1.12.0.0 | 1.10.2.0 | Framework Updated |
| scraper.Data.OMDb | scraper.Data.OMDb.dll | 1.12.0.0 | 1.10.2.0 | Framework Updated |
| scraper.Trakttv.Data | scraper.Data.Trakttv.dll | 1.12.0.0 | 1.10.2.0 | Framework Updated |
| scraper.OFDB.Data | scraper.Data.OFDB.dll | 1.12.0.0 | 1.10.1.0 | Deprecated |
| scraper.MoviepilotDE.Data | scraper.Data.MoviepilotDE.dll | 1.12.0.0 | 1.10.1.0 | Deprecated |

### Image Scrapers

| Project | Assembly Name | Current Version | Upstream Version | Change Type |
|---------|---------------|-----------------|------------------|-------------|
| scraper.FanartTV.Poster | scraper.Image.FanartTV.dll | 1.12.0.0 | 1.11.1.0 | Framework Updated |
| scraper.TMDB.Poster | scraper.Image.TMDB.dll | 1.12.0.0 | 1.11.1.0 | Framework Updated |
| scraper.Image.TVDB | scraper.Image.TVDB.dll | 1.12.0.0 | 1.11.1.0 | Framework Updated |

### Trailer Scrapers

| Project | Assembly Name | Current Version | Upstream Version | Change Type |
|---------|---------------|-----------------|------------------|-------------|
| scraper.TMDB.Trailer | scraper.Trailer.TMDB.dll | 1.12.0.0 | 1.10.1.0 | Framework Updated |
| scraper.Trailer.YouTube | scraper.Trailer.YouTube.dll | 1.12.0.0 | 1.10.1.0 | Framework Updated |
| scraper.Apple.Trailer | scraper.Trailer.Apple.dll | 1.12.0.0 | 1.10.1.0 | Deprecated |
| scraper.Davestrailerpage.Trailer | scraper.Trailer.Davestrailerpage.dll | 1.12.0.0 | 1.10.1.0 | Deprecated |
| scraper.Trailer.VideobusterDE | scraper.Trailer.VideobusterDE.dll | 1.12.0.0 | 1.10.1.0 | Deprecated |

### Theme Scrapers

| Project | Assembly Name | Current Version | Upstream Version | Change Type |
|---------|---------------|-----------------|------------------|-------------|
| scraper.TelevisionTunes.Theme | scraper.Theme.TelevisionTunes.dll | 1.12.0.0 | 1.10.1.0 | Deprecated |
| scraper.Theme.YouTube | scraper.Theme.YouTube.dll | 1.12.0.0 | 1.10.1.0 | Deprecated |

### Removed Projects

| Project | Upstream Version | Removal Date | Reason |
|---------|------------------|--------------|--------|
| scraper.EmberCore.XML | 1.10.1.0 | Dec 22, 2025 | Legacy XML scraper, unused |
| scraper.TVDB.Poster | 1.10.1.0 | Dec 22, 2025 | Duplicate functionality |
| EmberAPI_Test | 1.10.1.0 | Dec 22, 2025 | Outdated test project |

---

## Deprecated Features

The following features are deprecated and slated for removal in a future version. They are non-functional, broken, or not needed:

### NFO Format Support (Deprecated)

| Feature | Reason | Removal Target |
|---------|--------|----------------|
| YAMJ Support | Legacy format, not used | Future |
| NMT Support | Legacy format, not used | Future |
| Boxee Support | Service discontinued | Future |

### Scrapers (Deprecated/Broken)

| Scraper | Type | Reason |
|---------|------|--------|
| scraper.OFDB.Data | Data | Website no longer available |
| scraper.MoviepilotDE.Data | Data | API discontinued |
| scraper.Apple.Trailer | Trailer | Scraper broken |
| scraper.Davestrailerpage.Trailer | Trailer | Website structure changed |
| scraper.Trailer.VideobusterDE | Trailer | Regional, not maintained |
| scraper.TelevisionTunes.Theme | Theme | Service discontinued |
| scraper.Theme.YouTube | Theme | Broken / Not needed |

### Other Deprecated Features

| Feature | Reason |
|---------|--------|
| Kodi Addons | Not used in newer Kodi |
| TV Tunes / Themes | Broken / Not needed |
| External Subtitle Download | Broken / Not Needed |

---

## Detailed Change History

### Version 1.12.1.0 (December 23-26, 2025)

**Summary:** Bug fixes, code cleanup, version standardization, and documentation improvements.

**Code Changes:**

| Category | Change | Files Affected |
|----------|--------|----------------|
| Bug Fix | Fixed Genre Mapping functionality | `EmberAPI\clsXMLGenreMapping.vb` |
| Bug Fix | Fixed database genre handling | `EmberAPI\clsAPIDatabase.vb` |
| Enhancement | XML processing improvements | `EmberAPI\clsAPIXML.vb` |
| Cleanup | Removed unused YouTube code | `EmberAPI\clsAPIYouTube.vb` |
| Cleanup | Reorganized 29 media container classes with consistent region-based structure (~5,000 lines) | `EmberAPI\clsAPIMediaContainers.vb` |
| Bug Fix | Fixed typo in property name: `ValueNormalizedSpezified` → `ValueNormalizedSpecified` | `EmberAPI\clsAPIMediaContainers.vb`, `Addons\generic.Interface.Kodi\clsAPIKodi.vb` |
| Performance | Cached XmlSerializer instances for NFO processing | `EmberAPI\clsAPINFOSerializers.vb` (new), `EmberAPI\clsAPINFO.vb` |
| Enhancement | Added NormalizeLineEndings helper method | `EmberAPI\clsAPINFO.vb` |
| Enhancement | Added logging to empty catch blocks in GetIMDBFromNonConf | `EmberAPI\clsAPINFO.vb` |
| Bug Fix | Fixed image language filtering - uncommented/added missing FilterImages calls for EpisodeFanarts, EpisodePosters, SeasonFanarts, and MainFanarts | `EmberAPI\clsAPIMediaContainers.vb` |
| Enhancement | Added alphabetical sorting to Versions dialog (Ember Application/API pinned at top) | `EmberAPI\clsAPIModules.vb` |
| Bug Fix | Fixed duplicate entries in Versions dialog by using HashSet for assembly tracking | `EmberAPI\clsAPIModules.vb` |

**Version Updates:**

| Category | Change | Files Affected |
|----------|--------|----------------|
| Version | Standardized all addon versions to 1.12.0.0 | 27 AssemblyInfo files |
| Version | Updated KodiAPI to 1.10.1.0 | `KodiAPI\Properties\AssemblyInfo.cs` |
| Version | Updated EmberAPI to 1.12.1.1 for media container reorganization | `EmberAPI\My Project\AssemblyInfo.vb` |

**Documentation:**

| Category | Change | Files Affected |
|----------|--------|----------------|
| Documentation | Created BuildProcess.md | `EmberMediaManager\docs\BuildProcess.md` |
| Documentation | Created FileCleanUpPlan.md | `EmberMediaManager\docs\FileCleanUpPlan.md` |
| Documentation | Created PackageUpdatePlan.md | `EmberMediaManager\docs\PackageUpdatePlan.md` |
| Documentation | Created NfoFileProcess.md | `EmberMediaManager\docs\NfoFileProcess.md` |
| Documentation | Created NfoFileImprovements.md | `EmberMediaManager\docs\NfoFileImprovements.md` |
| Documentation | Created ReleaseNotes-v1.12.0.0.md | `EmberMediaManager\docs\ReleaseNotes-v1.12.0.0.md` |
| Documentation | Created ForkChangeLog.md | `EmberMediaManager\docs\ForkChangeLog.md` |
| Documentation | Created mapping process docs | Multiple mapping docs |
| Documentation | Created SolutionCleanupAnalysis.md | `EmberMediaManager\docs\SolutionCleanupAnalysis.md` |

**Build and Scripts:**

| Category | Change | Files Affected |
|----------|--------|----------------|
| Configuration | Created copilot-instructions.md | `.github\copilot-instructions.md` |
| Build | Created BuildCleanup.ps1 | `BuildCleanup.ps1` |
| Build | Created Directory.Build.targets | `Directory.Build.targets` |
| Script | Created UpdateAssemblyVersions.ps1 | `EmberMediaManager\scripts\UpdateAssemblyVersions.ps1` |
| Script | Created VersionConfig.json | `EmberMediaManager\scripts\VersionConfig.json` |

---

### Version 1.12.0.0 (December 22, 2025)

**Summary:** Major framework upgrade and package updates.

**Framework Changes:**

| Category | Change | Files Affected |
|----------|--------|----------------|
| Framework | Upgraded from .NET Framework 4.5 to 4.8 | All 28 .vbproj and .csproj files |

**Package Updates:**

| Package | From | To | Projects |
|---------|------|-----|----------|
| EntityFramework | 6.4.4 | 6.5.1 | EmberAPI, EmberMediaManager, BulkRename, MovieExport, Trakttv, Kodi |
| NLog | 5.x | 6.0.7 | EmberAPI, EmberMediaManager, multiple scrapers |
| System.Data.SQLite | 1.0.118.0 | 1.0.119.0 | EmberAPI, EmberMediaManager, BulkRename, MovieExport, Kodi |
| HtmlAgilityPack | 1.11.x | 1.12.4 | IMDB, Davestrailerpage, VideobusterDE |
| TMDbLib | 1.9.x | 2.3.0 | TMDB.Data, TMDB.Poster, TMDB.Trailer |
| VideoLibrary | 3.2.x | 3.2.9 | Trailer.YouTube |
| Newtonsoft.Json | 13.0.3 | 13.0.4 | Multiple projects |

**Code Fixes:**

| Category | Change | Files Affected |
|----------|--------|----------------|
| Bug Fix | Fixed async/await blocking in Kodi Interface | `Addons\generic.Interface.Kodi\clsAPIKodi.vb` |

**Project Removals:**

| Category | Change | Files Affected |
|----------|--------|----------------|
| Removal | Removed scraper.EmberCore.XML project | Entire folder deleted (~10,000 lines) |
| Removal | Removed scraper.TVDB.Poster project | Entire folder deleted (~10,000 lines) |
| Removal | Removed EmberAPI_Test project | Entire folder deleted (~5,000 lines) |
| Removal | Removed mediainfo-rar utilities | x64/x86 mediainfo-rar folders |

**Binary Updates:**

| Category | Change | Files Affected |
|----------|--------|----------------|
| Update | Updated MediaInfo.dll (x64) | `EmberAPI\x64\MediaInfo.dll` |
| Update | Updated MediaInfo.dll (x86) | `EmberAPI\x86\MediaInfo.dll` |
| Update | Updated ffmpeg.exe (x64) | `EmberAPI\x64\ffmpeg.exe` |
| Update | Updated ffmpeg.exe (x86) | `EmberAPI\x86\ffmpeg.exe` |

**Other Changes:**

| Category | Change | Files Affected |
|----------|--------|----------------|
| Resource | Renamed sci-fi.jpg to science-fiction.jpg | `EmberMediaManager\Images\Genres\` |
| Config | Updated NLog.config | `EmberAPI\NLog.config` |
| Config | Updated VideoSourceMapping defaults | `EmberAPI\Defaults\` |

---

### Version 1.11.1.7 (December 21, 2025)

**Summary:** Pulled upstream patch from nagten.

**Changes:**

| Category | Change | Files Affected |
|----------|--------|----------------|
| Upstream Merge | Added writers to IMDB comparison logic | `Addons\scraper.IMDB.Data\` |

---

### Version 1.11.1.0 (Pre-Fork - Upstream)

**Summary:** This was the state of nagten's repository at the time of forking.

**Original upstream version characteristics:**
- .NET Framework 4.5
- Older NuGet package versions
- Contained unused legacy projects (scraper.EmberCore.XML, scraper.TVDB.Poster, EmberAPI_Test)
- All scrapers present but some non-functional
- Outdated MediaInfo and ffmpeg binaries

---

## Git Diff Summary (vs Upstream)

Based on `git diff --stat upstream/master..HEAD`:

| Category | Files Changed | Lines Added | Lines Removed |
|----------|---------------|-------------|---------------|
| Total | 377 files | ~10,631 | ~39,500 |
| Documentation | 13 files | ~6,000 | 0 |
| Project Removals | ~60 files | 0 | ~25,000 |
| Framework/Package | ~200 files | ~2,000 | ~2,000 |
| Code Changes | ~10 files | ~500 | ~300 |
| Build Scripts | 3 files | ~230 | 0 |
| Binary Updates | 4 files | N/A | N/A |

---

## Related Documentation

The following documents provide detailed information about specific initiatives:

| Document | Description |
|----------|-------------|
| [PackageUpdatePlan.md](PackageUpdatePlan.md) | Detailed NuGet package update tracking |
| [FileCleanUpPlan.md](FileCleanUpPlan.md) | Solution cleanup and file removal tracking |
| [BuildProcess.md](BuildProcess.md) | Build configuration and output documentation |
| [NfoFileProcess.md](NfoFileProcess.md) | NFO file processing analysis |
| [NfoFileImprovements.md](NfoFileImprovements.md) | Planned improvements for NFO processing |
| [ReleaseNotes-v1.12.0.0.md](ReleaseNotes-v1.12.0.0.md) | Release notes for v1.12.0.0 |
| [GenreMappingProcess.md](GenreMappingProcess.md) | Genre mapping system documentation |
| [SolutionCleanupAnalysis.md](SolutionCleanupAnalysis.md) | Analysis of solution cleanup |

---

## Scripts

| Script | Location | Purpose |
|--------|----------|---------|
| UpdateAssemblyVersions.ps1 | `EmberMediaManager\scripts\` | Update assembly versions across all projects (run with -Help for usage) |
| VersionConfig.json | `EmberMediaManager\scripts\` | Configuration file for version targets |
| BuildCleanup.ps1 | `EmberMediaManager\scripts\` | Clean build artifacts |

---

## Change Entry Template

When making changes, add entries using this format:

### Version X.X.X.X (Date)

**Summary:** Brief description of the release.

**Changes:**

| Category | Change | Files Affected |
|----------|--------|----------------|
| Category | Description of change | File or project names |

**Categories:** Bug Fix, Feature, Enhancement, Cleanup, Documentation, Package, Framework, Upstream Merge, Deprecation, Removal, Version, Build, Config, Resource, Script

---

## Version Numbering Convention

The project uses a four-part version number: `Major.Minor.Build.Revision`

| Part | When to Increment |
|------|-------------------|
| **Major** | Breaking changes, major rewrites |
| **Minor** | New features, significant improvements, framework upgrades |
| **Build** | Bug fixes, minor improvements, package updates |
| **Revision** | Patches, hotfixes, documentation-only changes |

**Current Application Version:** 1.12.1.0
**Current Addon Baseline Version:** 1.12.0.0

- **1** - Major version (inherited from upstream)
- **12** - Minor version (incremented for .NET 4.8 upgrade)
- **1** - Build (core app: incremented for post-upgrade fixes)
- **0** - Revision

---

## Updating This Document

When making changes to the codebase:

1. Update the relevant project version in AssemblyInfo.vb/AssemblyInfo.cs
2. Update the Project Version Registry table in this document
3. Add a new entry to the Detailed Change History section
4. Update the Version History Summary table
5. If deprecating features, update the Deprecated Features section
6. Run `UpdateAssemblyVersions.ps1` if bulk version updates are needed (use -Help for options)

---

## Notes

- All dates are in YYYY-MM-DD or Month DD, YYYY format
- File paths are relative to the solution root
- Change Type values: Code Modified, Framework Updated, Deprecated, Removed, New