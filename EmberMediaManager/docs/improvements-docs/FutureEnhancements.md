# Future Enhancements

| Document Info | |
|---------------|---|
| **Version** | 2.6 |
| **Created** | December 31, 2025 |
| **Updated** | January 8, 2026 |
| **Author** | Eric H. Anderson |
| **Purpose** | Living backlog of deferred, planned, and potential improvements |

##### [← Return to Document Index](../DocumentIndex.md)

---

## Overview

This document tracks all future enhancement opportunities for Ember Media Manager. Items are organized by category with priority and effort estimates. This is a **living document** - items are added, updated, and moved to "Completed" as work progresses.

### Backlog ID Format

Items requiring detailed documentation use the format: `BL-XX-###`

where `XX` is the category code and `###` is a sequential number.

| Code | Section |
|------|---------|
| PE | Performance Enhancements |
| CC | Code Cleanup |
| UX | UI/UX Improvements |
| FR | Feature Requests |
| CQ | Code Quality |
| KI | Known Issues |

### Creating a Backlog Document

**When to create a BL document:**
- Bug fixes that required investigation or have multiple parts
- Enhancements with implementation details worth preserving
- Any item needing more than a one-line description

**Workflow:**

1. **Determine the category code** from the table above
2. **Find the next available number** in that category (check existing `BL-XX-###` files in `backlog/`)
3. **Create the document** using the template: [`backlog/_BL-TEMPLATE.md`](backlog/_BL-TEMPLATE.md)
4. **Name the file:** `BL-XX-###-BriefTitle.md` (e.g., `BL-KI-005-ImageCacheBug.md`)
5. **Add entry to this document** in the appropriate category section with link to the BL document
6. **When completed:** Move the entry to [Completed Items](#completed-items) with date and link

**Template location:** [`backlog/_BL-TEMPLATE.md`](backlog/_BL-TEMPLATE.md)

---

## Table of Contents

- [Creating a Backlog Document](#creating-a-backlog-document)
- [Performance Enhancements (PE)](#performance-enhancements-pe)
- [Code Cleanup (CC)](#code-cleanup-cc)
- [UI/UX Improvements (UX)](#uiux-improvements-ux)
- [Feature Requests (FR)](#feature-requests-fr)
- [Code Quality (CQ)](#code-quality-cq)
- [Known Issues (KI)](#known-issues-ki)
- [Cancelled Items](#cancelled-items)
- [Completed Items](#completed-items)

---

## [↑](#table-of-contents) Performance Enhancements (PE)

| Item | Priority | Effort | Source | Notes | Details |
|------|----------|--------|--------|-------|---------|
| TMDB append_to_response optimization | Low | 4-8 hrs | Phase 1 | Already well-optimized; minimal ROI | |
| Batch Actor Inserts | Low | 2-3 hrs | Phase 2 | Minimal impact after indices (0.64ms each) | |
| Response Caching (scrapers) | Low | 2-4 hrs | Phase 2/4, PerformanceAnalysis | Only benefits repeat scrapes | |
| IMDB Episode Parallelization | Low | 4 hrs | Phase 4.4 | Multi-scraper mode already performant | |
| Producer-Consumer Pattern | Low | 8-16 hrs | Phase 3 | Only if save phase becomes bottleneck | |
| Configurable Concurrency Setting | Low | 2-3 hrs | Phase 3 | Default `Min(ProcessorCount, 4)` works well | |
| IMDB HTML Response Caching | Low | 2-4 hrs | PerformanceAnalysis 5.4 | Cache parsed HTML documents | |
| Parallel Scraper Execution | Medium | 4-6 hrs | PerformanceAnalysis 8 | Run TMDB+IMDB concurrent per movie | |
| Disk-based Image Cache | Low | 4-6 hrs | PerformanceAnalysis R4.3 | Reduces bandwidth on repeat scrapes | |
| Memory Pooling for Large Objects | Low | 4-8 hrs | PerformanceAnalysis 8 | Reduces GC pressure | |

---

## [↑](#table-of-contents) Code Cleanup (CC)

| Item | Priority | Effort | Source | Notes | Details |
|------|----------|--------|--------|-------|---------|
| Remove NMT NFO Support | Medium | 2-4 hrs | ForkChangeLog | Legacy format, unused | |
| Remove Boxee NFO Support | Medium | 2-4 hrs | ForkChangeLog | Service discontinued 2015 | |
| Remove YAMJ NFO Support | Medium | 2-4 hrs | ForkChangeLog | Legacy format, unused | |
| Remove Kodi Addons Tab | Medium | 4-8 hrs | IntegratedAddonRemoval | Not used in newer Kodi | |
| Remove TV Tunes/Themes code | Low | 2-4 hrs | ForkChangeLog | Broken / Not needed | |
| Remove External Subtitle Download | Low | 2-4 hrs | ForkChangeLog | Broken / Not needed | |
| Remove commented code blocks | Low | 2-4 hrs | SolutionCleanupAnalysis | Multiple files identified | |
| Find/remove orphaned files | Low | 1-2 hrs | SolutionCleanupAnalysis | Script provided in doc | |
| Replace BinaryFormatter in CloneDeep | Medium | 4-6 hrs | NfoFileImprovements 3.1 | Deprecated, security vulnerabilities | [BL-CC-001](backlog/BL-CC-001-ReplaceBinaryFormatter.md) |
| Fix fragile multi-episode regex | Low | 2-3 hrs | NfoFileImprovements 3.2 | Use XmlReader instead | [BL-CQ-004](backlog/BL-CQ-004-MultiEpisodeRegex.md) |

---

## [↑](#table-of-contents) UI/UX Improvements (UX)

| Item | Priority | Effort | Source | Notes | Details |
|------|----------|--------|--------|-------|---------|
| Progress Bar - Marquee mode | Low | 2-3 hrs | Phase 3 | Indeterminate progress during scrape | |
| Progress Bar - Two-phase (0-50%, 50-100%) | Medium | 3-4 hrs | Phase 3 | Better progress feedback | |
| Progress Bar - Dual bars | Low | 4-6 hrs | Phase 3 | Scrape + Save separate bars | |
| Image selection dialog sorting options (resolution, language, scraper) | Low | 1 hr | Eric |Change the sort order for images  | [BL-UX-005](backlog/BL-UX-005-ImageDialogSorting.md) |

---

## [↑](#table-of-contents) Feature Requests (FR)

| Item | Priority | Effort | Source | Notes | Details |
|------|----------|--------|--------|-------|---------|
| Strategy pattern for NFO formats | Low | 8-12 hrs | NfoFileImprovements 4.1 | INfoWriter interface for extensibility | [BL-FR-001](backlog/BL-FR-001-NFOStrategyPattern.md) |
| Separate NFO reading/validation | Low | 6-10 hrs | NfoFileImprovements 4.2 | NFOValidator, NFOReader, NFONormalizer | [BL-FR-002](backlog/BL-FR-002-NFOSeparateConcerns.md) |

---

## [↑](#table-of-contents) Code Quality (CQ)

| Item | Priority | Effort | Source | Notes | Details |
|------|----------|--------|--------|-------|---------|
| IMDB strPosterURL thread safety | **High** | 3-4 hrs | Jan 2, 2026 | Race condition with parallel scraping | [BL-CQ-001](backlog/BL-CQ-001-IMDB-ThreadSafety.md) |
| Null check audit (.Count calls) | **High** | 2-3 hrs | Jan 2, 2026 | Multiple places call .Count without null check | [BL-CQ-002](backlog/BL-CQ-002-NullCheckAudit.md) |
| Add try-finally to MediaInfo handle cleanup | Low | 1 hr | MediaInfoMappingProcess | Ensure handles released | |
| Add FFmpeg version check on startup | Low | 1-2 hrs | FFmpegProcess | Display in About dialog | |
| Replace Application.DoEvents() anti-pattern | Medium | 2-4 hrs | PerformanceAnalysis 2.2 | clsScrapeIMDB.vb, clsScrapeTMDB.vb | |
| Force GC during long batch operations | Low | 1 hr | PerformanceAnalysis R6.2 | Every N items during batch scraping | |
| Specific exception handling in NFO Load | Low | 2-3 hrs | NfoFileImprovements 1.2 | Replace generic Exception catches | [BL-CQ-003](backlog/BL-CQ-003-NFOSpecificExceptionHandling.md) |
| Consistent date format handling | Low | 1-2 hrs | NfoFileImprovements 5.2 | Better invalid date handling | [BL-CQ-005](backlog/BL-CQ-005-NFODateHandling.md) |
| Reorganize the 'frmMain.vb' file | Low | 2-3 hrs | Eric | Organize the sections to group things better, like the "Friend", cmnu areas, etc | |



---

## [↑](#table-of-contents) Known Issues (KI)

*Items that may need attention if problems are reported. Monitor during testing.*

| Item | Priority | Source | Notes | Details |
|------|----------|--------|-------|---------|
| Parallel download race conditions in `SaveToFile` | Monitor | Phase 1 | Reverted `File.Exists` check that broke image editing | |
| Edit Images crash with All Seasons selected | Low | Jan 5, 2026 | Unscraped show + All Seasons causes ArgumentNullException | [BL-KI-003](backlog/BL-KI-003-EditImagesAllSeasonsCrash.md) |

---

## [↑](#table-of-contents) Cancelled Items

| Item | Source | Reason |
|------|--------|--------|
| Language strings (1400-1403) | Phase 3 | IDs conflict; English fallbacks work |
| Lightweight NFO Validation | NfoFileImprovements 2.2 | Full deserialization needed; cached serializers approach better |

---

## [↑](#table-of-contents) Completed Items

*Items moved here when completed, with completion date and link to implementation.*

| Item | Completed | Implementation |
|------|-----------|----------------|
| Legacy scraper addon removal (7 addons) | December 31, 2025 | [AddonRemovalPlan.md](AddonRemovalPlan.md) |
| Performance Phase 1 - Infrastructure | December 29, 2025 | [PerformanceImprovements-Phase1.md](PerformanceImprovements-Phase1.md) |
| Performance Phase 2 - TV Async | December 29, 2025 | [PerformanceImprovements-Phase2.md](PerformanceImprovements-Phase2.md) |
| Performance Phase 2-2 - Parallel Movies | December 30, 2025 | [PerformanceImprovements-Phase2-2.md](PerformanceImprovements-Phase2-2.md) |
| Performance Phase 3 - Parallel TV | December 31, 2025 | [PerformanceImprovements-Phase3.md](PerformanceImprovements-Phase3.md) |
| Performance Phase 4 - IMDB & Tracking | December 31, 2025 | [PerformanceImprovements-Phase4.md](PerformanceImprovements-Phase4.md) |
| Progress Bar - Status text updates | December 31, 2025 | Phase 3 Option 1 |
| NFO XmlSerializer caching | December 26, 2025 | [NfoFileImprovements.md](NfoFileImprovements.md) 2.1 |
| NFO empty catch block logging | December 26, 2025 | [NfoFileImprovements.md](NfoFileImprovements.md) 1.1 |
| NFO line ending normalization | December 26, 2025 | [NfoFileImprovements.md](NfoFileImprovements.md) 5.1 |
| MoveGenres null check | January 2, 2026 | frmMain.vb - Added null/empty check before .Count |
| Edit Movie Images Quick Access | January 4, 2026 | [BL-UX-001](backlog/BL-UX-001-EditImagesQuickAccess.md) |
| Edit TV Images Quick Access | January 4, 2026 | [BL-UX-002](backlog/BL-UX-002-EditImagesQuickAccess-TVShow.md) |
| TV Season images not saved after SingleScrape | January 4, 2026 | [BL-KI-001](backlog/BL-KI-001-TVSeasonImagesSaveIssue.md) |
| Edit Season Images Quick Access | January 4, 2026 | [BL-UX-003](backlog/BL-UX-003-EditImagesQuickAccess-TVSeason.md) |
| Edit Season dialog - most images not selectable | January 5, 2026 | [BL-KI-002](backlog/BL-KI-002-EditSeasonImageSelectionBug.md) |
| Fanarts Available for Landscape Image Selection | January 6, 2026 | [BL-UX-004](backlog/BL-UX-004-FanartsForLandscape.md) |
| Kodi-Compliant ExtraFanart Naming | January 7, 2026 | [BL-CC-002](backlog/BL-CC-002-KodiCompliantExtraFanartNaming.md) |
| Extrafanarts/Extrathumbs Dialog Preselect Fix | January 8, 2026 | [BL-KI-004](backlog/BL-KI-004-ExtrafanartsPreselectFix.md) |

---

*End of File*