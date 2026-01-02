
<!--This document is best viewed in Preview mode for readability and document linking-->

# Ember Media Manager - Project Index

| |Document Info |
|---------------|---|
| **Version** | 1.1 |
| **Created** | January 1, 2026 |
| **Updated** | January 1, 2026 |
| **Author** | Eric H. Anderson |
| **Purpose** | Index and overview for all projects in the Ember Media Manager solution

###### [← Return to Document Index](DocumentIndex.md)

---

## Overview

Need to know what's what in the Ember Media Manager codebase? You're in the right place!

This guide covers all 24 projects in the solution — from the core application to every scraper and addon. Each entry includes the project file location, key files, and what the project actually does.

**Quick links:**
- Jump to the [Quick Reference Table](#quick-reference-table) for a birds-eye view
- Check [Naming Conventions](#naming-conventions) to understand the project prefixes
- See [Folder vs Project Name Mismatches](#folder-vs-project-name-mismatches) if a folder name doesn't match what you expected

---

## Table of Contents

- [Solution Overview](#solution-overview)
- [Core Projects](#core-projects)
  - [EmberMediaManager](#embermediamanager)
  - [EmberAPI](#emberapi)
  - [KodiAPI](#kodiapi)
- [Data Scrapers](#data-scrapers)
  - [scraper.Data.TMDB](#scraper.data.tmdb)
  - [scraper.Data.IMDB](#scraper.data.imdb)
  - [scraper.Data.TVDB](#scraper.data.tvdb)
  - [scraper.Data.OMDb](#scraper.data.omdb)
  - [scraper.Data.Trakttv](#scraper.data.trakttv)
- [Image Scrapers](#image-scrapers)
  - [scraper.Image.TMDB](#scraper.image.tmdb)
  - [scraper.Image.FanartTV](#scraper.image.fanarttv)
  - [scraper.Image.TVDB](#scraper.image.tvdb)
- [Trailer Scrapers](#trailer-scrapers)
  - [scraper.Trailer.TMDB](#scraper.trailer.tmdb)
  - [scraper.Trailer.YouTube](#scraper.trailer.youtube)
- [Generic Addons - Tools](#generic-addons---tools)
  - [generic.EmberCore.BulkRename](#generic.embercore.bulkrename)
  - [generic.EmberCore.MediaFileManager](#generic.embercore.mediafilemanager)
  - [generic.EmberCore.MovieExporter](#generic.embercore.movieexporter)
  - [generic.EmberCore.TagManager](#generic.embercore.tagmanager)
- [Generic Addons - Mapping & Settings](#generic-addons---mapping--settings)
  - [generic.EmberCore.Mapping](#generic.embercore.mapping)
  - [generic.EmberCore.MetadataEditor](#generic.embercore.metadataeditor)
  - [generic.EmberCore.VideoSourceMapping](#generic.embercore.videosourcemapping)
  - [generic.EmberCore.MediaListEditor](#generic.embercore.medialisteditor)
  - [generic.EmberCore.ContextMenu](#generic.embercore.contextmenu)
- [Generic Addons - External Integrations](#generic-addons---external-integrations)
  - [generic.Interface.Kodi](#generic.interface.kodi)
  - [generic.Interface.Trakttv](#generic.interface.trakttv)
- [Project Dependencies](#project-dependencies)
- [Naming Conventions](#naming-conventions)
- [Quick Reference Table](#quick-reference-table)
- [Folder vs Project Name Mismatches](#folder-vs-project-name-mismatches)
- [See Also](#see-also)

**Related Documentation:** [Document Index](DocumentIndex.md) - Master index for all project documentation

---

## Solution Overview

The Ember Media Manager solution contains **24 projects** organized into three categories:

| Category | Count | Description |
|----------|-------|-------------|
| **Core** | 3 | Main application, API library, Kodi integration |
| **Scrapers** | 11 | Data, image, and trailer scrapers |
| **Generic Addons** | 10 | Tools, utilities, and integrations |

---

## Core Projects

### [↑](#table-of-contents) EmberMediaManager

| Property | Value |
|----------|-------|
| **Project File** | `EmberMediaManager\EmberMediaManager.vbproj` |
| **Type** | Windows Forms Application (EXE) |
| **Purpose** | Main application executable and UI |
| **Key Files** | [`frmMain.vb`](../../EmberMediaManager/frmMain.vb), [`ApplicationEvents.vb`](../../EmberMediaManager/ApplicationEvents.vb) |

The main application that users interact with. Contains:
- Main window with movie/TV show lists
- Edit dialogs for movies, TV shows, episodes
- Image selection dialogs
- Settings dialogs
- Background workers for scraping operations

---

### [↑](#table-of-contents) EmberAPI

| Property | Value |
|----------|-------|
| **Project File** | `EmberAPI\EmberAPI.vbproj` |
| **Type** | Class Library (DLL) |
| **Purpose** | Core API shared by all addons |
| **Key Files** | [`clsAPIMaster.vb`](../../EmberAPI/clsAPIMaster.vb), [`clsAPIDatabase.vb`](../../EmberAPI/clsAPIDatabase.vb), [`clsAPIModules.vb`](../../EmberAPI/clsAPIModules.vb) |

The foundation library that all other projects depend on. Contains:
- `Master` class - Global application state and settings
- `Database` class - SQLite database operations
- `ModulesManager` - Addon loading and execution
- Media containers (Movie, TVShow, etc.)
- Image handling and caching
- NFO file parsing/writing
- Performance tracking utilities

---

### [↑](#table-of-contents) KodiAPI

| Property | Value |
|----------|-------|
| **Project File** | `KodiAPI\KodiAPI.csproj` |
| **Type** | Class Library (DLL) - **C#** |
| **Purpose** | JSON-RPC client for Kodi communication |
| **Key Files** | [`Client.cs`](../../KodiAPI/Client.cs) |

**Note:** This is the only C# project in the solution.

Provides the low-level JSON-RPC communication layer for talking to Kodi/XBMC media centers. Used by `generic.Interface.Kodi`.

---

## Data Scrapers

These addons retrieve **metadata** (title, plot, cast, ratings, etc.) from online sources.

### [↑](#table-of-contents) scraper.Data.TMDB

| Property | Value |
|----------|-------|
| **Project File** | `Addons\scraper.TMDB.Data\scraper.Data.TMDB.vbproj` |
| **Folder** | `Addons\scraper.TMDB.Data\` |
| **Module Name** | "TMDB" |
| **Key File** | [`Scraper\clsScrapeTMDB.vb`](../../Addons/scraper.TMDB.Data/Scraper/clsScrapeTMDB.vb) |
| **API** | TheMovieDB.org |

Primary metadata scraper for movies and TV shows. Retrieves:
- Title, plot, tagline, runtime
- Cast, directors, writers
- Ratings, certifications, genres
- Collection/set information
- Unique IDs (TMDB, IMDB)

---

### [↑](#table-of-contents) scraper.Data.IMDB

| Property | Value |
|----------|-------|
| **Project File** | `Addons\scraper.IMDB.Data\scraper.Data.IMDB.vbproj` |
| **Folder** | `Addons\scraper.IMDB.Data\` |
| **Module Name** | "IMDB" |
| **Key File** | [`Scraper\clsScrapeIMDB.vb`](../../Addons/scraper.IMDB.Data/Scraper/clsScrapeIMDB.vb) |
| **API** | IMDb (web scraping) |

Supplementary metadata scraper. Particularly useful for:
- IMDB ratings and vote counts
- Top 250 ranking
- Additional cast information

---

### [↑](#table-of-contents) scraper.Data.TVDB

| Property | Value |
|----------|-------|
| **Project File** | `Addons\scraper.Data.TVDB\scraper.Data.TVDB.vbproj` |
| **Folder** | `Addons\scraper.Data.TVDB\` |
| **Module Name** | "TVDB" |
| **Key File** | [`Scraper\clsScrapeTVDB.vb`](../../Addons/scraper.Data.TVDB/Scraper/clsScrapeTVDB.vb) |
| **API** | TheTVDB.com |

TV show metadata scraper. Provides:
- Episode ordering (standard, DVD, absolute)
- Detailed episode information
- Season details
- TVDB IDs for cross-referencing

---

### [↑](#table-of-contents) scraper.Data.OMDb

| Property | Value |
|----------|-------|
| **Project File** | `Addons\scraper.Data.OMDb\scraper.Data.OMDb.vbproj` |
| **Folder** | `Addons\scraper.Data.OMDb\` |
| **Module Name** | "OMDb" |
| **Key File** | [`Scraper\clsScrapeOMDB.vb`](../../Addons/scraper.Data.OMDb/Scraper/clsScrapeOMDB.vb) |
| **API** | OMDb API (Open Movie Database) |

Lightweight metadata scraper. Good for:
- Quick lookups by IMDB ID
- Rotten Tomatoes ratings
- Basic movie information

---

### [↑](#table-of-contents) scraper.Data.Trakttv

| Property | Value |
|----------|-------|
| **Project File** | `Addons\scraper.Trakttv.Data\scraper.Data.Trakttv.vbproj` |
| **Folder** | `Addons\scraper.Trakttv.Data\` |
| **Module Name** | "Trakt.tv" |
| **Key File** | [`clsAPITrakt.vb`](../../Addons/scraper.Trakttv.Data/clsAPITrakt.vb) |
| **API** | Trakt.tv |

Social metadata scraper. Provides:
- User ratings and watch counts
- Trending/popular information
- Cross-referencing IDs

---

## Image Scrapers

These addons retrieve **artwork** (posters, fanart, banners, etc.) from online sources.

### [↑](#table-of-contents) scraper.Image.TMDB

| Property | Value |
|----------|-------|
| **Project File** | `Addons\scraper.TMDB.Poster\scraper.Image.TMDB.vbproj` |
| **Folder** | `Addons\scraper.TMDB.Poster\` |
| **Module Name** | "TMDB Images" |
| **Key File** | [`Scraper\clsScrapeTMDB.vb`](../../Addons/scraper.TMDB.Poster/Scraper/clsScrapeTMDB.vb) |
| **API** | TheMovieDB.org |

Primary image source for movies and TV shows:
- Posters (multiple languages)
- Backdrops/fanart
- Provides both thumbnail and full-size URLs

---

### [↑](#table-of-contents) scraper.Image.FanartTV

| Property | Value |
|----------|-------|
| **Project File** | `Addons\scraper.FanartTV.Poster\scraper.Image.FanartTV.vbproj` |
| **Folder** | `Addons\scraper.FanartTV.Poster\` |
| **Module Name** | "Fanart.tv" |
| **Key File** | [`Scraper\clsScrapeFANARTTV.vb`](../../Addons/scraper.FanartTV.Poster/Scraper/clsScrapeFANARTTV.vb) |
| **API** | Fanart.tv |

High-quality artwork source:
- ClearArt, ClearLogo
- CharacterArt, DiscArt
- Landscape thumbs
- HD versions of posters/fanart

---

### [↑](#table-of-contents) scraper.Image.TVDB

| Property | Value |
|----------|-------|
| **Project File** | `Addons\scraper.Image.TVDB\scraper.Image.TVDB.vbproj` |
| **Folder** | `Addons\scraper.Image.TVDB\` |
| **Module Name** | "TVDB Images" |
| **Key File** | [`Scraper\clsScrapeTVDB.vb`](../../Addons/scraper.Image.TVDB/Scraper/clsScrapeTVDB.vb) |
| **API** | TheTVDB.com |

TV-specific artwork:
- Season posters and banners
- Episode thumbnails
- Series banners and fanart

---

## Trailer Scrapers

### [↑](#table-of-contents) scraper.Trailer.TMDB

| Property | Value |
|----------|-------|
| **Project File** | `Addons\scraper.TMDB.Trailer\scraper.Trailer.TMDB.vbproj` |
| **Folder** | `Addons\scraper.TMDB.Trailer\` |
| **Module Name** | "TMDB Trailers" |
| **Key File** | [`Scraper\clsScrapeTMDB.vb`](../../Addons/scraper.TMDB.Trailer/Scraper/clsScrapeTMDB.vb) |

Retrieves trailer links from TMDB's video database.

---

### [↑](#table-of-contents) scraper.Trailer.YouTube

| Property | Value |
|----------|-------|
| **Project File** | `Addons\scraper.Trailer.YouTube\scraper.Trailer.YouTube.vbproj` |
| **Folder** | `Addons\scraper.Trailer.YouTube\` |
| **Module Name** | "YouTube Trailers" |
| **Key File** | [`Scraper\clsScrapeYouTube.vb`](../../Addons/scraper.Trailer.YouTube/Scraper/clsScrapeYouTube.vb) |

Searches YouTube for movie/TV trailers and provides:
- Direct video URLs
- Multiple quality options
- Kodi-compatible URL formats

---

## Generic Addons - Tools

### [↑](#table-of-contents) generic.EmberCore.BulkRename

| Property | Value |
|----------|-------|
| **Project File** | `Addons\generic.EmberCore.BulkRename\generic.EmberCore.BulkRename.vbproj` |
| **Folder** | `Addons\generic.EmberCore.BulkRename\` |
| **Module Name** | "Renamer" |
| **Key Files** | [`Module.BulkRenamer.vb`](../../Addons/generic.EmberCore.BulkRename/Module.BulkRenamer.vb), [`clsAPIRenamer.vb`](../../Addons/generic.EmberCore.BulkRename/clsAPIRenamer.vb) |

**Why "BulkRename"?** This addon renames media **files and folders** in bulk based on metadata patterns.

Features:
- Rename movie files/folders using patterns like `{Title} ({Year})`
- Rename TV episode files using patterns like `{ShowTitle} - S{Season}E{Episode}`
- Preview changes before applying
- Auto-rename after scraping (optional)
- Manual rename via context menu

---

### [↑](#table-of-contents) generic.EmberCore.MediaFileManager

| Property | Value |
|----------|-------|
| **Project File** | `Addons\generic.EmberCore.MediaFileManager\generic.EmberCore.MediaFileManager.vbproj` |
| **Folder** | `Addons\generic.EmberCore.MediaFileManager\` |
| **Module Name** | "Media File Manager" |
| **Key File** | [`Module.MediaFileManagerModule.vb`](../../Addons/generic.EmberCore.MediaFileManager/Module.MediaFileManagerModule.vb) |

Copy or move media files to different locations:
- Move movies to organized folder structures
- Copy TV shows to backup locations
- Bulk operations on selected items

---

### [↑](#table-of-contents) generic.EmberCore.MovieExporter

| Property | Value |
|----------|-------|
| **Project File** | `Addons\generic.EmberCore.MovieExport\generic.EmberCore.MovieExporter.vbproj` |
| **Folder** | `Addons\generic.EmberCore.MovieExport\` |
| **Module Name** | "MovieListExporter" |
| **Key Files** | [`Module.MovieExporter.vb`](../../Addons/generic.EmberCore.MovieExport/Module.MovieExporter.vb), [`clsAPIMediaExporter.vb`](../../Addons/generic.EmberCore.MovieExport/clsAPIMediaExporter.vb) |

Export your movie library to HTML or other formats:
- Customizable HTML templates
- Include posters and metadata
- Create shareable movie lists

---

### [↑](#table-of-contents) generic.EmberCore.TagManager

| Property | Value |
|----------|-------|
| **Project File** | `Addons\generic.EmberCore.TagManager\generic.EmberCore.TagManager.vbproj` |
| **Folder** | `Addons\generic.EmberCore.TagManager\` |
| **Module Name** | "Tag Manager" |
| **Key Files** | [`Module.Tag.vb`](../../Addons/generic.EmberCore.TagManager/Module.Tag.vb), [`dlgTagManager.vb`](../../Addons/generic.EmberCore.TagManager/dlgTagManager.vb) |

Manage tags across your library:
- Create, rename, delete tags
- Bulk assign tags to movies/shows
- Filter library by tags

---

## Generic Addons - Mapping & Settings

### [↑](#table-of-contents) generic.EmberCore.Mapping

| Property | Value |
|----------|-------|
| **Project File** | `Addons\generic.EmberCore.Mapping\generic.EmberCore.Mapping.vbproj` |
| **Folder** | `Addons\generic.EmberCore.Mapping\` |
| **Module Name** | "MappingManager" |
| **Key Files** | [`genericMapping.vb`](../../Addons/generic.EmberCore.Mapping/genericMapping.vb), [`dlgGenreMapping.vb`](../../Addons/generic.EmberCore.Mapping/dlgGenreMapping.vb) |

Standardize and translate metadata values:
- **Genre Mapping** - Translate "Sci-Fi" → "Science Fiction"
- **Studio Mapping** - Normalize studio names
- **Certification Mapping** - Convert ratings between systems
- **Country Mapping** - Standardize country names
- **Status Mapping** - Normalize show statuses
- **Edition Mapping** - Map edition types

---

### [↑](#table-of-contents) generic.EmberCore.MetadataEditor

| Property | Value |
|----------|-------|
| **Project File** | `Addons\generic.Embercore.MetadataEditor\generic.EmberCore.MetadataEditor.vbproj` |
| **Folder** | `Addons\generic.Embercore.MetadataEditor\` |
| **Module Name** | "Audio & Video Codec Mapping" |
| **Key File** | [`genericVCodecEditor.vb`](../../Addons/generic.Embercore.MetadataEditor/genericVCodecEditor.vb) |

Configure how audio/video codec information is displayed:
- Map codec identifiers to friendly names
- Customize codec display strings

---

### [↑](#table-of-contents) generic.EmberCore.VideoSourceMapping

| Property | Value |
|----------|-------|
| **Project File** | `Addons\generic.EmberCore.VideoSourceMapping\generic.EmberCore.VideoSourceMapping.vbproj` |
| **Folder** | `Addons\generic.EmberCore.VideoSourceMapping\` |
| **Module Name** | "Media Sources Editor" |
| **Key File** | [`genericVideoSourceMapping.vb`](../../Addons/generic.EmberCore.VideoSourceMapping/genericVideoSourceMapping.vb) |

Map video source paths and configure media sources:
- Define source folders
- Configure scan settings per source

---

### [↑](#table-of-contents) generic.EmberCore.MediaListEditor

| Property | Value |
|----------|-------|
| **Project File** | `Addons\generic.EmberCore.FilterEditor\generic.EmberCore.MediaListEditor.vbproj` |
| **Folder** | `Addons\generic.EmberCore.FilterEditor\` |
| **Module Name** | "Media List Editor" |
| **Key File** | [`genericMediaListEditor.vb`](../../Addons/generic.EmberCore.FilterEditor/genericMediaListEditor.vb) |

**Note:** Folder name says "FilterEditor" but it's the Media List Editor.

Edit and manage custom lists for filtering the library view.

---

### [↑](#table-of-contents) generic.EmberCore.ContextMenu

| Property | Value |
|----------|-------|
| **Project File** | `Addons\generic.EmberCore.ContextMenu\generic.EmberCore.ContextMenu.vbproj` |
| **Folder** | `Addons\generic.EmberCore.ContextMenu\` |
| **Module Name** | "Context Menu" |
| **Key File** | [`genericContextMenu.vb`](../../Addons/generic.EmberCore.ContextMenu/genericContextMenu.vb) |

Configure Windows Explorer context menu integration:
- Right-click on folders to scan into Ember
- Quick access from file system

---

## Generic Addons - External Integrations

### [↑](#table-of-contents) generic.Interface.Kodi

| Property | Value |
|----------|-------|
| **Project File** | `Addons\generic.Interface.Kodi\generic.Interface.Kodi.vbproj` |
| **Folder** | `Addons\generic.Interface.Kodi\` |
| **Module Name** | "Kodi Interface" |
| **Key Files** | [`clsAPIKodi.vb`](../../Addons/generic.Interface.Kodi/clsAPIKodi.vb), [`Interface.Kodi.vb`](../../Addons/generic.Interface.Kodi/Interface.Kodi.vb) |
| **Depends On** | `KodiAPI` project |

Synchronize with Kodi media centers:
- Trigger library updates on Kodi
- Sync watched status
- Remote control Kodi playback
- Multiple host support

---

### [↑](#table-of-contents) generic.Interface.Trakttv

| Property | Value |
|----------|-------|
| **Project File** | `Addons\generic.Interface.Trakttv\generic.Interface.Trakttv.vbproj` |
| **Folder** | `Addons\generic.Interface.Trakttv\` |
| **Module Name** | "Trakt.tv Manager" |
| **Key Files** | [`Interface.Trakt.vb`](../../Addons/generic.Interface.Trakttv/Interface.Trakt.vb), [`clsAPITrakt.vb`](../../Addons/generic.Interface.Trakttv/clsAPITrakt.vb) |

Synchronize with Trakt.tv:
- Sync watched/collected status
- Import watchlist
- Scrobble playback
- Rate movies/shows

---

## [↑](#table-of-contents) Project Dependencies

    EmberMediaManager (EXE)
        └── EmberAPI (DLL)

    All Addons (DLL)
        └── EmberAPI (DLL)

    generic.Interface.Kodi (DLL)
        ├── EmberAPI (DLL)
        └── KodiAPI (DLL)

---

## [↑](#table-of-contents) Naming Conventions

| Prefix | Meaning | Example |
|--------|---------|---------|
| `scraper.Data.*` | Metadata scraper | `scraper.Data.TMDB` |
| `scraper.Image.*` | Image/artwork scraper | `scraper.Image.FanartTV` |
| `scraper.Trailer.*` | Trailer scraper | `scraper.Trailer.YouTube` |
| `generic.EmberCore.*` | Utility/tool addon | `generic.EmberCore.BulkRename` |
| `generic.Interface.*` | External service integration | `generic.Interface.Kodi` |

---

## [↑](#table-of-contents) Quick Reference Table

| Project | Type | Purpose |
|---------|------|---------|
| EmberMediaManager | EXE | Main application UI |
| EmberAPI | DLL | Core shared library |
| KodiAPI | DLL (C#) | Kodi JSON-RPC client |
| scraper.Data.TMDB | Addon | TMDB metadata |
| scraper.Data.IMDB | Addon | IMDB metadata |
| scraper.Data.TVDB | Addon | TVDB TV metadata |
| scraper.Data.OMDb | Addon | OMDb metadata |
| scraper.Data.Trakttv | Addon | Trakt.tv metadata |
| scraper.Image.TMDB | Addon | TMDB images |
| scraper.Image.FanartTV | Addon | Fanart.tv images |
| scraper.Image.TVDB | Addon | TVDB images |
| scraper.Trailer.TMDB | Addon | TMDB trailers |
| scraper.Trailer.YouTube | Addon | YouTube trailers |
| generic.EmberCore.BulkRename | Addon | File/folder renaming |
| generic.EmberCore.MediaFileManager | Addon | Copy/move files |
| generic.EmberCore.MovieExporter | Addon | Export to HTML |
| generic.EmberCore.TagManager | Addon | Tag management |
| generic.EmberCore.Mapping | Addon | Genre/studio mapping |
| generic.EmberCore.MetadataEditor | Addon | Codec mapping |
| generic.EmberCore.VideoSourceMapping | Addon | Source configuration |
| generic.EmberCore.MediaListEditor | Addon | Custom lists |
| generic.EmberCore.ContextMenu | Addon | Explorer integration |
| generic.Interface.Kodi | Addon | Kodi sync |
| generic.Interface.Trakttv | Addon | Trakt.tv sync |

---

## [↑](#table-of-contents) Folder vs Project Name Mismatches

Some folders don't match their project names:

| Folder Name | Project Name | Reason |
|-------------|--------------|--------|
| `generic.EmberCore.FilterEditor` | `generic.EmberCore.MediaListEditor` | Renamed project |
| `generic.EmberCore.MovieExport` | `generic.EmberCore.MovieExporter` | Missing 'er' suffix |
| `generic.Embercore.MetadataEditor` | `generic.EmberCore.MetadataEditor` | Case difference (Embercore vs EmberCore) |
| `scraper.FanartTV.Poster` | `scraper.Image.FanartTV` | Standardized naming |
| `scraper.IMDB.Data` | `scraper.Data.IMDB` | Standardized naming |
| `scraper.TMDB.Data` | `scraper.Data.TMDB` | Standardized naming |
| `scraper.TMDB.Poster` | `scraper.Image.TMDB` | Standardized naming |
| `scraper.TMDB.Trailer` | `scraper.Trailer.TMDB` | Standardized naming |
| `scraper.Trailer.YouTube` | `scraper.Trailer.YouTube` | Folder matches, but file is `clsScrapeYouTube.vb` (missing 'r') |
| `scraper.Trakttv.Data` | `scraper.Data.Trakttv` | Standardized naming |

---

## [↑](#table-of-contents) See Also

- [ScrapingProcessMovies.md](process-docs/ScrapingProcessMovies.md) - Movie scraping flow
- [ScrapingProcessTvShows.md](process-docs/ScrapingProcessTvShows.md) - TV show scraping flow
- [GenreMappingProcess.md](process-docs/GenreMappingProcess.md) - Genre mapping system
- [DocumentIndex.md](DocumentIndex.md) - Master index for all documentation

---

*End of file*