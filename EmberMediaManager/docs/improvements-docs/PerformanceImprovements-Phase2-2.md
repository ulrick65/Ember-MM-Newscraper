# Performance Improvements - Phase 2 Item 2: Parallel Movie Scraping Design

| Document Info | |
|---------------|---|
| **Version** | 1.0 |
| **Created** | December 30, 2025 |
| **Updated** | December 30, 2025 |
| **Author** | Eric H. Anderson |
| **Status** | Planning |
| **Parent Document** | [PerformanceImprovements-Phase2.md](PerformanceImprovements-Phase2.md) |
| **Reference** | [ScrapingProcessMovies.md](../process-docs/ScrapingProcessMovies.md), [ScrapingProcessTvShows.md](../process-docs/ScrapingProcessTvShows.md), [PerformanceImprovements-Phase1.md](PerformanceImprovements-Phase1.md) |

---

## Table of Contents

- [Revision History](#revision-history)
- [Executive Summary](#executive-summary)
- [Part 1: Current Architecture Analysis](#part-1-current-architecture-analysis)
- [Part 2: Thread Safety Audit](#part-2-thread-safety-audit)
- [Part 3: Design Options](#part-3-design-options)
- [Part 4: Recommended Approach](#part-4-recommended-approach)
- [Part 5: Implementation Phases](#part-5-implementation-phases)
- [Part 6: Risk Assessment](#part-6-risk-assessment)
- [Part 7: Test Plan](#part-7-test-plan)
- [Part 8: Rollback Plan](#part-8-rollback-plan)
- [Part 9: Success Criteria](#part-9-success-criteria)
- [Appendix A: Code References](#appendix-a-code-references)
- [Appendix B: Metrics to Capture](#appendix-b-metrics-to-capture)

---

## Revision History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | 2025-12-30 | Eric H. Anderson | Initial design document creation |

---

## Executive Summary

### Problem Statement

The bulk movie scraping process in Ember Media Manager processes movies **sequentially**, resulting in long scrape times for large libraries. With TMDB + IMDB scraping consuming 73% of total time (~3 seconds per movie), a 50-movie scrape takes approximately 200 seconds.

### Proposed Solution

Implement parallel movie scraping using `Parallel.ForEach` with controlled concurrency, allowing multiple movies to be scraped simultaneously while maintaining thread safety and proper progress reporting.

### Expected Outcome

| Metric | Current | Target |
|--------|---------|--------|
| 50-movie scrape time | ~200 seconds | ~70-80 seconds |
| Throughput | 15 movies/min | 40-45 movies/min |
| **Improvement** | - | **60-65%** |

### Effort and Risk

| Aspect | Assessment |
|--------|------------|
| **Effort** | 1-2 days |
| **Risk Level** | Medium |
| **Complexity** | High (thread safety, UI coordination) |

---

## Part 1: Current Architecture Analysis

### 1.1 Overview

The movie scraping process is controlled by `bwMovieScraper`, a `BackgroundWorker` that runs in `frmMain.vb`. Movies are processed one at a time in a sequential loop.

### 1.2 Current Flow

    bwMovieScraper_DoWork()
            │
            ▼
    For Each movie in ScrapeList:     ← SEQUENTIAL
            │
            ├──► Load movie from database
            ├──► ScrapeData_Movie()      ← TMDB: 1.1s, IMDB: 1.9s
            ├──► ScrapeImage_Movie()     ← Collects URLs
            ├──► ScrapeTheme_Movie()     ← Optional
            ├──► ScrapeTrailer_Movie()   ← Optional
            ├──► Save_MovieAsync()       ← Downloads images, saves to DB
            ├──► ReportProgress()
            │
            ▼
    Next movie

### 1.3 Key Code Location

**File:** `EmberMediaManager\frmMain.vb`
**Method:** `bwMovieScraper_DoWork`
**Lines:** ~1483-1653

### 1.4 Time Breakdown Per Movie

| Phase | Time (ms) | % of Total |
|-------|-----------|------------|
| TMDB Scraping | 1,083 | 27% |
| IMDB Scraping | 1,857 | 46% |
| Image Downloads | 340 | 8% |
| Database Save | 556 | 14% |
| Other | 164 | 4% |
| **Total** | **~4,000** | **100%** |

### 1.5 Current Structures

**Arguments Structure** (defined in `frmMain.vb`):

    Private Structure Arguments
        Dim ScrapeList As List(Of ScrapeItem)
        Dim ScrapeOptions As Structures.ScrapeOptions
        Dim ScrapeType As Enums.ScrapeType
        ' ... other fields
    End Structure

**ScrapeItem Structure** (defined in `frmMain.vb`):

    Structure ScrapeItem
        Dim DataRow As DataRow
        Dim ScrapeModifiers As Structures.ScrapeModifiers
    End Structure

**Results Structure** (defined in `frmMain.vb`):

    Private Structure Results
        Dim DBElement As Database.DBElement
        Dim ScrapeType As Enums.ScrapeType
        Dim Cancelled As Boolean
    End Structure

---

## Part 2: Thread Safety Audit

### 2.1 Components to Analyze

| Component | File | Thread Safety Status |
|-----------|------|---------------------|
| `ModulesManager.ScrapeData_Movie` | `clsAPIModules.vb` | 🔴 Needs Analysis |
| `ModulesManager.ScrapeImage_Movie` | `clsAPIModules.vb` | 🔴 Needs Analysis |
| `Master.DB.Load_Movie` | `clsAPIDatabase.vb` | 🟡 SQLite WAL Mode |
| `Master.DB.Save_MovieAsync` | `clsAPIDatabase.vb` | 🟡 SQLite WAL Mode |
| `bwMovieScraper.ReportProgress` | `frmMain.vb` | 🟢 Thread-Safe |
| Event Handlers | `clsAPIModules.vb` | 🔴 Not Thread-Safe |

**Legend:** 🟢 Safe | 🟡 Conditionally Safe | 🔴 Needs Work

### 2.2 ScrapeData_Movie Analysis

**Location:** `clsAPIModules.vb`, lines ~962-1050

**Potential Issues:**

| Issue | Location | Severity |
|-------|----------|----------|
| Event handler registration | `AddHandler`/`RemoveHandler` | High |
| Shared module list iteration | `externalScrapersModules_Data_Movie` | Medium |
| Clone operation on DBElement | `DBElement.CloneDeep` | Low |

**Event Handler Pattern (Current):**

    For Each _externalScraperModule In modules
        AddHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_Movie
        ret = _externalScraperModule.ProcessorModule.Scraper_Movie(...)
        RemoveHandler _externalScraperModule.ProcessorModule.ScraperEvent, AddressOf Handler_ScraperEvent_Movie
    Next

**Problem:** Multiple threads adding/removing handlers simultaneously can cause race conditions.

### 2.3 Database Thread Safety

**SQLite WAL Mode** (implemented in Phase 1):

- Allows concurrent reads
- Allows one writer at a time
- Writers don't block readers
- Multiple writers queue automatically

**Current Connection:** Single shared connection in `Master.DB.MyVideosDBConn`

**Assessment:** WAL mode should handle concurrent saves, but we should verify with testing.

### 2.4 Scraper Module State

Each scraper module may have instance state. Need to verify:

| Scraper | File | Has Instance State? |
|---------|------|---------------------|
| TMDB Data | `clsScrapeTMDB.vb` | 🔴 To Verify |
| IMDB Data | `clsScrapeIMDB.vb` | 🔴 To Verify |
| TMDB Image | Addons folder | 🔴 To Verify |
| FanartTV Image | Addons folder | 🔴 To Verify |

### 2.5 UI Thread Interactions

| Operation | Thread | Marshaling Required? |
|-----------|--------|---------------------|
| `ReportProgress` | Background | No (built-in) |
| `DataRow` updates | UI | Yes |
| `RefreshRow_Movie` | UI | Yes |
| Status bar updates | UI | Via ReportProgress |

---

## Part 3: Design Options

### 3.1 Option A: Parallel.ForEach with Sequential Saves

**Description:** Scrape multiple movies in parallel, but save to database sequentially.

    Parallel.ForEach(scrapeList, options,
        Sub(item)
            ' Scrape data and images (parallel)
            Dim result = ScrapeMovieData(item)
            
            ' Add to thread-safe collection
            SyncLock saveLock
                resultsQueue.Enqueue(result)
            End SyncLock
        End Sub)
    
    ' Save all results sequentially
    For Each result In resultsQueue
        Save_MovieAsync(result).GetAwaiter().GetResult()
    Next

**Pros:**
- Simple database access pattern
- No SQLite contention concerns
- Easy progress reporting

**Cons:**
- Save phase is still sequential
- Memory usage higher (holds all results)
- Two-phase processing

### 3.2 Option B: Parallel.ForEach with Parallel Saves

**Description:** Full parallelism for both scraping and saving.

    Parallel.ForEach(scrapeList, options,
        Sub(item)
            Dim result = ScrapeMovieData(item)
            Save_MovieAsync(result).GetAwaiter().GetResult()
            ReportProgressThreadSafe(item)
        End Sub)

**Pros:**
- Maximum parallelism
- Lower memory usage
- Simpler code flow

**Cons:**
- SQLite contention possible
- Progress reporting more complex
- Higher risk of race conditions

### 3.3 Option C: Producer-Consumer Pattern

**Description:** Separate threads for scraping (producers) and saving (consumer).

    ' Producer threads (multiple)
    Parallel.ForEach(scrapeList, options,
        Sub(item)
            Dim result = ScrapeMovieData(item)
            blockingCollection.Add(result)
        End Sub)
    blockingCollection.CompleteAdding()
    
    ' Consumer thread (single)
    For Each result In blockingCollection.GetConsumingEnumerable()
        Save_MovieAsync(result).GetAwaiter().GetResult()
        ReportProgress(result)
    Next

**Pros:**
- Clean separation of concerns
- Controlled save throughput
- Backpressure handling

**Cons:**
- More complex implementation
- Requires coordination
- May not maximize throughput

### 3.4 Option Comparison

| Aspect | Option A | Option B | Option C |
|--------|----------|----------|----------|
| Complexity | Low | Medium | High |
| Parallelism | Partial | Full | Full |
| Memory Usage | Higher | Lower | Medium |
| SQLite Safety | High | Medium | High |
| Progress Accuracy | Good | Fair | Good |
| **Recommendation** | ✅ Start Here | Phase 2 | If Needed |

---

## Part 4: Recommended Approach

### 4.1 Selected Option: A (Parallel Scrape, Sequential Save)

**Rationale:**
1. Lowest risk for initial implementation
2. Addresses the main bottleneck (network I/O)
3. Avoids database contention issues
4. Easier to debug and validate

### 4.2 Architecture Overview

    bwMovieScraper_DoWork()
            │
            ▼
    ┌─────────────────────────────────────────────────────────────┐
    │  PHASE 1: PARALLEL SCRAPING                                 │
    │  Parallel.ForEach with MaxDegreeOfParallelism = 3           │
    │                                                             │
    │  For each movie (parallel):                                 │
    │    ├──► Load movie from database                            │
    │    ├──► ScrapeData_Movie() ← Skip event handlers            │
    │    ├──► ScrapeImage_Movie()                                 │
    │    ├──► ScrapeTheme_Movie()                                 │
    │    ├──► ScrapeTrailer_Movie()                               │
    │    └──► Add to ConcurrentBag<ScrapedMovie>                  │
    └─────────────────────────────────────────────────────────────┘
            │
            ▼
    ┌─────────────────────────────────────────────────────────────┐
    │  PHASE 2: SEQUENTIAL SAVES                                  │
    │  For each scraped movie:                                    │
    │    ├──► RunGeneric(ScraperMulti_Movie)                      │
    │    ├──► Save_MovieAsync()                                   │
    │    ├──► ReportProgress()                                    │
    │    └──► Check CancellationPending                           │
    └─────────────────────────────────────────────────────────────┘

### 4.3 Key Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Concurrency Level | 3 | Balance throughput vs API limits |
| Event Handlers | Skip in parallel | Not thread-safe |
| Progress Reporting | After save | More accurate |
| Cancellation | Check between phases | Clean cancellation |
| Error Handling | Continue on error | Don't fail entire batch |

### 4.4 New Method: ProcessMovieScrape_Parallel

**Purpose:** Thread-safe method to scrape a single movie without UI interaction.

**Signature:**

    Private Function ProcessMovieScrape_Parallel(
        ByVal tScrapeItem As ScrapeItem,
        ByVal ScrapeType As Enums.ScrapeType,
        ByVal ScrapeOptions As Structures.ScrapeOptions
    ) As ScrapedMovieResult

**Returns:**

    Private Class ScrapedMovieResult
        Public Property DBElement As Database.DBElement
        Public Property OldTitle As String
        Public Property NewTitle As String
        Public Property Cancelled As Boolean
        Public Property ScrapeModifiers As Structures.ScrapeModifiers
        Public Property ErrorMessage As String
    End Class

### 4.5 Modified bwMovieScraper_DoWork Flow

**Pseudocode:**

    Private Sub bwMovieScraper_DoWork(sender, e)
        Dim Args = DirectCast(e.Argument, Arguments)
        Dim scrapedResults As New ConcurrentBag(Of ScrapedMovieResult)
        Dim progressCount As Integer = 0
        
        ' Check if parallel scraping should be used
        Dim useParallel = Args.ScrapeList.Count > 1 AndAlso
                          Args.ScrapeType <> Enums.ScrapeType.SingleScrape
        
        If useParallel Then
            ' PHASE 1: Parallel Scraping
            Dim options As New ParallelOptions With {
                .MaxDegreeOfParallelism = 3
            }
            
            Parallel.ForEach(Args.ScrapeList, options,
                Sub(item, state)
                    If bwMovieScraper.CancellationPending Then
                        state.Break()
                        Return
                    End If
                    
                    Dim result = ProcessMovieScrape_Parallel(item, Args.ScrapeType, Args.ScrapeOptions)
                    scrapedResults.Add(result)
                End Sub)
            
            ' PHASE 2: Sequential Saves
            For Each result In scrapedResults
                If bwMovieScraper.CancellationPending Then Exit For
                If Not result.Cancelled Then
                    ModulesManager.Instance.RunGeneric(...)
                    Master.DB.Save_MovieAsync(result.DBElement, ...).GetAwaiter().GetResult()
                    bwMovieScraper.ReportProgress(...)
                End If
            Next
        Else
            ' Original sequential logic for single scrape
            ' ... existing code ...
        End If
    End Sub

---

## Part 5: Implementation Phases

### Phase 5.1: Thread Safety Analysis (2-4 hours)

**Objective:** Verify thread safety of all components.

**Tasks:**

- [ ] **5.1.1** Review `ScrapeData_Movie` for shared state
- [ ] **5.1.2** Review `ScrapeImage_Movie` for shared state
- [ ] **5.1.3** Review TMDB scraper module state
- [ ] **5.1.4** Review IMDB scraper module state
- [ ] **5.1.5** Test SQLite concurrent access with WAL mode
- [ ] **5.1.6** Document findings in this section

**Deliverable:** Updated thread safety table with verified status

### Phase 5.2: Create ScrapedMovieResult Class (1 hour)

**Objective:** Create data structure to hold scrape results.

**Tasks:**

- [ ] **5.2.1** Define `ScrapedMovieResult` class in `frmMain.vb`
- [ ] **5.2.2** Include all necessary properties
- [ ] **5.2.3** Add error handling properties

### Phase 5.3: Create ProcessMovieScrape_Parallel Method (2-4 hours)

**Objective:** Extract scrape logic into thread-safe method.

**Tasks:**

- [ ] **5.3.1** Create method signature
- [ ] **5.3.2** Copy scrape logic from `bwMovieScraper_DoWork`
- [ ] **5.3.3** Remove event handler registration
- [ ] **5.3.4** Remove UI interactions
- [ ] **5.3.5** Add comprehensive error handling
- [ ] **5.3.6** Add logging for debugging

### Phase 5.4: Modify bwMovieScraper_DoWork (2-4 hours)

**Objective:** Implement parallel scraping with sequential saves.

**Tasks:**

- [ ] **5.4.1** Add parallel vs sequential decision logic
- [ ] **5.4.2** Implement `Parallel.ForEach` loop
- [ ] **5.4.3** Implement sequential save loop
- [ ] **5.4.4** Update progress reporting
- [ ] **5.4.5** Handle cancellation in both phases
- [ ] **5.4.6** Preserve original logic for single scrape

### Phase 5.5: Testing and Validation (4-8 hours)

**Objective:** Verify correctness and measure performance.

**Tasks:**

- [ ] **5.5.1** Test with 10 movies
- [ ] **5.5.2** Test with 50 movies
- [ ] **5.5.3** Test cancellation during scrape phase
- [ ] **5.5.4** Test cancellation during save phase
- [ ] **5.5.5** Verify all metadata saved correctly
- [ ] **5.5.6** Verify all images downloaded correctly
- [ ] **5.5.7** Capture performance metrics
- [ ] **5.5.8** Compare with baseline

### Phase 5.6: Tuning and Documentation (2-4 hours)

**Objective:** Optimize concurrency level and document.

**Tasks:**

- [ ] **5.6.1** Test with `MaxDegreeOfParallelism` = 2
- [ ] **5.6.2** Test with `MaxDegreeOfParallelism` = 3
- [ ] **5.6.3** Test with `MaxDegreeOfParallelism` = 5
- [ ] **5.6.4** Document optimal setting
- [ ] **5.6.5** Update Phase 2 plan with results
- [ ] **5.6.6** Update process documentation

---

## Part 6: Risk Assessment

### 6.1 Risk Matrix

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| API Rate Limiting | Medium | High | Start with 3 concurrent, reduce if throttled |
| Data Corruption | Low | Critical | Extensive testing, rollback plan |
| Memory Pressure | Low | Medium | Process in batches if needed |
| Progress Inaccuracy | Medium | Low | Report after save, not scrape |
| Scraper Module Conflicts | Medium | High | Thread safety audit first |
| Database Deadlocks | Low | High | Sequential saves mitigate |

### 6.2 API Rate Limiting Details

**TMDB:**
- Rate limit: 40 requests per 10 seconds
- With 3 concurrent: ~12 requests/10 sec (safe)

**IMDB:**
- No official API, HTML scraping
- Risk of IP blocking with too many concurrent requests
- Mitigation: Keep concurrency at 3 or lower

### 6.3 Fallback Strategy

If parallel scraping causes issues:

1. **Immediate:** Reduce `MaxDegreeOfParallelism` to 2
2. **If still failing:** Reduce to 1 (effectively sequential)
3. **If data issues:** Revert to original code path

---

## Part 7: Test Plan

### 7.1 Unit Tests

| Test | Description | Expected Result |
|------|-------------|-----------------|
| `ProcessMovieScrape_Parallel` returns valid result | Scrape single movie | All metadata populated |
| `ProcessMovieScrape_Parallel` handles errors | Scrape with invalid ID | Returns error message, no exception |
| `ScrapedMovieResult` serialization | Round-trip test | All properties preserved |

### 7.2 Integration Tests

| Test | Movies | Expected Result |
|------|--------|-----------------|
| Small batch parallel | 10 | All scraped, ~30% faster |
| Medium batch parallel | 50 | All scraped, ~60% faster |
| Large batch parallel | 100 | All scraped, stable memory |
| Mixed content | 50 (various sources) | All scraped correctly |

### 7.3 Cancellation Tests

| Test | Action | Expected Result |
|------|--------|-----------------|
| Cancel during scrape phase | Click Cancel after 5 movies | Stops cleanly, partial results saved |
| Cancel during save phase | Click Cancel after 5 saves | Stops cleanly, completed saves preserved |
| Rapid cancel | Click Cancel immediately | No errors, no partial data |

### 7.4 Data Integrity Tests

| Test | Verification |
|------|--------------|
| Metadata accuracy | Compare scraped data to API responses |
| Image downloads | Verify all images saved to correct paths |
| NFO files | Validate NFO XML structure |
| Database records | Query database for all expected fields |

### 7.5 Performance Benchmarks

| Metric | Baseline | Target | Measurement Method |
|--------|----------|--------|-------------------|
| 50-movie scrape time | ~200 sec | ~70-80 sec | Wall clock time |
| Throughput | 15/min | 40-45/min | Movies completed / time |
| Memory usage | ~500 MB | <1 GB | Task Manager peak |
| CPU usage | ~25% | ~50-75% | Task Manager average |

---

## Part 8: Rollback Plan

### 8.1 Trigger Conditions

Rollback if any of these occur:
- Data corruption in scraped movies
- API rate limiting affecting normal operation
- Application crashes during scraping
- Memory usage exceeds 2 GB
- Performance worse than baseline

### 8.2 Rollback Steps

1. **Revert code changes** in `frmMain.vb`
2. **Remove** `ScrapedMovieResult` class
3. **Remove** `ProcessMovieScrape_Parallel` method
4. **Restore** original `bwMovieScraper_DoWork` logic
5. **Test** sequential scraping still works
6. **Document** failure reasons for future reference

### 8.3 Version Control

- All changes on feature branch: `feature/parallel-movie-scraping`
- Tag baseline before changes: `pre-parallel-scraping`
- Squash merge only after validation

---

## Part 9: Success Criteria

### 9.1 Must Have

- [ ] 50-movie scrape completes in under 90 seconds
- [ ] All metadata saved correctly (100% accuracy)
- [ ] All images downloaded correctly (100% accuracy)
- [ ] Cancellation works cleanly
- [ ] No memory leaks
- [ ] No API rate limiting issues

### 9.2 Should Have

- [ ] 50% improvement in bulk scrape time
- [ ] Progress reporting is accurate
- [ ] Error handling covers edge cases
- [ ] Documentation updated

### 9.3 Nice to Have

- [ ] 60% improvement in bulk scrape time
- [ ] Configurable concurrency level in settings
- [ ] Automatic concurrency adjustment based on errors

---

## Appendix A: Code References

### A.1 Key Files

| File | Purpose | Lines |
|------|---------|-------|
| `frmMain.vb` | Main scraping loop | ~1483-1653 |
| `clsAPIModules.vb` | Scraper orchestration | ~962-1050 |
| `clsAPIDatabase.vb` | Database operations | Various |
| `clsScrapeTMDB.vb` | TMDB scraper | Various |
| `clsScrapeIMDB.vb` | IMDB scraper | Various |

### A.2 Key Methods

| Method | File | Purpose |
|--------|------|---------|
| `bwMovieScraper_DoWork` | `frmMain.vb` | Main scraping loop |
| `ScrapeData_Movie` | `clsAPIModules.vb` | Data scraping orchestration |
| `ScrapeImage_Movie` | `clsAPIModules.vb` | Image scraping orchestration |
| `Save_MovieAsync` | `clsAPIDatabase.vb` | Database save with async images |

### A.3 Key Structures

| Structure | File | Purpose |
|-----------|------|---------|
| `Arguments` | `frmMain.vb` | BackgroundWorker arguments |
| `Results` | `frmMain.vb` | BackgroundWorker results |
| `ScrapeItem` | `frmMain.vb` | Single movie scrape info |
| `ScrapeModifiers` | `clsAPICommon.vb` | What to scrape |
| `ScrapeOptions` | `clsAPICommon.vb` | Field-level options |

---

## Appendix B: Metrics to Capture

### B.1 New Performance Metrics

| Metric Name | Location | Purpose |
|-------------|----------|---------|
| `BulkScrape.Movie.Parallel.Total` | `frmMain.vb` | Total parallel scrape time |
| `BulkScrape.Movie.Parallel.ScrapePhase` | `frmMain.vb` | Time in parallel scrape phase |
| `BulkScrape.Movie.Parallel.SavePhase` | `frmMain.vb` | Time in sequential save phase |
| `BulkScrape.Movie.Parallel.Count` | `frmMain.vb` | Number of movies scraped |
| `BulkScrape.Movie.Parallel.Errors` | `frmMain.vb` | Number of scrape errors |

### B.2 Comparison Metrics

| Metric | Baseline Source | New Source |
|--------|-----------------|------------|
| Total scrape time | `PerformanceTracker` logs | New metrics |
| Movies per minute | Calculated | New metrics |
| Memory usage | Manual observation | New metrics |
| Error rate | Log analysis | New metrics |

---

*Document Version: 1.0*
*Created: December 30, 2025*
*Author: Eric H. Anderson*