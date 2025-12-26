# NFO File Processing Analysis

## Overview

This document provides a comprehensive analysis of how Ember Media Manager processes NFO files for media content. NFO files are XML-based metadata files that store information about movies, TV shows, episodes, and movie sets. The processing is divided into two main parts: reading existing NFO files and creating/updating NFO files.

> **Note:** This documentation describes the code structure and flow. Specific line numbers are not included as they change frequently during development.

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

#### Movie Loading - Load_Movie()

**Primary Flow:**
1. Calls `GetFolderContents_Movie()` to discover related files
2. If `DBMovie.NfoPathSpecified` is true, calls `NFO.LoadFromNFO_Movie(DBMovie.NfoPath, DBMovie.IsSingle)`
3. Otherwise attempts to load from the movie filename path

**Additional Processing After NFO Load:**
- **MediaInfo Scan:** If title exists but no FileInfo, runs MediaInfo scan (if `MovieScraperMetaDataScan` enabled)
- **Year Extraction:** If year not in NFO, extracts from file path using `StringUtils.FilterYearFromPath_Movie()`
- **IMDB ID Extraction:** If not in NFO, extracts from filename using `StringUtils.GetIMDBIDFromString()`
- **Title Extraction:** If no title (invalid NFO), clears NFO path and extracts title from path
- **YAMJ Watched File:** Creates/reads `.watched` files if YAMJ mode enabled
- **Actor Thumb Matching:** Matches local actor thumb files to actors in NFO
- **Edition Detection:** Runs regex mapping to detect edition from filename

#### MovieSet Loading - Load_MovieSet()

**Primary Flow:**
1. Calls `GetFolderContents_MovieSet()` to discover related files
2. Calls `NFO.GetNfoPath_MovieSet()` to find NFO path if not already set
3. Calls `NFO.LoadFromNFO_MovieSet()` to load content
4. Extracts Language and Lock state from NFO

#### TV Episode Loading - Load_TVEpisode()

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

#### TV Show Loading - Load_TVShow()

**Primary Flow:**
1. Calls `GetFolderContents_TVShow()` to discover related files
2. Calls `NFO.LoadFromNFO_TVShow()` to load content
3. Fires `OnNFORead_TVShow` module event for extensibility

### 1.3 NFO Loading Methods

#### 1.3.1 Loading Movie NFO Files

**Method:** `LoadFromNFO_Movie(sPath, isSingle)`

**Process:**
1. Creates an `XmlSerializer` for `MediaContainers.Movie` type
2. If file exists and has `.nfo` extension:
   - Deserializes using `StreamReader`
   - Calls `CleanNFO_Movies()` to normalize the data
3. For non-conforming NFOs:
   - Attempts to extract IMDB ID using `GetIMDBFromNonConf()`
   - Tries to parse partial NFO content if `</movie>` tag found
   - Optionally renames to `.info` extension (if `GeneralOverwriteNfo` is false)

**Key Code Pattern:**

    DirectCast(xmlSer.Deserialize(xmlSR), MediaContainers.Movie)

#### 1.3.2 Loading MovieSet NFO Files

**Method:** `LoadFromNFO_MovieSet(sPath)`

**Process:**
1. Creates `XmlSerializer` for `MediaContainers.Movieset` type
2. Deserializes the file using `StreamReader`
3. Normalizes line endings in Plot field

#### 1.3.3 Loading TV Episode NFO Files

**Method:** `LoadFromNFO_TVEpisode(sPath, SeasonNumber, EpisodeNumber)`

**Process:**
1. Handles multi-episode NFO files using regex pattern matching
2. Pattern used: `<episodedetails.*?>.*?</episodedetails>`
3. If single match found, deserializes directly
4. If multiple matches, iterates to find matching season/episode number
5. Calls `CleanNFO_TVEpisodes()` for data normalization
6. Updates language information for audio/subtitle streams

**Alternative Method:** `LoadFromNFO_TVEpisode(sPath, SeasonNumber, Aired)` - Loads by aired date instead of episode number for date-based episode matching

