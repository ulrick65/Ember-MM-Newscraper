# Movie Scraping Process Analysis

> **Related Documentation:** This document covers Movie scraping. For TV Show scraping, see [ScrapingProcessTvShows.md](ScrapingProcessTvShows.md). Both processes share the same architectural patterns through the `ModulesManager` class.

---

## Overview

This document provides a comprehensive analysis of how Ember Media Manager scrapes movie metadata from external data sources. The scraping system uses an addon-based modular architecture where multiple scrapers can be enabled and configured to run in sequence. Each scraper contributes data that is then merged according to user-configured rules.

> **Note:** This documentation focuses primarily on Movie scraping. TV Show scraping follows similar patterns but uses different interfaces and methods. Overlaps and shared components are highlighted where applicable.

---

## Part 1: Scraper Architecture

### 1.1 Core Files Involved

| File | Purpose |
|------|---------|
| `EmberAPI\clsAPIModules.vb` | Module manager that loads, orders, and executes scrapers |
| `EmberAPI\clsAPINFO.vb` | Merges results from multiple scrapers |
| `EmberAPI\clsAPIInterfaces.vb` | Defines scraper interfaces and result structures |
| `EmberAPI\clsAPICommon.vb` | Contains ScrapeModifiers, ScrapeOptions, and ScrapeType enums |
| `EmberAPI\clsAPIDatabase.vb` | Database operations including `Save_Movie()` and `Save_MovieAsync()` |
| `EmberAPI\clsAPIImages.vb` | Image container and save operations |
| `Addons\scraper.TMDB.Data\*` | TMDB data scraper implementation |
| `Addons\scraper.IMDB.Data\*` | IMDB data scraper implementation |

### 1.2 Scraper Types

Ember supports four types of movie scrapers, each implementing a specific interface:

| Scraper Type | Interface | Purpose |
|--------------|-----------|---------|
| Data Scraper | `ScraperModule_Data_Movie` | Retrieves metadata (title, plot, cast, etc.) |
| Image Scraper | `ScraperModule_Image_Movie` | Retrieves artwork (posters, fanart, etc.) |
| Trailer Scraper | `ScraperModule_Trailer_Movie` | Retrieves trailer URLs |
| Theme Scraper | `ScraperModule_Theme_Movie` | Retrieves theme music |

**TV Show Overlap:** TV scrapers use parallel interfaces: `ScraperModule_Data_TV`, `ScraperModule_Image_TV`, etc. The module loading and execution patterns are identical.

### 1.3 Module Loading

Scrapers are loaded dynamically at application startup via `ModulesManager.LoadModules()`:

1. Scans the Addons directory for assemblies
2. Uses reflection to find types implementing scraper interfaces
3. Creates instances and stores in typed collections:
   - `externalScrapersModules_Data_Movie`
   - `externalScrapersModules_Image_Movie`
   - `externalScrapersModules_Trailer_Movie`
   - `externalScrapersModules_Theme_Movie`
4. Loads enabled state and order from settings
5. Assigns default order (999) for newly discovered scrapers

### 1.4 Generic Module Event System

Generic modules (like the BulkRenamer) can subscribe to scraping events via `ModuleEventType`:

| Event | When Fired |
|-------|------------|
| `ScraperMulti_Movie` | During bulk/auto scraping operations |
| `ScraperSingle_Movie` | During single movie scrape |
| `AfterEdit_Movie` | After movie data is edited |
| `BeforeEdit_Movie` | Before movie data is edited |
| `Sync_Movie` | When movie sync is requested |

Generic modules implement `RunGeneric()` to handle these events:

    Public Function RunGeneric(ByVal mType As Enums.ModuleEventType, ...) As Interfaces.ModuleResult
        Select Case mType
            Case Enums.ModuleEventType.ScraperMulti_Movie
                ' Handle bulk scrape event
            Case Enums.ModuleEventType.AfterEdit_Movie
                ' Handle after edit event
        End Select
    End Function

---

