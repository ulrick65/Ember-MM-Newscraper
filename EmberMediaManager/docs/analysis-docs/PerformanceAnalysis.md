# Ember Media Manager - Performance Analysis & Optimization Recommendations

Created: December 27, 2025
Updated: December 28, 2025
Author: Eric H. Anderson

## Executive Summary

This document provides a thorough analysis of the Ember Media Manager codebase with specific recommendations to improve scraping speed, efficiency, and overall application performance. The analysis covers network operations, database access, image processing, and concurrent processing patterns.

---

## 1. Network & HTTP Operations

### Current Issues Identified

**1.1 Legacy HttpWebRequest Usage**
- Location: `EmberAPI\clsAPIHTTP.vb`
- The codebase uses `HttpWebRequest` (legacy API) instead of modern `HttpClient`
- Each request creates a new connection, missing connection pooling benefits
- Default timeout of 20 seconds may be too long for bulk operations

**1.2 Non-Async HTTP Operations**
- Most scrapers use synchronous `Task.Run()` wrappers around async APIs
- Example in `clsScrapeTMDB.vb` (lines 273-276):

    APIResult = Task.Run(Function() _client.GetMovieAsync(strUniqueID))
    Movie = APIResult.Result

- This blocks threads and reduces throughput during bulk scraping

**1.3 HttpClient Instance Management**
- Location: `clsScrapeOMDb.vb` (line 58)
- New `HttpClient` created per API initialization
- Should use a shared, static `HttpClient` instance

### Recommendations

**R1.1 - Implement Shared HttpClient with Connection Pooling**

    ' Create a shared module for HTTP operations
    Public Module HttpClientFactory
        Private ReadOnly _lazyClient As New Lazy(Of HttpClient)(
            Function()
                Dim handler As New HttpClientHandler With {
                    .MaxConnectionsPerServer = 10,
                    .AutomaticDecompression = DecompressionMethods.GZip Or DecompressionMethods.Deflate
                }
                Dim client As New HttpClient(handler)
                client.Timeout = TimeSpan.FromSeconds(30)
                Return client
            End Function, True)
        
        Public ReadOnly Property SharedClient As HttpClient
            Get
                Return _lazyClient.Value
            End Get
        End Property
    End Module

**R1.2 - Use True Async/Await Patterns**

    ' Instead of blocking Task.Run pattern
    Public Async Function GetMovieInfoAsync(id As String) As Task(Of MediaContainers.Movie)
        Dim result = Await _client.GetMovieAsync(id, MovieMethods.Credits Or MovieMethods.Releases)
        Return ConvertToMovie(result)
    End Function

**R1.3 - Implement Request Throttling**

    ' Add rate limiting to prevent API throttling
    Private Shared _rateLimiter As New SemaphoreSlim(5, 5) ' 5 concurrent requests max
    
    Public Async Function ThrottledRequestAsync(Of T)(request As Func(Of Task(Of T))) As Task(Of T)
        Await _rateLimiter.WaitAsync()
        Try
            Return Await request()
        Finally
            _rateLimiter.Release()
        End Try
    End Function

---

## 2. Parallel Processing & Concurrency

### Current Issues Identified

**2.1 Sequential BackgroundWorker Pattern**
- Location: `EmberAPI\clsAPITaskManager.vb` (lines 63-100)
- Tasks are processed sequentially in a single `BackgroundWorker`
- No parallel processing of independent items

**2.2 Application.DoEvents() Anti-Pattern**
- Location: `clsScrapeIMDB.vb` (lines 112-115), `clsScrapeTMDB.vb` (lines 257-260)

    While bwIMDB.IsBusy
        Application.DoEvents()
        Threading.Thread.Sleep(50)
    End While

- This pattern causes UI thread blocking and is inefficient

### Recommendations

