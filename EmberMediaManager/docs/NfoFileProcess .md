# NFO File Processing Analysis

## Overview

This document provides a comprehensive analysis of how Ember Media Manager processes NFO files for media content. NFO files are XML-based metadata files that store information about movies, TV shows, episodes, and movie sets. The processing is divided into two main parts: reading existing NFO files and creating/updating NFO files.

---

## Part 1: Reading NFO Files

### 1.1 Core Files Involved

| File | Purpose |
|------|---------|
| `EmberAPI\clsAPINFO.vb` | Main NFO processing class containing all load/save methods |
| `EmberAPI\clsAPIScanner.vb` | Scanner that initiates NFO loading during media discovery |
| `EmberAPI\clsAPIMediaContainers.vb` | Data container classes with XML serialization attributes |

### 1.2 Entry Points for Reading NFO Files

The scanner class (`clsAPIScanner.vb`) initiates NFO reading when loading media:

**Movie Loading (Lines 633-648):**
- Method: `Load_Movie(ByRef DBMovie As Database.DBElement, ByVal Batchmode As Boolean)`
- If `DBMovie.NfoPathSpecified` is true, calls `NFO.LoadFromNFO_Movie(DBMovie.NfoPath, DBMovie.IsSingle)`
- Otherwise attempts to load from the movie filename path

**MovieSet Loading (Lines 741-766):**
- Method: `Load_MovieSet(ByRef DBMovieSet As Database.DBElement, ByVal Batchmode As Boolean)`
- Calls `NFO.GetNfoPath_MovieSet(DBMovieSet)` to find NFO path
- Then calls `NFO.LoadFromNFO_MovieSet(sNFO)` to load content

**TV Episode Loading (Lines 769-800):**
- Method: `Load_TVEpisode(ByVal DBTVEpisode As Database.DBElement, ...)`
- Calls `NFO.LoadFromNFO_TVEpisode(cEpisode.NfoPath, sEpisode.Season, sEpisode.Episode)`
- Supports loading by season/episode number or by aired date

### 1.3 NFO Loading Methods

#### 1.3.1 Loading Movie NFO Files

**Method:** `LoadFromNFO_Movie(ByVal sPath As String, ByVal isSingle As Boolean)` (Lines 1838-1902)

**Process:**
1. Creates an `XmlSerializer` for `MediaContainers.Movie` type
2. If file exists and has `.nfo` extension, deserializes using `StreamReader`
3. Calls `CleanNFO_Movies(xmlMov)` to normalize the data
4. For non-conforming NFOs, attempts to extract IMDB ID using `GetIMDBFromNonConf()`
5. Optionally renames non-conforming NFOs to `.info` extension

**Key Code Pattern:**
The deserialization uses `DirectCast(xmlSer.Deserialize(xmlSR), MediaContainers.Movie)` to convert XML to the Movie object.

#### 1.3.2 Loading MovieSet NFO Files

**Method:** `LoadFromNFO_MovieSet(ByVal sPath As String)` (Lines 1905-1929)

**Process:**
1. Creates `XmlSerializer` for `MediaContainers.Movieset` type
2. Deserializes the file using `StreamReader`
3. Normalizes line endings in Plot field: `xmlMovSet.Plot.Replace(vbCrLf, vbLf).Replace(vbLf, vbCrLf)`

#### 1.3.3 Loading TV Episode NFO Files

**Method:** `LoadFromNFO_TVEpisode(ByVal sPath As String, ByVal SeasonNumber As Integer, ByVal EpisodeNumber As Integer)` (Lines 1932-1994)

**Process:**
1. Handles multi-episode NFO files using regex pattern matching
2. Pattern used: `<episodedetails.*?>.*?</episodedetails>`
3. If single match found, deserializes directly
4. If multiple matches, iterates to find matching season/episode number
5. Calls `CleanNFO_TVEpisodes(xmlEp)` for data normalization
6. Updates language information for audio/subtitle streams

**Alternative Method:** `LoadFromNFO_TVEpisode(ByVal sPath As String, ByVal SeasonNumber As Integer, ByVal Aired As String)` (Lines 1997-2059) - Loads by aired date instead of episode number.

#### 1.3.4 Loading TV Show NFO Files

**Method:** `LoadFromNFO_TVShow(ByVal sPath As String)` (Lines 2062-2104)