## Part 2: Data Scraping Process

### 2.1 Entry Point

Data scraping is initiated by calling `ModulesManager.Instance.ScrapeData_Movie()`.

**Method Signature:**

    Function ScrapeData_Movie(
        ByRef DBElement As Database.DBElement,
        ByRef ScrapeModifiers As Structures.ScrapeModifiers,
        ByVal ScrapeType As Enums.ScrapeType,
        ByVal ScrapeOptions As Structures.ScrapeOptions,
        ByVal showMessage As Boolean
    ) As Boolean

### 2.2 ScrapeData_Movie Flow

**Step 1: Validate Online Status**

    If DBElement.IsOnline OrElse FileUtils.Common.CheckOnlineStatus_Movie(DBElement, showMessage) Then
        ' Continue with scraping
    Else
        Return True ' Cancelled - movie offline
    End If

**Step 2: Get Enabled Scrapers in Order**

    Dim modules As IEnumerable(Of _externalScraperModuleClass_Data_Movie) = 
        externalScrapersModules_Data_Movie.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)

**Step 3: Clean Movie Data (for New Scrapes)**

If `ScrapeType` is `SingleScrape` or `SingleAuto` AND `ScrapeModifiers.DoSearch` is True, the movie data is reset:

    DBElement.ImagesContainer = New MediaContainers.ImagesContainer
    DBElement.Movie = New MediaContainers.Movie With {
        .Edition = DBElement.Edition,
        .Title = StringUtils.FilterTitleFromPath_Movie(...),
        .VideoSource = DBElement.VideoSource,
        .Year = StringUtils.FilterYearFromPath_Movie(...)
    }

**Step 4: Clone for Scraping**

    Dim oDBMovie As Database.DBElement = CType(DBElement.CloneDeep, Database.DBElement)

**Step 5: Execute Each Scraper in Order**

    For Each _externalScraperModule As _externalScraperModuleClass_Data_Movie In modules
        ret = _externalScraperModule.ProcessorModule.Scraper_Movie(oDBMovie, ScrapeModifiers, ScrapeType, ScrapeOptions)
        
        If ret.Cancelled Then Return ret.Cancelled
        
        If ret.Result IsNot Nothing Then
            ScrapedList.Add(ret.Result)
            
            ' Pass IDs to subsequent scrapers
            If ret.Result.UniqueIDsSpecified Then
                oDBMovie.Movie.UniqueIDs.AddRange(ret.Result.UniqueIDs)
            End If
            If ret.Result.OriginalTitleSpecified Then
                oDBMovie.Movie.OriginalTitle = ret.Result.OriginalTitle
            End If
            If ret.Result.TitleSpecified Then
                oDBMovie.Movie.Title = ret.Result.Title
            End If
            If ret.Result.YearSpecified Then
                oDBMovie.Movie.Year = ret.Result.Year
            End If
        End If
        
        If ret.breakChain Then Exit For
    Next

**Key Behavior:** 
- ALL enabled scrapers run in order (unless `breakChain` is True)
- IDs discovered by earlier scrapers are passed to subsequent scrapers
- Each scraper's result is added to `ScrapedList` for later merging

**Step 6: Optionally Scrape Trailers from Trailer Scrapers**

    If ScrapeOptions.bMainTrailer AndAlso Master.eSettings.MovieScraperTrailerFromTrailerScrapers Then
        ' Call ScrapeTrailer_Movie and add preferred trailer to ScrapedList
    End If

**Step 7: Merge All Results**

    DBElement = NFO.MergeDataScraperResults_Movie(DBElement, ScrapedList, ScrapeType, ScrapeOptions)

### 2.3 Result Merging Logic

The `MergeDataScraperResults_Movie` method in `clsAPINFO.vb` implements a "first wins" strategy with lock protection:

