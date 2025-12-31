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
| **Document Version** | 1.0.7 |
| **Created** | December 25, 2025 |
| **Last Updated** | December 30, 2025 |
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
| 1.12.1.0 | Dec 23-28, 2025 | Bug fixes, cleanup, genre mapping fix, documentation, code organization, performance improvements, additional package updates |
| 1.12.1.0 | Dec 29-30, 2025 | Bug fixes, cleanup, documentation, code organization, **Phase 1 performance complete (61% improvement)**, Phase 2 planning |
| 1.12.1.0 | Dec 29-30, 2025 | Bug fixes, cleanup, documentation, **Phase 2-2 parallel scraping complete (60% improvement)** |

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

### Version 1.12.1.0 (December 23-28, 2025)

**Summary:** Bug fixes, code cleanup, version standardization, documentation improvements, and additional package updates.

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
| Feature | Added `PerformanceTracker` class for operation timing and metrics | `EmberAPI\clsAPIPerformanceTracker.vb` (new) |
| Feature | Added `HttpClientFactory` for shared HTTP client with connection pooling | `EmberAPI\clsAPIHttpClientFactory.vb` (new) |
| Enhancement | Added async HTTP methods for parallel downloads | `EmberAPI\clsAPIHTTP.vb` |
| Enhancement | Added `SaveAllImagesAsync` and `LoadAndCacheAsync` for parallel image downloads | `EmberAPI\clsAPIMediaContainers.vb` |
| Enhancement | Added `DownloadImagesParallelAsync` with SemaphoreSlim throttling | `EmberAPI\clsAPIImages.vb` |
| Enhancement | Added `Save_MovieAsync` for async database saves | `EmberAPI\clsAPIDatabase.vb` |
| Enhancement | Added performance instrumentation to TMDB scraper | `Addons\scraper.TMDB.Data\Scraper\clsScrapeTMDB.vb` |
| Enhancement | Added performance instrumentation to IMDB scraper | `Addons\scraper.IMDB.Data\Scraper\clsScrapeIMDB.vb` |
| Enhancement | Added parallel image downloads to Image Select dialog | `EmberMediaManager\dlgImgSelect.vb` |
| Enhancement | Added metrics export on application shutdown | `EmberMediaManager\ApplicationEvents.vb` |
| Performance | Added 10 database indices for faster lookups | `EmberAPI\DB\MyVideosDBSQL.txt` |
| Performance | Added SQLite PRAGMA optimizations (WAL mode, cache, mmap) | `EmberAPI\clsAPIDatabase.vb` |
| Performance | Updated OMDb scraper to use shared HttpClient | `Addons\scraper.Data.OMDb\Scraper\clsScrapeOMDb.vb` |
| Database | Database version upgraded from 48 to 49; added v48→v49 patch file | `EmberAPI\clsAPIDatabase.vb`, `EmberAPI\DB\MyVideosDBSQL_v48_Patch.xml` (new) |
| Documentation | Consolidated scraping documentation into ScrapingProcessMovies.md v2.0 | `EmberMediaManager\docs\process-docs\ScrapingProcessMovies.md` |
| Documentation | Updated ScrapingProcessTvShows.md to v2.0 with comprehensive coverage | `EmberMediaManager\docs\process-docs\ScrapingProcessTvShows.md` |
| Documentation | Archived BulkScrapingDocumentation.md with redirect to consolidated doc | `EmberMediaManager\docs\process-docs\BulkScrapingDocumentation.md` |
| Bug Fix | Fixed `DownloadImagesParallelAsync` downloading thumbnails instead of fullsize images (`needFullsize` parameter) | `EmberAPI\clsAPIImages.vb` |
| Performance | Phase 1 complete: 61% improvement in bulk scraping image operations | Multiple files |
| Performance | Parallel downloads: 64% faster image download phase | `EmberAPI\clsAPIImages.vb`, `EmberAPI\clsAPIMediaContainers.vb` |
| Performance | Database indices: 63% faster actor lookups | `EmberAPI\clsAPIDatabase.vb`, `EmberAPI\DB\MyVideosDBSQL.txt` |
| Performance | TMDB API calls: 47% faster with connection pooling | `EmberAPI\clsAPIHttpClientFactory.vb` |
| Documentation | Updated PerformanceImprovements-Phase1.md to v3.2 - Phase 1 complete with final metrics | `EmberMediaManager\docs\PerformanceImprovements-Phase1.md` |
| Documentation | Created Phase 2 Item 2 Parallel Movie Scraping design document | `EmberMediaManager\docs\improvements-docs\PerformanceImprovements-Phase2-2.md` |
| Documentation | Updated Phase 2 plan with design document reference | `EmberMediaManager\docs\improvements-docs\PerformanceImprovements-Phase2.md` |
| Enhancement | Added styled tooltips for genre thumbnails with dark gradient background and custom owner-draw rendering | `EmberMediaManager\frmMain.vb` |
| Enhancement | Added tooltips to both genre Panel and PictureBox controls for consistent hover behavior | `EmberMediaManager\frmMain.vb` |
| Feature | **Phase 2-2: Parallel Movie Scraping** - Implemented two-phase parallel architecture for bulk movie scraping | `EmberMediaManager\frmMain.vb` |
| Feature | Added `ScrapedMovieResult` class for thread-safe result collection | `EmberMediaManager\frmMain.vb` |
| Feature | Added `ProcessMovieScrape_Parallel` method for thread-safe movie scraping without UI interaction | `EmberMediaManager\frmMain.vb` |
| Enhancement | Modified `bwMovieScraper_DoWork` with parallel scrape + sequential save pattern | `EmberMediaManager\frmMain.vb` |
| Performance | **Phase 2-2 complete: 60% improvement** in bulk movie scraping (200s → 80s for 49 movies) | `EmberMediaManager\frmMain.vb` |
| Performance | Parallel scraping with `MaxDegreeOfParallelism = Min(ProcessorCount, 4)` | `EmberMediaManager\frmMain.vb` |
| Performance | Throughput increased from 15 movies/min to 37 movies/min (+147%) | `EmberMediaManager\frmMain.vb` |
| Documentation | Created Phase 2-2 Parallel Movie Scraping design document v1.3 | `EmberMediaManager\docs\improvements-docs\PerformanceImprovements-Phase2-2.md` |

