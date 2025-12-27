# Performance Improvements - Phase 1 Implementation Plan

| Document Info | |
|---------------|---|
| **Version** | 1.1 |
| **Created** | December 27, 2025 |
| **Updated** | December 27, 2025 |
| **Author** | Eric H. Anderson |
| **Status** | Planning |
| **Reference** | [PerformanceAnalysis.md](PerformanceAnalysis.md) |

---

## Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-27 | Eric H. Anderson | Initial plan creation |
| 1.1 | 2025-12-27 | Eric H. Anderson | Added Item 0: Performance Metrics Tracking as prerequisite |

---

## Overview

This plan covers the 4 high-priority performance improvements identified in the Performance Analysis. Items are ordered by impact-to-risk ratio for optimal implementation sequence.

**Prerequisites:** Performance metrics tracking must be implemented first to establish baselines.

**Total Estimated Effort:** 4-5 days
**Expected Performance Gain:** 40-60% improvement in bulk scraping operations

---

## Progress Tracker

| # | Item | Status | Assigned | Completed |
|---|------|--------|----------|-----------|
| 0 | Performance Metrics Tracking | ⬜ Not Started | | |
| 1 | Shared HttpClient | ⬜ Not Started | | |
| 2 | Database Indices | ⬜ Not Started | | |
| 3 | TMDB append_to_response | ⬜ Not Started | | |
| 4 | Parallel Image Downloads | ⬜ Not Started | | |

**Legend:** ⬜ Not Started | 🔄 In Progress | ✅ Complete | ⏸️ Blocked

---

## Item 0: Performance Metrics Tracking (PREREQUISITE)

**Effort:** 4-6 hours | **Risk:** Low | **Impact:** Critical for measuring improvements

### Objective
Create a reusable performance tracking utility to measure and log operation timings, enabling baseline capture and improvement validation.

### Files to Create/Modify
- `EmberAPI\clsAPIPerformanceTracker.vb` - New file for tracking utility
- `EmberAPI\EmberAPI.vbproj` - Add new file reference

### Implementation Steps

- [ ] **0.1** Create `PerformanceTracker` class in `EmberAPI`
    - Thread-safe metric collection using `ConcurrentDictionary`
    - Track operation name, duration, timestamp
    - Support for nested/hierarchical operations
    - Automatic min/max/avg calculation

- [ ] **0.2** Implement core tracking methods
    - `StartOperation(name As String) As OperationScope` - Returns disposable scope
    - `Track(Of T)(name As String, action As Func(Of T)) As T` - Inline tracking
    - `TrackAsync(Of T)(name As String, action As Func(Of Task(Of T))) As Task(Of T)` - Async support

- [ ] **0.3** Implement reporting methods
    - `GetMetrics() As Dictionary(Of String, MetricSummary)` - Get all metrics
    - `GetMetric(name As String) As MetricSummary` - Get specific metric
    - `LogAllMetrics()` - Write summary to NLog
    - `ExportToCsv(filePath As String)` - Export for analysis
    - `Reset()` - Clear all metrics

- [ ] **0.4** Add instrumentation points for baseline capture
    - TMDB movie scrape operation
    - IMDB movie scrape operation
    - Image download operation
    - Database actor lookup
    - Database movie save

- [ ] **0.5** Create baseline capture utility/test
    - Scrape 20 movies and log metrics
    - Document baseline values in Testing Plan section below

### Class Design

    Public Class PerformanceTracker
        Private Shared _metrics As New ConcurrentDictionary(Of String, MetricData)
        Private Shared _logger As Logger = LogManager.GetCurrentClassLogger()
        
        Public Shared Function StartOperation(name As String) As OperationScope
        Public Shared Function Track(Of T)(name As String, action As Func(Of T)) As T
        Public Shared Function TrackAsync(Of T)(name As String, action As Func(Of Task(Of T))) As Task(Of T)
        Public Shared Sub LogAllMetrics()
        Public Shared Sub ExportToCsv(filePath As String)
        Public Shared Sub Reset()
    End Class
    
    Public Class MetricData
        Public Property Name As String
        Public Property Count As Integer
        Public Property TotalMs As Double
        Public Property MinMs As Double
        Public Property MaxMs As Double
        Public ReadOnly Property AvgMs As Double
    End Class
    
    Public Class OperationScope
        Implements IDisposable
        ' Automatically records duration on Dispose
    End Class

### Usage Examples

    ' Method 1: Using scope (recommended for methods)
    Using scope = PerformanceTracker.StartOperation("TMDB.GetMovieInfo")
        result = GetMovieFromApi(id)
    End Using
    
    ' Method 2: Inline tracking (for simple operations)
    Dim movie = PerformanceTracker.Track("TMDB.GetMovieInfo", 
        Function() GetMovieFromApi(id))
    
    ' Method 3: Async tracking
    Dim movie = Await PerformanceTracker.TrackAsync("TMDB.GetMovieInfoAsync",
        Async Function() Await GetMovieFromApiAsync(id))
    
    ' Export results
    PerformanceTracker.LogAllMetrics()
    PerformanceTracker.ExportToCsv("C:\Temp\perf-baseline.csv")