#### 1.3.4 Loading TV Show NFO Files

**Method:** `LoadFromNFO_TVShow(sPath)`

**Process:**
1. Creates `XmlSerializer` for `MediaContainers.TVShow` type
2. Deserializes using `StreamReader`
3. Calls `CleanNFO_TVShow()` for normalization
4. Handles non-conforming NFOs by renaming to `.info`
5. **Fires `OnNFORead_TVShow` module event** for addon extensibility

### 1.4 NFO Validation Methods

**Conformance Checking:**
- `IsConformingNFO_Movie(sPath)` - Attempts deserialization, returns True if successful
- `IsConformingNFO_TVEpisode(sPath)` - Handles multi-episode validation
- `IsConformingNFO_TVShow(sPath)` - Attempts deserialization, returns True if successful

These methods attempt to deserialize the file and return `True` if successful, `False` if an exception occurs.

### 1.5 NFO Cleaning/Normalization

#### CleanNFO_Movies()
- Normalizes line endings in Outline and Plot fields
- Converts Premiered date to ISO8601 format using `NumUtils.DateToISO8601Date()`
- Cleans vote count formatting using `NumUtils.CleanVotes()`
- Adds long language names for audio/subtitle streams via `Localization.ISOGetLangByCode3()`
- Removes sets without titles
- Converts language names to Alpha2 codes

#### CleanNFO_TVEpisodes()
- Normalizes Aired date to ISO8601 format
- Cleans vote formatting
- Adds long language names for audio/subtitle streams

#### CleanNFO_TVShow()
- Normalizes Plot line endings
- Converts Premiered to ISO8601
- Cleans vote formatting
- Converts language to Alpha2 codes
- **Boxee Support:** If `TVUseBoxee` enabled and BoxeeTVDb ID present but no TVDb ID, copies BoxeeTVDb to TVDbId

### 1.6 Handling Non-Conforming NFO Files

#### GetIMDBFromNonConf Method
- Searches for `.nfo` and `.info` files in the directory
- Uses regex pattern `tt\d\d\d\d\d\d\d*` to extract IMDB ID
- Attempts to parse partial NFO content if `</movie>` tag is found
- For non-single movies, searches with filename pattern matching

#### Renaming Methods
- `RenameNonConfNFO_Movie(sPath, isChecked)` - Renames movie NFO to .info
- `RenameNonConfNFO_TVEpisode(sPath, isChecked)` - Renames episode NFO to .info
- `RenameNonConfNFO_TVShow(sPath)` - Renames show NFO to .info
- `RenameToInfo(sPath)` - Core method that renames `.nfo` to `.info` with collision handling

---

## Part 2: Creating and Updating NFO Files

### 2.1 Core Save Methods

#### 2.1.1 Saving Movie NFO Files

**Method:** `SaveToNFO_Movie(tDBElement, ForceFileCleanup)`

**Process:**
1. **Fires `OnNFOSave_Movie` module event** for addon extensibility
2. If `ForceFileCleanup`, deletes existing NFOs first
3. Creates a **deep clone** of the movie data to prevent modifying database
4. **YAMJ Support:** If enabled with YAMJ NFO setting, clears TMDb ID
5. **Digit Grouping:** Applies locale-specific formatting for vote counts if enabled
6. For each filename from `FileUtils.GetFilenameList.Movie()`:
   - Optionally renames existing non-conforming NFOs
   - Handles readonly file attributes
   - Serializes using `XmlSerializer` and `StreamWriter`
   - Restores original file attributes

#### 2.1.2 Saving MovieSet NFO Files

**Method:** `SaveToNFO_MovieSet(tDBElement)`

**Process:**
1. Checks if title has changed and deletes old NFO if needed
2. Creates `XmlSerializer` for `MediaContainers.Movieset`
3. For each filename from `FileUtils.GetFilenameList.MovieSet()`:
   - Handles file attributes and readonly status
   - Serializes using `StreamWriter`

#### 2.1.3 Saving TV Episode NFO Files

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

#### 2.1.4 Saving TV Show NFO Files

