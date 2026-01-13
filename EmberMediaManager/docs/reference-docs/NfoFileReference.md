# Ember Media Manager - NFO and File Naming Reference

| | Document Info |
|---------------|---|
| **Version** | 2.0 |
| **Created** | January 7, 2026 |
| **Updated** | January 7, 2026 |
| **Author** | ulrick65 |
| **Purpose** | Reference guide for NFO XML structure and media file naming conventions |

##### [← Return to Document Index](../DocumentIndex.md) · [→ NFO Process Documentation](../process-docs/NfoFileProcess.md)

---

## Overview

Looking up NFO XML elements or file naming patterns? You're in the right place!

This reference document provides complete specifications for:
- NFO XML structure for all media types (Movies, MovieSets, TV Shows, Episodes)
- XML serialization patterns used in the codebase
- File naming conventions for all media and image types
- ModifierType enum reference

### Kodi Compatibility

This reference documents file naming conventions compatible with **Kodi 19 (Matrix) and above**.

All naming schemes use the modern Kodi standard. The **Expert** scheme allows user-defined custom patterns for special requirements.

**Related documentation:**
- [NfoFileProcess.md](../process-docs/NfoFileProcess.md) — How NFO files are read and written

---

## Table of Contents

- [Part 1: NFO XML Structure](#part-1-nfo-xml-structure)
  - [XML Serialization Patterns](#xml-serialization-patterns)
  - [Movie NFO Structure](#movie-nfo-structure)
  - [MovieSet NFO Structure](#movieset-nfo-structure)
  - [TV Show NFO Structure](#tv-show-nfo-structure)
  - [Episode NFO Structure](#episode-nfo-structure)
  - [Supporting XML Types](#supporting-xml-types)
  - [Key .NET Attributes](#key-net-attributes)
- [Part 2: File Naming Conventions](#part-2-file-naming-conventions)
  - [ModifierType Enum Reference](#modifiertype-enum-reference)
  - [Movie File Naming](#movie-file-naming)
  - [MovieSet File Naming](#movieset-file-naming)
  - [TV Show File Naming](#tv-show-file-naming)
  - [TV Season File Naming](#tv-season-file-naming)
  - [TV Episode File Naming](#tv-episode-file-naming)
- [See Also](#see-also)

---

## [↑](#table-of-contents) Part 1: NFO XML Structure

### [↑](#table-of-contents) XML Serialization Patterns

The XML structure is defined by serializable classes in [clsAPIMediaContainers.vb](../../EmberAPI/clsAPIMediaContainers.vb). These classes use .NET XML serialization attributes.

#### [↑](#table-of-contents) Conditional Serialization with *Specified Properties

The `*Specified` pattern is a .NET XML Serialization convention — the serializer only writes `Xxx` to XML if `XxxSpecified` returns `True`. This prevents empty tags in NFO files.

**Example:**

    <XmlElement("tagline")>
    Public Property Tagline() As String = String.Empty

    <XmlIgnore()>
    Public ReadOnly Property TaglineSpecified() As Boolean
        Get
            Return Not String.IsNullOrEmpty(Tagline)
        End Get
    End Property

---

#### [↑](#table-of-contents) Settings-Controlled Serialization

Some properties only serialize if both a value exists AND a setting is enabled.

**Example:**

    <XmlIgnore()>
    Public ReadOnly Property DefaultIdSpecified() As Boolean
        Get
            Return DefaultId.ValueSpecified AndAlso Master.eSettings.MovieScraperIdWriteNodeDefaultId
        End Get
    End Property

---

#### [↑](#table-of-contents) Internal Properties (Never Serialized)

Properties marked with `<XmlIgnore()>` without a corresponding `*Specified` property are for internal use only.

**Examples:** `Scrapersource`, `Lev`, `ThumbPoster`

---

#### [↑](#table-of-contents) Wrapper Properties for Complex Serialization

The actual data container is ignored, while a wrapper property handles serialization in the correct format.

**Example:** `UniqueIDs` (ignored) vs `UniqueIDs_Kodi` (serialized as array)

---

### [↑](#table-of-contents) Movie NFO Structure

**XmlRoot:** `"movie"`

**Class:** `MediaContainers.Movie`

#### [↑](#table-of-contents) Movie XML Elements

| XML Element | Property | Description | Conditional |
|-------------|----------|-------------|-------------|
| `<title>` | Title | Movie title | If not empty |
| `<originaltitle>` | OriginalTitle | Original language title | If not empty |
| `<sorttitle>` | SortTitle | Title used for sorting | If not empty |
| `<year>` | Year | Release year (derived from Premiered) | If not empty |
| `<premiered>` | Premiered | Premiere date (ISO8601) | If not empty |
| `<releasedate>` | ReleaseDate | Alias for Premiered (legacy) | If setting enabled |
| `<rating>` | Rating | Rating value (legacy format) | If setting enabled |
| `<votes>` | Votes | Vote count (legacy format) | If setting enabled |
| `<ratings>` | Ratings_Kodi | Kodi-format ratings array | If any ratings exist |
| `<userrating>` | UserRating | User's personal rating (1-10) | If not 0 |
| `<top250>` | Top250 | IMDb Top 250 ranking | If not 0 |
| `<outline>` | Outline | Short description | If not empty |
| `<plot>` | Plot | Full plot description | If not empty |
| `<tagline>` | Tagline | Marketing tagline | If not empty |
| `<runtime>` | Runtime | Duration in minutes | If not empty/0 |
| `<mpaa>` | MPAA | Content rating | If not empty |
| `<certification>` | Certifications | Country-specific ratings | If any exist |
| `<id>` | DefaultId | Default unique ID | If setting enabled |
| `<uniqueid>` | UniqueIDs_Kodi | Kodi-format unique IDs | If any exist |
| `<tmdb>` | TMDbId | TMDb identifier | If setting enabled |
| `<tmdbcolid>` | TMDbCollectionId | TMDb Collection ID | If setting enabled |
| `<genre>` | Genres | Genre list | If any exist |
| `<studio>` | Studios | Production studios | If any exist |
| `<country>` | Countries | Production countries | If any exist |
| `<language>` | Language | Primary language | If not empty |
| `<director>` | Directors | Director names | If any exist |
| `<credits>` | Credits | Writer credits | If any exist |
| `<actor>` | Actors | Cast list with Person details | If any exist |
| `<set>` | Set_Kodi | Movie set (Kodi format) | If any sets exist |
| `<tag>` | Tags | User tags | If any exist |
| `<showlink>` | ShowLinks | Linked TV shows | If any exist |
| `<trailer>` | Trailer | Trailer URL | If not empty |
| `<thumb>` | Thumb | Poster URLs | If any exist |
| `<fanart>` | Fanart | Fanart URLs | If URL not empty |
| `<fileinfo>` | FileInfo | Media technical details | If streams exist |
| `<playcount>` | PlayCount | Watch count | If > 0 |
| `<lastplayed>` | LastPlayed | Last watched date | If not empty |
| `<dateadded>` | DateAdded | Library add date | If not empty |
| `<datemodified>` | DateModified | Last modified date | If not empty |
| `<videosource>` | VideoSource | Video source type | If not empty |
| `<edition>` | Edition | Movie edition | If not empty |
| `<locked>` | Locked | Lock state | Always |
| `<user_note>` | UserNote | User notes | If not empty |

**Internal Properties (not serialized):** `Scrapersource`, `Lev`, `ThumbPoster`

---

### [↑](#table-of-contents) MovieSet NFO Structure

**XmlRoot:** `"movieset"`

**Class:** `MediaContainers.Movieset`

#### [↑](#table-of-contents) MovieSet XML Elements

| XML Element | Property | Description | Conditional |
|-------------|----------|-------------|-------------|
| `<title>` | Title | Collection name | If not empty |
| `<plot>` | Plot | Collection description | If not empty |
| `<language>` | Language | Language code | If not empty |
| `<id>` | DefaultId | Default unique ID | If value exists |
| `<uniqueid>` | UniqueIDs_Kodi | Unique identifiers | If any exist |
| `<locked>` | Locked | Lock state | Always |

**Internal Properties:** `OldTitle` (for detecting renames)

---

### [↑](#table-of-contents) TV Show NFO Structure

**XmlRoot:** `"tvshow"`

**Class:** `MediaContainers.TVShow`

#### [↑](#table-of-contents) TV Show XML Elements

| XML Element | Property | Description | Conditional |
|-------------|----------|-------------|-------------|
| `<title>` | Title | Show title | If not empty |
| `<originaltitle>` | OriginalTitle | Original title | If not empty |
| `<sorttitle>` | SortTitle | Sort title | If not empty |
| `<tagline>` | Tagline | Show tagline | If not empty |
| `<rating>` | Rating | Show rating (legacy) | If setting enabled |
| `<votes>` | Votes | Vote count (legacy) | If setting enabled |
| `<ratings>` | Ratings_Kodi | Kodi ratings | If any exist |
| `<userrating>` | UserRating | User rating | If not 0 |
| `<plot>` | Plot | Show description | If not empty |
| `<mpaa>` | MPAA | Content rating | If not empty |
| `<certification>` | Certifications | Certifications list | If any exist |
| `<premiered>` | Premiered | First air date | If not empty |
| `<status>` | Status | Show status | If not empty |
| `<studio>` | Studios | Networks | If any exist |
| `<genre>` | Genres | Genres | If any exist |
| `<country>` | Countries | Production countries | If any exist |
| `<director>` | Directors | Directors | If any exist |
| `<actor>` | Actors | Cast | If any exist |
| `<creator>` | Creators | Show creators | If any exist |
| `<tag>` | Tags | User tags | If any exist |
| `<runtime>` | Runtime | Episode runtime | If not empty/0 |
| `<episodeguide>` | EpisodeGuide | Episode guide URL | If not empty |
| `<id>` | DefaultId | Default ID | If setting enabled |
| `<uniqueid>` | UniqueIDs_Kodi | Unique IDs | If any exist |
| `<imdb>` | IMDbId | IMDb ID | If setting enabled |
| `<tmdb>` | TMDbId | TMDb ID | If setting enabled |
| `<tvdb>` | TVDbId | TVDb ID | If setting enabled |
| `<seasons>` | Seasons | Season details container | If any exist |
| `<locked>` | Locked | Lock state | Always |
| `<user_note>` | UserNote | User notes | If not empty |

**Internal Properties:** `Scrapersource`, `KnownEpisodes`, `KnownSeasons`

---

### [↑](#table-of-contents) Episode NFO Structure

**XmlRoot:** `"episodedetails"`

**Class:** `MediaContainers.EpisodeDetails`

#### [↑](#table-of-contents) Episode XML Elements

| XML Element | Property | Description | Conditional |
|-------------|----------|-------------|-------------|
| `<title>` | Title | Episode title | If not empty |
| `<originaltitle>` | OriginalTitle | Original title | If not empty |
| `<season>` | Season | Season number | Always |
| `<episode>` | Episode | Episode number | Always |
| `<subepisode>` | SubEpisode | Sub-episode number | If not -1 |
| `<displayseason>` | DisplaySeason | Display season number | If setting enabled |
| `<displayepisode>` | DisplayEpisode | Display episode number | If setting enabled |
| `<aired>` | Aired | Air date | If not empty |
| `<rating>` | Rating | Episode rating (legacy) | If setting enabled |
| `<votes>` | Votes | Vote count (legacy) | If setting enabled |
| `<ratings>` | Ratings_Kodi | Kodi ratings | If any exist |
| `<userrating>` | UserRating | User rating | If not 0 |
| `<plot>` | Plot | Episode description | If not empty |
| `<runtime>` | Runtime | Duration | If not empty/0 |
| `<videosource>` | VideoSource | Video source | If not empty |
| `<director>` | Directors | Directors | If any exist |
| `<credits>` | Credits | Writers | If any exist |
| `<actor>` | Actors | Episode cast | If any exist |
| `<gueststar>` | GuestStars | Guest stars | If any exist |
| `<fileinfo>` | FileInfo | Technical details | If streams exist |
| `<id>` | DefaultId | Default ID | If setting enabled |
| `<uniqueid>` | UniqueIDs_Kodi | Unique IDs | If any exist |
| `<imdb>` | IMDbId | IMDb ID | If setting enabled |
| `<tmdb>` | TMDbId | TMDb ID | If setting enabled |
| `<playcount>` | Playcount | Watch count | If > 0 |
| `<lastplayed>` | LastPlayed | Last watched | If not empty |
| `<dateadded>` | DateAdded | Library add date | If not empty |
| `<locked>` | Locked | Lock state | Always |
| `<user_note>` | UserNote | User notes | If not empty |

**Internal Properties:** `Scrapersource`, `ThumbPoster`, `EpisodeAbsolute`, `EpisodeCombined`, `EpisodeDVD`, `SeasonCombined`, `SeasonDVD`

---

### [↑](#table-of-contents) Supporting XML Types

#### [↑](#table-of-contents) Person Class (Actors/GuestStars)

| XML Element | Property | Description |
|-------------|----------|-------------|
| `<name>` | Name | Actor name |
| `<role>` | Role | Character played |
| `<order>` | Order | Billing order |
| `<thumb>` | URLOriginal | Actor thumbnail URL |
| `<imdbid>` | IMDB | IMDb person ID |
| `<tmdbid>` | TMDB | TMDb person ID |
| `<tvdbid>` | TVDB | TVDb person ID |

---

#### [↑](#table-of-contents) Fileinfo/StreamDetails

| XML Element | Content |
|-------------|---------|
| `<video>` | Video stream info (codec, width, height, aspect, duration, etc.) |
| `<audio>` | Audio stream info (codec, channels, language, longlanguage) |
| `<subtitle>` | Subtitle stream info (language, longlanguage, forced, path, type) |

---

#### [↑](#table-of-contents) RatingDetails (Kodi Format)

    <rating name="imdb" max="10" default="true">
        <value>8.5</value>
        <votes>125000</votes>
    </rating>

---

#### [↑](#table-of-contents) Uniqueid (Kodi Format)

    <uniqueid type="imdb" default="true">tt1234567</uniqueid>
    <uniqueid type="tmdb">12345</uniqueid>

---

### [↑](#table-of-contents) Key .NET Attributes

| Attribute | Purpose | Effect |
|-----------|---------|--------|
| `<Serializable()>` | Enable serialization | Allows binary/XML conversion for NFO and cloning |
| `<XmlRoot("name")>` | Set root element | Defines the XML root element name |
| `<XmlElement("name")>` | Set element name | Defines the XML element name for property |
| `<XmlIgnore()>` | Exclude from XML | Property not serialized to XML |
| `<XmlArray("name")>` | Array container | Defines container element for arrays |
| `<XmlArrayItem("name")>` | Array item | Defines element name for array items |
| `<Obsolete()>` | Mark deprecated | Compiler warning when used; legacy support |

---

## [↑](#table-of-contents) Part 2: File Naming Conventions

File naming is handled by [clsAPIFileUtils.vb](../../EmberAPI/clsAPIFileUtils.vb) through the `GetFilenameList` class. Each method returns a list of filenames based on user settings and naming scheme selections.

### Naming Schemes

| Scheme | Description |
|--------|-------------|
| **Kodi** | Standard Kodi 19 (Matrix)+ compatible naming |
| **Expert** | User-defined custom patterns |

---

### [↑](#table-of-contents) ModifierType Enum Reference

The `ModifierType` enum (defined in [clsAPICommon.vb](../../EmberAPI/clsAPICommon.vb)) identifies what type of file is being processed:

| ModifierType | Description | Used By |
|--------------|-------------|---------|
| `All` | All file types | General operations |
| `AllSeasonsBanner` | All Seasons banner image | TV Show |
| `AllSeasonsFanart` | All Seasons fanart image | TV Show |
| `AllSeasonsLandscape` | All Seasons landscape image | TV Show |
| `AllSeasonsPoster` | All Seasons poster image | TV Show |
| `DoSearch` | Search operation marker | Internal |
| `EpisodeActorThumbs` | Episode actor thumbnail images | TV Episode |
| `EpisodeFanart` | Episode fanart image | TV Episode |
| `EpisodePoster` | Episode poster/thumb image | TV Episode |
| `EpisodeMeta` | Episode metadata file | TV Episode |
| `EpisodeNFO` | Episode NFO file | TV Episode |
| `EpisodeSubtitle` | Episode subtitle files | TV Episode |
| `MainActorThumbs` | Actor thumbnail images | Movie, TV Show |
| `MainBanner` | Banner image | Movie, TV Show |
| `MainCharacterArt` | Character art image | Movie, TV Show |
| `MainClearArt` | Clear art image | Movie, TV Show |
| `MainClearLogo` | Clear logo image | Movie, TV Show |
| `MainDiscArt` | Disc art image | Movie, MovieSet |
| `MainExtrafanarts` | Extra fanart images folder | Movie, TV Show |
| `MainFanart` | Primary fanart image | Movie, MovieSet, TV Show |
| `MainKeyart` | Keyart image | Movie, MovieSet, TV Show |
| `MainLandscape` | Landscape/thumb image | Movie, MovieSet, TV Show |
| `MainNFO` | NFO file | Movie, MovieSet, TV Show |
| `MainPoster` | Primary poster image | Movie, MovieSet, TV Show |
| `MainSubtitle` | Subtitle files | Movie |
| `MainTrailer` | Trailer video file | Movie |
| `SeasonBanner` | Season banner image | TV Season |
| `SeasonFanart` | Season fanart image | TV Season |
| `SeasonLandscape` | Season landscape image | TV Season |
| `SeasonPoster` | Season poster image | TV Season |
| `withEpisodes` | Include episodes flag | TV operations |
| `withSeasons` | Include seasons flag | TV operations |

---

### [↑](#table-of-contents) Movie File Naming

**Method:** `FileUtils.GetFilenameList.Movie(DBElement, ModifierType)`

**Source File:** [clsAPIFileUtils.vb](../../EmberAPI/clsAPIFileUtils.vb)

File naming varies based on:
- Single movie vs. multi-movie folder
- VIDEO_TS structure vs. BDMV structure vs. standard file

#### [↑](#table-of-contents) Movie NFO Files

| Scheme | Single Movie | Multi-Movie | VIDEO_TS | BDMV |
|--------|--------------|-------------|----------|------|
| Kodi | `<filename>.nfo` | `<filename>.nfo` | `VIDEO_TS.nfo` | `index.nfo` |
| Expert | User-defined | User-defined | User-defined | User-defined |

---

#### [↑](#table-of-contents) Movie Poster Files

| Scheme | Single Movie | Multi-Movie | VIDEO_TS | BDMV |
|--------|--------------|-------------|----------|------|
| Kodi | `<filename>-poster.jpg` | `<filename>-poster.jpg` | `poster.jpg` | `poster.jpg` |
| Expert | User-defined | User-defined | User-defined | User-defined |

---

#### [↑](#table-of-contents) Movie Fanart Files

| Scheme | Single Movie | Multi-Movie | VIDEO_TS | BDMV |
|--------|--------------|-------------|----------|------|
| Kodi | `<filename>-fanart.jpg` | `<filename>-fanart.jpg` | `fanart.jpg` | `fanart.jpg` |
| Expert | User-defined | User-defined | User-defined | User-defined |

---

#### [↑](#table-of-contents) Movie Banner Files

| Scheme | Single Movie | VIDEO_TS/BDMV |
|--------|--------------|---------------|
| Kodi | `<filename>-banner.jpg` | `banner.jpg` |
| Expert | User-defined | User-defined |

---

#### [↑](#table-of-contents) Movie Landscape Files

| Scheme | Single Movie | VIDEO_TS/BDMV |
|--------|--------------|---------------|
| Kodi | `<filename>-landscape.jpg` | `landscape.jpg` |
| Expert | User-defined | User-defined |

---

#### [↑](#table-of-contents) Movie ClearArt Files

| Scheme | Single Movie | VIDEO_TS/BDMV |
|--------|--------------|---------------|
| Kodi | `<filename>-clearart.png` | `clearart.png` |
| Expert | User-defined | User-defined |

---

#### [↑](#table-of-contents) Movie ClearLogo Files

| Scheme | Single Movie | VIDEO_TS/BDMV |
|--------|--------------|---------------|
| Kodi | `<filename>-clearlogo.png` | `clearlogo.png` |
| Expert | User-defined | User-defined |

---

#### [↑](#table-of-contents) Movie DiscArt Files

| Scheme | Single Movie | VIDEO_TS/BDMV |
|--------|--------------|---------------|
| Kodi | `<filename>-discart.png` | `discart.png` |
| Expert | User-defined | User-defined |

---

#### [↑](#table-of-contents) Movie Keyart Files

| Scheme | Single Movie | VIDEO_TS/BDMV |
|--------|--------------|---------------|
| Kodi | `<filename>-keyart.jpg` | `keyart.jpg` |
| Expert | User-defined | User-defined |

---

#### [↑](#table-of-contents) Movie CharacterArt Files

| Scheme | Single Movie | VIDEO_TS/BDMV |
|--------|--------------|---------------|
| Kodi | `<filename>-characterart.png` | `characterart.png` |
| Expert | User-defined | User-defined |

---

#### [↑](#table-of-contents) Movie Extrafanarts

| Scheme | Location |
|--------|----------|
| Kodi | `<movie folder>/extrafanart/` |
| Expert | `<movie folder>/extrafanart/` |

**Current Behavior:** Images saved with random numeric filenames inside the `extrafanart/` subfolder.

**Note:** See [BL-CC-002](../improvements-docs/backlog/BL-CC-002-KodiFanartNaming.md) for planned update to Kodi-compliant sequential naming (`fanart1.jpg`, `fanart2.jpg`, etc.) in the main folder.

---

#### [↑](#table-of-contents) Movie Actor Thumbs

| Scheme | Location |
|--------|----------|
| Kodi | `<movie folder>/.actors/<actorname>.jpg` |
| Expert | `<movie folder>/.actors/<actorname>.<ext>` |

---

#### [↑](#table-of-contents) Movie Trailer Files

| Scheme | Single Movie | VIDEO_TS/BDMV |
|--------|--------------|---------------|
| Kodi | `<filename>-trailer.<ext>` | `<folder>-trailer.<ext>` |
| Expert | User-defined | User-defined |

---

### [↑](#table-of-contents) MovieSet File Naming

**Method:** `FileUtils.GetFilenameList.MovieSet(DBElement, ModifierType)`

MovieSet files are stored in a central location based on settings. The `<settitle>` placeholder is replaced with the sanitized set title.

#### [↑](#table-of-contents) MovieSet NFO Files

| Scheme | Location |
|--------|----------|
| Kodi | `<path>/<settitle>/<settitle>.nfo` |
| Expert | `<path>/<settitle>.nfo` (customizable) |

---

#### [↑](#table-of-contents) MovieSet Poster Files

| Scheme | Location |
|--------|----------|
| Kodi | `<path>/<settitle>/poster.jpg` |
| Expert | `<path>/<settitle>.jpg` (customizable) |

---

#### [↑](#table-of-contents) MovieSet Fanart Files

| Scheme | Location |
|--------|----------|
| Kodi | `<path>/<settitle>/fanart.jpg` |
| Expert | `<path>/<settitle>-fanart.jpg` (customizable) |

---

#### [↑](#table-of-contents) MovieSet Banner Files

| Scheme | Location |
|--------|----------|
| Kodi | `<path>/<settitle>/banner.jpg` |
| Expert | User-defined |

---

#### [↑](#table-of-contents) MovieSet Landscape Files

| Scheme | Location |
|--------|----------|
| Kodi | `<path>/<settitle>/landscape.jpg` |
| Expert | User-defined |

---

#### [↑](#table-of-contents) MovieSet ClearArt Files

| Scheme | Location |
|--------|----------|
| Kodi | `<path>/<settitle>/clearart.png` |
| Expert | User-defined |

---

#### [↑](#table-of-contents) MovieSet ClearLogo Files

| Scheme | Location |
|--------|----------|
| Kodi | `<path>/<settitle>/clearlogo.png` |
| Expert | User-defined |

---

#### [↑](#table-of-contents) MovieSet DiscArt Files

| Scheme | Location |
|--------|----------|
| Kodi | `<path>/<settitle>/discart.png` |
| Expert | User-defined |

---

#### [↑](#table-of-contents) MovieSet Keyart Files

| Scheme | Location |
|--------|----------|
| Kodi | `<path>/<settitle>/keyart.jpg` |
| Expert | User-defined |

---

### [↑](#table-of-contents) TV Show File Naming

**Method:** `FileUtils.GetFilenameList.TVShow(DBElement, ModifierType)`

TV Show files are stored in the show's root folder.

#### [↑](#table-of-contents) TV Show NFO Files

| Scheme | Location |
|--------|----------|
| Kodi | `<show folder>/tvshow.nfo` |
| Expert | `<show folder>/<custom>.nfo` |

---

#### [↑](#table-of-contents) TV Show Poster Files

| Scheme | Location |
|--------|----------|
| Kodi | `<show folder>/poster.jpg` |
| Expert | `<show folder>/<custom>.jpg` |

---

#### [↑](#table-of-contents) TV Show Fanart Files

| Scheme | Location |
|--------|----------|
| Kodi | `<show folder>/fanart.jpg` |
| Expert | `<show folder>/<custom>.jpg` |

---

#### [↑](#table-of-contents) TV Show Banner Files

| Scheme | Location |
|--------|----------|
| Kodi | `<show folder>/banner.jpg` |
| Expert | `<show folder>/<custom>.jpg` |

---

#### [↑](#table-of-contents) TV Show Landscape Files

| Scheme | Location |
|--------|----------|
| Kodi | `<show folder>/landscape.jpg` |
| Expert | `<show folder>/<custom>.jpg` |

---

#### [↑](#table-of-contents) TV Show ClearArt Files

| Scheme | Location |
|--------|----------|
| Kodi | `<show folder>/clearart.png` |
| Expert | `<show folder>/<custom>.png` |

---

#### [↑](#table-of-contents) TV Show ClearLogo Files

| Scheme | Location |
|--------|----------|
| Kodi | `<show folder>/clearlogo.png` |
| Expert | `<show folder>/<custom>.png` |

---

#### [↑](#table-of-contents) TV Show CharacterArt Files

| Scheme | Location |
|--------|----------|
| Kodi | `<show folder>/characterart.png` |
| Expert | `<show folder>/<custom>.png` |

---

#### [↑](#table-of-contents) TV Show Keyart Files

| Scheme | Location |
|--------|----------|
| Kodi | `<show folder>/keyart.jpg` |
| Expert | `<show folder>/<custom>.jpg` |

---

#### [↑](#table-of-contents) TV Show Extrafanarts

| Scheme | Location |
|--------|----------|
| Kodi | `<show folder>/extrafanart/` |
| Expert | `<show folder>/extrafanart/` |

---

#### [↑](#table-of-contents) TV Show Actor Thumbs

| Scheme | Location |
|--------|----------|
| Kodi | `<show folder>/.actors/<actorname>.jpg` |
| Expert | `<show folder>/.actors/<actorname>.<ext>` |

---

### [↑](#table-of-contents) TV Season File Naming

**Method:** `FileUtils.GetFilenameList.TVSeason(DBElement, ModifierType)`

Season files are stored in the show folder.

#### [↑](#table-of-contents) All Seasons Images

| Image Type | Location |
|------------|----------|
| Banner | `<show folder>/season-all-banner.jpg` |
| Fanart | `<show folder>/season-all-fanart.jpg` |
| Landscape | `<show folder>/season-all-landscape.jpg` |
| Poster | `<show folder>/season-all-poster.jpg` |

---

#### [↑](#table-of-contents) Season Poster Files

| Scheme | Regular Season | Season Specials (S00) |
|--------|----------------|----------------------|
| Kodi | `<show folder>/season##-poster.jpg` | `<show folder>/season-specials-poster.jpg` |
| Expert | `<show folder>/season{0}-poster.jpg` | `<show folder>/season-specials-poster.jpg` |

**Note:** `{0}` = zero-padded season (01), `{1}` = unpadded season (1)

---

#### [↑](#table-of-contents) Season Banner Files

| Scheme | Regular Season | Season Specials (S00) |
|--------|----------------|----------------------|
| Kodi | `<show folder>/season##-banner.jpg` | `<show folder>/season-specials-banner.jpg` |
| Expert | `<show folder>/season{0}-banner.jpg` | `<show folder>/season-specials-banner.jpg` |

---

#### [↑](#table-of-contents) Season Fanart Files

| Scheme | Regular Season | Season Specials (S00) |
|--------|----------------|----------------------|
| Kodi | `<show folder>/season##-fanart.jpg` | `<show folder>/season-specials-fanart.jpg` |
| Expert | `<show folder>/season{0}-fanart.jpg` | `<show folder>/season-specials-fanart.jpg` |

---

#### [↑](#table-of-contents) Season Landscape Files

| Scheme | Regular Season | Season Specials (S00) |
|--------|----------------|----------------------|
| Kodi | `<show folder>/season##-landscape.jpg` | `<show folder>/season-specials-landscape.jpg` |
| Expert | `<show folder>/season{0}-landscape.jpg` | `<show folder>/season-specials-landscape.jpg` |

---

### [↑](#table-of-contents) TV Episode File Naming

**Method:** `FileUtils.GetFilenameList.TVEpisode(DBElement, ModifierType)`

Episode files are named based on the episode's video filename.

#### [↑](#table-of-contents) Episode NFO Files

| Scheme | Location |
|--------|----------|
| Kodi | `<episode filename>.nfo` |
| Expert | `<episode path>/<custom pattern>.nfo` |

---

#### [↑](#table-of-contents) Episode Poster/Thumb Files

| Scheme | Location |
|--------|----------|
| Kodi | `<episode filename>-thumb.jpg` |
| Expert | `<episode path>/<custom pattern>.jpg` |

---

#### [↑](#table-of-contents) Episode Fanart Files

| Scheme | Location |
|--------|----------|
| Expert | `<episode path>/<custom pattern>-fanart.jpg` |

---

#### [↑](#table-of-contents) Episode Actor Thumbs

| Scheme | Location |
|--------|----------|
| Kodi | `<episode folder>/.actors/<actorname>.jpg` |
| Expert | `<episode folder>/.actors/<actorname>.<ext>` |

---

## [↑](#table-of-contents) See Also

### [↑](#table-of-contents) Related Documentation

- [NfoFileProcess.md](../process-docs/NfoFileProcess.md) — NFO read/write process documentation
- [DocumentIndex.md](../DocumentIndex.md) — Master documentation index
- [ImageSelectionReference.md](ImageSelectionReference.md) — Image selection reference

---

### [↑](#table-of-contents) Related Source Files

| File | Purpose |
|------|---------|
| [clsAPIMediaContainers.vb](../../EmberAPI/clsAPIMediaContainers.vb) | XML serialization classes |
| [clsAPIFileUtils.vb](../../EmberAPI/clsAPIFileUtils.vb) | File naming logic |
| [clsAPICommon.vb](../../EmberAPI/clsAPICommon.vb) | ModifierType enum |

---

### [↑](#table-of-contents) Related Backlog Items

| Item | Description |
|------|-------------|
| [BL-CC-002](../improvements-docs/backlog/BL-CC-002-KodiFanartNaming.md) | Kodi-compliant fanart naming |

---

*End of file*