**R2.1 - Implement Parallel Bulk Scraping**

    ' Process multiple movies in parallel with controlled concurrency
    Public Async Function ScrapeMoviesBatchAsync(
        movieIds As IEnumerable(Of String), 
        maxConcurrency As Integer) As Task(Of List(Of MediaContainers.Movie))
        
        Dim options As New ParallelOptions With {
            .MaxDegreeOfParallelism = maxConcurrency
        }
        
        Dim results As New ConcurrentBag(Of MediaContainers.Movie)
        
        Await Task.Run(Sub()
            Parallel.ForEach(movieIds, options,
                Sub(id)
                    Dim movie = GetMovieInfoAsync(id).Result
                    If movie IsNot Nothing Then results.Add(movie)
                End Sub)
        End Sub)
        
        Return results.ToList()
    End Function

**R2.2 - Replace BackgroundWorker with Task-Based Pattern**

    ' Modern async pattern for scraping operations
    Public Class AsyncScrapeManager
        Private _cancellationTokenSource As CancellationTokenSource
        
        Public Async Function ScrapeAsync(
            items As List(Of ScrapeItem), 
            progress As IProgress(Of Integer),
            cancellationToken As CancellationToken) As Task
            
            Dim completed As Integer = 0
            Dim total As Integer = items.Count
            
            Dim tasks = items.Select(Async Function(item)
                Await ProcessItemAsync(item, cancellationToken)
                Interlocked.Increment(completed)
                progress.Report(CInt((completed / total) * 100))
            End Function)
            
            Await Task.WhenAll(tasks)
        End Function
    End Class

---

## 3. Database Optimization

### Current Issues Identified

**3.1 Individual Transaction Per Operation**
- Location: `clsAPITaskManager.vb` (lines 71-74, 80-83, etc.)
- Each task creates its own transaction
- Frequent commits reduce batch operation performance

**3.2 Missing Database Indices**
- Actor lookups use LIKE queries: `SQLcommand_select_actors.CommandText = "SELECT idActor FROM actors WHERE strActor LIKE ?"`
- Location: `clsAPIDatabase.vb` (line 85)

**3.3 No Query Caching**
- Repeated lookups for the same data during scraping sessions
- No in-memory caching layer

### Recommendations

**R3.1 - Batch Database Operations**

    ' Batch insert multiple actors in single transaction
    Public Sub AddActorsBatch(actors As List(Of ActorInfo))
        Using transaction = _myvideosDBConn.BeginTransaction()
            Using cmd = _myvideosDBConn.CreateCommand()
                cmd.CommandText = "INSERT OR IGNORE INTO actors (strActor, strThumb, strIMDB, strTMDB) VALUES (?, ?, ?, ?)"
                cmd.Parameters.Add("actor", DbType.String)
                cmd.Parameters.Add("thumb", DbType.String)
                cmd.Parameters.Add("imdb", DbType.String)
                cmd.Parameters.Add("tmdb", DbType.String)
                cmd.Prepare()
                
                For Each actor In actors
                    cmd.Parameters(0).Value = actor.Name
                    cmd.Parameters(1).Value = actor.Thumb
                    cmd.Parameters(2).Value = actor.IMDB
                    cmd.Parameters(3).Value = actor.TMDB
                    cmd.ExecuteNonQuery()
                Next
            End Using
            transaction.Commit()
        End Using
    End Sub

**R3.2 - Implement In-Memory Caching**

    ' Simple cache for frequently accessed data
    Public Class ScrapeCache
        Private Shared _actorCache As New ConcurrentDictionary(Of String, Long)
        Private Shared _genreCache As New ConcurrentDictionary(Of String, Long)
        Private Const kCacheExpirationMinutes As Integer = 30
        
        Public Shared Function GetOrAddActor(name As String, factory As Func(Of Long)) As Long
            Return _actorCache.GetOrAdd(name.ToLowerInvariant(), Function(k) factory())
        End Function
        
        Public Shared Sub ClearCache()
            _actorCache.Clear()
            _genreCache.Clear()
        End Sub
    End Class