**Merging Pattern:**

    ' Protects first scraped result against overwriting
    Dim new_Title As Boolean = False
    Dim new_Plot As Boolean = False
    ' ... (flags for each field)
    
    For Each scrapedmovie In ScrapedList
        ' Title
        If (Not DBMovie.Movie.TitleSpecified OrElse Not Master.eSettings.MovieLockTitle) AndAlso 
           ScrapeOptions.bMainTitle AndAlso
           scrapedmovie.TitleSpecified AndAlso 
           Master.eSettings.MovieScraperTitle AndAlso 
           Not new_Title Then
            DBMovie.Movie.Title = scrapedmovie.Title
            new_Title = True
        ElseIf Master.eSettings.MovieScraperCleanFields AndAlso Not Master.eSettings.MovieScraperTitle AndAlso Not Master.eSettings.MovieLockTitle Then
            DBMovie.Movie.Title = String.Empty
        End If
        
        ' ... similar logic for all other fields
    Next

**Merging Conditions:**

For each field, a value is accepted if ALL of these are true:
1. Field is not already set in DBMovie OR field is not locked
2. Field is requested in `ScrapeOptions`
3. Scraped movie has a value for this field
4. Field scraping is enabled in settings
5. Flag indicates first scraper hasn't already provided this field (`Not new_*`)

**Special Processing During Merge:**

| Field | Special Processing |
|-------|-------------------|
| Actors | Filter by image availability, apply count limit, reorder |
| Certifications | Filter by language, run certification mapping |
| Countries | Apply count limit, run country mapping |
| Genres | Run genre mapping, apply count limit |
| Studios | Run studio mapping, filter by image availability, apply count limit |
| Ratings | Remove "default" type entries, add new ratings |
| Trailer | Convert YouTube URLs to Kodi format if enabled |

**Post-Merge Processing:**

1. **Certification for MPAA:** Convert certification to MPAA format if enabled
2. **Default MPAA:** Apply "Not Rated" if no MPAA and setting configured
3. **OriginalTitle as Title:** Copy OriginalTitle to Title if enabled
4. **Plot for Outline:** Generate Outline from Plot if enabled
5. **Sort Ratings:** Sort by Default flag and Type name
6. **Sort UniqueIDs:** Sort by Default flag and Type name

---

## Part 3: Individual Scraper Behavior

### 3.1 TMDB Scraper (`scraper.TMDB.Data`)

**Location:** `Addons\scraper.TMDB.Data\TMDB_Data.vb` and `Scraper\clsScrapeTMDB.vb`

**ID Resolution Order:**
1. If TMDB ID available, scrape directly
2. If IMDB ID available, use it to find movie on TMDB
3. If neither, search by Title/Year

**Method:** `Scraper_Movie` in TMDB_Data.vb calls `_TMDBAPI_Movie.GetInfo_Movie()`

**Data Retrieved:**
- Title, OriginalTitle, Year, Premiered
- Plot, Tagline, Runtime
- Genres, Studios, Countries
- Cast (Actors with images)
- Directors, Writers
- Certifications, MPAA
- Ratings (TMDB rating)
- UniqueIDs (TMDB, IMDB)
- Collection information
- Trailer URLs

### 3.2 IMDB Scraper (`scraper.IMDB.Data`)

**Location:** `Addons\scraper.IMDB.Data\IMDB_Data.vb` and `Scraper\clsScrapeIMDB.vb`

**ID Resolution Order:**
1. If IMDB ID available, scrape directly
2. If not, search by Title/Year

**Method:** `Scraper_Movie` calls `_scraper.GetMovieInfo()`

**Data Retrieved:**
- Title, OriginalTitle, Year
- Plot, Outline, Tagline, Runtime
- Genres, Studios, Countries
- Cast (Actors)
- Directors, Writers
- Certifications, MPAA
- Ratings (IMDB, Metacritic)
- Top250 ranking
- UniqueIDs (IMDB)

### 3.3 Scraper Chain Behavior

**CRITICAL:** When both TMDB and IMDB scrapers are enabled:

1. TMDB runs first (typically order 0)
2. TMDB retrieves IMDB ID along with other data
3. IMDB ID is passed to IMDB scraper via `oDBMovie.Movie.UniqueIDs`
4. IMDB scraper runs and retrieves additional/supplementary data
5. Results are merged with "first wins" logic

**Practical Example (50 Movies):**
- TMDB runs 49 times (1,078ms average)
- IMDB runs 49 times (1,910ms average)
- Both contribute to merged results
- Database.Save_Movie runs ~98 times (~2 per movie)
  - First save: After data scraping/merging completes
  - Second save: After image processing completes

### 3.4 ModuleResult Structure

    Public Structure ModuleResult_Data_Movie
        Public breakChain As Boolean    ' If True, stop processing subsequent scrapers
        Public Cancelled As Boolean     ' If True, user cancelled or error occurred
        Public Result As MediaContainers.Movie  ' Scraped data
    End Structure

**Note:** Neither TMDB nor IMDB scrapers set `breakChain = True`, so both always run when enabled.

---

## Part 4: Image Scraping Process

### 4.1 Entry Point

Image scraping is initiated by calling `ModulesManager.Instance.ScrapeImage_Movie()`.

**Method Signature:**

    Function ScrapeImage_Movie(
        ByRef DBElement As Database.DBElement,
        ByRef ImagesContainer As MediaContainers.SearchResultsContainer,
        ByVal ScrapeModifiers As Structures.ScrapeModifiers,
        ByVal showMessage As Boolean
    ) As Boolean

### 4.2 ScrapeImage_Movie Flow

1. Validate online status
2. Get enabled image scrapers in order
3. For each scraper:
   - Check if scraper has capability for requested image types
   - Call `_externalScraperModule.ProcessorModule.Scraper()`
   - Aggregate results into single container
4. Sort and filter results
5. Create cache paths

**Image Types Collected:**
- MainBanners
- MainCharacterArts
- MainClearArts
- MainClearLogos
- MainDiscArts
- MainFanarts
- MainLandscapes
- MainPosters

### 4.3 Image Scrapers

| Scraper | Source | Image Types |
|---------|--------|-------------|
| TMDB Image | TMDB API | Posters, Fanarts, Banners |
| FanartTV | fanart.tv API | All artwork types |

### 4.4 Image Download and Save Process

**Critical Performance Note:** Image downloading occurs during `Save_Movie()`, not during `ScrapeImage_Movie()`.

The scraping phase collects image URLs and metadata into `ImagesContainer`. The actual download happens when:

    ' In Save_Movie() - clsAPIDatabase.vb
    If toDisk Then
        dbElement.ImagesContainer.SaveAllImages(dbElement, forceFileCleanup)  ' Downloads images here
        dbElement.Movie.SaveAllActorThumbs(dbElement)
        dbElement.Theme.Save(dbElement, Enums.ModifierType.MainTheme, forceFileCleanup)
        dbElement.Trailer.Save(dbElement, Enums.ModifierType.MainTrailer, forceFileCleanup)
    End If

**SaveAllImages Flow (clsAPIImages.vb):**

    Public Sub SaveAllImages(ByRef dbElement As Database.DBElement, ByVal forceFileCleanup As Boolean)
        ' Saves each image type sequentially
        Banner.Save(dbElement, Enums.ModifierType.MainBanner, forceFileCleanup)
        ClearArt.Save(dbElement, Enums.ModifierType.MainClearArt, forceFileCleanup)
        ClearLogo.Save(dbElement, Enums.ModifierType.MainClearLogo, forceFileCleanup)
        DiscArt.Save(dbElement, Enums.ModifierType.MainDiscArt, forceFileCleanup)
        Fanart.Save(dbElement, Enums.ModifierType.MainFanart, forceFileCleanup)
        Keyart.Save(dbElement, Enums.ModifierType.MainKeyart, forceFileCleanup)
        Landscape.Save(dbElement, Enums.ModifierType.MainLandscape, forceFileCleanup)
        Poster.Save(dbElement, Enums.ModifierType.MainPoster, forceFileCleanup)
        ' ... extrafanarts, extrathumbs
    End Sub

