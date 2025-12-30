# Performance Improvements - Phase 2 Implementation Plan

| Document Info | |
|---------------|---|
| **Version** | 1.0 |
| **Created** | December 29, 2025 |
| **Updated** | December 29, 2025 |
| **Author** | Eric H. Anderson |
| **Status** | Planning |
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
| 1 | TV Show Async Support | ⬜ Not Started | |
| 2 | Parallel Movie Scraping | ⬜ Not Started | |
| 3 | Batch Actor Inserts | ⬜ Not Started | |
| 4 | Response Caching | ⬜ Not Started | |

**Legend:** ⬜ Not Started | 🔄 In Progress | ✅ Complete | ⏸️ Deferred

---

## Item 1: TV Show Async Support

**Effort:** 4-8 hours | **Risk:** Low | **Impact:** High for TV scraping

### Objective

Extend `SaveAllImagesAsync` to support TV Shows, Seasons, and Episodes, enabling parallel image downloads for TV content.

### Current State

From `clsAPIMediaContainers.vb` `SaveAllImagesAsync`:

    ' Only Movie is supported for async
    If DBElement.ContentType <> Enums.ContentType.Movie Then
        SaveAllImages(DBElement, ForceFileCleanup)
        Return DBElement
    End If

TV Shows currently fall back to sequential `SaveAllImages()`.

### Why This Item First

1. **Lowest risk:** Infrastructure already exists from Phase 1
2. **Highest ROI for TV:** Same 61% improvement potential
3. **Multiplicative benefit:** TV shows have many more images (show + seasons + episodes)
4. **No architecture changes:** Just extend existing async methods

### Files to Modify

| File | Changes |
|------|---------|
| `EmberAPI\clsAPIMediaContainers.vb` | Extend `SaveAllImagesAsync` for TV content types |
| `EmberAPI\clsAPIDatabase.vb` | Create `Save_TVShowAsync`, `Save_TVSeasonAsync`, `Save_TVEpisodeAsync` |
| `EmberMediaManager\frmMain.vb` | Update TV bulk scrape to use async saves |

### Implementation Steps

- [ ] **1.1** Add TV Show support to `SaveAllImagesAsync`
    - Handle `ContentType.TVShow` images (Poster, Fanart, Banner, etc.)
    - Collect all show-level images for parallel download
    - Reuse existing `DownloadImagesParallelAsync` infrastructure

- [ ] **1.2** Add TV Season support to `SaveAllImagesAsync`
    - Handle `ContentType.TVSeason` images
    - Season Poster, Banner, Fanart, Landscape

- [ ] **1.3** Add TV Episode support to `SaveAllImagesAsync`
    - Handle `ContentType.TVEpisode` images
    - Episode Poster (Still), Fanart

- [ ] **1.4** Create `Save_TVShowAsync` method
    - Copy `Save_TVShow` method body
    - Replace `SaveAllImages` with `Await SaveAllImagesAsync`
    - Return `Task(Of DBElement)`

- [ ] **1.5** Create `Save_TVSeasonAsync` method
    - Same pattern as Save_TVShowAsync

- [ ] **1.6** Create `Save_TVEpisodeAsync` method
    - Same pattern as Save_TVShowAsync

- [ ] **1.7** Update TV bulk scrape workflow
    - Location: `frmMain.vb` `bwTVScraper_DoWork`
    - Replace `Save_TVShow` with `Save_TVShowAsync().GetAwaiter().GetResult()`
    - Same for seasons and episodes

- [ ] **1.8** Add performance instrumentation
    - `SaveAllImages.TVShow.Total`
    - `SaveAllImages.TVSeason.Total`
    - `SaveAllImages.TVEpisode.Total`

- [ ] **1.9** Test with TV show scrape
    - Verify no image corruption
    - Measure performance improvement

### Expected Impact

For a TV show with 5 seasons and 100 episodes:

| Level | Images | Current (Sequential) | Projected (Parallel) |
|-------|--------|---------------------|----------------------|
| Show | ~10 | 800 ms | 320 ms (-60%) |
| Seasons | ~20 (4/season) | 1,600 ms | 640 ms (-60%) |
| Episodes | ~200 (2/episode) | 16,000 ms | 6,400 ms (-60%) |
| **Total** | **~230** | **18,400 ms** | **7,360 ms** |

**Projected improvement:** ~60% faster image downloads for TV content

### Acceptance Criteria

- [ ] `SaveAllImagesAsync` handles TVShow, TVSeason, TVEpisode content types
- [ ] `Save_TVShowAsync`, `Save_TVSeasonAsync`, `Save_TVEpisodeAsync` created
- [ ] TV bulk scrape uses async saves
- [ ] No image corruption or missing images
- [ ] Metrics show ~60% improvement in TV image operations

---

## Item 2: Parallel Movie Scraping

**Effort:** 1-2 days | **Risk:** Medium | **Impact:** High

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
*Author: Eric H. Anderson*