### Acceptance Criteria
- [ ] Tracker compiles and integrates with EmberAPI
- [ ] Minimal performance overhead (<1ms per tracked operation)
- [ ] Thread-safe for concurrent scraping operations
- [ ] Metrics persist across multiple operations until Reset called
- [ ] CSV export produces valid, analyzable data
- [ ] NLog integration shows metrics in standard log output

---

## Item 1: Shared HttpClient Implementation

**Effort:** 2-4 hours | **Risk:** Low | **Impact:** High

**Depends On:** Item 0 (for measuring improvement)

### Objective
Replace per-instance `HttpClient` creation with a shared, thread-safe singleton using connection pooling.

### Files to Modify
- `EmberAPI\clsAPIHTTP.vb` - Add `HttpClientFactory` module
- `Addons\scraper.Data.OMDb\Scraper\clsScrapeOMDb.vb` - Use shared client

### Implementation Steps

- [ ] **1.1** Create `HttpClientFactory` module in `EmberAPI`
    - Lazy initialization with thread safety
    - Configure `MaxConnectionsPerServer = 10`
    - Enable automatic GZip/Deflate decompression
    - Set timeout to 30 seconds

- [ ] **1.2** Update `clsScrapeOMDb.vb`
    - Remove local `_HttpClient` field (line 37)
    - Replace `New HttpClient` (line 58) with `HttpClientFactory.SharedClient`

- [ ] **1.3** Search for other `New HttpClient` instances and refactor

- [ ] **1.4** Add performance tracking to HTTP operations

- [ ] **1.5** Test scraping operations with OMDb addon

- [ ] **1.6** Capture post-implementation metrics and compare to baseline

### Acceptance Criteria
- [ ] No new `HttpClient` instances created per request
- [ ] Connection reuse confirmed via network monitoring
- [ ] All existing scraper tests pass
- [ ] Metrics show improvement in HTTP operation times

---

## Item 2: Database Indices

**Effort:** 1-2 hours | **Risk:** Low | **Impact:** Medium-High

**Depends On:** Item 0 (for measuring improvement)

### Objective
Add missing indices to SQLite database for frequently queried columns.

### Files to Modify
- `EmberAPI\clsAPIDatabase.vb` - Add index creation in schema initialization

### Implementation Steps

- [ ] **2.1** Identify index creation location in `clsAPIDatabase.vb`
    - Locate database initialization/migration section

- [ ] **2.2** Add index creation SQL statements
    - `idx_actors_name` on `actors(strActor COLLATE NOCASE)`
    - `idx_movies_imdb` on `movies(strIMDB)`
    - `idx_movies_tmdb` on `movies(strTMDB)`
    - `idx_tvshows_imdb` on `tvshows(strIMDB)`
    - `idx_tvshows_tmdb` on `tvshows(strTMDB)`

- [ ] **2.3** Add SQLite PRAGMA optimizations
    - `PRAGMA journal_mode = WAL`
    - `PRAGMA synchronous = NORMAL`
    - `PRAGMA cache_size = -20000`

- [ ] **2.4** Add performance tracking to database operations

- [ ] **2.5** Test with existing database (migration scenario)

- [ ] **2.6** Test with fresh database (new install scenario)

- [ ] **2.7** Capture post-implementation metrics and compare to baseline

### Acceptance Criteria
- [ ] Indices created on new database initialization
- [ ] Existing databases upgraded without data loss
- [ ] Actor lookup queries show improved performance
- [ ] Metrics show improvement in database operation times

---

## Item 3: TMDB append_to_response Optimization

**Effort:** 4-8 hours | **Risk:** Low | **Impact:** High

**Depends On:** Item 0 (for measuring improvement)

### Objective
Consolidate multiple TMDB API calls into single requests using the `append_to_response` feature (already partially implemented via `MovieMethods` flags).

### Files to Modify
- `Addons\scraper.TMDB.Data\Scraper\clsScrapeTMDB.vb`

### Implementation Steps

- [ ] **3.1** Audit current API call patterns
    - Review `GetInfo_Movie` method (line 316+)
    - Identify any separate calls for Credits, Releases, Videos
    - Document current `MovieMethods` flags used

- [ ] **3.2** Ensure all needed methods combined in single call
    - Verify `MovieMethods.Credits` included
    - Verify `MovieMethods.Releases` included
    - Verify `MovieMethods.Videos` included
    - Add `MovieMethods.Images` if missing
    - Add `MovieMethods.AlternativeTitles` if needed