**Method:** `SaveToNFO_TVShow(tDBElement)`

**Process:**
1. **Fires `OnNFOSave_TVShow` module event** for addon extensibility
2. Creates **deep clone** of show data
3. **Boxee Support:** If enabled and TVDb ID exists, copies to BoxeeTVDb and blanks the regular ID
4. Applies digit grouping for votes if enabled
5. For each filename from `FileUtils.GetFilenameList.TVShow()`:
   - Optionally renames non-conforming NFOs
   - Handles file attributes
   - Serializes using `StreamWriter`

### 2.2 NFO Deletion Methods

- `DeleteNFO_Movie(DBMovie, ForceFileCleanup)` - Iterates through filename list and deletes all matching NFO files
- `DeleteNFO_MovieSet(DBMovieSet, ForceFileCleanup, bForceOldTitle)` - Deletes MovieSet NFOs, optionally using old title for path

---

## Part 3: NFO XML Structure

### 3.1 Media Container Classes

The XML structure is defined by serializable classes in `EmberAPI\clsAPIMediaContainers.vb`. These classes use .NET XML serialization attributes to control how data is read from and written to NFO files.

#### 3.1.1 Key XML Serialization Patterns

**Conditional Serialization with *Specified Properties:**

The `*Specified` pattern is a .NET XML Serialization convention - the serializer only writes `Xxx` to XML if `XxxSpecified` returns `True`. This prevents empty tags in NFO files.

Example: `TaglineSpecified` returns `Not String.IsNullOrEmpty(Tagline)`

**Settings-Controlled Serialization:**

Some properties only serialize if both a value exists AND a setting is enabled.

Example: `DefaultIdSpecified` checks `DefaultId.ValueSpecified AndAlso Master.eSettings.MovieScraperIdWriteNodeDefaultId`

**Internal Properties (Never Serialized):**

Properties marked with `<XmlIgnore()>` without a corresponding `*Specified` property are for internal use only.

Examples: `Scrapersource`, `Lev`, `ThumbPoster`

**Wrapper Properties for Complex Serialization:**

The actual data container is ignored, while a wrapper property handles serialization in the correct format.

Example: `UniqueIDs` (ignored) vs `UniqueIDs_Kodi` (serialized as array)

#### 3.1.2 Movie NFO Structure (XmlRoot: "movie")

**Class:** `MediaContainers.Movie`

| XML Element | Property | Description |
|-------------|----------|-------------|
| `<title>` | Title | Movie title |
| `<originaltitle>` | OriginalTitle | Original language title |
| `<sorttitle>` | SortTitle | Title used for sorting |
| `<year>` | Year | Release year (derived from Premiered if available) |
| `<premiered>` | Premiered | Premiere date |
| `<releasedate>` | ReleaseDate | Alias for Premiered (legacy) |
| `<rating>` | Rating | Rating value (legacy format) |
| `<votes>` | Votes | Vote count (legacy format) |
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
| `<tmdbcolid>` | TMDbCollectionId | TMDb Collection identifier |
| `<genre>` | Genres | Genre list |
| `<studio>` | Studios | Production studios |
| `<country>` | Countries | Production countries |
| `<director>` | Directors | Director names |
| `<credits>` | Credits | Writer credits |
| `<actor>` | Actors | Cast list with Person details |
| `<set>` | Set_Kodi | Movie set information (Kodi format) |
| `<sets>` | Sets_YAMJ | Movie sets (YAMJ format) |
| `<tag>` | Tags | User tags |
| `<trailer>` | Trailer | Trailer URL |
| `<fileinfo>` | FileInfo | Media technical details |
| `<thumb>` | Thumb | Poster URLs |
| `<fanart>` | Fanart | Fanart URLs |
| `<top250>` | Top250 | IMDb Top 250 ranking |
| `<playcount>` | PlayCount | Watch count |
| `<lastplayed>` | LastPlayed | Last watched date |
| `<dateadded>` | DateAdded | Library add date |
| `<datemodified>` | DateModified | Last modified date |
| `<videosource>` | VideoSource | Video source type |
| `<edition>` | Edition | Movie edition |
| `<showlink>` | ShowLinks | Linked TV shows |
| `<locked>` | Locked | Lock state |
| `<user_note>` | UserNote | User notes |