**Process:**
1. Creates `XmlSerializer` for `MediaContainers.TVShow` type
2. Deserializes using `StreamReader`
3. Calls `CleanNFO_TVShow(xmlShow)` for normalization
4. Handles non-conforming NFOs by renaming to `.info`

### 1.4 NFO Validation Methods

**Conformance Checking:**
- `IsConformingNFO_Movie(ByVal sPath As String)` (Lines ~1740-1760)
- `IsConformingNFO_TVEpisode(ByVal sPath As String)` (Lines ~1765-1810)
- `IsConformingNFO_TVShow(ByVal sPath As String)` (Lines 1814-1836)

These methods attempt to deserialize the file and return `True` if successful, `False` if an exception occurs.

### 1.5 NFO Cleaning/Normalization

**CleanNFO_Movies (Lines 1175-1218):**
- Normalizes line endings in Outline and Plot fields
- Converts Premiered date to ISO8601 format using `NumUtils.DateToISO8601Date()`
- Cleans vote count formatting using `NumUtils.CleanVotes()`
- Adds long language names for audio/subtitle streams
- Removes sets without titles
- Converts language names to Alpha2 codes

**CleanNFO_TVEpisodes (Lines 1221-1240):**
- Normalizes Aired date to ISO8601 format
- Cleans vote formatting
- Adds long language names for streams

**CleanNFO_TVShow (Lines 1243-1274):**
- Normalizes Plot line endings
- Converts Premiered to ISO8601
- Cleans vote formatting
- Handles Boxee TV database ID conversion
- Converts language to Alpha2 codes

### 1.6 Handling Non-Conforming NFO Files

**GetIMDBFromNonConf Method (Lines 1579-1637):**
- Searches for `.nfo` and `.info` files in the directory
- Uses regex pattern `tt\d\d\d\d\d\d\d*` to extract IMDB ID
- Attempts to parse partial NFO content if `</movie>` tag is found

**Renaming Methods:**
- `RenameNonConfNFO_Movie()` (Lines 2107-2116)
- `RenameNonConfNFO_TVEpisode()` (Lines 2119-2126)
- `RenameNonConfNFO_TVShow()` (Lines 2129-2136)
- `RenameToInfo()` (Lines 2139-2154) - Renames `.nfo` to `.info` with collision handling

---

## Part 2: Creating and Updating NFO Files

### 2.1 Core Save Methods

#### 2.1.1 Saving Movie NFO Files

**Method:** `SaveToNFO_Movie(ByRef tDBElement As Database.DBElement, ByVal ForceFileCleanup As Boolean)` (Lines 2165-2229)

**Process:**
1. Fires `OnNFOSave_Movie` event for module hooks
2. Creates a deep clone of the movie data to prevent modifying database
3. Handles YAMJ boxset format if enabled
4. Applies digit grouping for vote counts if enabled
5. Optionally renames existing non-conforming NFOs
6. Uses `XmlSerializer` to write to file
7. Preserves file attributes (readonly handling)

**Key Operations Before Save:**
- BoxSet formatting for YAMJ compatibility
- Vote count formatting with locale-specific digit grouping
- File attribute management for readonly files

#### 2.1.2 Saving MovieSet NFO Files

**Method:** `SaveToNFO_MovieSet(ByRef tDBElement As Database.DBElement)` (Lines 2232-2276)

**Process:**
1. Checks if title has changed and deletes old NFO if needed
2. Creates `XmlSerializer` for `MediaContainers.Movieset`
3. Iterates through filename list from `FileUtils.GetFilenameList.MovieSet()`
4. Handles file attributes and readonly status
5. Serializes using `StreamWriter`

#### 2.1.3 Saving TV Episode NFO Files

**Method:** `SaveToNFO_TVEpisode(ByRef tDBElement As Database.DBElement)` (Lines 2279-2370)

**Process:**
1. Creates deep clone of episode data
2. Uses `StringBuilder` to concatenate multiple episode entries
3. Handles multi-episode files by loading existing episodes
4. Applies digit grouping for votes if enabled
5. Writes combined episode XML to single file

**Multi-Episode Handling:**
- Loads existing episode details from NFO
- Merges with current episode data
- Serializes all episodes into single file with multiple `<episodedetails>` roots

#### 2.1.4 Saving TV Show NFO Files

**Method:** `SaveToNFO_TVShow(ByRef tDBElement As Database.DBElement)` (Lines 2373-2435)