Each `Image.Save()` call potentially downloads from a URL if the image hasn't been cached locally.

---

## Part 5: Trailer Scraping Process

### 5.1 Entry Point

Trailer scraping is initiated by calling `ModulesManager.Instance.ScrapeTrailer_Movie()`.

### 5.2 ScrapeTrailer_Movie Flow

1. Get enabled trailer scrapers in order
2. For each scraper:
   - Call `_externalScraperModule.ProcessorModule.Scraper()`
   - Build stream variants for each trailer
   - Aggregate into single list
3. Return combined trailer list

### 5.3 Trailer Scrapers

| Scraper | Source |
|---------|--------|
| TMDB Trailer | TMDB API (YouTube links) |
| Apple Trailer | Apple Trailers |
| YouTube | YouTube search |
| Davestrailerpage | davestrailerpage.co.uk |

---

## Part 6: Control Structures

### 6.1 ScrapeType Enum

Controls which movies are scraped and user interaction level:

| Value | Description |
|-------|-------------|
| `AllAuto` | All movies, auto-select results |
| `AllAsk` | All movies, ask user to select |
| `AllSkip` | All movies, skip if no match |
| `MissingAuto/Ask/Skip` | Movies missing data only |
| `NewAuto/Ask/Skip` | Newly added movies only |
| `MarkedAuto/Ask/Skip` | User-marked movies only |
| `FilterAuto/Ask/Skip` | Current filter results only |
| `SelectedAuto/Ask/Skip` | Selected movies only |
| `SingleScrape` | Single movie, manual selection |
| `SingleAuto` | Single movie, auto-select |
| `SingleField` | Single field update |

### 6.2 ScrapeModifiers Structure

Controls which data types to scrape:

    Public Structure ScrapeModifiers
        Dim DoSearch As Boolean         ' Force search even if IDs exist
        Dim MainActorthumbs As Boolean  ' Scrape actor thumbnails
        Dim MainBanner As Boolean       ' Scrape banner image
        Dim MainCharacterArt As Boolean ' Scrape character art
        Dim MainClearArt As Boolean     ' Scrape clear art
        Dim MainClearLogo As Boolean    ' Scrape clear logo
        Dim MainDiscArt As Boolean      ' Scrape disc art
        Dim MainExtrafanarts As Boolean ' Scrape extra fanarts
        Dim MainExtrathumbs As Boolean  ' Scrape extra thumbs
        Dim MainFanart As Boolean       ' Scrape fanart
        Dim MainKeyart As Boolean       ' Scrape keyart
        Dim MainLandscape As Boolean    ' Scrape landscape
        Dim MainMeta As Boolean         ' Scrape MediaInfo
        Dim MainNFO As Boolean          ' Scrape NFO data
        Dim MainPoster As Boolean       ' Scrape poster
        Dim MainSubtitles As Boolean    ' Scrape subtitles
        Dim MainTheme As Boolean        ' Scrape theme music
        Dim MainTrailer As Boolean      ' Scrape trailer
        ' ... additional fields for TV content
    End Structure

### 6.3 ScrapeOptions Structure

Controls which metadata fields to scrape:

    Public Structure ScrapeOptions
        Dim bMainActors As Boolean
        Dim bMainCertifications As Boolean
        Dim bMainCollectionID As Boolean
        Dim bMainCountries As Boolean
        Dim bMainDirectors As Boolean
        Dim bMainGenres As Boolean
        Dim bMainMPAA As Boolean
        Dim bMainOriginalTitle As Boolean
        Dim bMainOutline As Boolean
        Dim bMainPlot As Boolean
        Dim bMainPremiered As Boolean
        Dim bMainRating As Boolean
        Dim bMainRuntime As Boolean
        Dim bMainStudios As Boolean
        Dim bMainTagline As Boolean
        Dim bMainTitle As Boolean
        Dim bMainTop250 As Boolean
        Dim bMainTrailer As Boolean
        Dim bMainUserRating As Boolean
        Dim bMainWriters As Boolean
        ' ... additional fields for TV content
    End Structure