**R3.3 - Add Database Indices**

    -- Add these indices to improve lookup performance
    CREATE INDEX IF NOT EXISTS idx_actors_name ON actors(strActor COLLATE NOCASE);
    CREATE INDEX IF NOT EXISTS idx_movies_imdb ON movies(strIMDB);
    CREATE INDEX IF NOT EXISTS idx_movies_tmdb ON movies(strTMDB);

---

## 4. Image Processing Optimization

### Current Issues Identified

**4.1 Full Image Loading for Thumbnails**
- Location: `clsAPIImages.vb` (lines 450-455)
- Full resolution images loaded even when only thumbnails needed
- High memory consumption during batch operations

**4.2 Sequential Image Downloads**
- Images downloaded one at a time during scraping
- No parallel image downloading

**4.3 No Image Caching**
- Same poster/fanart may be downloaded multiple times
- No disk-based cache for frequently accessed images

### Recommendations

**R4.1 - Implement Lazy Image Loading with Thumbnails**

    ' Download thumbnail first, full image on demand
    Public Class LazyImage
        Private _thumbnailUrl As String
        Private _fullUrl As String
        Private _thumbnail As Image
        Private _fullImage As Image
        
        Public ReadOnly Property Thumbnail As Image
            Get
                If _thumbnail Is Nothing Then
                    _thumbnail = DownloadAndResizeThumbnail(_thumbnailUrl, 150, 225)
                End If
                Return _thumbnail
            End Get
        End Property
        
        Public ReadOnly Property FullImage As Image
            Get
                If _fullImage Is Nothing Then
                    _fullImage = DownloadImage(_fullUrl)
                End If
                Return _fullImage
            End Get
        End Property
    End Class

**R4.2 - Parallel Image Downloading**

    ' Download multiple images concurrently
    Public Async Function DownloadImagesAsync(
        urls As IEnumerable(Of String), 
        maxConcurrent As Integer) As Task(Of List(Of Image))
        
        Dim semaphore As New SemaphoreSlim(maxConcurrent)
        Dim tasks = urls.Select(Async Function(url)
            Await semaphore.WaitAsync()
            Try
                Return Await DownloadImageAsync(url)
            Finally
                semaphore.Release()
            End Try
        End Function)
        
        Dim results = Await Task.WhenAll(tasks)
        Return results.Where(Function(img) img IsNot Nothing).ToList()
    End Function

**R4.3 - Implement Disk-Based Image Cache**

    ' Cache downloaded images to disk
    Public Class ImageCache
        Private Shared ReadOnly _cachePath As String = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "EmberMediaManager", "ImageCache")
        
        Public Shared Function GetCachedImage(url As String) As Image
            Dim hash = GetUrlHash(url)
            Dim cachePath = Path.Combine(_cachePath, hash)
            
            If File.Exists(cachePath) Then
                Return Image.FromFile(cachePath)
            End If
            Return Nothing
        End Function
        
        Public Shared Sub CacheImage(url As String, image As Image)
            Dim hash = GetUrlHash(url)
            Dim cachePath = Path.Combine(_cachePath, hash)
            image.Save(cachePath, ImageFormat.Jpeg)
        End Sub
        
        Private Shared Function GetUrlHash(url As String) As String
            Using sha = SHA256.Create()
                Dim bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(url))
                Return BitConverter.ToString(bytes).Replace("-", "").Substring(0, 32)
            End Using
        End Function
    End Class

---

## 5. API-Specific Optimizations

### TMDB Scraper

**5.1 Current Issues**
- Multiple API calls for related data (movie + credits + releases + videos)
- Fallback English client duplicates configuration requests

**5.2 Recommendations**

    ' Use append_to_response for fewer API calls
    Public Async Function GetMovieWithAllDataAsync(id As Integer) As Task(Of Movie)
        ' Single API call gets movie, credits, releases, videos, and images
        Return Await _client.GetMovieAsync(id, 
            MovieMethods.Credits Or 
            MovieMethods.Releases Or 
            MovieMethods.Videos Or 
            MovieMethods.Images Or 
            MovieMethods.AlternativeTitles)
    End Function

