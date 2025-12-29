# Movie Scraping Process Analysis

> **Document Version:** 2.1 (Updated December 29, 2025 - Phase 1 Performance Complete)
> **Related Documentation:** For TV Show scraping, see [ScrapingProcessTvShows.md](ScrapingProcessTvShows.md). Both processes share architectural patterns through the `ModulesManager` class.
> **Performance Documentation:** See [PerformanceImprovements-Phase1.md](../PerformanceImprovements-Phase1.md) for optimization details.

---

## Table of Contents

- [Overview](#overview)
- [Part 1: Entry Points](#part-1-entry-points)
- [Part 2: Bulk Scraping Orchestration](#part-2-bulk-scraping-orchestration)
- [Part 3: Scraper Architecture](#part-3-scraper-architecture)
- [Part 4: Data Scraping Process](#part-4-data-scraping-process)
- [Part 5: Image Scraping Process](#part-5-image-scraping-process)
- [Part 6: Theme & Trailer Scraping](#part-6-theme--trailer-scraping)
- [Part 7: Saving to Database](#part-7-saving-to-database)
- [Part 8: Control Structures](#part-8-control-structures)
- [Part 9: Data Flow Diagrams](#part-9-data-flow-diagrams)
- [Part 10: Settings Reference](#part-10-settings-reference)
- [Part 11: Performance Analysis](#part-11-performance-analysis)
- [Part 12: Debugging Guide](#part-12-debugging-guide)
- [Part 13: TV Show Overlap](#part-13-tv-show-overlap)
---

## Overview

Ember Media Manager uses an addon-based modular architecture for scraping movie metadata. Multiple scrapers can be enabled and configured to run in sequence, with each scraper contributing data that is merged according to user-configured rules.

**High-Level Flow:**

    User Action (Context Menu / Tools Menu / Command Line)
            │
            ▼
    CreateScrapeList_Movie() - Builds list of movies to scrape
            │
            ▼
    bwMovieScraper.RunWorkerAsync() - Starts background worker
            │
            ▼
    bwMovieScraper_DoWork() - Main scraping loop
            │
            ▼
    For Each movie: Scrape Data → Scrape Images → Save_Movie()

**Key Insight:** Image downloads occur during `Save_Movie()`, not during the scraping phase. This is the primary performance bottleneck.

---

## Part 1: Entry Points

Movie scraping can be initiated from three locations:

### 1.1 Context Menu - "(Re)Scrape Selected Movies"

**UI Location:** Right-click on movie(s) → "(Re)Scrape Selected Movies" → [Ask/Auto/Skip] → [All Items/specific item]

**Code Path:**

| Step | File | Method | Line |
|------|------|--------|------|
| 1 | `frmMain.vb` | `mnuScrape_Click` | ~12240 |
| 2 | `frmMain.vb` | `CreateScrapeList_Movie()` | ~12350 |
| 3 | `frmMain.vb` | `bwMovieScraper.RunWorkerAsync()` | ~12726 |

**Handler Code:**

    Private Sub mnuScrape_Click(ByVal sender As Object, ByVal e As EventArgs)
        ' Parses menu item tag to determine ContentType, ScrapeType, and Modifier
        Select Case ContentType
            Case "movie"
                CreateScrapeList_Movie(Type, Master.DefaultOptions_Movie, ScrapeModifiers)
        End Select
    End Sub

### 1.2 Custom Scraper Dialog

**UI Location:** Tools menu → Custom Scraper, or context menu "custom" option

**Code Path:**

| Step | File | Method | Line |
|------|------|--------|------|
| 1 | `frmMain.vb` | Menu click handler | ~12366 |
| 2 | `dlgCustomScraper.vb` | `ShowDialog()` | - |
| 3 | `frmMain.vb` | `CreateScrapeList_Movie()` | ~12370 |

**Code:**

    Using dlgCustomScraper As New dlgCustomScraper(Enums.ContentType.Movie)
        Dim CustomScraper As Structures.CustomUpdaterStruct = dlgCustomScraper.ShowDialog()
        If Not CustomScraper.Canceled Then
            CreateScrapeList_Movie(CustomScraper.ScrapeType, CustomScraper.ScrapeOptions, CustomScraper.ScrapeModifiers)
        End If
    End Using

### 1.3 Command Line

**Command:** `-scrapemovies [scrapetype]`

**Code Path:**

| Step | File | Method | Line |
|------|------|--------|------|
| 1 | `clsAPICommandLine.vb` | `Parse()` | ~159 |
| 2 | `clsAPICommandLine.vb` | `RaiseEvent TaskEvent` | ~174 |
| 3 | `frmMain.vb` | `GenericRunCallBack()` | ~10673 |
| 4 | `frmMain.vb` | `CreateScrapeList_Movie()` | ~10677 |

**Code:**

    Case "scrapemovies"
        Master.fLoading.SetProgressBarStyle(ProgressBarStyle.Marquee)
        Master.fLoading.SetLoadingMesg(Master.eLang.GetString(861, "Command Line Scraping..."))
        Dim ScrapeModifiers As Structures.ScrapeModifiers = CType(_params(2), Structures.ScrapeModifiers)
        CreateScrapeList_Movie(CType(_params(1), Enums.ScrapeType), Master.DefaultOptions_Movie, ScrapeModifiers)
        While bwMovieScraper.IsBusy
            Application.DoEvents()
            Threading.Thread.Sleep(50)
        End While

---

## Part 2: Bulk Scraping Orchestration

### 2.1 CreateScrapeList_Movie

**Location:** `frmMain.vb`, lines 12575-12727

**Purpose:** Builds a list of movies to scrape based on `ScrapeType` and validates scraping permissions.

**Flow:**

    CreateScrapeList_Movie(sType, ScrapeOptions, ScrapeModifiers)
            │
            ▼
    ┌───────────────────────────────────────┐
    │ Step 1: Build DataRowList             │
    │ - SelectedAsk/Auto/Skip: Selected rows│
    │ - SingleAuto/Field/Scrape: Selected   │
    │ - Else: All movies in dtMovies        │
    └───────────────────────────────────────┘
            │
            ▼
    ┌───────────────────────────────────────┐
    │ Step 2: Check Allowed Scrapers        │
    │ - ActorThumbsAllowed, BannerAllowed   │
    │ - PosterAllowed, FanartAllowed, etc.  │
    └───────────────────────────────────────┘
            │
            ▼
    ┌───────────────────────────────────────┐
    │ Step 3: For Each DataRow              │
    │ - Skip if Locked (unless SingleScrape)│
    │ - Build ScrapeModifiers per movie     │
    │ - Apply ScrapeType filters            │
    │ - Add to ScrapeList                   │
    └───────────────────────────────────────┘
            │
            ▼
    ┌───────────────────────────────────────┐
    │ Step 4: Configure UI & Start Worker   │
    │ bwMovieScraper.RunWorkerAsync(Args)   │
    └───────────────────────────────────────┘

**Key Code - Building DataRowList (lines 12579-12590):**

    Select Case sType
        Case Enums.ScrapeType.SelectedAsk, Enums.ScrapeType.SelectedAuto, Enums.ScrapeType.SelectedSkip,
             Enums.ScrapeType.SingleAuto, Enums.ScrapeType.SingleField, Enums.ScrapeType.SingleScrape
            For Each sRow As DataGridViewRow In dgvMovies.SelectedRows
                DataRowList.Add(DirectCast(sRow.DataBoundItem, DataRowView).Row)
            Next
        Case Else
            For Each sRow As DataRow In dtMovies.Rows
                DataRowList.Add(sRow)
            Next
    End Select

**Key Code - Applying Filters (lines 12629-12651):**

    Select Case sType
        Case Enums.ScrapeType.NewAsk, Enums.ScrapeType.NewAuto, Enums.ScrapeType.NewSkip
            If Not Convert.ToBoolean(drvRow.Item("New")) Then Continue For
        Case Enums.ScrapeType.MarkedAsk, Enums.ScrapeType.MarkedAuto, Enums.ScrapeType.MarkedSkip
            If Not Convert.ToBoolean(drvRow.Item("Mark")) Then Continue For
        Case Enums.ScrapeType.MissingAsk, Enums.ScrapeType.MissingAuto, Enums.ScrapeType.MissingSkip
            ' Disable modifiers for items that already exist
            If Not String.IsNullOrEmpty(drvRow.Item("PosterPath").ToString) Then sModifier.MainPoster = False
    End Select

### 2.2 The Bulk Scrape Loop (bwMovieScraper_DoWork)

**Location:** `frmMain.vb`, lines 1453-1622

**Purpose:** Main scraping loop that processes each movie sequentially.

**Complete Flow:**

    bwMovieScraper_DoWork(e As DoWorkEventArgs)
            │
            ▼
    Args = DirectCast(e.Argument, Arguments)
            │
            ▼
    ┌─────────────────────────────────────────────────────────────┐
    │        FOR EACH tScrapeItem IN Args.ScrapeList (line 1460)  │
    └─────────────────────────────────────────────────────────────┘
            │
            ├──► Check CancellationPending → Exit If True
            ├──► Report Progress (movie name)
            ├──► Load Movie: Master.DB.Load_Movie(idMovie)
            │
            ▼
    ┌─────────────────────────────────────────────────────────────┐
    │  PHASE 1: DATA SCRAPING (lines 1478-1512)                   │
    │  If ScrapeModifiers.MainNFO Then                            │
    │    → ModulesManager.Instance.ScrapeData_Movie()             │
    │      (Calls TMDB, IMDB, etc. scrapers sequentially)         │
    └─────────────────────────────────────────────────────────────┘
            │
            ▼
    ┌─────────────────────────────────────────────────────────────┐
    │  PHASE 2: MEDIA INFO (lines 1516-1520)                      │
    │  If MovieScraperMetaDataScan And MainMeta Then              │
    │    → MediaInfo.UpdateMediaInfo(DBScrapeMovie)               │
    └─────────────────────────────────────────────────────────────┘
            │
            ▼
    ┌─────────────────────────────────────────────────────────────┐
    │  PHASE 3: IMAGE SCRAPING (lines 1529-1554)                  │
    │  If any image modifier enabled Then                         │
    │    → ModulesManager.Instance.ScrapeImage_Movie()            │
    │      (Collects URLs only - NO downloads yet)                │
    │    → Images.SetPreferredImages() or show dlgImgSelect       │
    └─────────────────────────────────────────────────────────────┘
            │
            ▼
    ┌─────────────────────────────────────────────────────────────┐
    │  PHASE 4: THEME SCRAPING (lines 1559-1578)                  │
    │  If MainTheme Then                                          │
    │    → ModulesManager.Instance.ScrapeTheme_Movie()            │
    └─────────────────────────────────────────────────────────────┘
            │
            ▼
    ┌─────────────────────────────────────────────────────────────┐
    │  PHASE 5: TRAILER SCRAPING (lines 1583-1602)                │
    │  If MainTrailer Then                                        │
    │    → ModulesManager.Instance.ScrapeTrailer_Movie()          │
    └─────────────────────────────────────────────────────────────┘
            │
            ▼
    ┌─────────────────────────────────────────────────────────────┐
    │  PHASE 6: SAVE TO DATABASE (lines 1607-1612)                │
    │  *** CRITICAL - IMAGE DOWNLOADS HAPPEN HERE ***             │
    │  → RunGeneric(ScraperMulti_Movie)                           │
    │  → Master.DB.Save_Movie() (LINE 1610)                       │
    │    - Writes NFO file                                        │
    │    - Downloads all images (SEQUENTIAL!)                     │
    │    - Saves actor thumbs, theme, trailer                     │
    │    - Updates database                                       │
    └─────────────────────────────────────────────────────────────┘
            │
            ▼
         NEXT MOVIE

### 2.3 The Critical Save Line

**Location:** `frmMain.vb`, line 1610

    Master.DB.Save_Movie(DBScrapeMovie, False, tScrapeItem.ScrapeModifiers.MainNFO OrElse tScrapeItem.ScrapeModifiers.MainMeta, True, True, False)

| Parameter | Value | Meaning |
|-----------|-------|---------|
| `dbElement` | `DBScrapeMovie` | The movie to save |
| `batchMode` | `False` | Not in batch transaction mode |
| `toNFO` | `MainNFO Or MainMeta` | Write NFO if scraping NFO or metadata |
| `toDisk` | `True` | **Save images to disk (triggers downloads)** |
| `doSync` | `True` | Sync with Kodi if enabled |
| `forceFileCleanup` | `False` | Don't force cleanup of old files |

**This is where image downloads happen.** The `toDisk = True` parameter causes `SaveAllImages()` to be called, which downloads each image sequentially.

---

## Part 3: Scraper Architecture

### 3.1 Core Files

| File | Purpose |
|------|---------|
| `EmberAPI\clsAPIModules.vb` | Module manager - loads, orders, executes scrapers |
| `EmberAPI\clsAPINFO.vb` | Merges results from multiple scrapers |
| `EmberAPI\clsAPIInterfaces.vb` | Defines scraper interfaces and result structures |
| `EmberAPI\clsAPICommon.vb` | ScrapeModifiers, ScrapeOptions, ScrapeType enums |
| `EmberAPI\clsAPIDatabase.vb` | Database operations including `Save_Movie()` |
| `EmberAPI\clsAPIImages.vb` | Image selection and save operations |
| `EmberAPI\clsAPIMediaContainers.vb` | `SaveAllImages()` and `SaveAllImagesAsync()` |
| `Addons\scraper.TMDB.Data\*` | TMDB data scraper |
| `Addons\scraper.IMDB.Data\*` | IMDB data scraper |

### 3.2 Scraper Types

| Type | Interface | Purpose |
|------|-----------|---------|
| Data | `ScraperModule_Data_Movie` | Metadata (title, plot, cast, etc.) |
| Image | `ScraperModule_Image_Movie` | Artwork (posters, fanart, etc.) |
| Trailer | `ScraperModule_Trailer_Movie` | Trailer URLs |
| Theme | `ScraperModule_Theme_Movie` | Theme music |

### 3.3 Module Loading

Scrapers are loaded at startup via `ModulesManager.LoadModules()`:

1. Scans Addons directory for assemblies
2. Uses reflection to find types implementing scraper interfaces
3. Creates instances and stores in typed collections:
   - `externalScrapersModules_Data_Movie`
   - `externalScrapersModules_Image_Movie`
   - `externalScrapersModules_Trailer_Movie`
   - `externalScrapersModules_Theme_Movie`
4. Loads enabled state and order from settings

### 3.4 Generic Module Event System

Generic modules can subscribe to scraping events:

| Event | When Fired |
|-------|------------|
| `ScraperMulti_Movie` | During bulk/auto scraping |
| `ScraperSingle_Movie` | During single movie scrape |
| `AfterEdit_Movie` | After movie data edited |
| `BeforeEdit_Movie` | Before movie data edited |
| `Sync_Movie` | When movie sync requested |

---

## Part 4: Data Scraping Process

### 4.1 ScrapeData_Movie Entry Point

**Location:** `clsAPIModules.vb`

    Function ScrapeData_Movie(
        ByRef DBElement As Database.DBElement,
        ByRef ScrapeModifiers As Structures.ScrapeModifiers,
        ByVal ScrapeType As Enums.ScrapeType,
        ByVal ScrapeOptions As Structures.ScrapeOptions,
        ByVal showMessage As Boolean
    ) As Boolean

### 4.2 Flow

1. **Validate Online Status** - Check if movie file is accessible
2. **Get Enabled Scrapers** - Order by `ModuleOrder` setting
3. **Clean Movie Data** - Reset for new scrapes if `DoSearch = True`
4. **Clone for Scraping** - Work on copy to preserve original
5. **Execute Each Scraper** - Run TMDB, IMDB, etc. in sequence
6. **Pass IDs Between Scrapers** - TMDB finds IMDB ID, passes to IMDB scraper
7. **Merge Results** - Combine all scraper results

**Key Code - Executing Scrapers:**

    For Each _externalScraperModule In modules
        ret = _externalScraperModule.ProcessorModule.Scraper_Movie(oDBMovie, ScrapeModifiers, ScrapeType, ScrapeOptions)
        
        If ret.Cancelled Then Return ret.Cancelled
        
        If ret.Result IsNot Nothing Then
            ScrapedList.Add(ret.Result)
            ' Pass IDs to subsequent scrapers
            If ret.Result.UniqueIDsSpecified Then
                oDBMovie.Movie.UniqueIDs.AddRange(ret.Result.UniqueIDs)
            End If
        End If
        
        If ret.breakChain Then Exit For
    Next

### 4.3 Result Merging ("First Wins" Strategy)

**Location:** `clsAPINFO.vb` - `MergeDataScraperResults_Movie()`

For each field, a value is accepted if ALL conditions are true:
1. Field not already set OR not locked
2. Field requested in `ScrapeOptions`
3. Scraped movie has value for this field
4. Field scraping enabled in settings
5. First scraper hasn't already provided this field

**Special Processing:**

| Field | Processing |
|-------|------------|
| Actors | Filter by image availability, apply count limit |
| Certifications | Filter by language, run mapping |
| Genres | Run genre mapping, apply count limit |
| Studios | Run studio mapping, filter by image |
| Trailer | Convert YouTube URLs to Kodi format |

### 4.4 Individual Scraper Behavior

**TMDB Scraper** (`Addons\scraper.TMDB.Data\`)

- ID Resolution: TMDB ID → IMDB ID → Title/Year search
- Data Retrieved: Title, Plot, Cast, Directors, Ratings, UniqueIDs, Collection info, Trailers

**IMDB Scraper** (`Addons\scraper.IMDB.Data\`)

- ID Resolution: IMDB ID → Title/Year search
- Data Retrieved: Title, Plot, Cast, Ratings (IMDB, Metacritic), Top250

**Scraper Chain Behavior:**

When both TMDB and IMDB are enabled:
1. TMDB runs first (typically order 0)
2. TMDB retrieves IMDB ID
3. IMDB ID passed to IMDB scraper via `UniqueIDs`
4. IMDB scraper uses ID directly (no search needed)
5. Results merged with "first wins" logic

---

## Part 5: Image Scraping Process

### 5.1 ScrapeImage_Movie Entry Point

**Location:** `clsAPIModules.vb`

    Function ScrapeImage_Movie(
        ByRef DBElement As Database.DBElement,
        ByRef ImagesContainer As MediaContainers.SearchResultsContainer,
        ByVal ScrapeModifiers As Structures.ScrapeModifiers,
        ByVal showMessage As Boolean
    ) As Boolean

### 5.2 Flow

1. Validate online status
2. Get enabled image scrapers in order
3. For each scraper with matching capabilities:
   - Call `Scraper()` method
   - Aggregate URLs into single container
4. Sort and filter results by language/size preferences
5. Create cache paths

**Image Types Collected:**
- MainBanners, MainPosters, MainFanarts
- MainClearArts, MainClearLogos, MainDiscArts
- MainLandscapes, MainCharacterArts, MainKeyarts

### 5.3 SetPreferredImages

**Location:** `clsAPIImages.vb`

Selects best images based on user preferences:
- Size preferences (MoviePosterPrefSize, MovieFanartPrefSize, etc.)
- Language preferences
- Keep existing settings
- Extrafanarts/Extrathumbs limits

**Critical:** This method only assigns image URLs to `DBElement.ImagesContainer`. **No downloads occur here.**

### 5.4 Image Download (During Save_Movie)

**Critical Performance Note:** Image downloading occurs during `Save_Movie()`, not during scraping.

**Location:** `clsAPIMediaContainers.vb` - `SaveAllImages()`

    Public Sub SaveAllImages(ByRef DBElement As Database.DBElement, ByVal ForceFileCleanup As Boolean)
        ' Downloads and saves each image type SEQUENTIALLY
        Banner.LoadAndCache(...)   → Download if URL → Save to disk
        ClearArt.LoadAndCache(...) → Download if URL → Save to disk
        Poster.LoadAndCache(...)   → Download if URL → Save to disk
        ' ... 8+ image types, each waits for previous
    End Sub

**Performance Metrics (per movie):**

| Phase | Avg Time | % of Total |
|-------|----------|------------|
| Download (network) | 815 ms | 94% |
| Disk Write | 46 ms | 5% |
| Overhead | 6 ms | 1% |

**Async Alternative (Phase 1 Complete):**

`Save_MovieAsync()` now uses `SaveAllImagesAsync()` for parallel downloads:
- 5 concurrent downloads (SemaphoreSlim throttled)
- 61% faster than sequential `SaveAllImages()`
- Integrated into bulk scrape at `frmMain.vb` line 1640

---

## Part 6: Theme & Trailer Scraping

### 6.1 Theme Scraping

**Entry Point:** `ModulesManager.Instance.ScrapeTheme_Movie()`

**Scrapers:** TelevisionTunes, YouTube Theme

**Flow:**
1. Get enabled theme scrapers
2. Each scraper searches for theme music
3. Aggregate results
4. Select preferred or show dialog

### 6.2 Trailer Scraping

**Entry Point:** `ModulesManager.Instance.ScrapeTrailer_Movie()`

**Scrapers:** TMDB Trailer, Apple Trailer, YouTube, Davestrailerpage

**Flow:**
1. Get enabled trailer scrapers
2. Each scraper returns trailer URLs
3. Build stream variants for each trailer
4. Select preferred or show dialog

---

## Part 7: Saving to Database

### 7.1 Save_Movie Method

**Location:** `clsAPIDatabase.vb`, line ~3616

    Public Function Save_Movie(
        ByVal dbElement As DBElement,
        ByVal batchMode As Boolean,
        ByVal toNFO As Boolean,
        ByVal toDisk As Boolean,
        ByVal doSync As Boolean,
        ByVal forceFileCleanup As Boolean
    ) As DBElement

**What Happens Inside:**

    If toDisk Then
        dbElement.ImagesContainer.SaveAllImages(dbElement, forceFileCleanup)  ' ← SEQUENTIAL
        dbElement.Movie.SaveAllActorThumbs(dbElement)
        dbElement.Theme.Save(...)
        dbElement.Trailer.Save(...)
    End If
    ' ... database write operations

### 7.2 Save_MovieAsync (Parallel Downloads)

**Location:** `clsAPIDatabase.vb`, line ~3957

    Public Async Function Save_MovieAsync(...) As Task(Of DBElement)
        If toDisk Then
            dbElement = Await dbElement.ImagesContainer.SaveAllImagesAsync(...)  ' ← PARALLEL
            dbElement.Movie.SaveAllActorThumbs(dbElement)
            ' ...
        End If
    End Function

**SaveAllImagesAsync Flow:**

    Phase 1: Collect all images needing download
    Phase 2: Download all in parallel (max 5 concurrent)
    Phase 3: Save to disk sequentially

### 7.3 Database Save Points

During scraping, `Save_Movie()` is called at multiple points:

| Location | File | When |
|----------|------|------|
| Scanner Load | `clsAPIScanner.vb` | After scanning movie file |
| Bulk Scrape | `frmMain.vb` (line 1610) | After scraping completes |
| Edit Dialog | `dlgEdit_Movie.vb` | User saves from dialog |
| Trakt Sync | `clsAPITrakt.vb` | After synchronization |

**Result:** ~2 saves per movie during typical scrape (after data scraping, after image processing).

---

## Part 8: Control Structures

### 8.1 ScrapeType Enum

| Value | Description |
|-------|-------------|
| `AllAuto/Ask/Skip` | All movies |
| `MissingAuto/Ask/Skip` | Movies missing data |
| `NewAuto/Ask/Skip` | Newly added movies |
| `MarkedAuto/Ask/Skip` | User-marked movies |
| `FilterAuto/Ask/Skip` | Current filter results |
| `SelectedAuto/Ask/Skip` | Selected movies |
| `SingleScrape` | Single movie, manual selection |
| `SingleAuto` | Single movie, auto-select |
| `SingleField` | Single field update |

### 8.2 ScrapeModifiers Structure

Controls **what content types** to scrape:

    Public Structure ScrapeModifiers
        Dim DoSearch As Boolean         ' Force search even if IDs exist
        Dim MainNFO As Boolean          ' Scrape metadata
        Dim MainMeta As Boolean         ' MediaInfo scan
        Dim MainPoster As Boolean
        Dim MainFanart As Boolean
        Dim MainBanner As Boolean
        Dim MainClearArt As Boolean
        Dim MainClearLogo As Boolean
        Dim MainDiscArt As Boolean
        Dim MainLandscape As Boolean
        Dim MainKeyart As Boolean
        Dim MainExtrafanarts As Boolean
        Dim MainExtrathumbs As Boolean
        Dim MainActorthumbs As Boolean
        Dim MainTheme As Boolean
        Dim MainTrailer As Boolean
    End Structure

### 8.3 ScrapeOptions Structure

Controls **which metadata fields** to scrape:

    Public Structure ScrapeOptions
        Dim bMainTitle As Boolean
        Dim bMainOriginalTitle As Boolean
        Dim bMainPlot As Boolean
        Dim bMainOutline As Boolean
        Dim bMainTagline As Boolean
        Dim bMainActors As Boolean
        Dim bMainDirectors As Boolean
        Dim bMainWriters As Boolean
        Dim bMainGenres As Boolean
        Dim bMainStudios As Boolean
        Dim bMainCountries As Boolean
        Dim bMainCertifications As Boolean
        Dim bMainMPAA As Boolean
        Dim bMainRating As Boolean
        Dim bMainRuntime As Boolean
        Dim bMainPremiered As Boolean
        Dim bMainTop250 As Boolean
        Dim bMainTrailer As Boolean
    End Structure

### 8.4 Bulk Scraping Data Structures

**Arguments** (passed to BackgroundWorker):

    Public Class Arguments
        Public ScrapeOptions As Structures.ScrapeOptions
        Public ScrapeList As List(Of ScrapeItem)
        Public ScrapeType As Enums.ScrapeType
    End Class

**ScrapeItem** (each movie in list):

    Public Class ScrapeItem
        Public DataRow As DataRow
        Public ScrapeModifiers As Structures.ScrapeModifiers
    End Class

**Results** (returned from BackgroundWorker):

    Public Class Results
        Public DBElement As Database.DBElement
        Public ScrapeType As Enums.ScrapeType
        Public Cancelled As Boolean
    End Class

---

## Part 9: Data Flow Diagrams

### 9.1 Complete Movie Scrape Flow

    User initiates scrape
            │
            ▼
    CreateScrapeList_Movie() → bwMovieScraper.RunWorkerAsync()
            │
            ▼
    For each movie in list:
            │
            ├──► Load_Movie() from database
            │
            ├──► ScrapeData_Movie()
            │         │
            │         ├──► TMDB Scraper → Returns data + IMDB ID
            │         ├──► IMDB Scraper → Uses IMDB ID, returns data
            │         └──► MergeDataScraperResults_Movie()
            │
            ├──► UpdateMediaInfo() (if enabled)
            │
            ├──► ScrapeImage_Movie()
            │         │
            │         ├──► TMDB Image Scraper → Returns URLs
            │         ├──► FanartTV Scraper → Returns URLs
            │         └──► SetPreferredImages() (selects best)
            │
            ├──► ScrapeTheme_Movie() (if enabled)
            │
            ├──► ScrapeTrailer_Movie() (if enabled)
            │
            └──► Save_Movie()  ← *** DOWNLOADS HAPPEN HERE ***
                      │
                      ├──► Write NFO file
                      ├──► SaveAllImages() → Downloads each image
                      ├──► Save actor thumbs
                      ├──► Save theme/trailer
                      └──► Update database

### 9.2 ID Propagation Flow

    TMDB Scraper runs first
            │
            ▼
    Retrieves TMDB ID and IMDB ID
            │
            ▼
    IDs added to oDBMovie.Movie.UniqueIDs
            │
            ▼
    IMDB Scraper runs second
            │
            ▼
    Uses IMDB ID from UniqueIDs (no search needed)
            │
            ▼
    Both results merged ("first wins")

---

## Part 10: Settings Reference

### 10.1 Scraper Settings

| Category | Effect |
|----------|--------|
| Scraper Enable | Which scrapers run |
| Scraper Order | Execution sequence |
| Module Order | Priority for data merging |

### 10.2 Field-Level Settings

| Pattern | Example | Effect |
|---------|---------|--------|
| `MovieScraper*` | `MovieScraperTitle` | Enable/disable field |
| `MovieLock*` | `MovieLockTitle` | Protect from overwrite |
| `MovieScraper*Limit` | `MovieScraperGenreLimit` | Limit list count |

### 10.3 Processing Settings

| Setting | Effect |
|---------|--------|
| `MovieScraperCleanFields` | Clear fields when disabled |
| `MovieScraperCertLang` | Certification language filter |
| `MovieScraperCastWithImgOnly` | Only actors with images |
| `MovieScraperOriginalTitleAsTitle` | Copy OriginalTitle to Title |
| `MovieScraperPlotForOutline` | Generate Outline from Plot |
| `MovieScraperXBMCTrailerFormat` | Convert YouTube to Kodi format |

---

## Part 11: Performance Analysis

### 11.1 Baseline Metrics (50 Movies - December 2025)

| Operation | Count | Avg (ms) | Total (sec) |
|-----------|-------|----------|-------------|
| TMDB.GetInfo_Movie | 49 | 1,078 | 52.8 |
| IMDB.GetMovieInfo | 49 | 1,910 | 93.6 |
| Image.LoadFromWeb | 312 | 142 | 44.4 |
| Database.Save_Movie | 98 | 619 | 60.7 |
| Database.Add_Actor | 2,400 | 1.73 | 4.1 |

### 11.2 Time Distribution Per Movie

| Phase | Avg Time | % |
|-------|----------|---|
| TMDB Scrape | 1,078 ms | 24% |
| IMDB Scrape | 1,910 ms | 43% |
| Image Download | 868 ms | 19% |
| Database Save | 619 ms | 14% |
| **Total** | ~4,475 ms | 100% |

### 11.3 SaveAllImages Breakdown

| Phase | Avg Time | % |
|-------|----------|---|
| Network Download | 815 ms | 94% |
| Disk Write | 46 ms | 5% |
| Overhead | 6 ms | 1% |

**Key Finding:** Network I/O dominates. Parallel downloads can reduce by ~64%.

### 11.4 Performance Bottleneck

**Current (Sequential):**

    SaveAllImages():
        Banner.Save()     → Wait → Write
        ClearArt.Save()   → Wait → Write
        Poster.Save()     → Wait → Write
        ... (8+ types, each waits)

**With SaveAllImagesAsync (Parallel):**

    SaveAllImagesAsync():
        Collect all images needing download
        Download all concurrently (max 5)
        Write all to disk

### 11.5 Optimization Status (Phase 1 Complete - December 29, 2025)

| Area | Status | Improvement |
|------|--------|-------------|
| Shared HttpClient | ✅ Complete | -47% TMDB API calls |
| Database Indices | ✅ Complete | -63% actor lookups |
| Parallel Image Downloads | ✅ Complete | -64% download phase |
| Bulk Scrape Integration | ✅ Complete | -61% SaveAllImages |

**Phase 1 Result:** 61% improvement in bulk scraping image operations.

See [PerformanceImprovements-Phase1.md](../PerformanceImprovements-Phase1.md) for complete details.

---

## Part 12: Debugging Guide

### 12.1 Recommended Breakpoints

| Priority | Line | File | Purpose |
|----------|------|------|---------|
| 1 | 12726 | `frmMain.vb` | Before `RunWorkerAsync` - see scrape list |
| 2 | 1460 | `frmMain.vb` | Start of each movie iteration |
| 3 | 1479 | `frmMain.vb` | Before `ScrapeData_Movie` |
| 4 | 1543 | `frmMain.vb` | Before `ScrapeImage_Movie` |
| **5** | **1610** | **`frmMain.vb`** | **Before `Save_Movie` - KEY LINE** |
| 6 | ~3616 | `clsAPIDatabase.vb` | Inside `Save_Movie` |

### 12.2 Debug Session Setup

1. Open `frmMain.vb` in Visual Studio
2. Set breakpoints at lines 1460 and 1610
3. Run debug (F5)
4. Select 2 movies in movie list
5. Right-click → "(Re)Scrape Selected Movies" → "Ask" → "All Items"
6. Step through to observe flow

### 12.3 NLog Tracing

Key log messages:

    [Movie Scraper] [Start] Movies Count [X]
    [Movie Scraper] [Start] Scraping {MovieTitle}
    [Movie Scraper] [Done] Scraping {MovieTitle}

Enable trace logging in `NLog.config` for detailed flow.

### 12.4 Performance Metrics

Metrics are logged on application shutdown to:
- NLog output
- CSV file in `Log` folder

Key metrics to watch:
- `SaveAllImages.Movie.Total`
- `SaveAllImages.Movie.Download`
- `TMDB.GetInfo_Movie`
- `IMDB.GetMovieInfo`

### 12.5 Cancel Scraper Mechanism

The application uses `BackgroundWorker.CancelAsync()` pattern for cancellation. Each major component has its own cancel method.

#### Cancel Method Locations

| Component | BackgroundWorker | Cancel Method | File |
|-----------|------------------|---------------|------|
| **Scanner** | `bwPrelim` | `Scanner.Cancel()` | `clsAPIScanner.vb` line 49 |
| **Scanner (wait)** | `bwPrelim` | `Scanner.CancelAndWait()` | `clsAPIScanner.vb` line 53 |
| **Task Manager** | `bwTaskManager` | `TaskManager.Cancel()` | `clsAPITaskManager.vb` line 127 |
| **Task Manager (wait)** | `bwTaskManager` | `TaskManager.CancelAndWait()` | `clsAPITaskManager.vb` line 131 |
| **Movie Scraper** | `bwMovieScraper` | In `frmMain.vb` | Check for cancel button handler |
| **TV Scraper** | `bwTVScraper` | In `frmMain.vb` | Check for cancel button handler |

#### Cancel Implementation Pattern

All cancel implementations follow this pattern:

    ' Simple cancel - non-blocking
    Public Sub Cancel()
        If bwWorker.IsBusy Then bwWorker.CancelAsync()
    End Sub
    
    ' Cancel and wait - blocking until complete
    Public Sub CancelAndWait()
        If bwWorker.IsBusy Then bwWorker.CancelAsync()
        While bwWorker.IsBusy
            Application.DoEvents()  ' or Threading.Thread.Sleep(50)
            Threading.Thread.Sleep(50)
        End While
    End Sub

#### DoWork Cancellation Check

Inside each `BackgroundWorker.DoWork` handler, cancellation is checked:

    Private Sub bwWorker_DoWork(sender As Object, e As DoWorkEventArgs)
        For Each item In items
            ' Check for cancellation at start of each iteration
            If bwWorker.CancellationPending Then
                e.Cancel = True
                Exit For
            End If
            
            ' ... process item ...
        Next
    End Sub

#### Key Files for Cancel Logic

| File | Contains |
|------|----------|
| `clsAPIScanner.vb` | Scanner `Cancel()` and `CancelAndWait()` methods |
| `clsAPITaskManager.vb` | TaskManager `Cancel()` and `CancelAndWait()` methods |
| `frmMain.vb` | Movie/TV scraper BackgroundWorkers and cancel handlers |
| `dlgCustomScraper.vb` | Custom scraper dialog cancel button |

#### Async Cancellation (Item 5)

For async operations (like `SaveAllImagesAsync`), use `CancellationToken`:

    ' Pass token through async chain
    Await Images.DownloadImagesParallelAsync(
        images,
        contentType,
        maxConcurrency:=5,
        cancellationToken:=token  ' Wire to BackgroundWorker cancellation
    )

When integrating async into BackgroundWorker context, convert cancellation:

    ' In BackgroundWorker.DoWork
    Dim cts As New CancellationTokenSource()
    
    ' Check periodically and cancel token if needed
    If bwWorker.CancellationPending Then
        cts.Cancel()
    End If

## Part 13: TV Show Overlap

### 13.1 Shared Architecture

| Movie | TV Equivalent |
|-------|---------------|
| `ScrapeData_Movie()` | `ScrapeData_TV()` |
| `ScrapeImage_Movie()` | `ScrapeImage_TV()` |
| `MergeDataScraperResults_Movie()` | `MergeDataScraperResults_TVShow()` |
| `externalScrapersModules_Data_Movie` | `externalScrapersModules_Data_TV` |

### 13.2 Shared Code

| Component | Shared |
|-----------|--------|
| `ModulesManager` class | Yes |
| Module loading logic | Yes |
| `ScrapeModifiers` structure | Yes |
| `ScrapeOptions` structure | Yes |
| `ScrapeType` enum | Yes |

### 13.3 Different Handling

| Aspect | Movie | TV |
|--------|-------|-----|
| Structure | Single `Movie` | `TVShow` + `TVSeason` + `TVEpisode` |
| Season images | N/A | Separate season poster/banner |
| Episode guide | N/A | EpisodeGuide field |

---

## Summary

The Ember Media Manager movie scraping system:

1. **Entry Points:** Context menu, Tools menu, command line
2. **Orchestration:** `CreateScrapeList_Movie()` → `bwMovieScraper_DoWork()`
3. **Data Scraping:** Multiple scrapers run sequentially, results merged
4. **Image Scraping:** URLs collected only, **downloads happen during Save_Movie()**
5. **Saving:** `Save_Movie()` writes NFO, downloads images, updates database

**Critical Performance Path:**

    Data scraping (network-bound) → Image URL collection (fast) → 
    **Image download during Save_Movie() (BOTTLENECK)** → Database write (fast)

**Key Code Locations:**

| Operation | File | Line |
|-----------|------|------|
| Start scraping | `frmMain.vb` | 12726 |
| Main loop | `frmMain.vb` | 1460 |
| Save (downloads) | `frmMain.vb` | 1610 |
| SaveAllImages | `clsAPIMediaContainers.vb` | ~3500 |

**Performance Optimization:** Use `Save_MovieAsync()` for parallel image downloads (~64% improvement in image download phase).