**Internal Properties (not serialized):** `Scrapersource`, `Lev`, `ThumbPoster`

#### 3.1.3 MovieSet NFO Structure (XmlRoot: "movieset")

**Class:** `MediaContainers.Movieset`

| XML Element | Property | Description |
|-------------|----------|-------------|
| `<title>` | Title | Collection name |
| `<plot>` | Plot | Collection description |
| `<language>` | Language | Language code |
| `<id>` | DefaultId | Default unique ID |
| `<uniqueid>` | UniqueIDs_Kodi | Unique identifiers |
| `<locked>` | Locked | Lock state |

**Internal Properties:** `OldTitle` (for detecting renames)

#### 3.1.4 TV Show NFO Structure (XmlRoot: "tvshow")

**Class:** `MediaContainers.TVShow`

| XML Element | Property | Description |
|-------------|----------|-------------|
| `<title>` | Title | Show title |
| `<originaltitle>` | OriginalTitle | Original title |
| `<sorttitle>` | SortTitle | Sort title |
| `<tagline>` | Tagline | Show tagline |
| `<rating>` | Rating | Show rating |
| `<votes>` | Votes | Vote count |
| `<ratings>` | Ratings_Kodi | Kodi ratings |
| `<userrating>` | UserRating | User rating |
| `<plot>` | Plot | Show description |
| `<mpaa>` | MPAA | Content rating |
| `<certification>` | Certifications | Certifications list |
| `<premiered>` | Premiered | First air date |
| `<status>` | Status | Show status |
| `<studio>` | Studios | Networks |
| `<genre>` | Genres | Genres |
| `<country>` | Countries | Production countries |
| `<director>` | Directors | Directors |
| `<actor>` | Actors | Cast |
| `<creator>` | Creators | Show creators |
| `<tag>` | Tags | User tags |
| `<runtime>` | Runtime | Episode runtime |
| `<episodeguide>` | EpisodeGuide | Episode guide URL |
| `<id>` | DefaultId | Default ID |
| `<uniqueid>` | UniqueIDs_Kodi | Unique IDs |
| `<imdb>` | IMDbId | IMDb ID |
| `<tmdb>` | TMDbId | TMDb ID |
| `<boxeeTvDb>` | BoxeeTVDb | Boxee TVDb ID |
| `<seasons>` | Seasons | Season details container |
| `<locked>` | Locked | Lock state |
| `<user_note>` | UserNote | User notes |

**Internal Properties:** `Scrapersource`, `KnownEpisodes`, `KnownSeasons`

#### 3.1.5 Episode NFO Structure (XmlRoot: "episodedetails")

**Class:** `MediaContainers.EpisodeDetails`

| XML Element | Property | Description |
|-------------|----------|-------------|
| `<title>` | Title | Episode title |
| `<originaltitle>` | OriginalTitle | Original title |
| `<season>` | Season | Season number |
| `<episode>` | Episode | Episode number |
| `<subepisode>` | SubEpisode | Sub-episode number |
| `<displayseason>` | DisplaySeason | Display season number |
| `<displayepisode>` | DisplayEpisode | Display episode number |
| `<aired>` | Aired | Air date |
| `<rating>` | Rating | Episode rating |
| `<votes>` | Votes | Vote count |
| `<ratings>` | Ratings_Kodi | Kodi ratings |
| `<userrating>` | UserRating | User rating |
| `<plot>` | Plot | Episode description |
| `<runtime>` | Runtime | Duration |
| `<videosource>` | VideoSource | Video source |
| `<director>` | Directors | Directors |
| `<credits>` | Credits | Writers |
| `<actor>` | Actors | Episode cast |
| `<gueststar>` | GuestStars | Guest stars |
| `<fileinfo>` | FileInfo | Technical details |
| `<id>` | DefaultId | Default ID |
| `<uniqueid>` | UniqueIDs_Kodi | Unique IDs |
| `<imdb>` | IMDbId | IMDb ID |
| `<tmdb>` | TMDbId | TMDb ID |
| `<playcount>` | Playcount | Watch count |
| `<lastplayed>` | LastPlayed | Last watched |
| `<dateadded>` | DateAdded | Library add date |
| `<locked>` | Locked | Lock state |
| `<user_note>` | UserNote | User notes |

