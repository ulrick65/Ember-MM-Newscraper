# IMDB Scraper Performance Analysis

| Document Info | |
|---------------|---|
| **Version** | 1.0 |
| **Created** | December 31, 2025 |
| **Author** | ulrick65 |
| **Status** | 📋 Analysis |
| **Scraper File** | Addons\scraper.IMDB.Data\Scraper\clsScrapeIMDB.vb |

---

## Table of Contents

- [Executive Summary](#executive-summary)
- [Architecture Overview](#architecture-overview)
- [Performance Bottlenecks](#performance-bottlenecks)
- [Comparison with TMDB Scraper](#comparison-with-tmdb-scraper)
- [Improvement Opportunities](#improvement-opportunities)
- [Implementation Recommendations](#implementation-recommendations)

---

## Executive Summary

### Current State

The IMDB scraper uses **HTML web scraping** to extract data from IMDB pages. This approach is inherently slower than API-based scrapers (like TMDB) because:

1. **Multiple HTTP requests** required per movie
2. **HTML parsing overhead** for each page
3. **Sequential request pattern** (no parallelization)
4. **Large payload sizes** (full HTML pages vs JSON)

### Key Findings

| Aspect | IMDB Scraper | TMDB Scraper |
|--------|--------------|--------------|
| Data Source | HTML web scraping | REST API |
| Request Count (single movie) | 1-2 requests | 1 request |
| Request Count (search) | Up to 6 requests | 1 request |
| Payload Size | 500KB-1MB HTML | 10-50KB JSON |
| Parallelization | None | Async/Await |
| Caching | None | Client caching |

---

## Architecture Overview

### Data Flow

    User Request
        │
        ▼
    Scraper_Movie() / Scraper_TV()
        │
        ├── Has IMDB ID? ──Yes──► GetMovieInfo() / GetTVShowInfo()
        │                              │
        │                              ├── HTTP Request (reference page)
        │                              ├── Parse __NEXT_DATA__ JSON
        │                              └── Extract fields
        │
        └── No IMDB ID ──► SearchMovie() / SearchTVShow()
                               │
                               ├── HTTP Request (exact search)
                               ├── HTTP Request (popular - optional)
                               ├── HTTP Request (partial - optional)
                               ├── HTTP Request (TV titles - optional)
                               ├── HTTP Request (video titles - optional)
                               └── HTTP Request (short titles - optional)

### Key Components

| File | Purpose |
|------|---------|
| clsScrapeIMDB.vb | Main scraper logic (~1560 lines) |
| clsIMDBJson.vb | JSON deserialization classes |
| IMDB_Data.vb | Module interface and settings |

### HTTP Request Pattern

The scraper uses `HtmlAgilityPack.HtmlWeb.Load()` which is **synchronous**:

    Dim webParsing As New HtmlWeb
    htmldReference = webParsing.Load(String.Concat("https://www.imdb.com/title/", id, "/reference/"))

---

## Performance Bottlenecks

### 1. Sequential Search Requests (CRITICAL)

**Location:** Lines 1301-1466 in SearchMovie()

**Problem:** Up to 6 sequential HTTP requests when all search options are enabled:

    ' Each of these is a separate HTTP request - executed sequentially
    htmldResultsExact = webParsing.Load(...)        ' Always
    htmldResultsTvTitles = webParsing.Load(...)     ' If SearchTvTitles enabled
    htmldResultsVideoTitles = webParsing.Load(...)  ' If SearchVideoTitles enabled
    htmldResultsShortTitles = webParsing.Load(...)  ' If SearchShortTitles enabled
    htmldResultsPartialTitles = webParsing.Load(...) ' If SearchPartialTitles enabled
    htmldResultsPopularTitles = webParsing.Load(...) ' If SearchPopularTitles enabled

**Impact:** Each request takes ~500-1500ms. Total: 3-9 seconds for search alone.

### 2. Synchronous HTTP Calls (HIGH)

**Location:** Throughout clsScrapeIMDB.vb

**Problem:** All HTTP calls are synchronous, blocking the thread.

Current IMDB pattern (synchronous):

    htmldReference = webParsing.Load(url)

TMDB pattern (asynchronous):

    APIResult = Task.Run(Function() _client.GetMovieAsync(...))

### 3. No Request Caching (MEDIUM)

**Problem:** Repeated requests for the same content are not cached. If you scrape the same movie twice, it makes all HTTP requests again.

### 4. Large Payload Parsing (MEDIUM)

**Problem:** Full HTML pages (~500KB-1MB) are downloaded and parsed to extract the `__NEXT_DATA__` JSON embedded in the page.

### 5. TV Show Episode Scraping (CRITICAL for TV)

**Location:** Lines 591-615 in GetTVSeasonInfo()

**Problem:** Each episode requires a separate HTTP request:

    For Each EpisodeItem In EpisodeItems
        Dim nEpisode As MediaContainers.EpisodeDetails = GetTVEpisodeInfo(EpisodeItem.id, filteredoptions)
        ' Each GetTVEpisodeInfo makes an HTTP request
    Next

**Impact:** A 20-episode season = 20+ HTTP requests sequentially.

---

## Comparison with TMDB Scraper

### API vs Web Scraping

| Feature | TMDB (API) | IMDB (Web Scraping) |
|---------|------------|---------------------|
| Authentication | API Key | None (public pages) |
| Data Format | JSON | HTML with embedded JSON |
| Rate Limiting | Documented limits | Undocumented, risk of blocking |
| Stability | Versioned API | HTML structure can change |
| Performance | Optimized endpoints | Full page downloads |

### Code Patterns

**TMDB - Async with proper error handling:**

    Public Function GetInfo_Movie(...) As MediaContainers.Movie
        Using scopeTotal = EmberAPI.PerformanceTracker.StartOperation("TMDB.GetInfo_Movie")
            Dim APIResult As Task(Of TMDbLib.Objects.Movies.Movie)
            
            Using scopeApi = EmberAPI.PerformanceTracker.StartOperation("TMDB.GetInfo_Movie.APICall")
                APIResult = Task.Run(Function() _client.GetMovieAsync(...))
                APIResult.Wait()
            End Using
            
            ' Process result...
        End Using
    End Function

**IMDB - Synchronous:**

    Public Function GetMovieInfo(...) As MediaContainers.Movie
        Using scopeTotal = EmberAPI.PerformanceTracker.StartOperation("IMDB.GetMovieInfo")
            Dim webParsing As New HtmlWeb
            
            Using scopeHttp = EmberAPI.PerformanceTracker.StartOperation("IMDB.GetMovieInfo.HttpRequest")
                htmldReference = webParsing.Load(url)
            End Using
            
            ' Process result...
        End Using
    End Function

---

## Improvement Opportunities

### Quick Wins (Low Risk, Medium Impact)

| Improvement | Effort | Impact | Risk |
|-------------|--------|--------|------|
| Disable unnecessary search options by default | 5 min | Medium | Very Low |
| Add HttpClient connection pooling | 30 min | Low-Medium | Low |
| Add basic response caching | 2 hours | Medium | Low |

### Medium-Term (Medium Risk, High Impact)

| Improvement | Effort | Impact | Risk |
|-------------|--------|--------|------|
| Parallelize search requests | 4 hours | High | Medium |
| Async HTTP requests | 8 hours | High | Medium |
| Parallelize episode scraping | 4 hours | Very High (TV) | Medium |

### Long-Term (Higher Risk, Very High Impact)

| Improvement | Effort | Impact | Risk |
|-------------|--------|--------|------|
| Use IMDB GraphQL API (if available) | 20+ hours | Very High | High |
| Implement scraper caching layer | 16 hours | High | Medium |

---

## Implementation Recommendations

### Phase 1: Quick Configuration Changes

**Recommendation:** Review default search settings

The following settings enable additional search requests. Consider if they should default to off:

    _SpecialSettings.SearchTvTitles      ' Additional HTTP request
    _SpecialSettings.SearchVideoTitles   ' Additional HTTP request
    _SpecialSettings.SearchShortTitles   ' Additional HTTP request
    _SpecialSettings.SearchPartialTitles ' Additional HTTP request
    _SpecialSettings.SearchPopularTitles ' Additional HTTP request

### Phase 2: Parallel Search Requests

**Current Code (Sequential):**

    If _SpecialSettings.SearchTvTitles Then
        htmldResultsTvTitles = webParsing.Load(url1)
    End If
    If _SpecialSettings.SearchVideoTitles Then
        htmldResultsVideoTitles = webParsing.Load(url2)
    End If
    ' etc...

**Proposed Code (Parallel):**

    Dim tasks As New List(Of Task(Of HtmlDocument))
    
    If _SpecialSettings.SearchTvTitles Then
        tasks.Add(Task.Run(Function() webParsing.Load(url1)))
    End If
    If _SpecialSettings.SearchVideoTitles Then
        tasks.Add(Task.Run(Function() webParsing.Load(url2)))
    End If
    
    Task.WaitAll(tasks.ToArray())

**Expected Improvement:** 6 sequential requests taking 6 seconds → parallel requests taking ~1-1.5 seconds

### Phase 3: Episode Scraping Parallelization

**Current Code (Sequential):**

    For Each EpisodeItem In EpisodeItems
        Dim nEpisode = GetTVEpisodeInfo(EpisodeItem.id, filteredoptions)
        nTVShow.KnownEpisodes.Add(nEpisode)
    Next

**Proposed Code (Parallel with throttling):**

    Dim semaphore As New SemaphoreSlim(4) ' Limit concurrent requests
    Dim tasks = EpisodeItems.Select(Function(ep)
        Return Task.Run(Async Function()
            Await semaphore.WaitAsync()
            Try
                Return GetTVEpisodeInfo(ep.id, filteredoptions)
            Finally
                semaphore.Release()
            End Try
        End Function)
    End Function).ToList()
    
    Dim results = Task.WhenAll(tasks).Result
    nTVShow.KnownEpisodes.AddRange(results.Where(Function(e) e IsNot Nothing))

**Expected Improvement:** 20 episodes taking 30 seconds → taking ~8 seconds (with 4 parallel requests)

### Phase 4: Response Caching

Add a simple in-memory cache for IMDB responses:

    Private Shared _responseCache As New Dictionary(Of String, CacheEntry)
    Private Shared _cacheLock As New Object()
    
    Private Class CacheEntry
        Public Property Response As HtmlDocument
        Public Property Timestamp As DateTime
    End Class
    
    Private Function LoadWithCache(url As String, cacheDuration As TimeSpan) As HtmlDocument
        SyncLock _cacheLock
            If _responseCache.ContainsKey(url) Then
                Dim entry = _responseCache(url)
                If DateTime.Now - entry.Timestamp < cacheDuration Then
                    Return entry.Response
                End If
            End If
        End SyncLock
        
        Dim response = webParsing.Load(url)
        
        SyncLock _cacheLock
            _responseCache(url) = New CacheEntry With {
                .Response = response,
                .Timestamp = DateTime.Now
            }
        End SyncLock
        
        Return response
    End Function

---

## Risk Considerations

### Rate Limiting

IMDB may rate-limit or block requests if too many are made in parallel. Recommendations:

- Implement throttling (max 4-5 concurrent requests)
- Add delay between batches
- Respect any 429 (Too Many Requests) responses

### HTML Structure Changes

IMDB's HTML structure can change without notice. The scraper relies on:

- `__NEXT_DATA__` script tag containing JSON
- Specific JSON structure within that data

**Mitigation:** The current approach of parsing embedded JSON is more stable than parsing HTML directly.

### Thread Safety

When implementing parallelization, ensure:

- No shared mutable state between requests
- Proper synchronization for shared collections
- HttpClient instance reuse (create once, use many times)

---

## Metrics to Track

Before implementing changes, establish baseline metrics:

| Metric | Current (Estimate) | Target |
|--------|-------------------|--------|
| Single movie scrape | 1-2 seconds | < 1 second |
| Movie search | 3-9 seconds | < 2 seconds |
| TV show (no episodes) | 2-3 seconds | < 1.5 seconds |
| TV show (full season) | 30-60 seconds | < 15 seconds |

---

## Next Steps

1. [ ] Gather baseline performance metrics
2. [ ] Review and adjust default search settings
3. [ ] Implement parallel search requests
4. [ ] Test with rate limiting considerations
5. [ ] Implement episode parallelization
6. [ ] Consider adding response caching

---

## Appendix: Key Code Locations

| Function | Line | Purpose |
|----------|------|---------|
| GetMovieInfo | 191-435 | Main movie scraping |
| GetTVShowInfo | 617-837 | Main TV show scraping |
| GetTVEpisodeInfo | 437-558 | Episode scraping |
| GetTVSeasonInfo | 591-615 | Season iteration |
| SearchMovie | 1301-1469 | Movie search (multiple requests) |
| SearchTVShow | 1483-1513 | TV search |
| ParseActors | 981-1031 | Actor extraction |
| ParseCertifications | 1033-1057 | Certification extraction |

---

*Document Version: 1.0*
*Created: December 31, 2025*
*Author: ulrick65*
*Status: 📋 Analysis*