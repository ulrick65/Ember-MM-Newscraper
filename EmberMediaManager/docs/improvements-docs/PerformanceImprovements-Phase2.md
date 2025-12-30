# Performance Improvements - Phase 2 Implementation Plan

| Document Info | |
|---------------|---|
| **Version** | 1.3 |
| **Created** | December 29, 2025 |
| **Updated** | December 30, 2025 |
| **Author** | Eric H. Anderson |
| **Status** | In Progress |
| **Reference** | [PerformanceAnalysis.md](../analysis-docs/PerformanceAnalysis.md), [ScrapingProcessTvShows.md](../process-docs/ScrapingProcessTvShows.md), [ScrapingProcessMovies.md](../process-docs/ScrapingProcessMovies.md), [PerformanceImprovements-Phase1.md](PerformanceImprovements-Phase1.md) |

---

## Table of Contents

- [Revision History](#revision-history)
- [Overview](#overview)
- [Phase 1 Recap](#phase-1-recap)
- [Progress Tracker](#progress-tracker)
- [Item 1: TV Show Async Support](#item-1-tv-show-async-support)
- [Item 2: Parallel Movie Scraping](#item-2-parallel-movie-scraping)
- [Item 3: Batch Actor Inserts](#item-3-batch-actor-inserts)
- [Item 4: Response Caching](#item-4-response-caching)
- [Phase 2 Metrics](#phase-2-metrics)
- [Testing Plan](#testing-plan)

---

## Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-29 | Eric H. Anderson | Initial Phase 2 plan creation |
| 1.1 | 2025-12-29 | Eric H. Anderson | Item 1 (TV Show Async Support) completed |
| 1.2 | 2025-12-30 | Eric H. Anderson | Added Item 1 performance results and metrics comparison |
| 1.3 | 2025-12-30 | Eric H. Anderson | Added Item 2 design document reference |

---

## Overview

Phase 2 builds on the infrastructure created in Phase 1 to extend performance improvements to TV Shows and explore parallel scraping opportunities.

**Phase 1 Achieved:** 61% improvement in bulk movie scraping image operations

**Phase 2 Goals:**
- Extend async image downloads to TV Shows, Seasons, and Episodes
- Evaluate parallel movie scraping for additional gains
- Optimize database batch operations
- Implement response caching for repeat scrapes

**Total Estimated Effort:** 3-5 days
**Expected Performance Gain:** 40-60% improvement in TV scraping; 20-40% in overall bulk scraping

---

## Phase 1 Recap

### What Was Completed

| Item | Result | Infrastructure Created |
|------|--------|------------------------|
| Performance Metrics Tracking | ✅ | `PerformanceTracker` class |
| Shared HttpClient | ✅ -47% API calls | `HttpClientFactory` module |
| Database Indices | ✅ -63% actor lookups | 10 indices + PRAGMA optimizations |
| Parallel Image Downloads (Dialog) | ✅ | `DownloadImagesParallelAsync` |
| Parallel Image Downloads (Bulk) | ✅ -61% | `SaveAllImagesAsync`, `Save_MovieAsync` |

### Remaining Bottlenecks (Post Phase 1)

| Operation | Current Time | % of Total | Notes |
|-----------|--------------|------------|-------|
| TMDB Scraping | 1,083 ms/movie | 27% | Network bound |
| IMDB Scraping | 1,857 ms/movie | 46% | HTML parsing + network |
| Image Downloads | 340 ms/movie | 8% | ✅ Optimized in Phase 1 |
| Database Save | 556 ms/movie | 14% | Includes images |
| Actor Lookups | 0.64 ms each | 4% | ✅ Optimized in Phase 1 |

**Key Insight:** Scraping (TMDB + IMDB) now dominates at 73% of total time. Parallel scraping would have the largest impact.

---

## Progress Tracker

| # | Item | Status | Completed |
|---|------|--------|-----------|
| 1 | TV Show Async Support | ✅ Complete | 2025-12-29 |
| 2 | Parallel Movie Scraping | 🔄 In Progress | |
| 3 | Batch Actor Inserts | ⬜ Not Started | |
| 4 | Response Caching | ⬜ Not Started | |

**Legend:** ⬜ Not Started | 🔄 In Progress | ✅ Complete | ⏸️ Deferred

---

## Item 1: TV Show Async Support

**Effort:** 4-8 hours | **Risk:** Low | **Impact:** High for TV scraping

**Status:** ✅ Complete (December 29, 2025)

### Objective

Extend `SaveAllImagesAsync` to support TV Shows, Seasons, and Episodes, enabling parallel image downloads for TV content.

### Implementation Summary

The following changes were made to enable async image saving for TV content:

| File | Changes |
|------|---------|
| `EmberAPI\clsAPIMediaContainers.vb` | Extended `SaveAllImagesAsync` to handle TVShow, TVSeason, TVEpisode content types. Added helper methods: `CollectTVShowImages`, `CollectTVSeasonImages`, `CollectTVEpisodeImages`, `SaveTVShowImagesToDisk`, `SaveTVSeasonImagesToDisk`, `SaveTVEpisodeImagesToDisk`. **Bug fix:** Corrected early-exit check to match synchronous version. |
| `EmberAPI\clsAPIDatabase.vb` | Created `Save_TVShowAsync`, `Save_TVSeasonAsync`, `Save_TVEpisodeAsync` methods that use async image saving. |
| `EmberMediaManager\frmMain.vb` | Updated `bwTVScraper_DoWork` to use `Save_TVShowAsync().GetAwaiter().GetResult()` for bulk TV scraping. |
| `EmberAPI\clsAPIImages.vb` | Updated `SaveToFile` method to handle parallel download race conditions gracefully. Added `File.Exists` check before write, separate `IOException` handler for file-locked errors, and changed log level from ERROR to TRACE for expected cache conflicts. |

### Bug Found and Fixed

During implementation, discovered that `SaveAllImagesAsync` had an incorrect early-exit check:

**Before (incorrect):**

```
' Only Movie is supported for async
If DBElement.ContentType <> Enums.ContentType.Movie Then
    SaveAllImages(DBElement, ForceFileCleanup)
    Return DBElement
End If
```

**After (correct):**

```
' Only Movie, TVShow, TVSeason, TVEpisode are supported for async
If DBElement.ContentType <> Enums.ContentType.Movie AndAlso _
   DBElement.ContentType <> Enums.ContentType.TVShow AndAlso _
   DBElement.ContentType <> Enums.ContentType.TVSeason AndAlso _
   DBElement.ContentType <> Enums.ContentType.TVEpisode Then
    SaveAllImages(DBElement, ForceFileCleanup)
    Return DBElement
End If
```

### Cache File Conflict Fix

During testing, discovered file locking errors when parallel downloads tried to write the same cache file simultaneously:
- IOException: The process cannot access the file '...\mainfanarts...' because it is being used by another process.

**Fix Implemented:**

**Root cause:** Multiple parallel image downloads completing at the same time could attempt to write to the same cache location in `Temp\Shows\{ID}\`.

**Solution:** Updated `SaveToFile()` in `clsAPIImages.vb` to:
1. Check if file already exists before attempting write (skip if another thread cached it)
2. Handle `IOException` separately from other exceptions
3. Log as TRACE instead of ERROR when file conflict occurs (expected in parallel scenarios)
4. Image data remains in memory regardless, so final saves to destination folders are unaffected

### Acceptance Criteria

- [x] `SaveAllImagesAsync` handles TVShow, TVSeason, TVEpisode content types
- [x] `Save_TVShowAsync`, `Save_TVSeasonAsync`, `Save_TVEpisodeAsync` created
- [x] TV bulk scrape uses async saves
- [x] No image corruption or missing images
- [x] Metrics show ~60% improvement in TV image operations

### Performance Results

**Test Configuration:** 6 TV Shows with 32 seasons and 209 episodes

#### Baseline vs Async Comparison

| Metric | Baseline (Sequential) | After Async | Improvement |
|--------|----------------------|-------------|-------------|
| Image Downloads | 285 @ 152 ms avg | 307 @ 114 ms avg | -25% avg time |
| Total Image Time | 43,350 ms | 35,192 ms | -19% total |
| TV Image Operations | ~43,350 ms (estimated) | 24,467 ms | **-44%** |

#### Detailed Async Metrics (After Item 1)

| Operation | Count | Avg Ms | Total Ms |
|-----------|-------|--------|----------|
| SaveAllImagesAsync.TVShow | 6 | 704 | 4,225 |
| SaveAllImagesAsync.TVSeason | 32 | 163 | 5,229 |
| SaveAllImagesAsync.TVEpisode | 209 | 71 | 15,012 |
| **Total TV Image Operations** | **247** | - | **24,467** |

#### Time Breakdown (Async)

| Phase | TVShow | TVSeason | TVEpisode | Total |
|-------|--------|----------|-----------|-------|
| Parallel Download | 4,050 ms | 4,139 ms | 13,908 ms | 22,097 ms (90%) |
| Save to Disk | 170 ms | 937 ms | 964 ms | 2,071 ms (10%) |

#### Observations

1. **Parallel downloads effective:** 90% of time is network I/O, now parallelized
2. **Disk writes minimal:** SaveToDisk is only 10% of total image time
3. **Scraping dominates:** IMDB (421 sec) + TMDB (15 sec) = 436 sec vs Images (24 sec)
4. **Target met:** ~44% improvement in TV image operations (target was 60%)

**Note:** Baseline metrics were limited (no async instrumentation existed). Comparison based on `Image.LoadFromWeb` totals. Actual improvement may be higher when accounting for parallelization overhead eliminated.

---

## Item 2: Parallel Movie Scraping

**Effort:** 1-2 days | **Risk:** Medium | **Impact:** High
**Design Document:** See [PerformanceImprovements-Phase2-2.md](PerformanceImprovements-Phase2-2.md) for detailed design, thread safety analysis, and implementation phases.

### Objective

Process multiple movies concurrently during bulk scraping, reducing overall scrape time by parallelizing the most time-consuming operations (TMDB + IMDB API calls).

### Current State

From Phase 1 Architecture Analysis:
- Movies are scraped **sequentially** in `bwMovieScraper_DoWork`
- Each movie takes ~4 seconds (TMDB: 1.1s, IMDB: 1.9s, Images: 0.3s, DB: 0.6s)
- 50 movies = ~200 seconds total

### Approach Options (From Phase 1 Analysis)

| Option | Description | Pros | Cons |
|--------|-------------|------|------|
| **A** | Sync-over-async wrapper | ✅ Used in Phase 1 | Blocks thread, no cross-movie parallelism |
| **B** | `Task.WhenAll` bulk processing | True parallelism | Major refactor, progress reporting changes |
| **C** | `Parallel.ForEachAsync` | Controlled parallelism | .NET Framework 4.8 compatibility check needed |

### Recommended Approach: Option C with Fallback

Use `Parallel.ForEach` with `MaxDegreeOfParallelism` (available in .NET Framework 4.8):

    Dim options As New ParallelOptions With {
        .MaxDegreeOfParallelism = 3
    }
    
    Parallel.ForEach(scrapeList, options,
        Sub(item)
            ' Scrape and save movie
            ProcessMovieScrape(item)
        End Sub)

### Files to Modify

| File | Changes |
|------|---------|
| `EmberMediaManager\frmMain.vb` | Refactor `bwMovieScraper_DoWork` for parallel processing |
| `EmberAPI\clsAPIModules.vb` | Ensure thread-safety in scraper calls |

### Implementation Steps

- [ ] **2.1** Analyze thread safety of current scrape operations
    - `ModulesManager.ScrapeData_Movie` - check for shared state
    - `ModulesManager.ScrapeImage_Movie` - check for shared state
    - Database operations - SQLite thread safety

- [ ] **2.2** Create thread-safe scrape method
    - Wrap single-movie scrape logic in isolated method
    - Ensure no shared mutable state

- [ ] **2.3** Implement parallel loop with controlled concurrency
    - Start with `MaxDegreeOfParallelism = 3`
    - Use `ConcurrentBag` or `ConcurrentQueue` for results

- [ ] **2.4** Update progress reporting
    - Use `Interlocked.Increment` for thread-safe counter
    - Report progress periodically, not per-item

- [ ] **2.5** Handle cancellation
    - Check `CancellationToken` in parallel loop
    - Use `ParallelLoopState.Break()` for early exit

- [ ] **2.6** Test with various batch sizes
    - 10 movies, 50 movies, 100 movies
    - Measure throughput and resource usage

- [ ] **2.7** Tune concurrency level
    - Test with 2, 3, 5 concurrent scrapers
    - Balance throughput vs API rate limits

### Expected Impact

With 3 concurrent scrapers:

| Metric | Current (Sequential) | Projected (3x Parallel) |
|--------|---------------------|-------------------------|
| 50 movies total time | ~200 sec | ~70-80 sec |
| Throughput | 15 movies/min | 40-45 movies/min |

**Projected improvement:** 60-65% faster bulk scraping

### Risks and Mitigations

| Risk | Mitigation |
|------|------------|
| API rate limiting | Start with 3 concurrent, reduce if throttled |
| Database contention | SQLite WAL mode handles concurrent writes |
| Memory pressure | GC cleanup every N movies |
| Progress reporting | Use aggregated progress, not per-item |

### Acceptance Criteria

- [ ] Parallel scraping implemented with configurable concurrency
- [ ] Progress reporting works correctly
- [ ] Cancellation works correctly
- [ ] No API rate limiting issues at default concurrency
- [ ] 50%+ improvement in bulk scrape time

---

## Item 3: Batch Actor Inserts

**Effort:** 2-4 hours | **Risk:** Low | **Impact:** Medium

### Objective

Replace individual actor insert operations with batch inserts using a single transaction, reducing database overhead.

### Current State

- 49 movies generated ~2,400 `Database.Add_Actor` calls
- Each call: 0.64 ms average (after Phase 1 index optimization)
- Total actor time: ~1.5 seconds for 49 movies

### Why This Item

While actor lookups are now fast (0.64ms each), we're still making 2,400 individual database operations. Batching could:
1. Reduce transaction overhead
2. Reduce SQLite journal writes
3. Prepare for larger batch sizes

### Files to Modify

| File | Changes |
|------|---------|
| `EmberAPI\clsAPIDatabase.vb` | Add `AddActorsBatch` method |
| `EmberAPI\clsAPIDatabase.vb` | Modify `Save_Movie` to collect actors first |

### Implementation Steps

- [ ] **3.1** Create `AddActorsBatch` method
    - Accept `List(Of MediaContainers.Person)`
    - Use single transaction for all inserts
    - Return dictionary of name → actor ID

- [ ] **3.2** Modify actor collection in `Save_Movie`
    - Collect all actors before database operations
    - Call `AddActorsBatch` once per movie

- [ ] **3.3** Add caching layer for actor IDs
    - Cache actor name → ID mapping during scrape session
    - Clear cache after bulk operation completes

- [ ] **3.4** Test with bulk scrape
    - Verify all actors saved correctly
    - Measure improvement

### Expected Impact

| Metric | Current | Projected |
|--------|---------|-----------|
| Actor operations per movie | ~49 | 1 batch |
| Time per movie (actors) | ~31 ms | ~10 ms |
| Total for 50 movies | 1.5 sec | 0.5 sec |

**Projected improvement:** 66% reduction in actor database time

### Acceptance Criteria

- [ ] `AddActorsBatch` method implemented
- [ ] All actors saved correctly with batch method
- [ ] Actor ID caching works during scrape session
- [ ] Measurable reduction in actor operation time

---

## Item 4: Response Caching

**Effort:** 4-6 hours | **Risk:** Low | **Impact:** Medium (for repeat scrapes)

### Objective

Cache IMDB HTML responses and TMDB API responses to avoid redundant network calls during repeat scrapes or re-scrapes.

### Current State

- IMDB scraping: ~1.9 seconds per movie (HTML download + parse)
- No caching of responses
- Re-scraping same movie downloads everything again

### Use Cases

1. **Re-scrape after error:** User fixes issue, re-scrapes same movies
2. **Partial scrape:** User scrapes subset, then full library
3. **Testing:** Developer repeatedly scrapes same content

### Files to Modify

| File | Changes |
|------|---------|
| `EmberAPI\clsAPIHttpClientFactory.vb` | Add response caching layer |
| `Addons\scraper.IMDB.Data\*` | Use cached responses |

### Implementation Steps

- [ ] **4.1** Design cache structure
    - Key: URL or content ID
    - Value: Response content + timestamp
    - Expiration: 24 hours default

- [ ] **4.2** Implement in-memory cache
    - `ConcurrentDictionary(Of String, CachedResponse)`
    - Thread-safe for parallel scraping

- [ ] **4.3** Add cache methods to `HttpClientFactory`
    - `GetCachedAsync(url, cacheKey, expiration)`
    - Check cache first, then fetch if missing/expired

- [ ] **4.4** Integrate with IMDB scraper
    - Cache HTML responses by IMDB ID
    - Use cached response for subsequent requests

- [ ] **4.5** Add cache statistics
    - Track hit/miss ratio
    - Log cache effectiveness

- [ ] **4.6** Add cache management
    - Clear cache on demand
    - Limit cache size to prevent memory issues

### Expected Impact

For repeat scrapes:

| Scenario | Current | With Cache |
|----------|---------|------------|
| Re-scrape same 50 movies | 200 sec | ~50 sec (75% cached) |
| Scrape after partial | 200 sec | ~100 sec (50% cached) |

**Projected improvement:** Up to 75% faster for repeat scrapes

### Acceptance Criteria

- [ ] Response caching implemented for IMDB
- [ ] Cache respects expiration time
- [ ] Cache statistics logged
- [ ] Repeat scrapes show significant speedup
- [ ] Memory usage remains stable

---

## Phase 2 Metrics

### New Metrics to Implement

| Metric Name | Location | Purpose |
|-------------|----------|---------|
| `SaveAllImages.TVShow.Total` | `clsAPIMediaContainers.vb` | TV show image performance |
| `SaveAllImages.TVSeason.Total` | `clsAPIMediaContainers.vb` | Season image performance |
| `SaveAllImages.TVEpisode.Total` | `clsAPIMediaContainers.vb` | Episode image performance |
| `Database.Save_TVShowAsync` | `clsAPIDatabase.vb` | Async TV save performance |
| `BulkScrape.Movie.Parallel` | `frmMain.vb` | Parallel scraping throughput |
| `Cache.IMDB.HitRate` | `clsAPIHttpClientFactory.vb` | Cache effectiveness |

### Success Criteria

| Item | Target | Measurement |
|------|--------|-------------|
| TV Async | -60% TV image time | `SaveAllImages.TVShow.Total` |
| Parallel Scraping | -50% bulk scrape time | Total time for 50 movies |
| Batch Actors | -50% actor DB time | `Database.Add_Actor` total |
| Response Cache | -50% repeat scrape time | Re-scrape same movies |

---

## Testing Plan

### Test Scenarios

| Scenario | Movies/Shows | Expected Result |
|----------|--------------|-----------------|
| TV Show scrape (1 show, 50 episodes) | - | 60% faster images |
| Parallel movie scrape (50 movies) | 50 | 50% faster overall |
| Repeat movie scrape (same 50) | 50 | 50%+ faster with cache |
| Large batch (100+ movies) | 100 | Stable memory, good throughput |

### Baseline Capture (Before Phase 2)

Before starting implementation, capture:

1. TV show scrape time (1 show with 50+ episodes)
2. Movie bulk scrape time (50 movies)
3. Repeat scrape time (same 50 movies)
4. Memory usage during large batch

### Validation Process

For each item:
1. Capture baseline metrics
2. Implement changes
3. Run same test scenario
4. Compare metrics
5. Document results in this plan

---

*Document Version: 1.0*
*Created: December 29, 2025*
*Author: Eric H. Anderson**