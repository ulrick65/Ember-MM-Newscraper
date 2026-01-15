# Future Enhancements

| Document Info | |
|---------------|---|
| **Version** | 3.0 |
| **Created** | December 31, 2025 |
| **Updated** | January 8, 2026 |
| **Author** | ulrick65 |
| **Purpose** | Living backlog of deferred, planned, and potential improvements |

##### [← Return to Document Index](../DocumentIndex.md)

---

## Overview

This document tracks all future enhancement opportunities for Ember Media Manager. Items are organized by category with priority and effort estimates. This is a **living document** — items are added, updated, and moved to "Completed" as work progresses.

---

## Backlog ID Format

Items requiring detailed documentation use the format: `BL-XX-###`

where `XX` is the category code and `###` is a sequential number.

| Code | Category | Purpose |
|------|----------|---------|
| CC | Code Cleanup | Remove dead code, organize, simplify internal code |
| CQ | Code Quality | Thread safety, null checks, exception handling, patterns |
| DR | Deprecation & Removal | Remove legacy features, old formats, broken functionality |
| FR | Feature Requests | New functionality |
| KI | Known Issues | Bugs and problems to monitor/fix |
| NI | Needs Investigation | Items needing further research before dispositioning |
| PE | Performance Enhancements | Speed/efficiency improvements |
| SC | Standards & Compatibility | External standards compliance, format updates, compatibility |
| UX | UI/UX Improvements | Interface enhancements, usability |

---

## Creating a Backlog Document

**When to create a BL document:**
- Bug fixes that required investigation or have multiple parts
- Enhancements with implementation details worth preserving
- Any item needing more than a one-line description

**Workflow:**

1. **Determine the category code** from the table above
2. **Find the next available number** in that category:
   - Check existing `BL-XX-###` files in `backlog/` folder
   - Also check this document's category section AND Completed Items for used numbers
   - Numbers are never reused, even for completed items
