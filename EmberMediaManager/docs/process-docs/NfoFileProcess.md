# Ember Media Manager - NFO and Media File Processing

| | Document Info |
|---------------|---|
| **Version** | 2.0 |
| **Created** | December 2025 |
| **Updated** | January 7, 2026 |
| **Author** | Eric H. Anderson |
| **Purpose** | Comprehensive reference for NFO file processing and media file naming conventions |

##### [← Return to Document Index](../DocumentIndex.md) · [→ NFO/File Naming Reference](../reference-docs/NfoFileReference.md)

---

## Overview

Need to understand how Ember Media Manager handles NFO files and media file naming? You're in the right place!

This document provides a comprehensive analysis of:
- How NFO files are read and written
- XML structure for all media types
- File naming conventions for NFO, images, and other media files
- Settings that control file output

**Quick links:**
- [Reading NFO Files](#part-1-reading-nfo-files) — How NFO files are loaded
- [Writing NFO Files](#part-2-creating-and-updating-nfo-files) — How NFO files are saved
- [NFO XML Structure](../reference-docs/NfoFileReference.md#part-1-nfo-xml-structure) — Complete XML element reference
- [File Naming Conventions](../reference-docs/NfoFileReference.md#part-2-file-naming-conventions) — All file types and naming patterns

> **Note:** This documentation describes the code structure and flow. Specific line numbers are not included as they change frequently during development.

---

## Table of Contents

- [Part 1: Reading NFO Files](#part-1-reading-nfo-files)
  - [Core Files Involved](#core-files-involved)
  - [Entry Points for Reading NFO Files](#entry-points-for-reading-nfo-files)
  - [NFO Loading Methods](#nfo-loading-methods)
  - [NFO Validation Methods](#nfo-validation-methods)
  - [NFO Cleaning and Normalization](#nfo-cleaning-and-normalization)
  - [Handling Non-Conforming NFO Files](#handling-non-conforming-nfo-files)
- [Part 2: Creating and Updating NFO Files](#part-2-creating-and-updating-nfo-files)
  - [Core Save Methods](#core-save-methods)
  - [NFO Deletion Methods](#nfo-deletion-methods)
- [Part 3: Data Flow Diagrams](#part-3-data-flow-diagrams)
- [Part 4: Settings Reference](#part-4-settings-reference)
- [Part 5: Module Events](#part-5-module-events)
- [See Also](#see-also)

---

## [↑](#table-of-contents) Part 1: Reading NFO Files

### [↑](#table-of-contents) Core Files Involved

| File | Purpose |
|------|---------|
| [clsAPINFO.vb](../../EmberAPI/clsAPINFO.vb) | Main NFO processing class containing all load/save methods |
| [clsAPIScanner.vb](../../EmberAPI/clsAPIScanner.vb) | Scanner that initiates NFO loading during media discovery |
| [clsAPIMediaContainers.vb](../../EmberAPI/clsAPIMediaContainers.vb) | Data container classes with XML serialization attributes |
| [clsAPINFOSerializers.vb](../../EmberAPI/clsAPINFOSerializers.vb) | Cached XmlSerializer instances for performance |

---

### [↑](#table-of-contents) Entry Points for Reading NFO Files

The scanner class (`clsAPIScanner.vb`) initiates NFO reading when loading media.

#### [↑](#table-of-contents) Movie Loading - Load_Movie()

**Primary Flow:**

1. Calls `GetFolderContents_Movie()` to discover related files
2. If `DBMovie.NfoPathSpecified` is true, calls `NFO.LoadFromNFO_Movie()`
3. Otherwise attempts to load from the movie filename path

**Additional Processing After NFO Load:**

| Processing Step | Condition | Action |
|-----------------|-----------|--------|
| MediaInfo Scan | Title exists but no FileInfo | Runs MediaInfo scan (if `MovieScraperMetaDataScan` enabled) |
| Year Extraction | Year not in NFO | Extracts from file path using `StringUtils.FilterYearFromPath_Movie()` |
| IMDB ID Extraction | Not in NFO | Extracts from filename using `StringUtils.GetIMDBIDFromString()` |
| Title Extraction | No title (invalid NFO) | Clears NFO path and extracts title from path |
| Actor Thumb Matching | Always | Matches local actor thumb files to actors in NFO |
| Edition Detection | Always | Runs regex mapping to detect edition from filename |

---

#### [↑](#table-of-contents) MovieSet Loading - Load_MovieSet()

**Primary Flow:**

1. Calls `GetFolderContents_MovieSet()` to discover related files
2. Calls `NFO.GetNfoPath_MovieSet()` to find NFO path if not already set
3. Calls `NFO.LoadFromNFO_MovieSet()` to load content
4. Extracts Language and Lock state from NFO

---

#### [↑](#table-of-contents) TV Episode Loading - Load_TVEpisode()

**Primary Flow:**

1. Creates list of existing episode information for the file path (for multi-episode handling)
2. Calls `GetFolderContents_TVEpisode()` to discover related files
3. For each episode detected by `RegexGetTVEpisode()`:
   - If NFO exists, calls `NFO.LoadFromNFO_TVEpisode()` with either:
     - Season/Episode number, OR
     - Season/Aired date (for date-based episodes)
   - If new episode with show IDs, attempts to scrape episode data
4. Scrapes episode images if new and show has IDs
5. Matches actor thumbs to actors in NFO

---

#### [↑](#table-of-contents) TV Show Loading - Load_TVShow()

**Primary Flow:**

1. Calls `GetFolderContents_TVShow()` to discover related files
2. Calls `NFO.LoadFromNFO_TVShow()` to load content
3. Fires `OnNFORead_TVShow` module event for extensibility

---

### [↑](#table-of-contents) NFO Loading Methods

#### [↑](#table-of-contents) Loading Movie NFO Files

**Method:** `LoadFromNFO_Movie(sPath, isSingle)`

**Process:**

1. Uses cached `XmlSerializer` from `NFOSerializers` class
2. If file exists and has `.nfo` extension:
   - Deserializes using `StreamReader`
   - Calls `CleanNFO_Movies()` to normalize the data
3. For non-conforming NFOs:
   - Attempts to extract IMDB ID using `GetIMDBFromNonConf()`
   - Tries to parse partial NFO content if `</movie>` tag found
   - Optionally renames to `.info` extension (if `GeneralOverwriteNfo` is false)

**Key Code Pattern:**

    DirectCast(NFOSerializers.MovieSerializer.Deserialize(xmlSR), MediaContainers.Movie)

---

#### [↑](#table-of-contents) Loading MovieSet NFO Files

**Method:** `LoadFromNFO_MovieSet(sPath)`

**Process:**

1. Uses cached `XmlSerializer` for `MediaContainers.Movieset` type
2. Deserializes the file using `StreamReader`
3. Normalizes line endings in Plot field

---

#### [↑](#table-of-contents) Loading TV Episode NFO Files

**Method:** `LoadFromNFO_TVEpisode(sPath, SeasonNumber, EpisodeNumber)`

**Process:**

1. Handles multi-episode NFO files using regex pattern matching
2. Pattern used: `<episodedetails.*?>.*?</episodedetails>`
3. If single match found, deserializes directly
4. If multiple matches, iterates to find matching season/episode number
5. Calls `CleanNFO_TVEpisodes()` for data normalization
6. Updates language information for audio/subtitle streams

**Alternative Method:** `LoadFromNFO_TVEpisode(sPath, SeasonNumber, Aired)` — Loads by aired date instead of episode number for date-based episode matching.

---

#### [↑](#table-of-contents) Loading TV Show NFO Files

**Method:** `LoadFromNFO_TVShow(sPath)`

**Process:**

1. Uses cached `XmlSerializer` for `MediaContainers.TVShow` type
2. Deserializes using `StreamReader`
3. Calls `CleanNFO_TVShow()` for normalization
4. Handles non-conforming NFOs by renaming to `.info`
5. **Fires `OnNFORead_TVShow` module event** for addon extensibility

---

### [↑](#table-of-contents) NFO Validation Methods

**Conformance Checking Methods:**

| Method | Purpose |
|--------|---------|
| `IsConformingNFO_Movie(sPath)` | Attempts deserialization, returns True if successful |
| `IsConformingNFO_TVEpisode(sPath)` | Handles multi-episode validation |
| `IsConformingNFO_TVShow(sPath)` | Attempts deserialization, returns True if successful |

These methods attempt to deserialize the file and return `True` if successful, `False` if an exception occurs.

---

### [↑](#table-of-contents) NFO Cleaning and Normalization

#### [↑](#table-of-contents) CleanNFO_Movies()

| Action | Description |
|--------|-------------|
| Line ending normalization | Uses `NormalizeLineEndings()` on Outline and Plot fields |
| Date conversion | Converts Premiered date to ISO8601 format |
| Vote formatting | Cleans vote count using `NumUtils.CleanVotes()` |
| Language names | Adds long language names for audio/subtitle streams |
| Set cleanup | Removes sets without titles |
| Language codes | Converts language names to Alpha2 codes |

---

#### [↑](#table-of-contents) CleanNFO_TVEpisodes()

| Action | Description |
|--------|-------------|
| Date conversion | Normalizes Aired date to ISO8601 format |
| Vote formatting | Cleans vote formatting |
| Language names | Adds long language names for audio/subtitle streams |

---

#### [↑](#table-of-contents) CleanNFO_TVShow()

| Action | Description |
|--------|-------------|
| Line ending normalization | Normalizes Plot line endings |
| Date conversion | Converts Premiered to ISO8601 |
| Vote formatting | Cleans vote formatting |
| Language codes | Converts language to Alpha2 codes |

---

### [↑](#table-of-contents) Handling Non-Conforming NFO Files

#### [↑](#table-of-contents) GetIMDBFromNonConf Method

- Searches for `.nfo` and `.info` files in the directory
- Uses regex pattern `tt\d\d\d\d\d\d\d*` to extract IMDB ID
- Attempts to parse partial NFO content if `</movie>` tag is found
- For non-single movies, searches with filename pattern matching

#### [↑](#table-of-contents) Renaming Methods

| Method | Purpose |
|--------|---------|
| `RenameNonConfNFO_Movie(sPath, isChecked)` | Renames movie NFO to .info |
| `RenameNonConfNFO_TVEpisode(sPath, isChecked)` | Renames episode NFO to .info |
| `RenameNonConfNFO_TVShow(sPath)` | Renames show NFO to .info |
| `RenameToInfo(sPath)` | Core method that renames `.nfo` to `.info` with collision handling |

---

## [↑](#table-of-contents) Part 2: Creating and Updating NFO Files

### [↑](#table-of-contents) Core Save Methods

#### [↑](#table-of-contents) Saving Movie NFO Files

**Method:** `SaveToNFO_Movie(tDBElement, ForceFileCleanup)`

**Process:**

1. **Fires `OnNFOSave_Movie` module event** for addon extensibility
2. If `ForceFileCleanup`, deletes existing NFOs first
3. Creates a **deep clone** of the movie data to prevent modifying database
5. **Digit Grouping:** Applies locale-specific formatting for vote counts if enabled
6. For each filename from `FileUtils.GetFilenameList.Movie()`:
   - Optionally renames existing non-conforming NFOs
   - Handles readonly file attributes
   - Serializes using `XmlSerializer` and `StreamWriter`
   - Restores original file attributes

---

#### [↑](#table-of-contents) Saving MovieSet NFO Files

**Method:** `SaveToNFO_MovieSet(tDBElement)`

**Process:**

1. Checks if title has changed and deletes old NFO if needed
2. Uses cached `XmlSerializer` for `MediaContainers.Movieset`
3. For each filename from `FileUtils.GetFilenameList.MovieSet()`:
   - Handles file attributes and readonly status
   - Serializes using `StreamWriter`

---

#### [↑](#table-of-contents) Saving TV Episode NFO Files

**Method:** `SaveToNFO_TVEpisode(tDBElement)`

**Process:**

1. Creates **deep clone** of episode data
2. For each filename from `FileUtils.GetFilenameList.TVEpisode()`:
   - Optionally renames non-conforming NFOs
   - **Multi-Episode Handling:** Executes SQL query to find other episodes sharing the same file
   - Loads each related episode from database
   - Adds current episode to list
3. For each episode (ordered by Season, then Episode):
   - Applies digit grouping for votes if enabled
   - Removes `<displayseason>` and `<displayepisode>` if setting disabled
   - Serializes to `StringBuilder` using `Utf8StringWriter`
4. Writes combined episode XML to single file with multiple `<episodedetails>` roots

**Multi-Episode SQL Query:**

    SELECT idEpisode FROM episode 
    WHERE idEpisode <> (?) 
    AND idFile IN (SELECT idFile FROM files WHERE strFilename = (?)) 
    ORDER BY Episode

---

#### [↑](#table-of-contents) Saving TV Show NFO Files

**Method:** `SaveToNFO_TVShow(tDBElement)`

**Process:**

1. **Fires `OnNFOSave_TVShow` module event** for addon extensibility
2. Creates **deep clone** of show data
4. Applies digit grouping for votes if enabled
5. For each filename from `FileUtils.GetFilenameList.TVShow()`:
   - Optionally renames non-conforming NFOs
   - Handles file attributes
   - Serializes using `StreamWriter`

---

### [↑](#table-of-contents) NFO Deletion Methods

| Method | Purpose |
|--------|---------|
| `DeleteNFO_Movie(DBMovie, ForceFileCleanup)` | Iterates through filename list and deletes all matching NFO files |
| `DeleteNFO_MovieSet(DBMovieSet, ForceFileCleanup, bForceOldTitle)` | Deletes MovieSet NFOs, optionally using old title for path |

---

## [↑](#table-of-contents) Part 3: Data Flow Diagrams

### [↑](#table-of-contents) Scanning/Reading Flow

    Scanner discovers media files
            │
            ▼
    Scanner calls Load_* method
            │
            ▼
    GetFolderContents_* discovers related files
            │
            ▼
    NFO.LoadFromNFO_* called
            │
            ├──► File exists with .nfo extension?
            │           │
            │           ▼ Yes
            │       XmlSerializer deserializes file
            │           │
            │           ▼
            │       CleanNFO_* normalizes data
            │           │
            │           ▼
            │       Return MediaContainer object
            │
            └──► No valid NFO?
                        │
                        ▼
                    GetIMDBFromNonConf extracts IDs
                        │
                        ▼
                    Optionally rename to .info

---

### [↑](#table-of-contents) Saving Flow

    User edits or scraper updates data
            │
            ▼
    SaveToNFO_* method called
            │
            ▼
    Fire module event (OnNFOSave_*)
            │
            ▼
    Create deep clone of data
            │
            ▼
    Apply transformations (digit grouping, etc.)
            │
            ▼
    For each filename in GetFilenameList:
            │
            ├──► Handle readonly attributes
            │
            ├──► Optionally rename non-conforming NFO
            │
            ├──► XmlSerializer writes to file
            │
            └──► Restore file attributes

---

### [↑](#table-of-contents) Image Save Flow

    User selects images or scraper downloads
            │
            ▼
    Images.SaveToFile() called
            │
            ▼
    For each ModifierType (Poster, Fanart, etc.):
            │
            ▼
    GetFilenameList returns target paths
            │
            ▼
    For each path:
            │
            ├──► Check if image exists in container
            │
            ├──► Download if URL only (not cached)
            │
            ├──► Resize if settings require
            │
            └──► Write to disk with proper extension

---

## [↑](#table-of-contents) Part 4: Settings Reference

### [↑](#table-of-contents) NFO Reading Settings

| Setting | Property | Effect |
|---------|----------|--------|
| Overwrite NFO | `GeneralOverwriteNfo` | If false, renames non-conforming NFOs to `.info` instead of overwriting |
| Movie MetaData Scan | `MovieScraperMetaDataScan` | Triggers MediaInfo scan if NFO has title but no FileInfo |
| TV MetaData Scan | `TVScraperMetaDataScan` | Same for TV episodes |

---

### [↑](#table-of-contents) NFO Writing Settings

| Setting | Property | Effect |
|---------|----------|--------|
| Overwrite NFO | `GeneralOverwriteNfo` | Controls whether non-conforming NFOs are renamed or overwritten |
| Digit Grouping | `GeneralDigitGrpSymbolVotes` | Formats vote counts with locale-specific grouping |
| Collections Extended | `MovieScraperCollectionsExtendedInfo` | Controls set node format (simple vs extended) |

---

### [↑](#table-of-contents) Movie ID Node Settings

| Setting | Property | Effect |
|---------|----------|--------|
| Write Default ID | `MovieScraperIdWriteNodeDefaultId` | Write `<id>` node |
| Write TMDb ID | `MovieScraperIdWriteNodeTMDbId` | Write `<tmdb>` node |
| Write TMDb Collection ID | `MovieScraperIdWriteNodeTMDbCollectionId` | Write `<tmdbcolid>` node |
| Write Release Date | `MovieScraperReleaseDateWriteNode` | Write `<releasedate>` node |
| Write Rating/Votes | `MovieScraperRatingVotesWriteNode` | Write legacy `<rating>` and `<votes>` nodes |

---

### [↑](#table-of-contents) TV ID Node Settings

| Setting | Property | Effect |
|---------|----------|--------|
| Write Default ID | `TVScraperIdWriteNodeDefaultId` | Write `<id>` node for TV |
| Write IMDb ID | `TVScraperIdWriteNodeIMDbId` | Write `<imdb>` node for TV |
| Write TMDb ID | `TVScraperIdWriteNodeTMDbId` | Write `<tmdb>` node for TV |
| Write TVDb ID | `TVScraperIdWriteNodeTVDbId` | Write `<tvdb>` node for TV |
| Write Rating/Votes | `TVScraperRatingVotesWriteNode` | Write legacy rating/votes for TV |

---

### [↑](#table-of-contents) Naming Scheme Settings

File naming uses **Kodi 19 (Matrix)+ compatible** patterns by default. The **Expert** scheme allows user-defined custom patterns.

See [NfoFileReference.md](../reference-docs/NfoFileReference.md#part-2-file-naming-conventions) for complete file naming details.

---

### [↑](#table-of-contents) Image Type Enable Settings

Each image type can be enabled/disabled per naming scheme. Examples:

### [↑](#table-of-contents) Image Type Enable Settings

Each image type can be enabled/disabled per naming scheme. 

> **Note:** The code currently uses legacy scheme names internally (Frodo, Extended, etc.) but all produce Kodi 19 (Matrix)+ compatible output. These will be cleaned up in a future refactor to remove legacy naming.

| Setting Pattern | Example | Effect |
|-----------------|---------|--------|
| `Movie<Type>Frodo` | `MoviePosterFrodo` | Enable poster (Kodi-compatible) |
| `Movie<Type>Extended` | `MovieBannerExtended` | Enable banner (extended artwork) |
| `TVShow<Type>Frodo` | `TVShowBannerFrodo` | Enable TV show banner |
| `TVSeason<Type>Frodo` | `TVSeasonPosterFrodo` | Enable season poster |
| `TVEpisode<Type>Frodo` | `TVEpisodeNFOFrodo` | Enable episode NFO |

---

## [↑](#table-of-contents) Part 5: Module Events

The NFO system fires events that addons can hook into for extensibility.

### [↑](#table-of-contents) NFO Events

| Event | When Fired | Purpose |
|-------|------------|---------|
| `OnNFOSave_Movie` | Before movie NFO save | Allow addons to modify data or cancel save |
| `OnNFOSave_TVShow` | Before TV show NFO save | Allow addons to modify data or cancel save |
| `OnNFORead_TVShow` | After TV show NFO load | Allow addons to process loaded data |

---

### [↑](#table-of-contents) Image Events

| Event | When Fired | Purpose |
|-------|------------|---------|
| `OnBannerSave_Movie` | Before movie banner save | Allow addons to modify or cancel |
| `OnClearArtSave_Movie` | Before movie clearart save | Allow addons to modify or cancel |
| `OnClearLogoSave_Movie` | Before movie clearlogo save | Allow addons to modify or cancel |
| `OnDiscArtSave_Movie` | Before movie discart save | Allow addons to modify or cancel |
| `OnFanartSave_Movie` | Before movie fanart save | Allow addons to modify or cancel |
| `OnFanartDelete_Movie` | Before movie fanart delete | Allow addons to cancel |
| `OnLandscapeSave_Movie` | Before movie landscape save | Allow addons to modify or cancel |
| `OnPosterSave_Movie` | Before movie poster save | Allow addons to modify or cancel |
| `OnPosterDelete_Movie` | Before movie poster delete | Allow addons to cancel |
| `OnThemeSave_Movie` | Before movie theme save | Allow addons to modify or cancel |
| `OnTrailerSave_Movie` | Before movie trailer save | Allow addons to modify or cancel |

---

### [↑](#table-of-contents) Event Handler Pattern

Addons implement event handlers using the module interface:

    Public Function RunGeneric(
        ByVal mType As Enums.ModuleEventType,
        ByRef _params As List(Of Object),
        ByRef _singleobjekt As Object,
        ByRef _dbelement As Database.DBElement
    ) As Interfaces.ModuleResult Implements Interfaces.GenericModule.RunGeneric
    
        Select Case mType
            Case Enums.ModuleEventType.OnNFOSave_Movie
                ' Process before movie NFO save
                ' Return ModuleResult with Cancelled=True to prevent save
            Case Enums.ModuleEventType.OnNFORead_TVShow
                ' Process after TV show NFO read
        End Select
        
        Return New Interfaces.ModuleResult
    End Function

---

## [↑](#table-of-contents) See Also

### [↑](#table-of-contents) Related Documentation

- [DocumentIndex.md](../DocumentIndex.md) — Master documentation index
- [NfoFileReference.md](../reference-docs/NfoFileReference.md) — NFO XML structure and file naming reference
- [ImageSelectionProcess.md](ImageSelectionProcess.md) — Image selection and saving process
- [ScrapingProcessMovies.md](ScrapingProcessMovies.md) — Movie scraping workflow
- [ScrapingProcessTvShows.md](ScrapingProcessTvShows.md) — TV show scraping workflow

---

### [↑](#table-of-contents) Related Source Files

| File | Purpose |
|------|---------|
| [clsAPINFO.vb](../../EmberAPI/clsAPINFO.vb) | NFO read/write operations |
| [clsAPIFileUtils.vb](../../EmberAPI/clsAPIFileUtils.vb) | File naming logic |
| [clsAPIMediaContainers.vb](../../EmberAPI/clsAPIMediaContainers.vb) | XML serialization classes |
| [clsAPICommon.vb](../../EmberAPI/clsAPICommon.vb) | ModifierType enum and utilities |
| [clsAPIScanner.vb](../../EmberAPI/clsAPIScanner.vb) | Media scanning and NFO loading |
| [clsAPIImages.vb](../../EmberAPI/clsAPIImages.vb) | Image save operations |

---

### [↑](#table-of-contents) Related Backlog Items

| Item | Description |
|------|-------------|
| [BL-CC-002](../improvements-docs/backlog/BL-CC-002-KodiFanartNaming.md) | Kodi-compliant fanart naming (fanart1.jpg, fanart2.jpg) |
| [BL-CC-001](../improvements-docs/backlog/BL-CC-001-ReplaceBinaryFormatter.md) | Replace BinaryFormatter in CloneDeep |
| [BL-CQ-003](../improvements-docs/backlog/BL-CQ-003-NFOSpecificExceptionHandling.md) | Specific exception handling in NFO Load |
| [BL-CQ-004](../improvements-docs/backlog/BL-CQ-004-MultiEpisodeRegex.md) | Fix fragile multi-episode regex |
| [BL-CQ-005](../improvements-docs/backlog/BL-CQ-005-NFODateHandling.md) | Consistent date format handling |

---

*End of file*