- [ ] **3.3** Review TV show scraping for similar optimization
    - Check `GetInfo_TVShow` method
    - Apply same consolidation pattern

- [ ] **3.4** Remove any redundant follow-up API calls

- [ ] **3.5** Add performance tracking to TMDB API operations

- [ ] **3.6** Test fallback language behavior still works

- [ ] **3.7** Verify all scraped data fields populate correctly

- [ ] **3.8** Capture post-implementation metrics and compare to baseline

### Acceptance Criteria
- [ ] Single API call retrieves all movie data
- [ ] Single API call retrieves all TV show data
- [ ] Fallback English language still functions
- [ ] No regression in scraped data completeness
- [ ] Metrics show reduction in API call count

---

## Item 4: Parallel Image Downloads

**Effort:** 1-2 days | **Risk:** Medium | **Impact:** High

**Depends On:** Item 0 (for measuring improvement), Item 1 (shared HttpClient)

### Objective
Enable concurrent image downloads with controlled parallelism using `SemaphoreSlim` throttling.

### Files to Modify
- `EmberAPI\clsAPIHTTP.vb` - Add async download methods
- `EmberAPI\clsAPIImages.vb` - Add parallel download orchestration
- `EmberMediaManager\dlgImgSelect.vb` - Update UI to use parallel downloads

### Implementation Steps

- [ ] **4.1** Add async image download method to `clsAPIHTTP.vb`
    - Create `DownloadImageAsync(url As String) As Task(Of Image)`
    - Use shared `HttpClient` from Item 1
    - Handle exceptions gracefully (return Nothing on failure)

- [ ] **4.2** Create parallel download orchestrator in `clsAPIImages.vb`
    - Add `DownloadImagesParallelAsync` method
    - Implement `SemaphoreSlim` with max 5 concurrent downloads
    - Return results as they complete

- [ ] **4.3** Update `dlgImgSelect.vb` image loading
    - Replace sequential download loop with parallel method
    - Ensure UI thread marshaling for progress updates
    - Handle cancellation properly

- [ ] **4.4** Add performance tracking to image download operations

- [ ] **4.5** Test with various image counts (5, 20, 50 images)

- [ ] **4.6** Verify memory usage remains stable

- [ ] **4.7** Test cancellation mid-download

- [ ] **4.8** Capture post-implementation metrics and compare to baseline

### Acceptance Criteria
- [ ] Images download 3-5x faster than sequential
- [ ] No UI freezing during downloads
- [ ] Memory usage does not spike excessively
- [ ] Cancellation works cleanly
- [ ] Failed downloads don't crash the batch
- [ ] Metrics confirm 3-5x improvement in image download times

---

## Testing Plan

### Baseline Capture (After Item 0)

Record baseline metrics using `PerformanceTracker`:

| Metric | Baseline Value | Captured Date |
|--------|----------------|---------------|
| TMDB Single Movie Scrape (avg ms) | | |
| IMDB Single Movie Scrape (avg ms) | | |
| 20 Movie Batch Scrape (total sec) | | |
| Single Image Download (avg ms) | | |
| 20 Image Batch Download (total sec) | | |
| Actor DB Lookup (avg ms) | | |
| Movie DB Save (avg ms) | | |
| API Calls per Movie Scrape | | |
| Memory Peak During Batch (MB) | | |

### Post-Implementation Validation

After each item, capture metrics and calculate improvement:

| Metric | Baseline | Item 1 | Item 2 | Item 3 | Item 4 | Final Improvement |
|--------|----------|--------|--------|--------|--------|-------------------|
| TMDB Movie Scrape (ms) | | | | | | |
| 20 Movie Batch (sec) | | | | | | |
| 20 Image Download (sec) | | | | | | |
| Actor Lookup (ms) | | | | | | |
| API Calls per Movie | | | | | | |
| Memory Peak (MB) | | | | | | |

---

## Rollback Plan

Each item can be reverted independently:

0. **Performance Tracker** - Can be disabled via config flag, no functional impact
1. **HttpClient** - Restore original `New HttpClient` calls
2. **Database Indices** - Indices can be dropped without data loss
3. **TMDB Methods** - Revert to original `MovieMethods` flags
4. **Parallel Downloads** - Restore sequential download loop

---

## Sign-Off

| Phase | Reviewer | Date | Notes |
|-------|----------|------|-------|
| Planning Complete | | | |
| Item 0 Complete | | | |
| Baseline Captured | | | |
| Item 1 Complete | | | |
| Item 2 Complete | | | |
| Item 3 Complete | | | |
| Item 4 Complete | | | |
| Phase 1 Complete | | | |

---

*Next Phase: See [PerformanceAnalysis.md](PerformanceAnalysis.md) Section 8 for Medium Priority items*