3. **Create the document** using the template: [`backlog/_BL-TEMPLATE.md`](backlog/_BL-TEMPLATE.md)
4. **Name the file:** `BL-XX-###-BriefTitle.md` (e.g., `BL-KI-008-NewBugName.md`)
5. **Add entry to this document** in the appropriate category section with link to the BL document
6. **When completed:** Move the entry to [Completed Items](#completed-items) with date and link

**Entry Format:**

Each category table uses this column order:

| Priority | Item | Effort | Added | Source | Notes | Details |

**Section Ordering Rules:**
- **Priority order:** High → Medium → Low → Monitor (if applicable)
- **Within same priority:** Order by Added date, oldest first
- Example: A "Low" item from January 5 comes before a "Low" item from January 10

**Template location:** [`backlog/_BL-TEMPLATE.md`](backlog/_BL-TEMPLATE.md) *(authoritative template — see [Documentation Standards](../process-docs/DocumentationStandardsProcess.md#backlog-document-template-for-backlog) for quick reference)*

---

## Table of Contents

- [Backlog ID Format](#backlog-id-format)
- [Creating a Backlog Document](#creating-a-backlog-document)
- [Code Cleanup (CC)](#code-cleanup-cc)
- [Code Quality (CQ)](#code-quality-cq)
- [Deprecation & Removal (DR)](#deprecation--removal-dr)
- [Feature Requests (FR)](#feature-requests-fr)
- [Known Issues (KI)](#known-issues-ki)
- [Needs Investigation (NI)](#needs-investigation-ni)
- [Performance Enhancements (PE)](#performance-enhancements-pe)
- [Standards & Compatibility (SC)](#standards--compatibility-sc)
- [UI/UX Improvements (UX)](#uiux-improvements-ux)
- [Cancelled Items](#cancelled-items)
- [Completed Items](#completed-items)

---

## [↑](#table-of-contents) Code Cleanup (CC)

*Remove dead code, organize, simplify internal code.*

| Priority | Item | Effort | Added | Source | Notes | Details |
|----------|------|--------|-------|--------|-------|---------|
| Low | Find/remove orphaned files | 1-2 hrs | December 31, 2025 | SolutionCleanupAnalysis | Script provided in doc | |
| Low | Remove commented code blocks | 2-4 hrs | December 31, 2025 | SolutionCleanupAnalysis | Multiple files identified | |

---

## [↑](#table-of-contents) Code Quality (CQ)

*Thread safety, null checks, exception handling, patterns.*

| Priority | Item | Effort | Added | Source | Notes | Details |
|----------|------|--------|-------|--------|-------|---------|
| **High** | IMDB strPosterURL thread safety | 3-4 hrs | January 2, 2026 | Analysis | Race condition with parallel scraping | [BL-CQ-001](backlog/BL-CQ-001-IMDB-ThreadSafety.md) |
| **High** | Null check audit (.Count calls) | 2-3 hrs | January 2, 2026 | Analysis | Multiple places call .Count without null check | [BL-CQ-002](backlog/BL-CQ-002-NullCheckAudit.md) |
| Medium | Replace Application.DoEvents() anti-pattern | 2-4 hrs | December 31, 2025 | PerformanceAnalysis 2.2 | clsScrapeIMDB.vb, clsScrapeTMDB.vb | |
| Low | Add FFmpeg version check on startup | 1-2 hrs | December 31, 2025 | FFmpegProcess | Display in About dialog | |
| Low | Add try-finally to MediaInfo handle cleanup | 1 hr | December 31, 2025 | MediaInfoMappingProcess | Ensure handles released | |
| Low | Consistent date format handling | 1-2 hrs | December 26, 2025 | NfoFileImprovements 5.2 | Better invalid date handling | [BL-CQ-005](backlog/BL-CQ-005-NFODateHandling.md) |
| Low | Fix fragile multi-episode regex | 2-3 hrs | December 26, 2025 | NfoFileImprovements 3.2 | Use XmlReader instead | [BL-CQ-004](backlog/BL-CQ-004-MultiEpisodeRegex.md) |
| Low | Force GC during long batch operations | 1 hr | December 31, 2025 | PerformanceAnalysis R6.2 | Every N items during batch scraping | |
| Low | Reorganize the frmMain.vb file | 2-3 hrs | January 8, 2026 | ulrick65 | Organize sections to group things better | |
| Low | Specific exception handling in NFO Load | 2-3 hrs | December 26, 2025 | NfoFileImprovements 1.2 | Replace generic Exception catches | [BL-CQ-003](backlog/BL-CQ-003-NFOSpecificExceptionHandling.md) |
| Low | Improve logging capabilities | 4-6 hrs | January 10, 2026 | ulrick65 | Better logging levels (Debug, Info, Trace); settings to configure; cleanup existing logs | |
| Low | Improve logging capabilities | 4-6 hrs | January 10, 2026 | ulrick65 | Add settings for log level/retention; enhance log viewer | [BL-CQ-006](backlog/BL-CQ-006-ImproveLogging.md) |
| Low | Add missing certification languages as encountered | 1 hr | January 10, 2026 | ulrick65 | Add countries to XML when "Unhandled certification language" appears in logs | [BL-CQ-007](backlog/BL-CQ-007-AddCertificationLanguages.md) |

---

## [↑](#table-of-contents) Deprecation & Removal (DR)

*Remove legacy features, old formats, broken functionality.*

| Priority | Item | Effort | Added | Source | Notes | Details |
|----------|------|--------|-------|--------|-------|---------|
| Medium | Remove Boxee NFO Support | 2-4 hrs | December 31, 2025 | ForkChangeLog | Service discontinued 2015 | |
| Medium | Remove Kodi Addons Tab | 4-8 hrs | December 31, 2025 | IntegratedAddonRemoval | Not used in newer Kodi | |
| Medium | Remove NMT NFO Support | 2-4 hrs | December 31, 2025 | ForkChangeLog | Legacy format, unused | |
| Medium | Remove YAMJ NFO Support | 2-4 hrs | December 31, 2025 | ForkChangeLog | Legacy format, unused | |
| Low | Remove External Subtitle Download | 2-4 hrs | December 31, 2025 | ForkChangeLog | Broken / Not needed | |
| Low | Remove TV Tunes/Themes code | 2-4 hrs | December 31, 2025 | ForkChangeLog | Broken / Not needed | |
| Low | Remove Theme Scraper Code | 2-4 hrs | January 8, 2026 | Settings cleanup | Quick fix applied (panels hidden); full removal pending | [BL-DR-001](backlog/BL-DR-001-RemoveThemeScraperCode.md) |
| Low | Remove ISO file supprt | 2-4 hrs | January 11, 2026 | ulrick65 | We need to remove all the settings and associated code, it is not needed anymore | |

---

## [↑](#table-of-contents) Feature Requests (FR)

*New functionality.*

| Priority | Item | Effort | Added | Source | Notes | Details |
|----------|------|--------|-------|--------|-------|---------|
| Low | Separate NFO reading/validation | 6-10 hrs | December 26, 2025 | NfoFileImprovements 4.2 | NFOValidator, NFOReader, NFONormalizer | [BL-FR-002](backlog/BL-FR-002-NFOSeparateConcerns.md) |
| Low | Strategy pattern for NFO formats | 8-12 hrs | December 26, 2025 | NfoFileImprovements 4.1 | INfoWriter interface for extensibility | [BL-FR-001](backlog/BL-FR-001-NFOStrategyPattern.md) |
| Low | Keyboard Shortcuts Settings Page | 8-16 hrs | January 10, 2026 | ulrick65 | Create settings UI to display/edit shortcuts; currently all hardcoded | [BL-FR-004](backlog/BL-FR-004-KeyboardShortcutsSettings.md) |
| Low | Implement yt-dlp for trailer downloads | 8-12 hrs | January 10, 2026 | ulrick65 | Auto-download trailers using yt-dlp; GitHub: https://github.com/yt-dlp/yt-dlp | |
| Low | Migrate to .NET 8 LTS | 30-60 hrs | January 14, 2026 | ulrick65 | Requires: BL-CC-001 (BinaryFormatter), package updates, `My.*` replacements, SDK-style projects, addon testing; enables EF Core | |


---

## [↑](#table-of-contents) Known Issues (KI)

*Bugs and problems to monitor/fix.*

| Priority | Item | Effort | Added | Source | Notes | Details |
|----------|------|--------|-------|--------|-------|---------|
| High | Setting "Create imdb/tmdb node in nfo file" will not stay selected | 1-2 hrs | January 11, 2026 | ulrick65 | Location is `Settings-TV Shows-Scrapers-Data` the ID of the settings are: `chkMovieScraperIdWriteNodeIMDbId` and `chkTVScraperIdWriteNodeTMDbId` | |
| Medium | MovieSet crash after removing movies from database | 1-2 hrs | January 11, 2026 | ulrick65 | Null collection in `FillScreenInfoWith_Movieset()` causes ArgumentNullException | [BL-KI-006](backlog/BL-KI-006-MovieSetCrashAfterMovieRemoval.md) |
| Low | Edit Images crash with All Seasons selected | 2-3 hrs | January 5, 2026 | Testing | Unscraped show + All Seasons causes ArgumentNullException | [BL-KI-003](backlog/BL-KI-003-EditImagesAllSeasonsCrash.md) |
| Low | Simple Mapping NullReference on Empty Row | 1-2 hrs | January 14, 2026 | BL-CC-001 Testing | Certification/Country/Status/Studio mapping dialogs crash when saving cleared rows | [BL-KI-007](backlog/BL-KI-007-SimpleMappingNullReference.md) |
| Monitor | Parallel download race conditions in SaveToFile | — | December 29, 2025 | Phase 1 | Reverted `File.Exists` check that broke image editing | |


---

## [↑](#table-of-contents) Needs Investigation (NI)

*Items needing further research before dispositioning*

| Priority | Item | Effort | Added | Source | Notes | Details |
|----------|------|--------|-------|--------|-------|---------|
| Low | Settings: Find out what "Remove Images and NFOs with MovieSets" does  | 1-2 hrs | January 11, 2026 | ulrick65 | In `Settings-MovieSets-Files and Sources`: parameter name is `chkMovieSetCleanFiles`  | |
| Low | Settings: Find out what "Build Episode Title Instead of Filtering" does  | 1-2 hrs | January 11, 2026 | ulrick65 | In `Settings-TVShows-General`: Parameter name is `chkTVEpisodeNoFilter` | |
| Low | Settings: Setting indicate that Kodi Interface has to be enabled to use enhaced artwork, we need to confirm | 1-2 hrs | January 11, 2026 | ulrick65 | In `Settings-TVShows/Movies-Image Scrapers` | |
| Low | Settings: Setting "also use Trailer Scrapers" doesn't work. If it is `false` the trailer scrapers still run if enabled. | 1-2 hrs | January 14, 2026 | ulrick65 | In `Settings-Movies-Scrapers-Data` | |

---

## [↑](#table-of-contents) Performance Enhancements (PE)

*Speed/efficiency improvements.*

| Priority | Item | Effort | Added | Source | Notes | Details |
|----------|------|--------|-------|--------|-------|---------|
| Medium | Parallel Scraper Execution | 4-6 hrs | December 31, 2025 | PerformanceAnalysis 8 | Run TMDB+IMDB concurrent per movie | |
| Low | Batch Actor Inserts | 2-3 hrs | December 29, 2025 | Phase 2 | Minimal impact after indices (0.64ms each) | |
| Low | Configurable Concurrency Setting | 2-3 hrs | December 31, 2025 | Phase 3 | Default `Min(ProcessorCount, 4)` works well | |
| Low | Disk-based Image Cache | 4-6 hrs | December 31, 2025 | PerformanceAnalysis R4.3 | Reduces bandwidth on repeat scrapes | |
| Low | IMDB Episode Parallelization | 4 hrs | December 31, 2025 | Phase 4.4 | Multi-scraper mode already performant | |
| Low | IMDB HTML Response Caching | 2-4 hrs | December 31, 2025 | PerformanceAnalysis 5.4 | Cache parsed HTML documents | |
| Low | Memory Pooling for Large Objects | 4-8 hrs | December 31, 2025 | PerformanceAnalysis 8 | Reduces GC pressure | |
| Low | Producer-Consumer Pattern | 8-16 hrs | December 31, 2025 | Phase 3 | Only if save phase becomes bottleneck | |
| Low | Response Caching (scrapers) | 2-4 hrs | December 29, 2025 | Phase 2/4, PerformanceAnalysis | Only benefits repeat scrapes | |
| Medium | Edit Images Quick Access - Show progress during scrape | 2-3 hrs | January 10, 2026 | ulrick65 | Add progress indicator while scraping images for Edit Images context menu | [BL-PE-001](backlog/BL-PE-001-EditImagesQuickAccessPerformance.md) |

---

## [↑](#table-of-contents) Standards & Compatibility (SC)

*External standards compliance, format updates, compatibility.*

| Priority | Item | Effort | Added | Source | Notes | Details |
|----------|------|--------|-------|--------|-------|---------|
| Low | Need to create a document of the SQLite database structure | 2-3 hrs | January 11, 2026 | ulrick65 | Similar to the link in the Media List Editor | SQLite can probably do it |


---

## [↑](#table-ofcontents) UI/UX Improvements (UX)

*Interface enhancements, usability.*

| Priority | Item | Effort | Added | Source | Notes | Details |
|----------|------|--------|-------|--------|-------|---------|
| Medium | Progress Bar - Two-phase (0-50%, 50-100%) | 3-4 hrs | December 31, 2025 | Phase 3 | Better progress feedback | |
| Low | Image selection dialog sorting options | 1 hr | January 4, 2026 | ulrick65 | Sort by resolution, language, scraper | [BL-UX-004](backlog/BL-UX-004-ImageDialogSorting.md) |
| Low | Modern Button Styles for WinForms UI | 2-3 hrs | January 8, 2026 | ulrick65 | Flat-style buttons with hover effects | [BL-UX-005](backlog/BL-UX-005-ModernButtonStyles.md) |
| Low | Progress Bar - Dual bars | 4-6 hrs | December 31, 2025 | Phase 3 | Scrape + Save separate bars | |
| Low | Adjustable Column Widths in Main Media List | 2-3 hrs | January 10, 2026 | ulrick65 | Allow users to resize columns | [BL-UX-005](backlog/BL-UX-006-AdjustableColumnWidths.md) |
| Low | Auto-select Extrafanarts view when only image type enabled | 1-2 hrs | January 10, 2026 | ulrick65 | When dlgImgSelect opens with only Extrafanarts enabled, auto-show extrafanarts panel | [BL-UX-007](backlog/BL-UX-007-AutoSelectExtrafanarts.md) |
| Low | Clarify "Clean Plot/Outline" setting label and tooltip | 0.5 hrs | January 13, 2026 | ulrick65 | Current label implies HTML cleaning; should clarify it removes bracketed content like actor names; add tooltip | |

---

## [↑](#table-of-contents) Cancelled Items

| Item | Added | Source | Reason |
|------|-------|--------|--------|
| Language strings (1400-1403) | December 31, 2025 | Phase 3 | IDs conflict; English fallbacks work |
| Lightweight NFO Validation | December 26, 2025 | NfoFileImprovements 2.2 | Full deserialization needed; cached serializers approach better |
| TMDB append_to_response optimization | December 29, 2025 | Phase 1 | Already well-optimized; minimal ROI | |

---

## [↑](#table-of-contents) Completed Items

*Items moved here when completed, with Added date showing time to completion.*

| Item | Added | Completed | Days | Implementation |
|------|-------|-----------|------|----------------|
| NFO empty catch block logging | December 26, 2025 | December 26, 2025 | 0 | [NfoFileImprovements.md](NfoFileImprovements.md) 1.1 |
| NFO line ending normalization | December 26, 2025 | December 26, 2025 | 0 | [NfoFileImprovements.md](NfoFileImprovements.md) 5.1 |
| NFO XmlSerializer caching | December 26, 2025 | December 26, 2025 | 0 | [NfoFileImprovements.md](NfoFileImprovements.md) 2.1 |
| Legacy scraper addon removal (7 addons) | December 26, 2025 | December 31, 2025 | 5 | [AddonRemovalPlan.md](AddonRemovalPlan.md) |
| Performance Phase 1 - Infrastructure | December 26, 2025 | December 29, 2025 | 3 | [PerformanceImprovements-Phase1.md](PerformanceImprovements-Phase1.md) |
| Performance Phase 2 - TV Async | December 29, 2025 | December 29, 2025 | 0 | [PerformanceImprovements-Phase2.md](PerformanceImprovements-Phase2.md) |
| Performance Phase 2-2 - Parallel Movies | December 29, 2025 | December 30, 2025 | 1 | [PerformanceImprovements-Phase2-2.md](PerformanceImprovements-Phase2-2.md) |
| Performance Phase 3 - Parallel TV | December 30, 2025 | December 31, 2025 | 1 | [PerformanceImprovements-Phase3.md](PerformanceImprovements-Phase3.md) |
| Performance Phase 4 - IMDB & Tracking | December 31, 2025 | December 31, 2025 | 0 | [PerformanceImprovements-Phase4.md](PerformanceImprovements-Phase4.md) |
| Progress Bar - Status text updates | December 31, 2025 | December 31, 2025 | 0 | Phase 3 Option 1 |
| MoveGenres null check | January 2, 2026 | January 2, 2026 | 0 | frmMain.vb - Added null/empty check before .Count |
| Edit Movie Images Quick Access | January 4, 2026 | January 4, 2026 | 0 | [BL-UX-001](backlog/BL-UX-001-EditImagesQuickAccess.md) |
| Edit Season Images Quick Access | January 4, 2026 | January 4, 2026 | 0 | [BL-UX-003](backlog/BL-UX-003-EditImagesQuickAccess-TVSeason.md) |
| Edit TV Images Quick Access | January 4, 2026 | January 4, 2026 | 0 | [BL-UX-002](backlog/BL-UX-002-EditImagesQuickAccess-TVShow.md) |
| Fanarts Available for Landscape Image Selection | January 4, 2026 | January 6, 2026 | 2 | [BL-FR-003](backlog/BL-FR-003-FanartsForLandscape.md) |
| TV Season images not saved after SingleScrape | January 4, 2026 | January 4, 2026 | 0 | [BL-KI-001](backlog/BL-KI-001-TVSeasonImagesSaveIssue.md) |
| Edit Season dialog - most images not selectable | January 5, 2026 | January 5, 2026 | 0 | [BL-KI-002](backlog/BL-KI-002-EditSeasonImageSelectionBug.md) |
| Kodi-Compliant ExtraFanart Naming | January 5, 2026 | January 7, 2026 | 2 | [BL-SC-001](backlog/BL-SC-001-KodiCompliantExtraFanartNaming.md) |
| Extrafanarts/Extrathumbs Dialog Preselect Fix | January 8, 2026 | January 8, 2026 | 0 | [BL-KI-004](backlog/BL-KI-004-ExtrafanartsPreselectFix.md) |
| Modern Button Styles for WinForms UI | January 8, 2026 | January 8, 2026 | 0 | [BL-UX-005](backlog/BL-UX-005-ModernButtonStyles.md) |
| Remove Bug Tracker from Help Menu | January 8, 2026 | January 8, 2026 | 0 | [BL-DR-002](backlog/BL-DR-002-RemoveBugTrackerHelpMenu.md) |
| Remove Check for Updates and Donate features | January 8, 2026 | January 8, 2026 | 0 | [BL-DR-003](backlog/BL-DR-003-RemoveCheckForUpdates.md) |
| "Always Show Genre Text" setting not saving | January 9, 2026 | January 9, 2026 | 0 | [BL-KI-005](backlog/BL-KI-005-GenreTextSettingNotSaving.md) |
| Auto-select Extrafanarts view when only image type enabled | January 10, 2026 | January 10, 2026 | 0 | [BL-UX-007](backlog/BL-UX-007-AutoSelectExtrafanarts.md) |
| Replace BinaryFormatter in CloneDeep | December 26, 2025 | January 14, 2026 | 19 | [BL-CC-001](backlog/BL-CC-001-ReplaceBinaryFormatter.md) |


---

*End of file*



---

*End of file*