---

## Part 7: Data Flow Diagrams

### 7.1 Complete Movie Scrape Flow

    User initiates scrape (UI/CommandLine)
            |
            v
    CreateScrapeList_Movie() builds movie list
            |
            v
    For each movie in list:
            |
            +---> ScrapeData_Movie()
            |           |
            |           v
            |       Get enabled Data scrapers in order
            |           |
            |           v
            |       For each scraper (TMDB, IMDB, etc.):
            |           |
            |           v
            |       Call Scraper_Movie()
            |           |
            |           v
            |       Add result to ScrapedList
            |           |
            |           v
            |       Pass IDs to next scraper
            |           |
            |           v
            |       MergeDataScraperResults_Movie()
            |
            +---> ScrapeImage_Movie()
            |           |
            |           v
            |       Get enabled Image scrapers
            |           |
            |           v
            |       For each scraper (TMDB, FanartTV):
            |           |
            |           v
            |       Aggregate image URLs into container
            |           |
            |           v
            |       (Images NOT downloaded yet - only URLs collected)
            |
            +---> ScrapeTrailer_Movie() (if requested)
            |
            +---> Save_Movie() to database
            |           |
            |           v
            |       Write NFO file
            |           |
            |           v
            |       SaveAllImages() - DOWNLOADS images here (sequential)
            |           |
            |           v
            |       Save actor thumbs
            |           |
            |           v
            |       Save theme/trailer files
            |           |
            |           v
            |       Write to database
            |
            +---> RunGeneric(ScraperMulti_Movie) - notify modules
            |
            v
    Process complete

### 7.2 ID Propagation Flow

    TMDB Scraper runs first
            |
            v
    Retrieves TMDB ID and IMDB ID
            |
            v
    IDs added to oDBMovie.Movie.UniqueIDs
            |
            v
    IMDB Scraper runs second
            |
            v
    Uses IMDB ID from UniqueIDs (no search needed)
            |
            v
    Both results merged

---

### 7.3 Database Save Points

During movie scraping, `Save_Movie()` is called at multiple points:

| Location | File | When Called |
|----------|------|-------------|
| Scanner Load | `clsAPIScanner.vb` | After loading/scanning a movie file |
| Task Manager | `clsAPITaskManager.vb` | During bulk edit operations |
| Image Save | `clsAPIImages.vb` | After saving images to disk |
| Edit Dialog | `dlgEdit_Movie.vb` | When user saves from edit dialog |
| NFO Operations | `clsAPINFO.vb` | During NFO save operations |
| Media Files | `clsAPIMediaFiles.vb` | After media file operations |
| TMDB Scraper | `TMDB_Data.vb` | After collection ID update |
| Trakt Sync | `clsAPITrakt.vb` | After Trakt synchronization |

**During a typical scrape operation, saves occur:**
1. After data scraping completes (metadata saved)
2. After image processing completes (image paths updated)

This explains why 50 movies result in ~100 `Save_Movie()` calls.

### 7.4 Save_Movie Method Variants

The database layer provides two save methods:

| Method | Location | Purpose |
|--------|----------|---------|
| `Save_Movie()` | `clsAPIDatabase.vb` | Synchronous save with sequential image downloads |
| `Save_MovieAsync()` | `clsAPIDatabase.vb` | Async save with parallel image downloads |

**Save_Movie() - Synchronous (Current Default):**

    Public Function Save_Movie(...) As DBElement
        ' ... database operations ...
        If toDisk Then
            dbElement.ImagesContainer.SaveAllImages(dbElement, forceFileCleanup)  ' Sequential
            dbElement.Movie.SaveAllActorThumbs(dbElement)
            dbElement.Theme.Save(...)
            dbElement.Trailer.Save(...)
        End If
        ' ... more database operations ...
    End Function

