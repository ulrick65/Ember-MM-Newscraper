# Image Selection Process in Ember Media Manager

> **Document Version:** 1.0 (Created January 4, 2026)
> **Related Documentation:** 
> - [BL-KI-002-EditSeasonImageSelectionBug.md](../improvements-docs/backlog/BL-KI-002-EditSeasonImageSelectionBug.md)

##### [← Return to Document Index](DocumentIndex.md)

---

## Overview

The Image Selection system in Ember Media Manager handles the discovery, download, selection, and persistence of artwork for movies and TV content. The system supports multiple image types (posters, fanart, banners, etc.) from multiple scraper sources (TMDB, FanartTV, TVDB).

**High-Level Flow:**

    User Action (Edit Dialog / Scrape / Quick Access)
            │
            ▼
    ModulesManager.Instance.ScrapeImage_*() - Collects image URLs from scrapers
            │
            ▼
    dlgImgSelect.ShowDialog() - Displays image selection UI
            │
            ▼
    Background Workers download thumbnails for preview
            │
            ▼
    User selects images (or auto-selection via preferences)
            │
            ▼
    Save_*() downloads full-size images and writes to disk

**Key Insight:** Image URLs are collected during scraping, but actual image downloads happen in two phases:

1. **Thumbnail downloads** - During `dlgImgSelect` for preview display
2. **Full-size downloads** - During `Save_Movie()` / `Save_TVShow()` for persistence

---

## Table of Contents