**Internal Properties:** `Scrapersource`, `ThumbPoster`, `EpisodeAbsolute`, `EpisodeCombined`, `EpisodeDVD`, `SeasonCombined`, `SeasonDVD`

### 3.2 Supporting XML Types

#### Person Class (for Actors/GuestStars)

| XML Element | Property |
|-------------|----------|
| `<name>` | Name |
| `<role>` | Role |
| `<order>` | Order |
| `<thumb>` | URLOriginal |
| `<imdbid>` | IMDB |
| `<tmdbid>` | TMDB |
| `<tvdbid>` | TVDB |

#### Fileinfo/StreamDetails

| XML Element | Content |
|-------------|---------|
| `<video>` | Video stream info (codec, width, height, aspect, duration, etc.) |
| `<audio>` | Audio stream info (codec, channels, language, longlanguage) |
| `<subtitle>` | Subtitle stream info (language, longlanguage, forced, path, type) |

#### RatingDetails (Kodi format)

    <rating name="imdb" max="10" default="true">
        <value>8.5</value>
        <votes>125000</votes>
    </rating>

#### Uniqueid (Kodi format)

    <uniqueid type="imdb" default="true">tt1234567</uniqueid>
    <uniqueid type="tmdb">12345</uniqueid>

---

### 3.3 Key .NET Attributes Used

#### `<Serializable()>` Attribute

**Purpose:** Indicates that a class can be serialized - converted to a format that can be stored or transmitted and later reconstructed.

**What it does:**
- Allows the class to be serialized using `BinaryFormatter` (used in `CloneDeep()` method)
- Allows the class to be serialized using `XmlSerializer` (used for NFO file read/write)
- Required for deep cloning via binary serialization

**Why it matters:** Every MediaContainer class has this attribute because:
1. Classes are serialized to/from XML for NFO files
2. The `CloneDeep()` method uses `BinaryFormatter` which requires this attribute
3. Deep cloning protects database data when applying save-time transformations

#### `<Obsolete()>` Attribute

**Purpose:** Marks a code element (property, method, class) as deprecated - meaning it should no longer be used and may be removed in future versions.

**What it does:**
- Generates a compiler warning when the marked element is used
- Serves as documentation to other developers that this code is outdated
- Can optionally generate a compiler error instead of warning

**In this codebase:** The `<Obsolete()>` attributes appear on legacy NFO format properties like `TMDbIdSpecified`, `RatingSpecified`, `VotesSpecified`, and `ReleaseDateSpecified`. These exist for backward compatibility with older NFO files but the newer Kodi format properties (like `Ratings_Kodi`, `UniqueIDs_Kodi`) should be preferred.

| Attribute | Purpose | Effect |
|-----------|---------|--------|
| `<Serializable()>` | Enable serialization | Allows binary/XML conversion for NFO and cloning |
| `<Obsolete()>` | Mark deprecated code | Compiler warning when used; indicates legacy support |

---

## Part 4: File Naming Conventions

NFO file paths are determined by `FileUtils.GetFilenameList` methods which return lists based on user settings:

| Content Type | Method |
|--------------|--------|
| Movies | `FileUtils.GetFilenameList.Movie(DBElement, Enums.ModifierType.MainNFO)` |
| MovieSets | `FileUtils.GetFilenameList.MovieSet(DBElement, Enums.ModifierType.MainNFO)` |
| TV Shows | `FileUtils.GetFilenameList.TVShow(DBElement, Enums.ModifierType.MainNFO)` |
| TV Episodes | `FileUtils.GetFilenameList.TVEpisode(DBElement, Enums.ModifierType.EpisodeNFO)` |

