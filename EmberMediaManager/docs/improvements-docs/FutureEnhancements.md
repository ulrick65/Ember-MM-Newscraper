# Future Enhancements

| Document Info | |
|---------------|---|
| **Version** | 1.0 |
| **Created** | December 31, 2025 |
| **Author** | Eric H. Anderson |
| **Purpose** | Living backlog of deferred, planned, and potential improvements |

---

## Overview

This document tracks all future enhancement opportunities for Ember Media Manager. Items are organized by category with priority and effort estimates. This is a **living document** - items are added, updated, and moved to "Completed" as work progresses.

---

## Categories

- [Performance Enhancements](#performance-enhancements)
- [Code Cleanup](#code-cleanup)
- [UI/UX Improvements](#uiux-improvements)
- [Feature Requests](#feature-requests)
- [Technical Debt](#technical-debt)
- [Completed Items](#completed-items)

---

## Performance Enhancements

| Item | Priority | Effort | Source | Notes |
|------|----------|--------|--------|-------|
| TMDB append_to_response optimization | Low | 4-8 hrs | Phase 1 | Already well-optimized; minimal ROI |
| Batch Actor Inserts | Low | 2-3 hrs | Phase 2 | Minimal impact after indices (0.64ms each) |
| Response Caching (scrapers) | Low | 2-4 hrs | Phase 2/4, PerformanceAnalysis | Only benefits repeat scrapes |
| IMDB Episode Parallelization | Low | 4 hrs | Phase 4.4 | Multi-scraper mode already performant |
| Producer-Consumer Pattern | Low | 8-16 hrs | Phase 3 | Only if save phase becomes bottleneck |
| Configurable Concurrency Setting | Low | 2-3 hrs | Phase 3 | Default `Min(ProcessorCount, 4)` works well |
| IMDB HTML Response Caching | Low | 2-4 hrs | PerformanceAnalysis 5.4 | Cache parsed HTML documents |
| Parallel Scraper Execution | Medium | 4-6 hrs | PerformanceAnalysis 8 | Run TMDB+IMDB concurrent per movie |
| Disk-based Image Cache | Low | 4-6 hrs | PerformanceAnalysis R4.3 | Reduces bandwidth on repeat scrapes |
| Memory Pooling for Large Objects | Low | 4-8 hrs | PerformanceAnalysis 8 | Reduces GC pressure |

---

## Code Cleanup

| Item | Priority | Effort | Source | Notes |
|------|----------|--------|--------|-------|
| Remove NMT NFO Support | Medium | 2-4 hrs | ForkChangeLog | Legacy format, unused |
| Remove Boxee NFO Support | Medium | 2-4 hrs | ForkChangeLog | Service discontinued 2015 |
| Remove YAMJ NFO Support | Medium | 2-4 hrs | ForkChangeLog | Legacy format, unused |
| Remove Kodi Addons Tab | Medium | 4-8 hrs | IntegratedAddonRemoval | Not used in newer Kodi |
| Remove TV Tunes/Themes code | Low | 2-4 hrs | ForkChangeLog | Broken / Not needed |
| Remove External Subtitle Download | Low | 2-4 hrs | ForkChangeLog | Broken / Not needed |
| Remove commented code blocks | Low | 2-4 hrs | SolutionCleanupAnalysis | Multiple files identified |
| Find/remove orphaned files | Low | 1-2 hrs | SolutionCleanupAnalysis | Script provided in doc |
| Replace BinaryFormatter in CloneDeep | Medium | 4-6 hrs | NfoFileImprovements 3.1 | Deprecated, security vulnerabilities |
| Fix fragile multi-episode regex | Low | 2-3 hrs | NfoFileImprovements 3.2 | Use XmlReader instead |

---

## UI/UX Improvements

| Item | Priority | Effort | Source | Notes |
|------|----------|--------|--------|-------|
| Progress Bar - Marquee mode | Low | 2-3 hrs | Phase 3 | Indeterminate progress during scrape |
| Progress Bar - Two-phase (0-50%, 50-100%) | Medium | 3-4 hrs | Phase 3 | Better progress feedback |
| Progress Bar - Dual bars | Low | 4-6 hrs | Phase 3 | Scrape + Save separate bars |

---

## Feature Requests

| Item | Priority | Effort | Source | Notes |
|------|----------|--------|--------|-------|
| Strategy pattern for NFO formats | Low | 8-12 hrs | NfoFileImprovements 4.1 | INfoWriter interface for extensibility |
| Separate NFO reading/validation | Low | 6-10 hrs | NfoFileImprovements 4.2 | NFOValidator, NFOReader, NFONormalizer |

---

## Technical Debt

| Item | Priority | Effort | Source | Notes |
|------|----------|--------|--------|-------|
| Add try-finally to MediaInfo handle cleanup | Low | 1 hr | MediaInfoMappingProcess | Ensure handles released |
| Add FFmpeg version check on startup | Low | 1-2 hrs | FFmpegProcess | Display in About dialog |
| Replace Application.DoEvents() anti-pattern | Medium | 2-4 hrs | PerformanceAnalysis 2.2 | clsScrapeIMDB.vb, clsScrapeTMDB.vb |
| Force GC during long batch operations | Low | 1 hr | PerformanceAnalysis R6.2 | Every N items during batch scraping |
| Specific exception handling in NFO Load | Low | 2-3 hrs | NfoFileImprovements 1.2 | Replace generic Exception catches |
| Consistent date format handling | Low | 1-2 hrs | NfoFileImprovements 5.2 | Better invalid date handling |

---

## Cancelled Items

| Item | Source | Reason |
|------|--------|--------|
| Language strings (1400-1403) | Phase 3 | IDs conflict; English fallbacks work |
| Lightweight NFO Validation | NfoFileImprovements 2.2 | Full deserialization needed; cached serializers approach better |

---

## Completed Items

*Items moved here when completed, with completion date and link to implementation.*

| Item | Completed | Implementation |
|------|-----------|----------------|
| Legacy scraper addon removal (7 addons) | Dec 31, 2025 | [AddonRemovalPlan.md](AddonRemovalPlan.md) |
| Performance Phase 1 - Infrastructure | Dec 29, 2025 | [PerformanceImprovements-Phase1.md](PerformanceImprovements-Phase1.md) |
| Performance Phase 2 - TV Async | Dec 29, 2025 | [PerformanceImprovements-Phase2.md](PerformanceImprovements-Phase2.md) |
| Performance Phase 2-2 - Parallel Movies | Dec 30, 2025 | [PerformanceImprovements-Phase2-2.md](PerformanceImprovements-Phase2-2.md) |
| Performance Phase 3 - Parallel TV | Dec 31, 2025 | [PerformanceImprovements-Phase3.md](PerformanceImprovements-Phase3.md) |
| Performance Phase 4 - IMDB & Tracking | Dec 31, 2025 | [PerformanceImprovements-Phase4.md](PerformanceImprovements-Phase4.md) |
| Progress Bar - Status text updates | Dec 31, 2025 | Phase 3 Option 1 |
| NFO XmlSerializer caching | Dec 26, 2025 | [NfoFileImprovements.md](NfoFileImprovements.md) 2.1 |
| NFO empty catch block logging | Dec 26, 2025 | [NfoFileImprovements.md](NfoFileImprovements.md) 1.1 |
| NFO line ending normalization | Dec 26, 2025 | [NfoFileImprovements.md](NfoFileImprovements.md) 5.1 |

---

*Last Updated: December 31, 2025*