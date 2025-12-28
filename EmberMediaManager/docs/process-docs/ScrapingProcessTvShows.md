# TV Show Scraping Process Analysis

> **Related Documentation:** This document covers TV Show scraping. For Movie scraping, see [ScrapingProcessMovies.md](ScrapingProcessMovies.md). Both processes share the same architectural patterns through the `ModulesManager` class.

---

## Overview

This document provides a comprehensive analysis of how Ember Media Manager scrapes TV Show metadata from external data sources. The TV scraping system uses the same addon-based modular architecture as Movies, but handles a hierarchical content structure: TV Shows contain Seasons, which contain Episodes.

> **Note:** TV scraping follows similar patterns to Movie scraping but uses different interfaces and methods. This document highlights TV-specific behavior and differences.

---

## Part 1: TV Scraper Architecture

### 1.1 Core Files Involved

| File | Purpose |
|------|---------|
| `EmberAPI\clsAPIModules.vb` | Module manager that loads, orders, and executes scrapers |
| `EmberAPI\clsAPINFO.vb` | Merges results from multiple scrapers |
| `EmberAPI\clsAPIInterfaces.vb` | Defines TV scraper interfaces and result structures |
| `EmberAPI\clsAPICommon.vb` | Contains ScrapeModifiers, ScrapeOptions, and ScrapeType enums |
| `Addons\scraper.Data.TVDB\*` | TVDB data scraper implementation |
| `Addons\scraper.TMDB.Data\*` | TMDB TV data scraper implementation |
| `Addons\scraper.IMDB.Data\*` | IMDB TV data scraper implementation |

### 1.2 TV Scraper Types

Ember supports three types of TV scrapers:

| Scraper Type | Interface | Purpose |
|--------------|-----------|---------|
| Data Scraper | `ScraperModule_Data_TV` | Retrieves metadata for shows, seasons, and episodes |
| Image Scraper | `ScraperModule_Image_TV` | Retrieves artwork at show, season, and episode levels |
| Theme Scraper | `ScraperModule_Theme_TV` | Retrieves theme music for shows |

**Difference from Movies:** TV does not have a dedicated Trailer scraper interface.

### 1.3 TV Content Hierarchy

Unlike Movies (single content unit), TV content is hierarchical:

    TV Show
        |
        +-- Season 1
        |       +-- Episode 1
        |       +-- Episode 2
        |       +-- ...
        |
        +-- Season 2
        |       +-- Episode 1
        |       +-- ...
        |
        +-- ...

Each level has its own:
- Metadata fields
- Images (posters, fanarts, banners)
- Scraping methods

### 1.4 Module Loading

TV scrapers are loaded identically to Movie scrapers via `ModulesManager.LoadModules()`:

1. Scans the Addons directory for assemblies
2. Uses reflection to find types implementing `ScraperModule_Data_TV`
3. Creates instances and stores in `externalScrapersModules_Data_TV`
4. Loads enabled state and order from settings

---

## Part 2: TV Show Data Scraping

### 2.1 Entry Point

TV Show scraping is initiated by calling `ModulesManager.Instance.ScrapeData_TVShow()`.

**Method Signature:**

    Function ScrapeData_TVShow(
        ByRef DBElement As Database.DBElement,
        ByRef ScrapeModifiers As Structures.ScrapeModifiers,
        ByVal ScrapeType As Enums.ScrapeType,
        ByVal ScrapeOptions As Structures.ScrapeOptions,
        ByVal showMessage As Boolean
    ) As Boolean

### 2.2 ScrapeData_TVShow Flow

**Step 1: Validate Online Status**

    If DBElement.IsOnline OrElse FileUtils.Common.CheckOnlineStatus_TVShow(DBElement, showMessage) Then
        ' Continue with scraping
    Else
        Return True ' Cancelled - show offline
    End If

**Step 2: Get Enabled Scrapers in Order**

    Dim modules As IEnumerable(Of _externalScraperModuleClass_Data_TV) = 
        externalScrapersModules_Data_TV.Where(Function(e) e.ProcessorModule.ScraperEnabled).OrderBy(Function(e) e.ModuleOrder)

**Step 3: Clean TV Show Data (for New Scrapes)**

If `ScrapeType` is `SingleScrape` or `SingleAuto` AND `ScrapeModifiers.DoSearch` is True:

    DBElement.ExtrafanartsPath = String.Empty
    DBElement.ImagesContainer = New MediaContainers.ImagesContainer
    DBElement.NfoPath = String.Empty
    DBElement.Seasons.Clear()
    DBElement.Theme = New MediaContainers.MediaFile
    DBElement.TVShow = New MediaContainers.TVShow With {
        .Title = StringUtils.FilterTitleFromPath_TVShow(DBElement.ShowPath)
    }
    
    ' Also clean all episodes
    For Each sEpisode As Database.DBElement In DBElement.Episodes
        ' Reset episode data while preserving Season/Episode numbers and Aired date
    Next