---

## Part 5: Data Flow Diagrams

### 5.1 Scanning/Reading Flow

    Scanner discovers media files
            |
            v
    Scanner calls Load_* method
            |
            v
    GetFolderContents_* discovers related files
            |
            v
    NFO.LoadFromNFO_* called
            |
            +---> File exists with .nfo extension?
            |           |
            |           v Yes
            |       XmlSerializer deserializes file
            |           |
            |           v
            |       CleanNFO_* normalizes data
            |           |
            |           v
            |       Return MediaContainer object
            |
            +---> No valid NFO?
                        |
                        v
                    GetIMDBFromNonConf extracts IDs
                        |
                        v
                    Optionally rename to .info

### 5.2 Saving Flow

    User edits or scraper updates data
            |
            v
    SaveToNFO_* method called
            |
            v
    Fire module event (OnNFOSave_*)
            |
            v
    Create deep clone of data
            |
            v
    Apply transformations (YAMJ, Boxee, digit grouping)
            |
            v
    For each filename in GetFilenameList:
            |
            +---> Handle readonly attributes
            |
            +---> Optionally rename non-conforming NFO
            |
            +---> XmlSerializer writes to file
            |
            +---> Restore file attributes

---

## Part 6: Settings That Affect NFO Processing

### Reading Settings

| Setting | Effect |
|---------|--------|
| `GeneralOverwriteNfo` | If false, renames non-conforming NFOs to .info instead of overwriting |
| `MovieScraperMetaDataScan` | Triggers MediaInfo scan if NFO has title but no FileInfo |
| `TVScraperMetaDataScan` | Same for TV episodes |

### Writing Settings

| Setting | Effect |
|---------|--------|
| `GeneralOverwriteNfo` | Controls whether non-conforming NFOs are renamed or overwritten |
| `GeneralDigitGrpSymbolVotes` | Formats vote counts with locale-specific grouping |
| `MovieUseYAMJ` | Enables YAMJ-specific NFO formatting |
| `MovieNFOYAMJ` | Part of YAMJ support - clears TMDb ID when saving |
| `MovieScraperCollectionsExtendedInfo` | Controls set node format (simple vs extended) |
| `MovieScraperCollectionsYAMJCompatibleSets` | Enables YAMJ sets format |
| `TVUseBoxee` | Enables Boxee-specific TV show ID handling |
| `TVScraperUseDisplaySeasonEpisode` | Controls displayseason/displayepisode output |

### ID Node Settings

| Setting | Effect |
|---------|--------|
| `MovieScraperIdWriteNodeDefaultId` | Write `<id>` node |
| `MovieScraperIdWriteNodeTMDbId` | Write `<tmdb>` node |
| `MovieScraperIdWriteNodeTMDbCollectionId` | Write `<tmdbcolid>` node |
| `MovieScraperReleaseDateWriteNode` | Write `<releasedate>` node |
| `MovieScraperRatingVotesWriteNode` | Write legacy `<rating>` and `<votes>` nodes |
| `TVScraperIdWriteNodeDefaultId` | Write `<id>` node for TV |
| `TVScraperIdWriteNodeIMDbId` | Write `<imdb>` node for TV |
| `TVScraperIdWriteNodeTMDbId` | Write `<tmdb>` node for TV |
| `TVScraperIdWriteNodeTVDbId` | Write `<tvdb>` node for TV |
| `TVScraperRatingVotesWriteNode` | Write legacy rating/votes for TV |

---

## Part 7: Module Events

The NFO system fires events that addons can hook into:

| Event | When Fired | Purpose |
|-------|------------|---------|
| `OnNFOSave_Movie` | Before movie NFO save | Allow addons to modify/cancel save |
| `OnNFOSave_TVShow` | Before TV show NFO save | Allow addons to modify/cancel save |
| `OnNFORead_TVShow` | After TV show NFO load | Allow addons to process loaded data |

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
8. Uses the *Specified pattern for conditional XML serialization
9. Creates deep clones before save to protect database data