**Version Updates:**

| Category | Change | Files Affected |
|----------|--------|----------------|
| Version | Standardized all addon versions to 1.12.0.0 | 27 AssemblyInfo files |
| Version | Updated KodiAPI to 1.10.1.0 | `KodiAPI\Properties\AssemblyInfo.cs` |
| Version | Updated EmberAPI to 1.12.1.1 for media container reorganization | `EmberAPI\My Project\AssemblyInfo.vb` |

**Package Updates (December 26-27, 2025):**

| Package | From | To | Projects |
|---------|------|-----|----------|
| System.Buffers | 4.5.1 | 4.6.1 | EmberAPI |
| System.Memory | 4.5.5 | 4.6.3 | EmberAPI |
| System.Numerics.Vectors | 4.5.0 | 4.6.1 | EmberAPI |
| System.ValueTuple | 4.5.0 | 4.6.1 | EmberAPI |
| System.Threading.Tasks.Extensions | 4.5.4 | 4.6.3 | EmberAPI |
| System.Runtime.CompilerServices.Unsafe | 6.0.0 | 6.1.2 | EmberAPI |
| System.Security.Cryptography.Algorithms | 4.3.0 | 4.3.1 | EmberAPI, EmberMediaManager, scraper.TMDB.Data |
| System.Security.Cryptography.X509Certificates | 4.3.0 | 4.3.2 | scraper.TMDB.Data |
| System.Runtime | 4.3.0 | 4.3.1 | EmberAPI, EmberMediaManager |
| System.Runtime.Extensions | 4.3.0 | 4.3.1 | EmberAPI, EmberMediaManager |
| System.Net.Primitives | 4.3.0 | 4.3.1 | EmberAPI, EmberMediaManager |
| System.Xml.ReaderWriter | 4.3.0 | 4.3.1 | EmberAPI, EmberMediaManager |
| Microsoft.Bcl.AsyncInterfaces | 9.0.1 | 10.0.1 | EmberAPI |
| System.IO.Pipelines | 9.0.1 | 10.0.1 | EmberAPI |
| System.Text.Encodings.Web | 9.0.1 | 10.0.1 | EmberAPI |
| System.Text.Json | 9.0.1 | 10.0.1 | EmberAPI |
| MovieCollection.OpenMovieDatabase | 2.0.1 | 4.0.3 | scraper.Data.OMDb |

**Packages Removed:**

| Package | Version | Reason |
|---------|---------|--------|
| NETStandard.Library | 1.6.1 | Not needed for .NET Framework 4.8 |
| Microsoft.NETCore.Platforms | 1.1.0 | Not needed for .NET Framework 4.8 |