**Save_MovieAsync() - Asynchronous (For Bulk Operations):**

    Public Async Function Save_MovieAsync(...) As Task(Of DBElement)
        ' ... database operations ...
        If toDisk Then
            dbElement = Await dbElement.ImagesContainer.SaveAllImagesAsync(dbElement, forceFileCleanup)  ' Parallel
            dbElement.Movie.SaveAllActorThumbs(dbElement)
            dbElement.Theme.Save(...)
            dbElement.Trailer.Save(...)
        End If
        ' ... more database operations ...
    End Function

The async version enables parallel image downloads during bulk scraping, significantly reducing total scrape time.

---

## Part 8: Settings That Affect Scraping

### 8.1 Scraper Enable/Order Settings

| Setting Category | Effect |
|-----------------|--------|
| Scraper Enable | Which scrapers run |
| Scraper Order | Sequence of execution |
| Module Order | Priority for data merging |

### 8.2 Field-Level Settings

| Setting Pattern | Example | Effect |
|-----------------|---------|--------|
| `MovieScraper*` | `MovieScraperTitle` | Enable/disable field scraping |
| `MovieLock*` | `MovieLockTitle` | Protect field from overwriting |
| `MovieScraper*Limit` | `MovieScraperGenreLimit` | Limit count of list items |

### 8.3 Processing Settings

| Setting | Effect |
|---------|--------|
| `MovieScraperCleanFields` | Clear fields when scraping disabled |
| `MovieScraperCleanPlotOutline` | Remove brackets from plot/outline |
| `MovieScraperCertLang` | Certification language filter |
| `MovieScraperCastWithImgOnly` | Only actors with images |
| `MovieScraperStudioWithImgOnly` | Only studios with icons |
| `MovieScraperOriginalTitleAsTitle` | Copy OriginalTitle to Title |
| `MovieScraperPlotForOutline` | Generate Outline from Plot |
| `MovieScraperXBMCTrailerFormat` | Convert YouTube URLs to Kodi format |
| `MovieScraperTrailerFromTrailerScrapers` | Include trailer scraper results |

---

## Part 9: TV Show Overlap

### 9.1 Shared Architecture

TV Show scraping uses the same architectural patterns:

| Movie Component | TV Equivalent |
|-----------------|---------------|
| `ScrapeData_Movie()` | `ScrapeData_TV()` |
| `ScrapeImage_Movie()` | `ScrapeImage_TV()` |
| `ScrapeTrailer_Movie()` | N/A (no trailer scrapers for TV) |
| `MergeDataScraperResults_Movie()` | `MergeDataScraperResults_TVShow()` |
| `externalScrapersModules_Data_Movie` | `externalScrapersModules_Data_TV` |

### 9.2 Shared Code

| Component | Shared Between Movie/TV |
|-----------|------------------------|
| `ModulesManager` class | Yes - manages all module types |
| Module loading logic | Yes - same reflection pattern |
| Scraper ordering | Yes - same ModuleOrder property |
| `ScrapeModifiers` structure | Yes - contains fields for both |
| `ScrapeOptions` structure | Yes - contains fields for both |
| `ScrapeType` enum | Yes - same values apply |

### 9.3 Different Handling

| Aspect | Movie | TV |
|--------|-------|-----|
| Content structure | Single `Movie` object | `TVShow` + `TVSeason` + `TVEpisode` hierarchy |
| Episode handling | N/A | Episodes scraped individually or with show |
| Season images | N/A | Separate season poster/banner scraping |
| Episode guide | N/A | EpisodeGuide field for external sources |

---

## Part 10: Performance Considerations

### 10.1 Observed Metrics (50 Movie Baseline)

| Operation | Count | Avg (ms) | Total (sec) |
|-----------|-------|----------|-------------|
| TMDB.GetInfo_Movie | 49 | 1,078 | 52.8 |
| IMDB.GetMovieInfo | 49 | 1,910 | 93.6 |
| Image.LoadFromWeb | 312 | 142 | 44.4 |
| Database.Save_Movie | 98 | 619 | 60.7 |
| Database.Add_Actor | 2,400 | 1.73 | 4.1 |