**Step 4: Clone for Scraping**

    Dim oShow As Database.DBElement = CType(DBElement.CloneDeep, Database.DBElement)

**Step 5: Execute Each Scraper in Order**

    For Each _externalScraperModule As _externalScraperModuleClass_Data_TV In modules
        ret = _externalScraperModule.ProcessorModule.Scraper_TVShow(oShow, ScrapeModifiers, ScrapeType, ScrapeOptions)
        
        If ret.Cancelled Then Return ret.Cancelled
        
        If ret.Result IsNot Nothing Then
            ScrapedList.Add(ret.Result)
            
            ' Pass IDs to subsequent scrapers
            If ret.Result.UniqueIDsSpecified Then
                oShow.TVShow.UniqueIDs.AddRange(ret.Result.UniqueIDs)
            End If
            If ret.Result.OriginalTitleSpecified Then
                oShow.TVShow.OriginalTitle = ret.Result.OriginalTitle
            End If
            If ret.Result.TitleSpecified Then
                oShow.TVShow.Title = ret.Result.Title
            End If
        End If
        
        If ret.breakChain Then Exit For
    Next

**Step 6: Merge All Results**

    DBElement = NFO.MergeDataScraperResults_TV(DBElement, ScrapedList, ScrapeType, ScrapeOptions, ScrapeModifiers.withEpisodes)

**Step 7: Create Actor Thumb Cache Paths**

    DBElement.TVShow.CreateCachePaths_ActorsThumbs()
    If ScrapeModifiers.withEpisodes Then
        For Each tEpisode As Database.DBElement In DBElement.Episodes
            tEpisode.TVEpisode.CreateCachePaths_ActorsThumbs()
        Next
    End If

---

## Part 3: Season and Episode Scraping

### 3.1 Season Scraping

**Entry Point:** `ModulesManager.Instance.ScrapeData_TVSeason()`

**Method Signature:**

    Function ScrapeData_TVSeason(
        ByRef DBElement As Database.DBElement,
        ByVal ScrapeOptions As Structures.ScrapeOptions,
        ByVal showMessage As Boolean
    ) As Boolean

**Note:** Season scraping does NOT use `ScrapeModifiers` or `ScrapeType` - it only accepts `ScrapeOptions`.

**Flow:**
1. Clone the season element
2. Execute each enabled scraper via `Scraper_TVSeason()`
3. Pass UniqueIDs to subsequent scrapers
4. Merge results via `MergeDataScraperResults_TVSeason()`

### 3.2 Episode Scraping

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

---

## Part 4: TV Data Scrapers

### 4.1 TVDB Scraper (`scraper.Data.TVDB`)

**Primary TV scraper** - uses TheTVDB.com API.

**Location:** `Addons\scraper.Data.TVDB\TVDB_Data.vb`

**ID Resolution Order:**
1. If TVDB ID available, scrape directly
2. If IMDB ID available, search TVDB
3. If neither, search by Title

**Methods Implemented:**
- `Scraper_TVShow()` - Full show metadata
- `Scraper_TVSeason()` - Season-specific data
- `Scraper_TVEpisode()` - Episode-specific data

**Data Retrieved (Show Level):**
- Title, OriginalTitle, Premiered, Status
- Plot, Runtime, MPAA
- Genres, Studios (Networks), Countries
- Cast (Actors with images)
- Creators
- Ratings
- UniqueIDs (TVDB, IMDB)
- Episode Guide URL
- Known Episodes and Seasons lists

### 4.2 TMDB TV Scraper (`scraper.TMDB.Data`)

**Location:** `Addons\scraper.TMDB.Data\TMDB_Data.vb`

**ID Resolution Order:**
1. If TMDB ID available, scrape directly
2. If TVDB ID available, find on TMDB
3. If IMDB ID available, find on TMDB
4. If neither, search by Title

**Methods Implemented:**
- `Scraper_TVShow()` - Full show metadata
- `Scraper_TVSeason()` - Season-specific data
- `Scraper_TVEpisode()` - Episode-specific data

### 4.3 IMDB TV Scraper (`scraper.IMDB.Data`)

**Location:** `Addons\scraper.IMDB.Data\IMDB_Data.vb`

**ID Resolution:** Uses IMDB ID to scrape show and episode data.

**Methods Implemented:**
- `Scraper_TVShow()` - Show metadata
- `Scraper_TVEpisode()` - Episode metadata

### 4.4 Scraper Chain Behavior

When multiple TV scrapers are enabled (e.g., TVDB + TMDB + IMDB):