**SQLite Update Attempt (Rolled Back):**

| Package | Attempted | Result |
|---------|-----------|--------|
| System.Data.SQLite | 1.0.119 → 2.0.2 | ❌ Rolled back - native DLL incompatibility |
| System.Data.SQLite.EF6 | 1.0.119 → 2.0.2 | ❌ Rolled back - native DLL incompatibility |

⚠️ **WARNING:** Do NOT update SQLite packages beyond 1.0.119. Version 2.0.x uses different native interop DLL (`e_sqlite3.dll` vs `SQLite.Interop.dll`). Build succeeds but runtime fails.

**Documentation:**

| Category | Change | Files Affected |
|----------|--------|----------------|
| Documentation | Created BuildProcess.md | `EmberMediaManager\docs\BuildProcess.md` |
| Documentation | Created FileCleanUpPlan.md | `EmberMediaManager\docs\FileCleanUpPlan.md` |
| Documentation | Created PackageUpdatePlan.md | `EmberMediaManager\docs\PackageUpdatePlan.md` |
| Documentation | Created NuGetPackageUpdatePlan.md | `EmberMediaManager\docs\NuGetPackageUpdatePlan.md` |
| Documentation | Created NfoFileProcess.md | `EmberMediaManager\docs\NfoFileProcess.md` |
| Documentation | Created NfoFileImprovements.md | `EmberMediaManager\docs\NfoFileImprovements.md` |
| Documentation | Created ReleaseNotes-v1.12.0.0.md | `EmberMediaManager\docs\ReleaseNotes-v1.12.0.0.md` |
| Documentation | Created ForkChangeLog.md | `EmberMediaManager\docs\ForkChangeLog.md` |
| Documentation | Created mapping process docs | Multiple mapping docs |
| Documentation | Created SolutionCleanupAnalysis.md | `EmberMediaManager\docs\SolutionCleanupAnalysis.md` |
| Documentation | Created PerformanceAnalysis.md | `EmberMediaManager\docs\PerformanceAnalysis.md` |
| Documentation | Created PerformanceImprovements-Phase1.md | `EmberMediaManager\docs\PerformanceImprovements-Phase1.md` |
| Documentation | Created ScrapingProcessMovies.md | `EmberMediaManager\docs\process-docs\ScrapingProcessMovies.md` |
| Documentation | Created ScrapingProcessTvShows.md | `EmberMediaManager\docs\process-docs\ScrapingProcessTvShows.md` |

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
| Documentation | 14 files | ~7,000 | 0 |
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
| [PackageUpdatePlan.md](archived-docs/PackageUpdatePlan.md) | Core NuGet package update tracking (Dec 22-23) |
| [NuGetPackageUpdatePlan.md](archived-docs/NuGetPackageUpdatePlan.md) | System.* and Microsoft stack package updates (Dec 26-27) |
| [FileCleanUpPlan.md](archived-docs/FileCleanUpPlan.md) | Solution cleanup and file removal tracking |
| [BuildProcess.md](process-docs/BuildProcess.md) | Build configuration and output documentation |
| [NfoFileProcess.md](process-docs/NfoFileProcess.md) | NFO file processing analysis |
| [NfoFileImprovements.md](improvements-docs/NfoFileImprovements.md) | Planned improvements for NFO processing |
| [ReleaseNotes-v1.12.0.0.md](release-notes/ReleaseNotes-v1.12.0.0.md) | Release notes for v1.12.0.0 |
| [GenreMappingProcess.md](process-docs/GenreMappingProcess.md) | Genre mapping system documentation |
| [SolutionCleanupAnalysis.md](analysis-docs/SolutionCleanupAnalysis.md) | Analysis of solution cleanup |
| [PerformanceAnalysis.md](analysis-docs/PerformanceAnalysis.md) | Performance analysis and optimization recommendations |
| [PerformanceImprovements-Phase1.md](improvements-docs/PerformanceImprovements-Phase1.md) | Phase 1 performance implementation plan and progress |
| [ScrapingProcessMovies.md](process-docs/ScrapingProcessMovies.md) | Movie scraping process architecture documentation |
| [ScrapingProcessTvShows.md](process-docs/ScrapingProcessTvShows.md) | TV Show scraping process architecture documentation |
| [PerformanceImprovements-Phase2-2.md](improvements-docs/PerformanceImprovements-Phase2-2.md) | Parallel Movie Scraping detailed design document (v1.3 - Complete) |

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