- [Overview](#overview)
- [Part 1: Entry Points](#part-1-entry-points)
- [Part 2: Image Selection Dialog Architecture](#part-2-image-selection-dialog-architecture)
- [Part 3: Image Download Pipeline](#part-3-image-download-pipeline)
- [Part 4: Image Type System](#part-4-image-type-system)
- [Part 5: Progress Reporting Mechanism](#part-5-progress-reporting-mechanism)
- [Part 6: Image Containers and Data Structures](#part-6-image-containers-and-data-structures)
- [Part 7: Preferred Image Selection](#part-7-preferred-image-selection)
- [Part 8: Saving Images to Disk](#part-8-saving-images-to-disk)
- [Part 9: Content Type Specific Handling](#part-9-content-type-specific-handling)
- [Part 10: Known Issues and Fixes](#part-10-known-issues-and-fixes)
- [Part 11: Debugging Guide](#part-11-debugging-guide)

---

## [↑](#table-of-contents) Part 1: Entry Points

Image selection can be initiated from multiple locations depending on content type and user action.

### [↑](#table-of-contents)  1.1 During Bulk/Auto Scraping

When scraping multiple items, images are selected automatically based on user preferences.

**Code Path (Movies):**

| Step | File | Method | Line |
|------|------|--------|------|
| 1 | `frmMain.vb` | `bwMovieScraper_DoWork` | ~1529 |
| 2 | `clsAPIModules.vb` | `ScrapeImage_Movie()` | ~1200 |
| 3 | `clsAPIImages.vb` | `SetPreferredImages()` | - |

**Code Path (TV Shows):**

| Step | File | Method | Line |
|------|------|--------|------|
| 1 | `frmMain.vb` | `bwTVScraper_DoWork` | - |
| 2 | `clsAPIModules.vb` | `ScrapeImage_TV()` | ~1476 |
| 3 | `clsAPIImages.vb` | `SetPreferredImages()` | - |

### [↑](#table-of-contents)  1.2 From Edit Dialogs (Manual Selection)

When editing a single item, users can manually select images via the Images tab.

**Movie Edit Dialog:**

| Step | File | Method |
|------|------|--------|
| 1 | `dlgEdit_Movie.vb` | `btnSetPoster_Click` / `btnSetFanart_Click` |
| 2 | `clsAPIModules.vb` | `ScrapeImage_Movie()` |
| 3 | `dlgImgSelect.vb` | `ShowDialog()` |

**TV Season Edit Dialog:**

| Step | File | Method |
|------|------|--------|
| 1 | `dlgEdit_TVSeason.vb` | `Image_Scrape_Click` (line 477) |
| 2 | `clsAPIModules.vb` | `ScrapeImage_TV()` (line 1476) |
| 3 | `dlgImgSelect.vb` | `ShowDialog()` (line 193) |

### [↑](#table-of-contents) 1.3 Quick Access (Double-Click on Image)

Double-clicking an image in the main window opens the image selection dialog directly.

**Code Path:**

| Step | File | Method |
|------|------|--------|
| 1 | `frmMain.vb` | `pbPoster_DoubleClick` / `pbFanart_DoubleClick` |
| 2 | `clsAPIModules.vb` | `ScrapeImage_*()` |
| 3 | `dlgImgSelect.vb` | `ShowDialog()` |

---

## [↑](#table-of-contents) Part 2: Image Selection Dialog Architecture

The `dlgImgSelect` dialog (`EmberMediaManager\dlgImgSelect.vb`) is the central UI for image selection.

### [↑](#table-of-contents) 2.1 Dialog Layout

The dialog consists of three main areas:

    ┌─────────────────────────────────────────────────────────────────┐
    │                      TOP PANEL (pnlTopImages)                   │
    │  [Poster] [Fanart] [Banner] [ClearArt] [ClearLogo] [Landscape]  │
    │  Selected image types for this content - click to view options  │
    ├──────────────┬──────────────────────────────────────────────────┤
    │   LEFT PANEL │              MAIN PANEL (pnlImgSelectMain)       │
    │ (pnlSubImages)│                                                  │
    │              │  ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐      │
    │  Season List │  │    │ │    │ │    │ │    │ │    │ │    │      │
    │  or          │  │ Img│ │ Img│ │ Img│ │ Img│ │ Img│ │ Img│      │
    │  Extrafanarts│  │  1 │ │  2 │ │  3 │ │  4 │ │  5 │ │  6 │      │
    │              │  └────┘ └────┘ └────┘ └────┘ └────┘ └────┘      │
    │              │                                                  │
    │              │  Grid of available images for selected type      │
    │              │  Green plus icon = selectable                    │
    └──────────────┴──────────────────────────────────────────────────┘

### [↑](#table-of-contents) 2.2 Key UI Components

| Component | Purpose |
|-----------|---------|
| `pnlTopImages` | Displays selected/preferred images for each type |
| `pnlSubImages` | Shows seasons (TV) or extrafanarts/extrathumbs (Movies) |
| `pnlImgSelectMain` | Grid of available images for the selected type |
| `pbListImage_Select` | Green plus icon indicating image can be selected |
| `pnlLoading` | Loading indicator while images download |

### [↑](#table-of-contents) 2.3 Control Arrays

The dialog uses dynamic control arrays for flexibility:

    ' List images (main panel)
    Private pnlListImage_Panel() As Panel
    Private pbListImage_Image() As PictureBox
    Private pbListImage_Select() As PictureBox
    Private lblListImage_Scraper() As Label
    Private lblListImage_Resolution() As Label
    
    ' Top images (selected images)
    Private pnlTopImage_Panel() As Panel
    Private pbTopImage_Image() As PictureBox
    
    ' Sub images (seasons or extras)
    Private pnlSubImage_Panel() As Panel
    Private pbSubImage_Image() As PictureBox

### [↑](#table-of-contents) 2.4 State Tracking Variables

    Private currTopImage As iTag           ' Currently selected top image type
    Private currSubImage As iTag           ' Currently selected sub image (season)
    Private currListImage As iTag          ' Currently highlighted list image
    Private currListImageSelectedImageType As Enums.ModifierType  ' Tracks displayed list type
    Private currListImageSelectedSeason As Integer                ' Tracks displayed season

---

## [↑](#table-of-contents) Part 3: Image Download Pipeline

### [↑](#table-of-contents) 3.1 Two-Phase Download Architecture

Image downloading occurs in two distinct phases:

**Phase 1: Thumbnail Download (for preview)**

    dlgImgSelect_Shown
        │
        ▼
    bwImgDefaults.RunWorkerAsync()
        │
        ▼
    bwImgDefaults_DoWork
        ├── GetPreferredImages()      ' Select default images
        └── DownloadDefaultImages()   ' Download thumbnails for defaults
        │
        ▼
    bwImgDefaults_RunWorkerCompleted
        ├── CreateTopImages()         ' Display in top panel
        └── bwImgDownload.RunWorkerAsync()
            │
            ▼
    bwImgDownload_DoWork
        └── DownloadAllImages()       ' Download all thumbnails
            │
            ▼
    bwImgDownload_ProgressChanged
        └── CreateListImages()        ' Populate main panel as images arrive

**Phase 2: Full-Size Download (for saving)**

    DoneAndClose()
        │
        ▼
    For each image type:
        Result.ImagesContainer.*.LoadAndCache(tContentType, True)
        ' Downloads full-size image and caches it

### [↑](#table-of-contents) 3.2 Background Worker Structure

The dialog uses two background workers:

| Worker | Purpose | Reports Progress |
|--------|---------|------------------|
| `bwImgDefaults` | Downloads default/preferred images | No |
| `bwImgDownload` | Downloads all available images | Yes |

### [↑](#table-of-contents) 3.3 DownloadAllImages Method

**Location:** `dlgImgSelect.vb`, `DownloadAllImages()` method

This method downloads thumbnails for all image types in sequence:

    1. Main Posters
    2. Main Keyarts
    3. Main Banners
    4. Main CharacterArts
    5. Main ClearArts
    6. Main ClearLogos
    7. Main DiscArts
    8. Main Landscapes
    9. Main Fanarts
    10. Season Banners
    11. Season Fanarts
    12. Season Landscapes
    13. Season Posters
    14. Episode Fanarts
    15. Episode Posters

After each category downloads, progress is reported to update the UI.

---

## [↑](#table-of-contents) Part 4: Image Type System

### [↑](#table-of-contents) 4.1 ModifierType Enum

The `Enums.ModifierType` enum defines all image types:

| Type | Content | Description |
|------|---------|-------------|
| `MainPoster` | Movie/TVShow | Primary poster artwork |
| `MainFanart` | Movie/TVShow | Background/fanart image |
| `MainBanner` | Movie/TVShow | Wide banner image |
| `MainClearArt` | Movie/TVShow | Transparent artwork |
| `MainClearLogo` | Movie/TVShow | Transparent logo |
| `MainDiscArt` | Movie | CD/DVD disc image |
| `MainLandscape` | Movie/TVShow | Thumbnail/landscape image |
| `MainCharacterArt` | TVShow | Character artwork |
| `MainKeyart` | Movie | Keyart/theatrical poster |
| `MainExtrafanarts` | Movie/TVShow | Additional fanart images |
| `MainExtrathumbs` | Movie | Additional thumbnail images |
| `SeasonPoster` | TVSeason | Season-specific poster |
| `SeasonFanart` | TVSeason | Season-specific fanart |
| `SeasonBanner` | TVSeason | Season-specific banner |
| `SeasonLandscape` | TVSeason | Season-specific landscape |
| `AllSeasonsPoster` | TVShow | "All Seasons" poster |
| `AllSeasonsFanart` | TVShow | "All Seasons" fanart |
| `AllSeasonsBanner` | TVShow | "All Seasons" banner |
| `AllSeasonsLandscape` | TVShow | "All Seasons" landscape |
| `EpisodePoster` | TVEpisode | Episode still/thumbnail |
| `EpisodeFanart` | TVEpisode | Episode fanart |

### [↑](#table-of-contents) 4.2 Do* Flags

Boolean flags control which image types are processed:

    Private DoMainPoster As Boolean = False
    Private DoMainFanart As Boolean = False
    Private DoMainBanner As Boolean = False
    Private DoSeasonPoster As Boolean = False
    Private DoSeasonFanart As Boolean = False
    Private DoAllSeasonsPoster As Boolean = False
    Private DoAllSeasonsFanart As Boolean = False
    ' ... etc

These are set in `SetParameters()` based on:
- `ScrapeModifiers` (what user requested)
- Settings (`Master.eSettings.*AnyEnabled`)
- Content type

### [↑](#table-of-contents) 4.3 Loaded* Flags

Track which image categories have finished downloading:

    Private LoadedMainPoster As Boolean = False
    Private LoadedMainFanart As Boolean = False
    Private LoadedSeasonFanart As Boolean = False
    ' ... etc

These flags are used by `CreateListImages()` to determine when to populate the UI.

---

## [↑](#table-of-contents) Part 5: Progress Reporting Mechanism

### [↑](#table-of-contents) 5.1 Progress Report Flow

The `bwImgDownload` worker reports progress after each image category completes:

    bwImgDownload_DoWork
        │
        ├── Download MainPosters
        │   └── ReportProgress(Enums.ModifierType.MainPoster)
        │
        ├── Download MainFanarts
        │   └── ReportProgress(Enums.ModifierType.MainFanart)
        │   └── ReportProgress(Enums.ModifierType.SeasonFanart)  ' If not doing season downloads
        │
        ├── Download SeasonFanarts (if enabled)
        │   └── ReportProgress(Enums.ModifierType.SeasonFanart)
        │
        └── ... etc

### [↑](#table-of-contents) 5.2 Progress Changed Handler

    Private Sub bwImgDownload_ProgressChanged(...) Handles bwImgDownload.ProgressChanged
        If e.UserState.ToString = "progress" Then
            pbStatus.Value = e.ProgressPercentage
        ElseIf e.UserState.ToString = "max" Then
            pbStatus.Maximum = e.ProgressPercentage
        ElseIf DirectCast(e.UserState, Enums.ModifierType) = currTopImage.ImageType Then
            CreateListImages(currTopImage)      ' Refresh if viewing this type
        ElseIf DirectCast(e.UserState, Enums.ModifierType) = currSubImage.ImageType Then
            CreateListImages(currSubImage)      ' Refresh if viewing this season
        End If
    End Sub

### [↑](#table-of-contents) 5.3 Conditional Progress Reporting (Bug Fix)

**Important:** Progress for `SeasonFanart` and `AllSeasonsFanart` is only reported from one location to prevent duplicate `CreateListImages()` calls.

    ' After MainFanarts download - only report if NOT doing season downloads
    If Not (DoSeasonFanart OrElse DoAllSeasonsFanart) Then
        bwImgDownload.ReportProgress(iProgress, Enums.ModifierType.AllSeasonsFanart)
        bwImgDownload.ReportProgress(iProgress, Enums.ModifierType.SeasonFanart)
    End If
    
    ' After SeasonFanarts download - always report (if we got here, we're doing season downloads)
    bwImgDownload.ReportProgress(iProgress, Enums.ModifierType.AllSeasonsFanart)
    bwImgDownload.ReportProgress(iProgress, Enums.ModifierType.SeasonFanart)

This prevents the image list from being built twice, which would cause duplicate images and UI hangs.

---

## [↑](#table-of-contents) Part 6: Image Containers and Data Structures

### [↑](#table-of-contents) 6.1 SearchResultsContainer

Holds all scraped image URLs before selection:

    Public Class SearchResultsContainer
        Public MainPosters As New List(Of Image)
        Public MainFanarts As New List(Of Image)
        Public MainBanners As New List(Of Image)
        Public MainClearArts As New List(Of Image)
        Public MainClearLogos As New List(Of Image)
        Public MainDiscArts As New List(Of Image)
        Public MainLandscapes As New List(Of Image)
        Public MainCharacterArts As New List(Of Image)
        Public MainKeyarts As New List(Of Image)
        Public SeasonPosters As New List(Of Image)
        Public SeasonFanarts As New List(Of Image)
        Public SeasonBanners As New List(Of Image)
        Public SeasonLandscapes As New List(Of Image)
        Public EpisodePosters As New List(Of Image)
        Public EpisodeFanarts As New List(Of Image)
    End Class

### [↑](#table-of-contents) 6.2 PreferredImagesContainer

Holds the user's selected images:

    Public Class PreferredImagesContainer
        Public ImagesContainer As New ImagesContainer
        Public Seasons As New List(Of EpisodeOrSeasonImagesContainer)
        Public Episodes As New List(Of EpisodeOrSeasonImagesContainer)
    End Class

### [↑](#table-of-contents) 6.3 ImagesContainer

Holds actual image data for a single item:

    Public Class ImagesContainer
        Public Banner As New Image
        Public CharacterArt As New Image
        Public ClearArt As New Image
        Public ClearLogo As New Image
        Public DiscArt As New Image
        Public Fanart As New Image
        Public Keyart As New Image
        Public Landscape As New Image
        Public Poster As New Image
        Public Extrafanarts As New List(Of Image)
        Public Extrathumbs As New List(Of Image)
    End Class

### [↑](#table-of-contents) 6.4 Image Class

Represents a single image with URLs and cached data:

    Public Class Image
        Public URLOriginal As String        ' Full-size URL
        Public URLThumb As String           ' Thumbnail URL
        Public LocalFilePath As String      ' Path after saving
        Public ImageOriginal As ImageContainer  ' Cached full image
        Public ImageThumb As ImageContainer     ' Cached thumbnail
        Public Width As String
        Public Height As String
        Public Season As Integer            ' For season images
        Public Scraper As String            ' Source scraper name
        Public LongLang As String           ' Language
        Public DiscType As String           ' For disc art
    End Class

### [↑](#table-of-contents) 6.5 iTag Structure (Dialog Internal)

Used to tag UI controls with image metadata:

    Private Structure iTag
        Dim Image As MediaContainers.Image
        Dim ImageType As Enums.ModifierType
        Dim iIndex As Integer
        Dim iSeason As Integer
        Dim strResolution As String
        Dim strSeason As String
        Dim strTitle As String
    End Structure

---

## [↑](#table-of-contents) Part 7: Preferred Image Selection

### [↑](#table-of-contents) 7.1 Automatic Selection

When scraping in auto mode, `Images.GetPreferredImagesContainer()` selects images based on user preferences:

    Public Function GetPreferredImagesContainer(
        DBElement As Database.DBElement,
        SearchResultsContainer As MediaContainers.SearchResultsContainer,
        ScrapeModifiers As Structures.ScrapeModifiers
    ) As MediaContainers.PreferredImagesContainer

### [↑](#table-of-contents) 7.2 Selection Criteria

Images are selected based on:

| Criterion | Setting |
|-----------|---------|
| Size | `MoviePosterPrefSize`, `MovieFanartPrefSize`, etc. |
| Language | `MoviePosterPrefLanguage`, etc. |
| Keep Existing | `MoviePosterKeepExisting`, etc. |

### [↑](#table-of-contents) 7.3 Manual Selection (SetImage)

When user clicks an image in `dlgImgSelect`:

    Private Sub SetImage(ByVal tTag As iTag)
        Select Case tTag.ImageType
            Case Enums.ModifierType.MainPoster
                Result.ImagesContainer.Poster = tTag.Image
                RefreshTopImage(tTag)
            Case Enums.ModifierType.SeasonFanart
                If tContentType = Enums.ContentType.TVSeason Then
                    Result.ImagesContainer.Fanart = tTag.Image
                    RefreshTopImage(tTag)
                End If
            ' ... etc
        End Select
    End Sub

---

## [↑](#table-of-contents) Part 8: Saving Images to Disk

### [↑](#table-of-contents) 8.1 Save Flow

    DoneAndClose() [dlgImgSelect.vb]
        │
        ▼
    LoadAndCache() for each selected image (downloads full-size)
        │
        ▼
    DialogResult = DialogResult.OK
        │
        ▼
    Caller receives Result.ImagesContainer
        │
        ▼
    Save_Movie() / Save_TVShow() [clsAPIDatabase.vb]
        │
        ▼
    SaveAllImages() [clsAPIMediaContainers.vb]

### [↑](#table-of-contents) 8.2 SaveAllImages Method

**Location:** `clsAPIMediaContainers.vb`

    Public Sub SaveAllImages(ByRef DBElement As Database.DBElement, ByVal ForceFileCleanup As Boolean)
        Banner.Save(DBElement, Enums.ModifierType.MainBanner, ForceFileCleanup)
        ClearArt.Save(DBElement, Enums.ModifierType.MainClearArt, ForceFileCleanup)
        ClearLogo.Save(DBElement, Enums.ModifierType.MainClearLogo, ForceFileCleanup)
        DiscArt.Save(DBElement, Enums.ModifierType.MainDiscArt, ForceFileCleanup)
        Fanart.Save(DBElement, Enums.ModifierType.MainFanart, ForceFileCleanup)
        Keyart.Save(DBElement, Enums.ModifierType.MainKeyart, ForceFileCleanup)
        Landscape.Save(DBElement, Enums.ModifierType.MainLandscape, ForceFileCleanup)
        Poster.Save(DBElement, Enums.ModifierType.MainPoster, ForceFileCleanup)
        ' ... extrafanarts, extrathumbs
    End Sub

### [↑](#table-of-contents) 8.3 Async Save (Performance Optimized)

**Location:** `clsAPIMediaContainers.vb`

    Public Async Function SaveAllImagesAsync(...) As Task(Of Database.DBElement)
        ' Parallel download with SemaphoreSlim throttling
        Await Images.DownloadImagesParallelAsync(
            imagesToDownload,
            contentType,
            maxConcurrency:=5,
            thumbnailOnly:=False,
            cancellationToken,
            progressCallback
        )
    End Function

---

## [↑](#table-of-contents) Part 9: Content Type Specific Handling

### [↑](#table-of-contents) 9.1 Content Type Detection

    Select Case DBElement.ContentType
        Case Enums.ContentType.Movie
            tContentType = Enums.ContentType.Movie
        Case Enums.ContentType.TVShow
            If ScrapeModifiers.withEpisodes OrElse ScrapeModifiers.withSeasons Then
                tContentType = Enums.ContentType.TV  ' Full TV scrape
            Else
                tContentType = Enums.ContentType.TVShow  ' Show only
            End If
        Case Enums.ContentType.TVSeason
            tContentType = Enums.ContentType.TVSeason
            DoOnlySeason = DBElement.TVSeason.Season  ' Filter to specific season
        Case Enums.ContentType.TVEpisode
            tContentType = Enums.ContentType.TVEpisode
    End Select

### [↑](#table-of-contents) 9.2 TVSeason Special Handling

When editing a single season:

- `DoOnlySeason` is set to the season number
- Image downloads are filtered: `f.Season = DoOnlySeason`
- Top panel shows season-specific types (SeasonPoster, SeasonFanart, etc.)
- MainFanarts are included as fallback options for SeasonFanart

### [↑](#table-of-contents) 9.3 TV vs TVShow vs TVSeason

| Content Type | Top Images | Sub Images | Notes |
|--------------|------------|------------|-------|
| `TV` | Show images | All seasons | Full scrape with seasons |
| `TVShow` | Show images | None | Show only, no seasons |
| `TVSeason` | Season images | None | Single season |
| `TVEpisode` | Episode images | None | Single episode |

---

## [↑](#table-of-contents) Part 10: Known Issues and Fixes

### [↑](#table-of-contents) 10.1 BL-KI-002: Edit Season Dialog - Images Not Selectable

**Status:** ✅ Resolved (January 4, 2026)

**Problem:** When editing a TV Season, most images couldn't be selected (no green plus icon).

**Root Cause:** `CreateListImages()` was called twice for `SeasonFanart` because progress was reported from both MainFanarts download and SeasonFanarts download.

**Fix:** Added conditional check before reporting progress:

    If Not (DoSeasonFanart OrElse DoAllSeasonsFanart) Then
        bwImgDownload.ReportProgress(iProgress, Enums.ModifierType.SeasonFanart)
    End If

**Related Documentation:** [BL-KI-002-EditSeasonImageSelectionBug.md](../improvements-docs/backlog/BL-KI-002-EditSeasonImageSelectionBug.md)

---

## [↑](#table-of-contents) Part 11: Debugging Guide

### [↑](#table-of-contents) 11.1 Key Breakpoint Locations

| Priority | File | Method/Line | Purpose |
|----------|------|-------------|---------|
| 1 | `dlgImgSelect.vb` | `ShowDialog()` | Dialog entry point |
| 2 | `dlgImgSelect.vb` | `DownloadAllImages()` | Image download start |
| 3 | `dlgImgSelect.vb` | `bwImgDownload_ProgressChanged` | Progress handling |
| 4 | `dlgImgSelect.vb` | `CreateListImages()` | UI population |
| 5 | `dlgImgSelect.vb` | `SetImage()` | Image selection |
| 6 | `dlgImgSelect.vb` | `DoneAndClose()` | Final save |

### [↑](#table-of-contents) 11.2 Debug Variables to Watch

| Variable | Purpose |
|----------|---------|
| `currTopImage.ImageType` | Currently selected image type |
| `currListImageSelectedImageType` | What type is displayed in list |
| `tContentType` | Content being edited |
| `DoSeasonFanart` | Whether season fanart is enabled |
| `LoadedSeasonFanart` | Whether season fanarts have downloaded |
| `pnlImgSelectMain.Controls.Count` | Number of images displayed |

### [↑](#table-of-contents) 11.3 Common Issues

| Symptom | Likely Cause | Check |
|---------|--------------|-------|
| No images appear | Downloads not complete | `Loaded*` flags |
| Images appear twice | Multiple progress reports | Progress reporting logic |
| Green plus missing | `ModifierType` mismatch | `tTag.ImageType` vs `currTopImage.ImageType` |
| Dialog hangs | Too many controls | Control count in `AddListImage` |
| Wrong images shown | Season filtering | `DoOnlySeason` value |

### [↑](#table-of-contents) 11.4 Logging

Add debug statements to trace flow:

    Debug.WriteLine($"CreateListImages: Type={tTag.ImageType}, Season={tTag.iSeason}")
    Debug.WriteLine($"Progress: {e.UserState}, currTop={currTopImage.ImageType}")

---

## Summary

The Image Selection system in Ember Media Manager:

1. **Entry Points:** Edit dialogs, bulk scraping, quick access (double-click)
2. **Dialog:** `dlgImgSelect.vb` with three-panel layout
3. **Download Phases:** Thumbnails (preview) then full-size (save)
4. **Progress Reporting:** Must avoid duplicate reports to prevent UI issues
5. **Content Types:** Different handling for Movie, TVShow, TVSeason, TVEpisode
6. **Saving:** `SaveAllImages()` or `SaveAllImagesAsync()` for parallel performance

**Key Code Locations:**

| Operation | File | Method |
|-----------|------|--------|
| Dialog entry | `dlgImgSelect.vb` | `ShowDialog()` |
| Download all | `dlgImgSelect.vb` | `DownloadAllImages()` |
| Progress update | `dlgImgSelect.vb` | `bwImgDownload_ProgressChanged` |
| Build UI | `dlgImgSelect.vb` | `CreateListImages()` |
| Image selection | `dlgImgSelect.vb` | `SetImage()` |
| Save to disk | `clsAPIMediaContainers.vb` | `SaveAllImages()` |

---

*End of document*