**Process:**
1. Fires `OnNFOSave_TVShow` event for module hooks
2. Creates deep clone of show data
3. Handles Boxee TVDb ID format if enabled
4. Applies digit grouping for votes
5. Iterates through filename list and writes NFO

### 2.2 NFO Deletion Methods

- `DeleteNFO_Movie(ByVal DBMovie As Database.DBElement, ByVal ForceFileCleanup As Boolean)` (Lines 1281-1292)
- `DeleteNFO_MovieSet(ByVal DBMovieSet As Database.DBElement, ByVal ForceFileCleanup As Boolean, Optional bForceOldTitle As Boolean)` (Lines 1299-1310)

These methods iterate through the filename list and delete all matching NFO files.

---

## Part 3: NFO XML Structure

### 3.1 Media Container Classes

The XML structure is defined by serializable classes in `EmberAPI\clsAPIMediaContainers.vb`:

#### 3.1.1 Movie NFO Structure (XmlRoot: "movie")

**Class:** `MediaContainers.Movie` (Lines 1148-2025)

**Key Elements:**
| XML Element | Property | Description |
|-------------|----------|-------------|
| `<title>` | Title | Movie title |
| `<originaltitle>` | OriginalTitle | Original language title |
| `<sorttitle>` | SortTitle | Title used for sorting |
| `<year>` | Year | Release year |
| `<premiered>` | Premiered | Premiere date |
| `<rating>` | Rating | Rating value (legacy) |
| `<votes>` | Votes | Vote count (legacy) |
| `<ratings>` | Ratings_Kodi | Kodi-format ratings array |
| `<userrating>` | UserRating | User's personal rating |
| `<outline>` | Outline | Short description |
| `<plot>` | Plot | Full plot description |
| `<tagline>` | Tagline | Marketing tagline |
| `<runtime>` | Runtime | Duration in minutes |
| `<mpaa>` | MPAA | Content rating |
| `<certification>` | Certifications | Country-specific ratings |
| `<id>` | DefaultId | Default unique ID |
| `<uniqueid>` | UniqueIDs_Kodi | Kodi-format unique IDs |
| `<tmdb>` | TMDbId | TMDb identifier |
| `<genre>` | Genres | Genre list |
| `<studio>` | Studios | Production studios |
| `<country>` | Countries | Production countries |
| `<director>` | Directors | Director names |
| `<credits>` | Credits | Writer credits |
| `<actor>` | Actors | Cast list with Person details |
| `<set>` | Set_Kodi | Movie set information |
| `<tag>` | Tags | User tags |
| `<trailer>` | Trailer | Trailer URL |
| `<fileinfo>` | FileInfo | Media technical details |
| `<thumb>` | Thumb | Poster URLs |
| `<fanart>` | Fanart | Fanart URLs |
| `<top250>` | Top250 | IMDb Top 250 ranking |
| `<playcount>` | PlayCount | Watch count |
| `<lastplayed>` | LastPlayed | Last watched date |
| `<dateadded>` | DateAdded | Library add date |

#### 3.1.2 MovieSet NFO Structure (XmlRoot: "movieset")

**Class:** `MediaContainers.Movieset` (Lines 2055-2150)

**Key Elements:**
| XML Element | Property | Description |
|-------------|----------|-------------|
| `<title>` | Title | Collection name |
| `<plot>` | Plot | Collection description |
| `<id>` | DefaultId | Default unique ID |
| `<uniqueid>` | UniqueIDs_Kodi | Unique identifiers |

#### 3.1.3 TV Show NFO Structure (XmlRoot: "tvshow")

**Class:** `MediaContainers.TVShow` (Lines 2626-3200)

**Key Elements:**
| XML Element | Property | Description |
|-------------|----------|-------------|
| `<title>` | Title | Show title |
| `<originaltitle>` | OriginalTitle | Original title |
| `<sorttitle>` | SortTitle | Sort title |
| `<rating>` | Rating | Show rating |
| `<votes>` | Votes | Vote count |
| `<ratings>` | Ratings_Kodi | Kodi ratings |
| `<userrating>` | UserRating | User rating |
| `<plot>` | Plot | Show description |
| `<mpaa>` | MPAA | Content rating |
| `<premiered>` | Premiered | First air date |
| `<status>` | Status | Show status |
| `<studio>` | Studios | Networks |
| `<genre>` | Genres | Genres |
| `<actor>` | Actors | Cast |
| `<episodeguide>` | EpisodeGuide | Episode guide URL |
| `<id>` | DefaultId | Default ID |
| `<uniqueid>` | UniqueIDs_Kodi | Unique IDs |
| `<boxeeTvDb>` | BoxeeTVDb | Boxee TVDb ID |

