# TV Show Scraping Process Analysis

> **Document Version:** 2.0 (Updated December 29, 2025)
> **Related Documentation:** For Movie scraping, see [ScrapingProcessMovies.md](ScrapingProcessMovies.md). Both processes share architectural patterns through the `ModulesManager` class.
> **Performance Documentation:** See [PerformanceImprovements-Phase1.md](../PerformanceImprovements-Phase1.md) for optimization details.

---

## Table of Contents

- [Overview](#overview)
- [Part 1: Entry Points](#part-1-entry-points)
- [Part 2: Bulk Scraping Orchestration](#part-2-bulk-scraping-orchestration)
- [Part 3: TV Content Hierarchy](#part-3-tv-content-hierarchy)
- [Part 4: Scraper Architecture](#part-4-scraper-architecture)
- [Part 5: TV Show Data Scraping](#part-5-tv-show-data-scraping)
- [Part 6: Season and Episode Scraping](#part-6-season-and-episode-scraping)
- [Part 7: Image Scraping Process](#part-7-image-scraping-process)
- [Part 8: Theme Scraping](#part-8-theme-scraping)
- [Part 9: Saving to Database](#part-9-saving-to-database)
- [Part 10: Control Structures](#part-10-control-structures)
- [Part 11: Data Flow Diagrams](#part-11-data-flow-diagrams)
- [Part 12: Settings Reference](#part-12-settings-reference)
- [Part 13: Performance Analysis](#part-13-performance-analysis)
- [Part 14: Debugging Guide](#part-14-debugging-guide)
- [Part 15: Differences from Movie Scraping](#part-15-differences-from-movie-scraping)

---

## Overview

Ember Media Manager uses the same addon-based modular architecture for TV Show scraping as Movies, but handles a **hierarchical content structure**: TV Shows contain Seasons, which contain Episodes. Each level has its own metadata, images, and scraping methods.

**High-Level Flow:**

    User Action (Context Menu / Tools Menu / Command Line)
            │
            ▼
    CreateScrapeList_TVShow() - Builds list of shows to scrape
            │
            ▼
    bwTVScraper.RunWorkerAsync() - Starts background worker
            │
            ▼
    bwTVScraper_DoWork() - Main scraping loop
            │
            ▼
    For Each show: Scrape Data → Scrape Images → Save_TVShow()
                        │
                        ▼
                   For Each episode: Save_TVEpisode()

**Key Insight:** TV scraping generates significantly more database operations than Movies due to the hierarchical structure. A show with 5 seasons and 100 episodes can trigger 100+ save operations.

---

## Part 1: Entry Points

TV Show scraping can be initiated from three locations:

### 1.1 Context Menu - "(Re)Scrape Selected TV Shows"

**UI Location:** Right-click on TV show(s) → "(Re)Scrape Selected TV Shows" → [Ask/Auto/Skip] → [All Items/specific item]

**Code Path:**

| Step | File | Method | Line |
|------|------|--------|------|
| 1 | `frmMain.vb` | `mnuScrape_Click` | ~12240 |
| 2 | `frmMain.vb` | `CreateScrapeList_TVShow()` | ~12800 |
| 3 | `frmMain.vb` | `bwTVScraper.RunWorkerAsync()` | ~13100 |

**Handler Code:**

    Private Sub mnuScrape_Click(ByVal sender As Object, ByVal e As EventArgs)
        ' Parses menu item tag to determine ContentType, ScrapeType, and Modifier
        Select Case ContentType
            Case "tvshow"
                CreateScrapeList_TVShow(Type, Master.DefaultOptions_TV, ScrapeModifiers)
        End Select
    End Sub

### 1.2 Custom Scraper Dialog

**UI Location:** Tools menu → Custom Scraper, or context menu "custom" option

**Code Path:**

| Step | File | Method | Line |
|------|------|--------|------|
| 1 | `frmMain.vb` | Menu click handler | ~12366 |
| 2 | `dlgCustomScraper.vb` | `ShowDialog()` | - |
| 3 | `frmMain.vb` | `CreateScrapeList_TVShow()` | ~12380 |

**Code:**

    Using dlgCustomScraper As New dlgCustomScraper(Enums.ContentType.TVShow)
        Dim CustomScraper As Structures.CustomUpdaterStruct = dlgCustomScraper.ShowDialog()
        If Not CustomScraper.Canceled Then
            CreateScrapeList_TVShow(CustomScraper.ScrapeType, CustomScraper.ScrapeOptions, CustomScraper.ScrapeModifiers)
        End If
    End Using

### 1.3 Command Line

**Command:** `-scrapetvshows [scrapetype]`

**Code Path:**

| Step | File | Method | Line |
|------|------|--------|------|
| 1 | `clsAPICommandLine.vb` | `Parse()` | ~180 |
| 2 | `clsAPICommandLine.vb` | `RaiseEvent TaskEvent` | ~195 |
| 3 | `frmMain.vb` | `GenericRunCallBack()` | ~10690 |
| 4 | `frmMain.vb` | `CreateScrapeList_TVShow()` | ~10695 |

**Code:**

    Case "scrapetvshows"
        Master.fLoading.SetProgressBarStyle(ProgressBarStyle.Marquee)
        Master.fLoading.SetLoadingMesg(Master.eLang.GetString(862, "Command Line Scraping TV Shows..."))
        Dim ScrapeModifiers As Structures.ScrapeModifiers = CType(_params(2), Structures.ScrapeModifiers)
        CreateScrapeList_TVShow(CType(_params(1), Enums.ScrapeType), Master.DefaultOptions_TV, ScrapeModifiers)
        While bwTVScraper.IsBusy
            Application.DoEvents()
            Threading.Thread.Sleep(50)
        End While

---

## Part 2: Bulk Scraping Orchestration

### 2.1 CreateScrapeList_TVShow

**Location:** `frmMain.vb`, lines ~12800-13050

**Purpose:** Builds a list of TV shows to scrape based on `ScrapeType` and validates scraping permissions.

**Flow:**

    CreateScrapeList_TVShow(sType, ScrapeOptions, ScrapeModifiers)
            │
            ▼
    ┌───────────────────────────────────────┐
    │ Step 1: Build DataRowList             │
    │ - SelectedAsk/Auto/Skip: Selected rows│
    │ - SingleAuto/Field/Scrape: Selected   │
    │ - Else: All shows in dtTVShows        │
    └───────────────────────────────────────┘
            │
            ▼
    ┌───────────────────────────────────────┐
    │ Step 2: Check Allowed Scrapers        │
    │ - MainBannerAllowed, MainPosterAllowed│
    │ - SeasonBannerAllowed, etc.           │
    │ - EpisodePosterAllowed, etc.          │
    └───────────────────────────────────────┘
            │
            ▼
    ┌───────────────────────────────────────┐
    │ Step 3: For Each DataRow              │
    │ - Skip if Locked (unless SingleScrape)│
    │ - Build ScrapeModifiers per show      │
    │ - Apply ScrapeType filters            │
    │ - Add to ScrapeList                   │
    └───────────────────────────────────────┘
            │
            ▼
    ┌───────────────────────────────────────┐
    │ Step 4: Configure UI & Start Worker   │
    │ bwTVScraper.RunWorkerAsync(Args)      │
    └───────────────────────────────────────┘

**Key Code - Building DataRowList:**

    Select Case sType
        Case Enums.ScrapeType.SelectedAsk, Enums.ScrapeType.SelectedAuto, Enums.ScrapeType.SelectedSkip,
             Enums.ScrapeType.SingleAuto, Enums.ScrapeType.SingleField, Enums.ScrapeType.SingleScrape
            For Each sRow As DataGridViewRow In dgvTVShows.SelectedRows
                DataRowList.Add(DirectCast(sRow.DataBoundItem, DataRowView).Row)
            Next
        Case Else
            For Each sRow As DataRow In dtTVShows.Rows
                DataRowList.Add(sRow)
            Next
    End Select

### 2.2 The Bulk Scrape Loop (bwTVScraper_DoWork)

**Location:** `frmMain.vb`, lines ~1700-1950

**Purpose:** Main scraping loop that processes each TV show sequentially.

**Complete Flow:**

    bwTVScraper_DoWork(e As DoWorkEventArgs)
            │
            ▼
    Args = DirectCast(e.Argument, Arguments)
            │
            ▼
    ┌─────────────────────────────────────────────────────────────┐
    │      FOR EACH tScrapeItem IN Args.ScrapeList (line ~1720)   │
    └─────────────────────────────────────────────────────────────┘
            │
            ├──► Check CancellationPending → Exit If True
            ├──► Report Progress (show name)
            ├──► Load Show: Master.DB.Load_TVShow(idShow, True, True)
            │    (Loads show with seasons and episodes)
            │
            ▼
    ┌─────────────────────────────────────────────────────────────┐
    │  PHASE 1: DATA SCRAPING (lines ~1740-1780)                  │
    │  If ScrapeModifiers.MainNFO Then                            │
    │    → ModulesManager.Instance.ScrapeData_TVShow()            │
    │      (Calls TVDB, TMDB, IMDB scrapers sequentially)         │
    │      (Also scrapes episodes if withEpisodes = True)         │
    └─────────────────────────────────────────────────────────────┘
            │
            ▼
    ┌─────────────────────────────────────────────────────────────┐
    │  PHASE 2: MEDIA INFO (lines ~1785-1800)                     │
    │  If TVScraperMetaDataScan And EpisodeMeta Then              │
    │    For Each episode:                                        │
    │      → MediaInfo.UpdateMediaInfo(episode)                   │
    └─────────────────────────────────────────────────────────────┘
            │
            ▼
    ┌─────────────────────────────────────────────────────────────┐
    │  PHASE 3: IMAGE SCRAPING (lines ~1805-1850)                 │
    │  If any image modifier enabled Then                         │
    │    → ModulesManager.Instance.ScrapeImage_TV()               │
    │      (Collects URLs for show, season, and episode images)   │
    │    → Images.SetPreferredImages() or show dlgImgSelect       │
    └─────────────────────────────────────────────────────────────┘
            │
            ▼
    ┌─────────────────────────────────────────────────────────────┐
    │  PHASE 4: THEME SCRAPING (lines ~1855-1875)                 │
    │  If MainTheme Then                                          │
    │    → ModulesManager.Instance.ScrapeTheme_TV()               │
    └─────────────────────────────────────────────────────────────┘
            │
            ▼
    ┌─────────────────────────────────────────────────────────────┐
    │  PHASE 5: SAVE TO DATABASE (lines ~1880-1920)               │
    │  *** CRITICAL - IMAGE DOWNLOADS HAPPEN HERE ***             │
    │  → RunGeneric(ScraperMulti_TVShow)                          │
    │  → Master.DB.Save_TVShow() (LINE ~1890)                     │
    │    - Writes show NFO file                                   │
    │    - Downloads show images (SEQUENTIAL!)                    │
    │    - Saves actor thumbs, theme                              │
    │    - Updates database                                       │
    │                                                             │
    │  → For Each season:                                         │
    │      Master.DB.Save_TVSeason()                              │
    │        - Writes season NFO                                  │
    │        - Downloads season images                            │
    │                                                             │
    │  → For Each episode:                                        │
    │      Master.DB.Save_TVEpisode()                             │
    │        - Writes episode NFO                                 │
    │        - Downloads episode images                           │
    │        - Saves actor thumbs                                 │
    └─────────────────────────────────────────────────────────────┘
            │
            ▼
         NEXT SHOW

### 2.3 The Critical Save Lines

**Show Save - Location:** `frmMain.vb`, line ~1890

    Master.DB.Save_TVShow(DBScrapeTVShow, False, True, True, True, False)

**Season Save - Location:** Inside show save loop

    Master.DB.Save_TVSeason(DBScrapeSeason, False, True, True, False)

**Episode Save - Location:** Inside episode loop

    Master.DB.Save_TVEpisode(DBScrapeEpisode, False, True, True, True, False)

**This is where image downloads happen.** The `toDisk = True` parameter causes `SaveAllImages()` to be called at each level, which downloads images sequentially.

---

## Part 3: TV Content Hierarchy

Unlike Movies (single content unit), TV content is hierarchical:

    TV Show
        │
        ├── Show-level metadata (title, plot, actors, etc.)
        ├── Show-level images (poster, fanart, banner, etc.)
        │
        ├── Season 1
        │       ├── Season-level metadata (title, plot, aired)
        │       ├── Season-level images (poster, banner, landscape)
        │       │
        │       ├── Episode 1
        │       │       ├── Episode metadata (title, plot, aired, etc.)
        │       │       └── Episode images (poster/still, fanart)
        │       │
        │       ├── Episode 2
        │       │       └── ...
        │       └── ...
        │
        ├── Season 2
        │       └── ...
        │
        └── All Seasons (virtual season for all-seasons artwork)

### 3.1 Content Level Responsibilities

| Level | Metadata | Images | NFO File |
|-------|----------|--------|----------|
| Show | Title, Plot, Actors, Genres, Studios, Ratings | Poster, Fanart, Banner, ClearArt, ClearLogo, Landscape, CharacterArt, Keyart, Extrafanarts | `tvshow.nfo` |
| Season | Title, Plot, Aired | Poster, Banner, Fanart, Landscape | `season##.nfo` |
| Episode | Title, Plot, Aired, Directors, Writers, GuestStars, Rating | Poster (Still), Fanart | `episode.nfo` |
| All Seasons | N/A | Poster, Banner, Fanart, Landscape | N/A |

### 3.2 Hierarchy Impact on Scraping

A typical TV show scrape involves:
- **1 show** scrape operation
- **N seasons** × season operations
- **M episodes** × episode operations

**Example:** Show with 5 seasons, 100 episodes total:
- 1 show data scrape + 1 show save
- 5 season saves
- 100 episode saves
- **Total: 106+ database operations** (vs 2 for a single movie)

---

## Part 4: Scraper Architecture

### 4.1 Core Files

| File | Purpose |
|------|---------|
| `EmberAPI\clsAPIModules.vb` | Module manager - loads, orders, executes scrapers |
| `EmberAPI\clsAPINFO.vb` | Merges results from multiple scrapers |
| `EmberAPI\clsAPIInterfaces.vb` | Defines TV scraper interfaces |
| `EmberAPI\clsAPICommon.vb` | ScrapeModifiers, ScrapeOptions, ScrapeType enums |
| `EmberAPI\clsAPIDatabase.vb` | `Save_TVShow()`, `Save_TVSeason()`, `Save_TVEpisode()` |
| `EmberAPI\clsAPIImages.vb` | Image selection and save operations |
| `Addons\scraper.Data.TVDB\*` | TVDB data scraper (primary) |
| `Addons\scraper.TMDB.Data\*` | TMDB TV data scraper |
| `Addons\scraper.IMDB.Data\*` | IMDB TV data scraper |

### 4.2 TV Scraper Types

| Type | Interface | Purpose |
|------|-----------|---------|
| Data | `ScraperModule_Data_TV` | Metadata for shows, seasons, episodes |
| Image | `ScraperModule_Image_TV` | Artwork at all levels |
| Theme | `ScraperModule_Theme_TV` | Theme music |

**Note:** TV does **NOT** have a dedicated Trailer scraper interface (unlike Movies).

### 4.3 Module Loading

TV scrapers are loaded at startup via `ModulesManager.LoadModules()`:

1. Scans Addons directory for assemblies
2. Uses reflection to find types implementing `ScraperModule_Data_TV`
3. Creates instances and stores in `externalScrapersModules_Data_TV`
4. Loads enabled state and order from settings

### 4.4 Generic Module Event System

| Event | When Fired |
|-------|------------|
| `ScraperMulti_TVShow` | During bulk/auto TV scraping |
| `ScraperSingle_TVShow` | During single show scrape |
| `AfterEdit_TVShow` | After show data edited |
| `AfterEdit_TVEpisode` | After episode data edited |
| `Sync_TVShow` | When show sync requested |

---

## Part 5: TV Show Data Scraping

### 5.1 ScrapeData_TVShow Entry Point

**Location:** `clsAPIModules.vb`

    Function ScrapeData_TVShow(
        ByRef DBElement As Database.DBElement,
        ByRef ScrapeModifiers As Structures.ScrapeModifiers,
        ByVal ScrapeType As Enums.ScrapeType,
        ByVal ScrapeOptions As Structures.ScrapeOptions,
        ByVal showMessage As Boolean
    ) As Boolean

### 5.2 Flow

1. **Validate Online Status** - Check if show folder is accessible
2. **Get Enabled Scrapers** - Order by `ModuleOrder` setting
3. **Clean Show Data** - Reset for new scrapes if `DoSearch = True`
4. **Clone for Scraping** - Work on copy to preserve original
5. **Execute Each Scraper** - Run TVDB, TMDB, IMDB in sequence
6. **Pass IDs Between Scrapers** - TVDB finds IMDB ID, passes to others
7. **Merge Results** - Combine all scraper results
8. **Create Actor Thumb Paths** - For show and episodes

**Key Code - Executing Scrapers:**

    For Each _externalScraperModule In modules
        ret = _externalScraperModule.ProcessorModule.Scraper_TVShow(oShow, ScrapeModifiers, ScrapeType, ScrapeOptions)
        
        If ret.Cancelled Then Return ret.Cancelled
        
        If ret.Result IsNot Nothing Then
            ScrapedList.Add(ret.Result)
            ' Pass IDs to subsequent scrapers
            If ret.Result.UniqueIDsSpecified Then
                oShow.TVShow.UniqueIDs.AddRange(ret.Result.UniqueIDs)
            End If
        End If
        
        If ret.breakChain Then Exit For
    Next

**Key Code - Merging with Episodes:**

    DBElement = NFO.MergeDataScraperResults_TV(DBElement, ScrapedList, ScrapeType, ScrapeOptions, ScrapeModifiers.withEpisodes)

### 5.3 Result Merging ("First Wins" Strategy)

**Location:** `clsAPINFO.vb` - `MergeDataScraperResults_TV()`

For each field, a value is accepted if ALL conditions are true:
1. Field not already set OR not locked
2. Field requested in `ScrapeOptions`
3. Scraped show has value for this field
4. Field scraping enabled in settings
5. First scraper hasn't already provided this field

**Protected Fields (Lock Settings):**

| Field | Lock Setting |
|-------|--------------|
| Actors | `TVLockShowActors` |
| Certifications | `TVLockShowCert` |
| Countries | `TVLockShowCountry` |
| Creators | `TVLockShowCreators` |
| Genres | `TVLockShowGenre` |
| MPAA | `TVLockShowMPAA` |
| OriginalTitle | `TVLockShowOriginalTitle` |
| Plot | `TVLockShowPlot` |
| Premiered | `TVLockShowPremiered` |
| Runtime | `TVLockShowRuntime` |
| Status | `TVLockShowStatus` |
| Studios | `TVLockShowStudio` |
| Tagline | `TVLockShowTagline` |
| Title | `TVLockShowTitle` |
| UserRating | `TVLockShowUserRating` |

### 5.4 Individual Scraper Behavior

**TVDB Scraper** (`Addons\scraper.Data.TVDB\`) - Primary TV Scraper

- ID Resolution: TVDB ID → IMDB ID search → Title search
- Data Retrieved: Title, OriginalTitle, Premiered, Status, Plot, Runtime, MPAA, Genres, Studios, Countries, Cast, Creators, Ratings, UniqueIDs, Episode Guide, Known Episodes/Seasons
- Methods: `Scraper_TVShow()`, `Scraper_TVSeason()`, `Scraper_TVEpisode()`

**TMDB TV Scraper** (`Addons\scraper.TMDB.Data\`)

- ID Resolution: TMDB ID → TVDB ID → IMDB ID → Title search
- Data Retrieved: Similar to TVDB, supplementary data
- Methods: `Scraper_TVShow()`, `Scraper_TVSeason()`, `Scraper_TVEpisode()`

**IMDB TV Scraper** (`Addons\scraper.IMDB.Data\`)

- ID Resolution: Uses IMDB ID from previous scrapers
- Data Retrieved: Ratings (IMDB), supplementary metadata
- Methods: `Scraper_TVShow()`, `Scraper_TVEpisode()`

**Scraper Chain Behavior:**

When TVDB, TMDB, and IMDB are all enabled:
1. **TVDB runs first** (typically order 0) - retrieves TVDB ID and IMDB ID
2. IDs passed to TMDB via `oShow.TVShow.UniqueIDs`
3. **TMDB runs second** - uses TVDB/IMDB IDs, retrieves TMDB ID
4. All IDs passed to IMDB
5. **IMDB runs third** - uses IMDB ID directly
6. Results merged with "first wins" logic

---

## Part 6: Season and Episode Scraping

### 6.1 Season Scraping

**Entry Point:** `ModulesManager.Instance.ScrapeData_TVSeason()`

**Method Signature:**

    Function ScrapeData_TVSeason(
        ByRef DBElement As Database.DBElement,
        ByVal ScrapeOptions As Structures.ScrapeOptions,
        ByVal showMessage As Boolean
    ) As Boolean

**Note:** Season scraping does NOT use `ScrapeModifiers` or `ScrapeType`.

**Flow:**
1. Clone the season element
2. Execute each enabled scraper via `Scraper_TVSeason()`
3. Pass UniqueIDs to subsequent scrapers
4. Merge results via `MergeDataScraperResults_TVSeason()`

### 6.2 Episode Scraping

**Entry Point:** `ModulesManager.Instance.ScrapeData_TVEpisode()`

**Method Signature:**

    Function ScrapeData_TVEpisode(
        ByRef DBElement As Database.DBElement,
        ByVal ScrapeOptions As Structures.ScrapeOptions,
        ByVal showMessage As Boolean
    ) As Boolean

**Flow:**
1. Clone the episode element
2. Execute each enabled scraper via `Scraper_TVEpisode()`
3. Pass to subsequent scrapers:
   - UniqueIDs
   - Aired date
   - Episode number
   - Season number
   - Title
4. Merge results via `MergeDataScraperResults_TVEpisode_Single()`
5. Create actor thumb cache paths

### 6.3 Episode Matching During Show Scrape

When `ScrapeModifiers.withEpisodes = True`, episodes are populated from `KnownEpisodes`:

**Matching Logic:**
1. Match by Season + Episode number
2. If not found, match by Aired date (for date-based shows)

**Code:**

    For Each scrapedEpisode In scraper.Result.KnownEpisodes
        Dim match = DBElement.Episodes.FirstOrDefault(Function(e) 
            e.TVEpisode.Season = scrapedEpisode.Season AndAlso 
            e.TVEpisode.Episode = scrapedEpisode.Episode)
        If match IsNot Nothing Then
            ' Merge episode data
        End If
    Next

---

## Part 7: Image Scraping Process

### 7.1 ScrapeImage_TV Entry Point

**Location:** `clsAPIModules.vb`

    Function ScrapeImage_TV(
        ByRef DBElement As Database.DBElement,
        ByRef ImagesContainer As MediaContainers.SearchResultsContainer,
        ByVal ScrapeModifiers As Structures.ScrapeModifiers,
        ByVal showMessage As Boolean
    ) As Boolean

### 7.2 Image Type Cascading

TV image scraping has special logic to cascade requests between levels:

    ' AllSeasons images cascade to Main and Season levels
    If ScrapeModifiers.AllSeasonsBanner Then
        ScrapeModifiers.MainBanner = True
        ScrapeModifiers.SeasonBanner = True
    End If
    If ScrapeModifiers.AllSeasonsFanart Then
        ScrapeModifiers.MainFanart = True
        ScrapeModifiers.SeasonFanart = True
    End If
    If ScrapeModifiers.AllSeasonsLandscape Then
        ScrapeModifiers.MainLandscape = True
        ScrapeModifiers.SeasonLandscape = True
    End If
    If ScrapeModifiers.AllSeasonsPoster Then
        ScrapeModifiers.MainPoster = True
        ScrapeModifiers.SeasonPoster = True
    End If
    
    ' Episode and Season fanarts use Main fanarts as source
    If ScrapeModifiers.EpisodeFanart Then
        ScrapeModifiers.MainFanart = True
    End If
    If ScrapeModifiers.SeasonFanart Then
        ScrapeModifiers.MainFanart = True
    End If

### 7.3 TV Image Types by Level

| Level | Image Types |
|-------|-------------|
| **Show (Main)** | Banners, CharacterArts, ClearArts, ClearLogos, Fanarts, Keyarts, Landscapes, Posters, Extrafanarts |
| **All Seasons** | Banners, Fanarts, Landscapes, Posters |
| **Season** | Banners, Fanarts, Landscapes, Posters |
| **Episode** | Fanarts, Posters (Stills) |

### 7.4 TV Image Scrapers

| Scraper | Source | Capabilities |
|---------|--------|--------------|
| TVDB Image | TheTVDB API | All TV image types at all levels |
| TMDB Image | TMDB API | Show and season images |
| FanartTV | fanart.tv API | Extended artwork (ClearArt, ClearLogo, etc.) |

### 7.5 Image Download (During Save)

**Critical:** Like Movies, TV image downloads occur during save operations, not during scraping.

**Show Level:** `Save_TVShow()` → `SaveAllImages()`
**Season Level:** `Save_TVSeason()` → `SaveAllImages()`
**Episode Level:** `Save_TVEpisode()` → `SaveAllImages()`

Each level downloads images sequentially, multiplying the performance impact.

---

## Part 8: Theme Scraping

### 8.1 Entry Point

**Location:** `ModulesManager.Instance.ScrapeTheme_TV()`

    Function ScrapeTheme_TVShow(
        ByRef DBElement As Database.DBElement,
        ByVal Type As Enums.ModifierType,
        ByRef ThemeList As List(Of MediaContainers.MediaFile)
    ) As Boolean

### 8.2 Theme Scrapers

| Scraper | Source |
|---------|--------|
| TelevisionTunes | televisiontunes.com |
| YouTube Theme | YouTube search |

### 8.3 Flow

1. Get enabled theme scrapers
2. Each scraper searches for show theme music
3. Aggregate results
4. Select preferred or show dialog

---

## Part 9: Saving to Database

### 9.1 Save_TVShow Method

**Location:** `clsAPIDatabase.vb`

    Public Function Save_TVShow(
        ByVal dbElement As DBElement,
        ByVal batchMode As Boolean,
        ByVal toNFO As Boolean,
        ByVal toDisk As Boolean,
        ByVal doSync As Boolean,
        ByVal forceFileCleanup As Boolean
    ) As DBElement

**What Happens Inside:**

    If toDisk Then
        dbElement.ImagesContainer.SaveAllImages(dbElement, forceFileCleanup)  ' Show images
        dbElement.TVShow.SaveAllActorThumbs(dbElement)
        dbElement.Theme.Save(...)
    End If
    ' ... database write operations

### 9.2 Save_TVSeason Method

**Location:** `clsAPIDatabase.vb`

    Public Function Save_TVSeason(
        ByVal dbElement As DBElement,
        ByVal batchMode As Boolean,
        ByVal toNFO As Boolean,
        ByVal toDisk As Boolean,
        ByVal forceFileCleanup As Boolean
    ) As DBElement

**What Happens Inside:**

    If toDisk Then
        dbElement.ImagesContainer.SaveAllImages(dbElement, forceFileCleanup)  ' Season images
    End If
    ' ... database write operations

### 9.3 Save_TVEpisode Method

**Location:** `clsAPIDatabase.vb`

    Public Function Save_TVEpisode(
        ByVal dbElement As DBElement,
        ByVal batchMode As Boolean,
        ByVal toNFO As Boolean,
        ByVal toDisk As Boolean,
        ByVal doSync As Boolean,
        ByVal forceFileCleanup As Boolean
    ) As DBElement

**What Happens Inside:**

    If toDisk Then
        dbElement.ImagesContainer.SaveAllImages(dbElement, forceFileCleanup)  ' Episode images
        dbElement.TVEpisode.SaveAllActorThumbs(dbElement)
    End If
    ' ... database write operations

### 9.4 Database Save Points

| Location | File | When |
|----------|------|------|
| Scanner Load | `clsAPIScanner.vb` | After scanning TV files |
| Bulk Scrape (Show) | `frmMain.vb` (~1890) | After show scraping |
| Bulk Scrape (Season) | `frmMain.vb` | After season processing |
| Bulk Scrape (Episode) | `frmMain.vb` | After episode processing |
| Edit Dialog (Show) | `dlgEdit_TVShow.vb` | User saves show |
| Edit Dialog (Season) | `dlgEdit_TVSeason.vb` | User saves season |
| Edit Dialog (Episode) | `dlgEdit_TVEpisode.vb` | User saves episode |
| Trakt Sync | `clsAPITrakt.vb` | After sync |

### 9.5 Save Complexity Example

**Show:** "Breaking Bad" (5 seasons, 62 episodes)

| Operation | Count |
|-----------|-------|
| Show save | 1 |
| Season saves | 5 |
| Episode saves | 62 |
| **Total saves** | **68** |

Compare to Movie: 2 saves per movie

---

## Part 10: Control Structures

### 10.1 ScrapeType Enum

Same as Movies - see [ScrapingProcessMovies.md](ScrapingProcessMovies.md#81-scrapetype-enum).

### 10.2 ScrapeModifiers - TV Fields

    Public Structure ScrapeModifiers
        ' Show-level
        Dim DoSearch As Boolean
        Dim MainActorthumbs As Boolean
        Dim MainBanner As Boolean
        Dim MainCharacterArt As Boolean
        Dim MainClearArt As Boolean
        Dim MainClearLogo As Boolean
        Dim MainExtrafanarts As Boolean
        Dim MainFanart As Boolean
        Dim MainKeyart As Boolean
        Dim MainLandscape As Boolean
        Dim MainNFO As Boolean
        Dim MainPoster As Boolean
        Dim MainTheme As Boolean
        
        ' All Seasons level
        Dim AllSeasonsBanner As Boolean
        Dim AllSeasonsFanart As Boolean
        Dim AllSeasonsLandscape As Boolean
        Dim AllSeasonsPoster As Boolean
        
        ' Season level
        Dim SeasonBanner As Boolean
        Dim SeasonFanart As Boolean
        Dim SeasonLandscape As Boolean
        Dim SeasonNFO As Boolean
        Dim SeasonPoster As Boolean
        
        ' Episode level
        Dim EpisodeActorThumbs As Boolean
        Dim EpisodeFanart As Boolean
        Dim EpisodePoster As Boolean
        Dim EpisodeMeta As Boolean
        Dim EpisodeNFO As Boolean
        
        ' Control flags
        Dim withEpisodes As Boolean
        Dim withSeasons As Boolean
    End Structure

### 10.3 ScrapeOptions - TV Fields

    Public Structure ScrapeOptions
        ' Show-level
        Dim bMainActors As Boolean
        Dim bMainCertifications As Boolean
        Dim bMainCountries As Boolean
        Dim bMainCreators As Boolean
        Dim bMainEpisodeGuide As Boolean
        Dim bMainGenres As Boolean
        Dim bMainMPAA As Boolean
        Dim bMainOriginalTitle As Boolean
        Dim bMainPlot As Boolean
        Dim bMainPremiered As Boolean
        Dim bMainRating As Boolean
        Dim bMainRuntime As Boolean
        Dim bMainStatus As Boolean
        Dim bMainStudios As Boolean
        Dim bMainTagline As Boolean
        Dim bMainTitle As Boolean
        Dim bMainUserRating As Boolean
        
        ' Season-level
        Dim bSeasonAired As Boolean
        Dim bSeasonPlot As Boolean
        Dim bSeasonTitle As Boolean
        
        ' Episode-level
        Dim bEpisodeActors As Boolean
        Dim bEpisodeAired As Boolean
        Dim bEpisodeCredits As Boolean
        Dim bEpisodeDirectors As Boolean
        Dim bEpisodeGuestStars As Boolean
        Dim bEpisodeOriginalTitle As Boolean
        Dim bEpisodePlot As Boolean
        Dim bEpisodeRating As Boolean
        Dim bEpisodeRuntime As Boolean
        Dim bEpisodeTitle As Boolean
        Dim bEpisodeUserRating As Boolean
        Dim bEpisodeVideoSource As Boolean
    End Structure

---

## Part 11: Data Flow Diagrams

### 11.1 Complete TV Show Scrape Flow

    User initiates scrape
            │
            ▼
    CreateScrapeList_TVShow() → bwTVScraper.RunWorkerAsync()
            │
            ▼
    For each show in list:
            │
            ├──► Load_TVShow(withSeasons, withEpisodes) from database
            │
            ├──► ScrapeData_TVShow()
            │         │
            │         ├──► TVDB Scraper → Returns data + IDs + KnownEpisodes
            │         ├──► TMDB Scraper → Uses IDs, returns supplementary data
            │         ├──► IMDB Scraper → Uses IMDB ID, returns ratings
            │         └──► MergeDataScraperResults_TV()
            │                   │
            │                   └──► Match KnownEpisodes to local episodes
            │
            ├──► For each episode (if EpisodeMeta):
            │         └──► UpdateMediaInfo(episode)
            │
            ├──► ScrapeImage_TV()
            │         │
            │         ├──► TVDB Image Scraper → URLs for all levels
            │         ├──► TMDB Image Scraper → Show/season URLs
            │         ├──► FanartTV Scraper → Extended artwork URLs
            │         └──► SetPreferredImages() at each level
            │
            ├──► ScrapeTheme_TV() (if enabled)
            │
            └──► SAVE HIERARCHY ← *** DOWNLOADS HAPPEN HERE ***
                      │
                      ├──► Save_TVShow()
                      │         ├──► Write tvshow.nfo
                      │         ├──► SaveAllImages() (show)
                      │         └──► Save actor thumbs, theme
                      │
                      ├──► For each season:
                      │         └──► Save_TVSeason()
                      │                   ├──► Write season##.nfo
                      │                   └──► SaveAllImages() (season)
                      │
                      └──► For each episode:
                                └──► Save_TVEpisode()
                                          ├──► Write episode.nfo
                                          ├──► SaveAllImages() (episode)
                                          └──► Save actor thumbs

### 11.2 ID Propagation Flow

    TVDB Scraper runs first
            │
            ▼
    Retrieves TVDB ID and IMDB ID
            │
            ▼
    IDs added to oShow.TVShow.UniqueIDs
            │
            ▼
    TMDB Scraper runs second
            │
            ▼
    Uses TVDB/IMDB IDs to find TMDB ID
            │
            ▼
    All IDs now in UniqueIDs (TVDB, TMDB, IMDB)
            │
            ▼
    IMDB Scraper runs third
            │
            ▼
    Uses IMDB ID directly (no search)
            │
            ▼
    All results merged ("first wins")

---

## Part 12: Settings Reference

### 12.1 TV Scraper Enable/Order

Same pattern as Movies - scrapers enabled/disabled and ordered independently.

### 12.2 TV Field-Level Settings

| Pattern | Example | Effect |
|---------|---------|--------|
| `TVScraperShow*` | `TVScraperShowTitle` | Enable/disable show field |
| `TVScraperEpisode*` | `TVScraperEpisodeTitle` | Enable/disable episode field |
| `TVLockShow*` | `TVLockShowTitle` | Protect show field |
| `TVLockEpisode*` | `TVLockEpisodeTitle` | Protect episode field |
| `TVScraperShow*Limit` | `TVScraperShowGenreLimit` | Limit count |

### 12.3 TV-Specific Processing Settings

| Setting | Effect |
|---------|--------|
| `TVScraperCleanFields` | Clear fields when scraping disabled |
| `TVScraperShowCertLang` | Certification language filter |
| `TVScraperCastWithImgOnly` | Only actors with images |
| `TVScraperShowOriginalTitleAsTitle` | Copy OriginalTitle to Title |
| `TVScraperEpisodeGuestStarsToActors` | Include guest stars in actors |
| `TVScraperUseMDDuration` | Use MediaInfo duration |

---

## Part 13: Performance Analysis

### 13.1 TV vs Movie Complexity

| Aspect | Movie | TV Show (5 seasons, 100 episodes) |
|--------|-------|-----------------------------------|
| Data scrape calls | 1 | 1 (show) + up to 100 (episodes) |
| Database saves | ~2 | ~106 (1 show + 5 seasons + 100 episodes) |
| Image downloads | 6-10 | 6-10 (show) + 3-4×5 (seasons) + 1-2×100 (episodes) |
| NFO files written | 1 | 106 |

### 13.2 Performance Multiplier

TV scraping has a **multiplicative effect** on operations:

    Total Operations = Show Operations + (Seasons × Season Operations) + (Episodes × Episode Operations)

For a show with 5 seasons and 100 episodes:
- **Image downloads:** ~230 (show: 10, seasons: 5×4=20, episodes: 100×2=200)
- **Database writes:** ~106
- **NFO writes:** ~106

### 13.3 Estimated Time (Large Show)

| Phase | Time Estimate |
|-------|---------------|
| Show data scrape | 2-5 sec |
| Episode data merge | 1-2 sec |
| Image URL collection | 3-5 sec |
| Image downloads (show) | 1-2 sec |
| Image downloads (seasons) | 2-4 sec |
| Image downloads (episodes) | 20-40 sec |
| Database writes | 10-20 sec |
| **Total** | **40-80 sec** |

### 13.4 Optimization Opportunities

| Area | Current | Potential |
|------|---------|-----------|
| Episode image downloads | Sequential | Parallel (like Movies) |
| Season image downloads | Sequential | Parallel |
| Database batch mode | Individual saves | Transaction batching |
| Episode matching | Linear search | Indexed lookup |

---

## Part 14: Debugging Guide

### 14.1 Recommended Breakpoints

| Priority | Line | File | Purpose |
|----------|------|------|---------|
| 1 | ~13100 | `frmMain.vb` | Before `RunWorkerAsync` - see scrape list |
| 2 | ~1720 | `frmMain.vb` | Start of each show iteration |
| 3 | ~1740 | `frmMain.vb` | Before `ScrapeData_TVShow` |
| 4 | ~1805 | `frmMain.vb` | Before `ScrapeImage_TV` |
| **5** | **~1890** | **`frmMain.vb`** | **Before `Save_TVShow` - KEY LINE** |
| 6 | - | `clsAPIDatabase.vb` | Inside `Save_TVShow` |
| 7 | - | `clsAPIDatabase.vb` | Inside `Save_TVEpisode` |

### 14.2 Debug Session Setup

1. Open `frmMain.vb` in Visual Studio
2. Set breakpoints at lines ~1720 and ~1890
3. Run debug (F5)
4. Select 1 TV show with few episodes (for quick testing)
5. Right-click → "(Re)Scrape Selected TV Shows" → "Ask" → "All Items"
6. Step through to observe flow

### 14.3 NLog Tracing

Key log messages:

    [TV Scraper] [Start] Shows Count [X]
    [TV Scraper] [Start] Scraping {ShowTitle}
    [TV Scraper] [Done] Scraping {ShowTitle}
    [TV Scraper] [Start] Saving Episodes for {ShowTitle}

Enable trace logging in `NLog.config` for detailed flow.

### 14.4 Performance Metrics

Metrics logged on shutdown:
- `SaveAllImages.TVShow.Total`
- `SaveAllImages.TVSeason.Total`
- `SaveAllImages.TVEpisode.Total`
- `TVDB.GetInfo_TVShow`
- `TMDB.GetInfo_TVShow`

### 14.5 Cancel Scraper Mechanism

The TV Show scraper uses the same `BackgroundWorker.CancelAsync()` pattern as Movies, with `bwTVScraper` instead of `bwMovieScraper`.

See [ScrapingProcessMovies.md - Section 12.5](ScrapingProcessMovies.md#125-cancel-scraper-mechanism) for the complete cancel mechanism documentation, including:
- Cancel method locations
- Implementation pattern
- DoWork cancellation check
- Async cancellation integration

---

## Part 15: Differences from Movie Scraping

### 15.1 Key Differences

| Aspect | Movies | TV Shows |
|--------|--------|----------|
| Content Structure | Single unit | Hierarchical (Show/Season/Episode) |
| Scrape Methods | 1 (`Scraper_Movie`) | 3 (`Scraper_TVShow`, `_TVSeason`, `_TVEpisode`) |
| Trailer Scrapers | Yes | No |
| Theme Scrapers | Yes | Yes |
| Image Levels | 1 | 4 (Show, AllSeasons, Season, Episode) |
| Episode Guide | N/A | Yes (`EpisodeGuide` field) |
| Known Episodes | N/A | Yes (batch episode matching) |
| Status Field | N/A | Yes (Continuing, Ended, etc.) |
| Creators Field | N/A | Yes |
| Database Saves | ~2 per movie | N+M+1 (show + seasons + episodes) |

### 15.2 Shared Components

| Component | Shared |
|-----------|--------|
| `ModulesManager` | Yes |
| Module loading | Yes |
| `ScrapeType` enum | Yes |
| `ScrapeModifiers` | Yes (contains fields for both) |
| `ScrapeOptions` | Yes (contains fields for both) |
| "First wins" merge logic | Yes |
| Lock field protection | Yes |
| `SaveAllImages()` | Yes |

### 15.3 Performance Impact

The hierarchical structure means TV scraping is inherently more expensive:

| Metric | Movie (1 item) | TV Show (100 episodes) |
|--------|----------------|------------------------|
| API calls | 2-3 | 3-5 (show level) |
| Image downloads | 6-10 | 200+ |
| Database saves | 2 | 106 |
| NFO writes | 1 | 106 |
| Total time | 4-5 sec | 40-80 sec |

---

## Summary

The Ember Media Manager TV Show scraping system:

1. **Entry Points:** Context menu, Tools menu, command line (same as Movies)
2. **Orchestration:** `CreateScrapeList_TVShow()` → `bwTVScraper_DoWork()`
3. **Hierarchical Content:** Show → Season → Episode (each with own metadata/images)
4. **Data Scraping:** Multiple scrapers (TVDB, TMDB, IMDB) run sequentially
5. **Episode Matching:** `KnownEpisodes` matched to local files
6. **Image Cascading:** AllSeasons → Main/Season; Episode/Season → Main fanart
7. **Saving:** Hierarchical saves: `Save_TVShow()` → `Save_TVSeason()` → `Save_TVEpisode()`
8. **Image Downloads:** Occur during save at each level (sequential)

**Critical Performance Path:**

    Show scrape (network) → Episode matching (CPU) → Image URL collection (network) →
    **Hierarchical saves with image downloads (BOTTLENECK)** → Database writes

**Key Code Locations:**

| Operation | File | Line |
|-----------|------|------|
| Start scraping | `frmMain.vb` | ~13100 |
| Main loop | `frmMain.vb` | ~1720 |
| Save show | `frmMain.vb` | ~1890 |
| Save season | `clsAPIDatabase.vb` | - |
| Save episode | `clsAPIDatabase.vb` | - |

**Performance Note:** TV scraping generates significantly more operations than Movies. A show with 100 episodes can trigger 200+ image downloads and 100+ database saves. Parallel image downloads would provide even greater benefit for TV than Movies.