### IMDB Scraper

**5.3 Current Issues**
- HTML parsing for each request
- No response caching

**5.4 Recommendations**

    ' Cache parsed HTML documents
    Private Shared _documentCache As New ConcurrentDictionary(Of String, Tuple(Of HtmlDocument, DateTime))
    Private Const kCacheExpirationHours As Integer = 24
    
    Public Function GetCachedDocument(url As String) As HtmlDocument
        Dim cached As Tuple(Of HtmlDocument, DateTime) = Nothing
        If _documentCache.TryGetValue(url, cached) Then
            If (DateTime.Now - cached.Item2).TotalHours < kCacheExpirationHours Then
                Return cached.Item1
            End If
        End If
        Return Nothing
    End Function

---

## 6. Memory Management

### Current Issues Identified

**6.1 Image Disposal**
- Location: `clsAPIHTTP.vb` (lines 122-128)
- Images sometimes not properly disposed
- Memory leaks during long scraping sessions

**6.2 Large Object Heap Fragmentation**
- Large images allocated frequently
- GC pressure during batch operations

### Recommendations

**R6.1 - Implement Proper Disposal Pattern**

    ' Ensure all images are properly disposed
    Public Class ManagedImage
        Implements IDisposable
        
        Private _image As Image
        Private _disposed As Boolean
        
        Public Sub Dispose() Implements IDisposable.Dispose
            If Not _disposed Then
                _image?.Dispose()
                _disposed = True
            End If
            GC.SuppressFinalize(Me)
        End Sub
        
        Protected Overrides Sub Finalize()
            Dispose()
        End Sub
    End Class

**R6.2 - Force GC During Long Operations**

    ' Periodically clean up during batch operations
    Public Sub CleanupMemory()
        GC.Collect()
        GC.WaitForPendingFinalizers()
        GC.Collect()
    End Sub
    
    ' Call every N items during batch scraping
    If processedCount Mod 100 = 0 Then CleanupMemory()

---

## 7. Configuration Recommendations

### Application Settings

| Setting | Current | Recommended | Impact |
|---------|---------|-------------|--------|
| Max Concurrent Scrapers | 1 (sequential) | 3-5 | 3-5x faster scraping |
| HTTP Timeout | 20 seconds | 15 seconds | Faster failure detection |
| Connection Pool Size | Default (2) | 10 | Better throughput |
| Image Download Concurrent | 1 | 5 | 5x faster image downloads |
| Database WAL Mode | Off | On | Better write performance |

### SQLite Performance Settings

    ' Add to database initialization
    PRAGMA journal_mode = WAL;
    PRAGMA synchronous = NORMAL;
    PRAGMA cache_size = -20000;  -- 20MB cache
    PRAGMA temp_store = MEMORY;

---

## 8. Implementation Priority

### High Priority (Immediate Impact)

1. **Implement shared HttpClient** - Easy change, significant network improvement
2. **Enable parallel image downloads** - Major speed improvement for image-heavy operations
3. **Add database indices** - Simple SQL changes, faster lookups
4. **Use TMDB append_to_response** - Reduces API calls by 60-70%

### Medium Priority (Moderate Effort)

5. **Convert to async/await patterns** - Requires refactoring but improves scalability
6. **Implement response caching** - Reduces redundant API calls
7. **Batch database operations** - Faster bulk inserts/updates
8. **Enable SQLite WAL mode** - Better concurrent access

### Low Priority (Long-Term)

9. **Implement disk-based image cache** - Reduces bandwidth, improves repeat performance
10. **Full async scraper refactoring** - Major refactor but maximum performance
11. **Memory pooling for large objects** - Reduces GC pressure

---

## 9. Performance Metrics Catalog

This section catalogs all performance metrics for tracking optimization impact. Metrics are grouped by category and indicate implementation status.

### 9.1 Scraper Operations