1. **TVDB runs first** (typically order 0)
2. TVDB retrieves TVDB ID and often IMDB ID
3. IDs passed to TMDB scraper via `oShow.TVShow.UniqueIDs`
4. **TMDB runs second** - uses TVDB/IMDB IDs, retrieves TMDB ID
5. All IDs passed to IMDB scraper
6. **IMDB runs third** - supplementary data
7. Results merged with "first wins" logic

---

## Part 5: TV Image Scraping

### 5.1 Entry Point

Image scraping is initiated by calling `ModulesManager.Instance.ScrapeImage_TV()`.

**Method Signature:**

    Function ScrapeImage_TV(
        ByRef DBElement As Database.DBElement,
        ByRef ImagesContainer As MediaContainers.SearchResultsContainer,
        ByVal ScrapeModifiers As Structures.ScrapeModifiers,
        ByVal showMessage As Boolean
    ) As Boolean

### 5.2 TV-Specific Image Handling

TV image scraping has special logic to handle the content hierarchy:

**Image Type Cascading:**

    ' AllSeasons images cascade to Main and Season levels
    If ScrapeModifiers.AllSeasonsBanner Then
        ScrapeModifiers.MainBanner = True
        ScrapeModifiers.SeasonBanner = True
    End If
    If ScrapeModifiers.AllSeasonsFanart Then
        ScrapeModifiers.MainFanart = True
        ScrapeModifiers.SeasonFanart = True
    End If
    ' ... similar for Landscape, Poster
    
    ' Episode and Season fanarts use Main fanarts
    If ScrapeModifiers.EpisodeFanart Then
        ScrapeModifiers.MainFanart = True
    End If
    If ScrapeModifiers.SeasonFanart Then
        ScrapeModifiers.MainFanart = True
    End If

### 5.3 TV Image Types Collected

| Level | Image Types |
|-------|-------------|
| Show (Main) | Banners, CharacterArts, ClearArts, ClearLogos, Fanarts, Keyarts, Landscapes, Posters |
| Season | Banners, Fanarts, Landscapes, Posters |
| Episode | Fanarts, Posters (Stills) |
| AllSeasons | Banners, Fanarts, Landscapes, Posters |

### 5.4 TV Image Scrapers

| Scraper | Source | Capabilities |
|---------|--------|--------------|
| TVDB Image | TheTVDB API | All TV image types |
| TMDB Image | TMDB API | Show and Season images |
| FanartTV | fanart.tv API | Extended artwork types |

---

## Part 6: TV Theme Scraping

### 6.1 Entry Point

Theme scraping is initiated by calling `ModulesManager.Instance.ScrapeTheme_TV()`.

### 6.2 Theme Scrapers

| Scraper | Source |
|---------|--------|
| TelevisionTunes | televisiontunes.com |
| YouTube Theme | YouTube search |

---

## Part 7: Result Merging for TV

### 7.1 TV Show Merge

The `MergeDataScraperResults_TV` method handles show-level merging with these protected fields:

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

### 7.2 Episode Merge via Show Scrape

When `ScrapeModifiers.withEpisodes` is True, the show scrape also populates episode data using `KnownEpisodes` from scrapers. Episodes are matched by:
- Season number + Episode number, OR
- Aired date (for date-based episode matching)

### 7.3 Single Episode Merge

`MergeDataScraperResults_TVEpisode_Single` handles standalone episode scraping with fields:
- Actors, Aired, Credits, Directors
- GuestStars, OriginalTitle, Plot
- Rating, Runtime, Title, UserRating

---

## Part 8: TV-Specific Control Structures

### 8.1 ScrapeModifiers - TV Fields

    Public Structure ScrapeModifiers
        ' Show-level
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
        
        ' Season-level
        Dim AllSeasonsBanner As Boolean
        Dim AllSeasonsFanart As Boolean
        Dim AllSeasonsLandscape As Boolean
        Dim AllSeasonsPoster As Boolean
        Dim SeasonBanner As Boolean
        Dim SeasonFanart As Boolean
        Dim SeasonLandscape As Boolean
        Dim SeasonNFO As Boolean
        Dim SeasonPoster As Boolean
        
        ' Episode-level
        Dim EpisodeActorThumbs As Boolean
        Dim EpisodeFanart As Boolean
        Dim EpisodePoster As Boolean
        Dim EpisodeMeta As Boolean
        Dim EpisodeNFO As Boolean
        
        ' Control flags
        Dim withEpisodes As Boolean
        Dim withSeasons As Boolean
    End Structure

### 8.2 ScrapeOptions - TV Fields

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

## Part 9: TV Data Flow Diagrams