#### 3.1.4 Episode NFO Structure (XmlRoot: "episodedetails")

**Class:** `MediaContainers.EpisodeDetails` (Lines 121-600)

**Key Elements:**
| XML Element | Property | Description |
|-------------|----------|-------------|
| `<title>` | Title | Episode title |
| `<season>` | Season | Season number |
| `<episode>` | Episode | Episode number |
| `<aired>` | Aired | Air date |
| `<rating>` | Rating | Episode rating |
| `<votes>` | Votes | Vote count |
| `<ratings>` | Ratings_Kodi | Kodi ratings |
| `<plot>` | Plot | Episode description |
| `<runtime>` | Runtime | Duration |
| `<director>` | Directors | Directors |
| `<credits>` | Credits | Writers |
| `<actor>` | Actors | Guest cast |
| `<fileinfo>` | FileInfo | Technical details |
| `<id>` | DefaultId | Default ID |
| `<uniqueid>` | UniqueIDs_Kodi | Unique IDs |
| `<playcount>` | PlayCount | Watch count |
| `<lastplayed>` | LastPlayed | Last watched |

### 3.2 Supporting XML Types

**Person Class (for Actors):**
- `<name>` - Actor name
- `<role>` - Character name
- `<order>` - Cast order
- `<thumb>` - Actor photo URL

**Fileinfo/StreamDetails:**
- `<video>` - Video stream info (codec, width, height, aspect, duration)
- `<audio>` - Audio stream info (codec, channels, language)
- `<subtitle>` - Subtitle stream info (language)

**RatingDetails (Kodi format):**
- `<rating name="..." max="..." default="..."><value>...</value><votes>...</votes></rating>`

**Uniqueid (Kodi format):**
- `<uniqueid type="..." default="...">value</uniqueid>`

---

## Part 4: File Naming Conventions

NFO file paths are determined by `FileUtils.GetFilenameList` methods which return lists based on user settings:

- **Movies:** `FileUtils.GetFilenameList.Movie(DBElement, Enums.ModifierType.MainNFO)`
- **MovieSets:** `FileUtils.GetFilenameList.MovieSet(DBElement, Enums.ModifierType.MainNFO)`
- **TV Shows:** `FileUtils.GetFilenameList.TVShow(DBElement, Enums.ModifierType.MainNFO)`
- **TV Episodes:** `FileUtils.GetFilenameList.TVEpisode(DBElement, Enums.ModifierType.EpisodeNFO)`

---

## Part 5: Data Flow Diagram

1. **Scanning Phase:**
   - Scanner discovers media files
   - Scanner calls appropriate `Load_*` method
   - `Load_*` method calls `NFO.LoadFromNFO_*`
   - Data is deserialized into MediaContainers class
   - Data is cleaned/normalized
   - Data is saved to database

2. **Saving Phase:**
   - User edits or scraper updates data
   - `SaveToNFO_*` method is called
   - Data is cloned to prevent database modification
   - Optional transformations applied (Boxee, YAMJ, digit grouping)
   - XmlSerializer writes to file(s)
   - File attributes are preserved

---

## Part 6: Settings That Affect NFO Processing

| Setting | Effect |
|---------|--------|
| `GeneralOverwriteNfo` | Controls whether non-conforming NFOs are renamed or overwritten |
| `MovieUseYAMJ` | Enables YAMJ-specific NFO formatting |
| `TVUseBoxee` | Enables Boxee-specific TV show ID handling |
| `GeneralDigitGrpSymbolVotes` | Formats vote counts with locale-specific grouping |
| `*ScraperIdWriteNode*` | Controls which ID nodes are written to NFO |
| `*ScraperRatingVotesWriteNode` | Controls legacy rating/votes node output |

---

## Summary

The NFO file processing in Ember Media Manager is a robust system that:

1. Uses XML serialization for reading and writing NFO files
2. Supports multiple NFO formats (Kodi, YAMJ, Boxee)
3. Handles non-conforming NFO files gracefully
4. Normalizes data during read operations
5. Provides extensibility through module events
6. Maintains file attribute preservation during writes
7. Supports multi-episode NFO files for TV content