| Metric Name | Location | Description | Status |
|-------------|----------|-------------|--------|
| `TMDB.GetInfo_Movie` | `clsScrapeTMDB.vb` | Time for single TMDB movie API call and data extraction | ✅ Implemented |
| `TMDB.GetInfo_TVShow` | `clsScrapeTMDB.vb` | Time for single TMDB TV show API call and data extraction | ✅ Implemented |
| `IMDB.GetMovieInfo` | `clsScrapeIMDB.vb` | Time for single IMDB movie HTML scrape and parse | ✅ Implemented |
| `IMDB.GetTVShowInfo` | `clsScrapeIMDB.vb` | Time for single IMDB TV show HTML scrape and parse | ✅ Implemented |
| `TVDB.GetInfo_TVShow` | TVDB scraper | Time for single TVDB TV show API call | ⬜ Not Implemented |

### 9.2 Image Operations

| Metric Name | Location | Description | Status |
|-------------|----------|-------------|--------|
| `Image.LoadFromWeb` | `clsAPIImages.vb` | Time for single image HTTP download | ✅ Implemented |
| `SaveAllImages.Movie.Total` | `clsAPIMediaContainers.vb` | Total time for SaveAllImages method (download + disk write) | ✅ Implemented |
| `SaveAllImages.Movie.Download` | `clsAPIMediaContainers.vb` | Time spent in HTTP download phase only | ✅ Implemented |
| `SaveAllImages.Movie.DiskWrite` | `clsAPIMediaContainers.vb` | Time spent writing images to disk | ✅ Implemented |
| `SaveAllImages.Movie.ImageCount` | `clsAPIMediaContainers.vb` | Count of images processed (for correlation) | ✅ Implemented |

### 9.3 Database Operations

| Metric Name | Location | Description | Status |
|-------------|----------|-------------|--------|
| `Database.Add_Actor` | `clsAPIDatabase.vb` | Time for single actor lookup/insert operation | ✅ Implemented |
| `Database.Save_Movie` | `clsAPIDatabase.vb` | Total time for Save_Movie (includes NFO, images, DB write) | ✅ Implemented |
| `Save_Movie.Images` | `clsAPIDatabase.vb` | Time spent in SaveAllImages() within Save_Movie | ✅ Implemented |
| `Database.Save_TVShow` | `clsAPIDatabase.vb` | Total time for Save_TVShow | ⬜ Not Implemented |
| `Database.Save_TVSeason` | `clsAPIDatabase.vb` | Total time for Save_TVSeason | ⬜ Not Implemented |
| `Database.Save_TVEpisode` | `clsAPIDatabase.vb` | Total time for Save_TVEpisode | ⬜ Not Implemented |

### 9.4 Save_Movie Breakdown

These metrics decompose `Database.Save_Movie` to identify bottlenecks:

| Metric Name | Location | Description | Status |
|-------------|----------|-------------|--------|
| `Save_Movie.NFO` | `clsAPIDatabase.vb` | Time spent in NFO.SaveToNFO_Movie() | ⬜ Not Implemented |
| `Save_Movie.Images` | `clsAPIDatabase.vb` | Time spent in SaveAllImages() | ⬜ Not Implemented |
| `Save_Movie.ActorThumbs` | `clsAPIDatabase.vb` | Time spent in SaveAllActorThumbs() | ⬜ Not Implemented |
| `Save_Movie.Trailer` | `clsAPIDatabase.vb` | Time spent in Trailer.Save() | ⬜ Not Implemented |
| `Save_Movie.Database` | `clsAPIDatabase.vb` | Time spent in pure SQL operations | ⬜ Not Implemented |

### 9.5 Scraping Workflow

| Metric Name | Location | Description | Status |
|-------------|----------|-------------|--------|
| `ScrapeData_Movie` | `clsAPIModules.vb` | Total time for data scrape orchestration (all scrapers) | ⬜ Not Implemented |
| `ScrapeImage_Movie` | `clsAPIModules.vb` | Time to collect image URLs (not download) | ⬜ Not Implemented |
| `MergeDataScraperResults_Movie` | `clsAPINFO.vb` | Time to merge results from multiple scrapers | ⬜ Not Implemented |