### 9.1 Complete TV Show Scrape Flow

    User initiates TV Show scrape
            |
            v
    ScrapeData_TVShow()
            |
            v
    Get enabled Data scrapers in order
            |
            v
    For each scraper (TVDB, TMDB, IMDB):
            |
            v
    Call Scraper_TVShow()
            |
            v
    Add result to ScrapedList
            |
            v
    Pass IDs to next scraper
            |
            v
    MergeDataScraperResults_TV()
            |
            v
    If withEpisodes:
            |
            +---> Merge episode data from KnownEpisodes
            |
            v
    ScrapeImage_TV()
            |
            v
    ScrapeTheme_TV() (if requested)
            |
            v
    Save_TVShow() to database
            |
            v
    For each episode:
            +---> Save_TVEpisode()

### 9.2 ID Propagation Flow

    TVDB Scraper runs first
            |
            v
    Retrieves TVDB ID and IMDB ID
            |
            v
    IDs added to oShow.TVShow.UniqueIDs
            |
            v
    TMDB Scraper runs second
            |
            v
    Uses TVDB/IMDB IDs to find TMDB ID
            |
            v
    All IDs in UniqueIDs
            |
            v
    IMDB Scraper runs third
            |
            v
    Uses IMDB ID (no search needed)
            |
            v
    All results merged

---

## Part 10: Database Save Points for TV

### 10.1 TV Save Methods

| Location | File | When Called |
|----------|------|-------------|
| Scanner Load | `clsAPIScanner.vb` | After loading/scanning TV show files |
| Task Manager | `clsAPITaskManager.vb` | During bulk edit operations |
| Image Save | `clsAPIImages.vb` | After saving TV images |
| Edit Dialog (Season) | `dlgEdit_TVSeason.vb` | When user saves season edits |
| Edit Dialog (Show) | `dlgEdit_TVShow.vb` | When user saves show edits |
| Edit Dialog (Episode) | `dlgEdit_TVEpisode.vb` | When user saves episode edits |
| NFO Operations | `clsAPINFO.vb` | During NFO save operations |
| TVDB Scraper | `TVDB_Data.vb` | After TVDB scraper completes |
| Trakt Sync | `clsAPITrakt.vb` | After Trakt synchronization |

### 10.2 TV Save Complexity

TV saving is more complex than Movies due to the hierarchy:

1. **Show save** - `Save_TVShow()` saves show metadata
2. **Season saves** - Each season may trigger `Save_TVSeason()`
3. **Episode saves** - Each episode triggers `Save_TVEpisode()`

A show with 5 seasons and 100 episodes could trigger 106+ database saves during a full scrape.

---

## Part 11: Differences from Movie Scraping

### 11.1 Key Differences

| Aspect | Movies | TV Shows |
|--------|--------|----------|
| Content Structure | Single unit | Hierarchical (Show/Season/Episode) |
| Scrape Methods | 1 (`Scraper_Movie`) | 3 (`Scraper_TVShow`, `_TVSeason`, `_TVEpisode`) |
| Trailer Scrapers | Yes | No |
| Theme Scrapers | Yes | Yes |
| Image Levels | 1 (Movie) | 3 (Show, Season, Episode) |
| Episode Guide | N/A | Yes (`EpisodeGuide` field) |
| Known Episodes | N/A | Yes (for batch episode matching) |
| Status Field | N/A | Yes (Continuing, Ended, etc.) |
| Creators Field | N/A | Yes |

### 11.2 Shared Components

| Component | Shared |
|-----------|--------|
| `ModulesManager` | Yes |
| Module loading | Yes |
| `ScrapeType` enum | Yes |
| `ScrapeModifiers` structure | Yes (contains fields for both) |
| `ScrapeOptions` structure | Yes (contains fields for both) |
| "First wins" merge logic | Yes |
| Lock field protection | Yes |

---

## Part 12: Settings That Affect TV Scraping

### 12.1 TV Scraper Enable/Order

Same pattern as Movies - scrapers can be enabled/disabled and ordered independently.

### 12.2 TV Field-Level Settings

| Setting Pattern | Example | Effect |
|-----------------|---------|--------|
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
| `TVScraperEpisodeGuestStarsToActors` | Include guest stars in actors list |
| `TVScraperUseMDDuration` | Use MediaInfo duration |

---

## Summary

The TV Show scraping system in Ember Media Manager:

1. **Uses the same architectural patterns** as Movie scraping
2. **Handles hierarchical content** (Show → Season → Episode)
3. **Provides three scraper methods** per data scraper interface
4. **Supports multiple scrapers** running in sequence (TVDB, TMDB, IMDB)
5. **Merges results** using "first wins" logic with lock protection
6. **Cascades image requests** between content levels
7. **Generates more database saves** due to multi-level structure
8. **Shares control structures** with Movie scraping

The key insight for TV performance is that the hierarchical structure multiplies the number of operations compared to Movies - a show with many episodes will trigger many more API calls and database saves.