### 10.2 Key Observations

1. **Both scrapers run:** TMDB and IMDB both execute for each movie
2. **Sequential processing:** Scrapers run one after another, not in parallel
3. **Network dominates:** IMDB HTTP requests account for most of IMDB scrape time
4. **Multiple saves:** ~2 database saves per movie during scrape process
5. **Actor lookups:** ~48 actor lookups per movie (checking for existing actors)
6. **Save points:** Database saves occur after data scraping AND after image processing (~2 per movie)
7. **Image bottleneck:** Image downloads during `SaveAllImages()` are sequential

### 10.3 Performance Bottleneck Analysis

**Where Time Is Spent During Save_Movie():**

| Phase | Operations | Bottleneck |
|-------|------------|------------|
| NFO Write | Write XML to disk | Fast (< 10ms) |
| Image Save | Download + write each image | **SLOW - Sequential downloads** |
| Actor Thumbs | Download actor images | Moderate |
| Database Write | SQL operations | Fast with transactions |

**Image Download Pattern (Current - Sequential):**

    SaveAllImages():
        Banner.Save()     -> Download if URL, write to disk (wait)
        ClearArt.Save()   -> Download if URL, write to disk (wait)
        ClearLogo.Save()  -> Download if URL, write to disk (wait)
        ... (8+ image types, each waits for previous)

**With Parallel Downloads (SaveAllImagesAsync):**

    SaveAllImagesAsync():
        Start all downloads concurrently
        Await Task.WhenAll(allDownloadTasks)
        Write all to disk

### 10.4 Optimization Opportunities

| Area | Current Behavior | Potential Improvement | Status |
|------|-----------------|----------------------|--------|
| HTTP Clients | New instance per request | Shared HttpClient with connection pooling | Available |
| Database | Individual actor lookups | Batch lookups with indices | Partial |
| Image Downloads | Sequential in Save_Movie | Parallel via Save_MovieAsync | **Implemented** |
| API Calls | Separate calls for related data | Consolidated append_to_response | Possible |
| Bulk Scraping | Sequential movie processing | Parallel with throttling | Future |

### 10.5 Using Async Save for Bulk Operations

To leverage parallel image downloads during bulk scraping, call `Save_MovieAsync()` instead of `Save_Movie()`:

    ' In bulk scraping loop
    For Each movie In movieList
        ' ... scraping logic ...
        Await Master.DB.Save_MovieAsync(dbElement, True, True, True, True, False)
    Next

Or use `Task.WhenAll` for true parallelism across movies:

    Dim saveTasks = movieList.Select(Function(m) Master.DB.Save_MovieAsync(m, True, True, True, True, False))
    Await Task.WhenAll(saveTasks)

---

## Summary

The Ember Media Manager scraping system is a flexible, addon-based architecture that:

1. **Loads scrapers dynamically** from the Addons directory
2. **Executes all enabled scrapers** in configured order
3. **Passes IDs between scrapers** for efficient lookups
4. **Merges results** using "first wins" logic with lock protection
5. **Applies mappings and filters** during merge
6. **Supports granular control** via ScrapeModifiers and ScrapeOptions
7. **Shares architecture** between Movie and TV content types
8. **Downloads images during Save_Movie()** not during scraping
9. **Provides async save option** for parallel image downloads

The key insight for performance is that ALL enabled scrapers run for each movie, with results merged afterward. This means enabling both TMDB and IMDB scrapers doubles the scrape time but provides more comprehensive data coverage.

**Performance Critical Path:**
1. Data scraping (network-bound, sequential per scraper)
2. Image URL collection (fast)
3. **Image download during save (network-bound, sequential by default)**
4. Database write (fast)

The async `Save_MovieAsync()` method addresses the image download bottleneck by enabling parallel downloads