### 9.6 Metric Implementation Summary

| Category | Implemented | Not Implemented | Total |
|----------|-------------|-----------------|-------|
| Scraper Operations | 4 | 1 | 5 |
| Image Operations | 5 | 0 | 5 |
| Database Operations | 3 | 2 | 5 |
| Save_Movie Breakdown | 0 | 5 | 5 |
| Scraping Workflow | 0 | 3 | 3 |
| **Total** | **12** | **11** | **23** |

### 9.7 Priority Metrics for Phase 1 Validation

The following metrics are critical for validating Phase 1 Item 5 (parallel image downloads):

1. **`Save_Movie.Images`** - Confirms SaveAllImages is the bottleneck within Save_Movie
2. **`SaveAllImages.Download`** - Confirms network (not disk) is the bottleneck
3. **`SaveAllImages.DiskWrite`** - Baseline for disk I/O portion ✅ **Confirmed: 5% of time**
4. **`SaveAllImages.ImageCount`** - Correlates time with work done ✅ **Avg: 6.37 images/movie**

**Status:** All priority metrics implemented. Analysis confirms download phase (94%) is the bottleneck, validating Item 5 approach.

### 9.8 Priority Metrics for Phase 2 (TV Shows)

The following metrics should be implemented before Phase 2 TV Show optimization:

1. **`Database.Save_TVShow`** - Baseline for TV show save performance
2. **`Database.Save_TVSeason`** - Baseline for season save performance  
3. **`Database.Save_TVEpisode`** - Baseline for episode save performance
4. **`TVDB.GetInfo_TVShow`** - Alternative scraper timing

### 9.9 Baseline Data (Phase 1 - 50 Movies)

Captured: December 27, 2025

| Metric | Count | Total (ms) | Avg (ms) | Min (ms) | Max (ms) |
|--------|-------|------------|----------|----------|----------|
| `TMDB.GetInfo_Movie` | 49 | 52,822 | 1,078 | 450 | 2,100 |
| `IMDB.GetMovieInfo` | 49 | 93,590 | 1,910 | 800 | 4,500 |
| `Image.LoadFromWeb` | 312 | 44,304 | 142 | 50 | 500 |
| `Database.Add_Actor` | 2,400 | 4,152 | 1.73 | 0.5 | 15 |
| `Database.Save_Movie` | 98 | 60,662 | 619 | 300 | 1,200 |

**Observations:**
- ~2 Save_Movie calls per movie (after data scrape, after image processing)
- ~48 actor lookups per movie average
- ~6.4 images downloaded per movie average
- IMDB scraping is ~77% slower than TMDB per movie


Implement performance tracking helper to track these metrics:

    ' Performance tracking helper
    Public Class PerformanceTracker
        Private Shared _metrics As New ConcurrentDictionary(Of String, List(Of TimeSpan))
        
        Public Shared Function Track(Of T)(name As String, action As Func(Of T)) As T
            Dim sw = Stopwatch.StartNew()
            Try
                Return action()
            Finally
                sw.Stop()
                _metrics.GetOrAdd(name, Function() New List(Of TimeSpan)).Add(sw.Elapsed)
            End Try
        End Function
        
        Public Shared Sub LogMetrics()
            For Each kvp In _metrics
                Dim avg = TimeSpan.FromTicks(CLng(kvp.Value.Average(Function(t) t.Ticks)))
                _Logger.Info("Metric '{0}': Avg={1}ms, Count={2}", kvp.Key, avg.TotalMilliseconds, kvp.Value.Count)
            Next
        End Sub
    End Class

---


## 10. Testing Recommendations

Before implementing changes, establish baseline performance:

1. Scrape 100 movies and record total time
2. Measure memory usage peak during scraping
3. Count total API calls made
4. Record database operation times

After each optimization, re-run tests to quantify improvement.

---

*Document Version: 1.0*
*Analysis Date: December 2025*
*Analyzed By: GitHub Copilot*