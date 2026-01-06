 # Ember Media Manager - Image Processing Reference

| | Document Info |
|---------------|---|
| **Version** | 1.1 |
| **Created** | January 5, 2026 |
| **Updated** | January 5, 2026 |
| **Author** | Eric H. Anderson |
| **Purpose** | Technical reference for the image selection dialog and image processing system |

##### [← Return to Document Index](../DocumentIndex.md)

---

## Overview

This document provides a detailed technical reference for the image selection system in Ember Media Manager. It covers the `dlgImgSelect` dialog, content type determination, image downloading, and the button-based UI for selecting images.

**Key Files:**
- [`dlgImgSelect.vb`](../../dlgImgSelect.vb) — Main image selection dialog
- [`frmMain.vb`](../../frmMain.vb) — Entry points for image editing
- [`clsAPIModules.vb`](../../EmberAPI/clsAPIModules.vb) — Scraper module management
- [`clsAPIMediaContainers.vb`](../../EmberAPI/clsAPIMediaContainers.vb) — Image container classes

---

## Table of Contents

- [Content Type System](#content-type-system)
- [Dialog Initialization Flow](#dialog-initialization-flow)
- [ScrapeModifiers Structure](#scrapemodifiers-structure)
- [SetParameters Method](#setparameters-method)
- [Image Download Pipeline](#image-download-pipeline)
- [UI Components](#ui-components)
- [Include Fanarts Button](#include-fanarts-button)
- [Image Selection and Saving](#image-selection-and-saving)
- [Key Methods Reference](#key-methods-reference)

---

## [↑](#table-of-contents) Content Type System

The `dlgImgSelect` dialog supports multiple content types, each with different image options:

| Content Type | Description | Season Support | Left Panel |
|--------------|-------------|:--------------:|:----------:|
| `Movie` | Single movie | ❌ | Extrafanarts/Extrathumbs |
| `MovieSet` | Movie collection | ❌ | ❌ Hidden |
| `TV` | Full TV show with seasons | ✅ | Season list + buttons |
| `TVShow` | TV show only (no seasons) | ❌ | Extrafanarts only |
| `TVSeason` | Single season | ❌ | ❌ Hidden |
| `TVEpisode` | Single episode | ❌ | ❌ Hidden |

### Content Type Determination

**File:** [`dlgImgSelect.vb`](../../dlgImgSelect.vb) — `ShowDialog()` method (lines 194-230)

The content type is determined when the dialog opens based on `DBElement.ContentType` and `ScrapeModifiers`:

    Public Overloads Function ShowDialog(ByVal DBElement As Database.DBElement, 
                                         ByVal SearchResultsContainer As MediaContainers.SearchResultsContainer, 
                                         ByVal ScrapeModifiers As Structures.ScrapeModifiers) As DialogResult
        tSearchResultsContainer = SearchResultsContainer
        tDBElement = DBElement
        tScrapeModifiers = ScrapeModifiers

        Select Case DBElement.ContentType
            Case Enums.ContentType.TVShow
                If ScrapeModifiers.withEpisodes OrElse ScrapeModifiers.withSeasons Then
                    tContentType = Enums.ContentType.TV      ' Full TV mode with seasons
                Else
                    tContentType = Enums.ContentType.TVShow  ' Show-only mode
                End If
            Case Enums.ContentType.TVSeason
                tContentType = Enums.ContentType.TVSeason
                DoOnlySeason = DBElement.TVSeason.Season
            Case Else
                tContentType = DBElement.ContentType
                ' ... image filtering for movies ...
        End Select

        SetParameters()
        Return ShowDialog()
    End Function

**Critical Logic:**
- For `TVShow` content, the dialog checks `ScrapeModifiers.withSeasons` or `ScrapeModifiers.withEpisodes`
- If either is `True`, the dialog uses `TV` mode (full interface with season buttons)
- If both are `False`, the dialog uses `TVShow` mode (show images only, no season buttons)

---

## [↑](#table-of-contents) Dialog Initialization Flow

    Entry Point (frmMain.vb or Edit Dialog)
                    ↓
    dlgImgSelect.ShowDialog(DBElement, SearchResultsContainer, ScrapeModifiers)
                    ↓
    ┌─────────────────────────────────────────┐
    │  Determine tContentType based on:       │
    │  - DBElement.ContentType                │
    │  - ScrapeModifiers.withSeasons          │
    │  - ScrapeModifiers.withEpisodes         │
    └─────────────────────────────────────────┘
                    ↓
    SetParameters() — Sets Do* flags based on tContentType
                    ↓
    dlgImgSelect_Load — Sets up UI, double buffering
                    ↓
    dlgImgSelect_Shown — Starts background workers
                    ↓
    bwImgDefaults.RunWorkerAsync()
        ├── GetPreferredImages() — Selects default images
        └── DownloadDefaultImages() — Downloads thumbnails for defaults
                    ↓
    bwImgDefaults_RunWorkerCompleted
        ├── CreateTopImages() — Builds top panel with selected images
        └── bwImgDownload.RunWorkerAsync()
                    ↓
    bwImgDownload_DoWork
        └── DownloadAllImages() — Downloads all thumbnails in parallel
                    ↓
    User interacts with dialog
                    ↓
    btnOK_Click → DoneAndClose()
        └── Downloads full-size images (including season images), returns DialogResult.OK

---

## [↑](#table-of-contents) ScrapeModifiers Structure

**File:** [`Structures.vb`](../../EmberAPI/Structures.vb)

The `ScrapeModifiers` structure controls which image types are scraped and displayed:

### Show-Level Modifiers

| Property | Description |
|----------|-------------|
| `MainBanner` | Show/Movie banner |
| `MainCharacterArt` | Character artwork (TV only) |
| `MainClearArt` | Clear art overlay |
| `MainClearLogo` | Clear logo |
| `MainDiscArt` | Disc artwork (Movie only) |
| `MainExtrafanarts` | Additional fanart images |
| `MainExtrathumbs` | Additional thumbnails (Movie only) |
| `MainFanart` | Primary fanart/background |
| `MainKeyart` | Key art/theatrical poster |
| `MainLandscape` | Landscape/thumb image |
| `MainPoster` | Primary poster |

### Season-Level Modifiers

| Property | Description |
|----------|-------------|
| `AllSeasonsBanner` | "All Seasons" banner |
| `AllSeasonsFanart` | "All Seasons" fanart |
| `AllSeasonsLandscape` | "All Seasons" landscape |
| `AllSeasonsPoster` | "All Seasons" poster |
| `SeasonBanner` | Season-specific banner |
| `SeasonFanart` | Season-specific fanart |
| `SeasonLandscape` | Season-specific landscape |
| `SeasonPoster` | Season-specific poster |

### Episode-Level Modifiers

| Property | Description |
|----------|-------------|
| `EpisodeFanart` | Episode fanart |
| `EpisodePoster` | Episode still/thumbnail |

### Control Flags

| Property | Description |
|----------|-------------|
| `withSeasons` | **Critical** — Enables TV mode with season UI |
| `withEpisodes` | Enables episode-level scraping |

---

## [↑](#table-of-contents) SetParameters Method

**File:** [`dlgImgSelect.vb`](../../dlgImgSelect.vb) — Lines 2569-2644

This method translates `ScrapeModifiers` into internal `Do*` flags based on `tContentType`:

    Private Sub SetParameters()
        Dim noSubImages As Boolean = True

        Select Case tContentType
            Case Enums.ContentType.Movie
                DoMainBanner = tScrapeModifiers.MainBanner AndAlso Master.eSettings.MovieBannerAnyEnabled
                DoMainClearArt = tScrapeModifiers.MainClearArt AndAlso Master.eSettings.MovieClearArtAnyEnabled
                DoMainClearLogo = tScrapeModifiers.MainClearLogo AndAlso Master.eSettings.MovieClearLogoAnyEnabled
                DoMainDiscArt = tScrapeModifiers.MainDiscArt AndAlso Master.eSettings.MovieDiscArtAnyEnabled
                DoMainExtrafanarts = tScrapeModifiers.MainExtrafanarts AndAlso Master.eSettings.MovieExtrafanartsAnyEnabled
                DoMainExtrathumbs = tScrapeModifiers.MainExtrathumbs AndAlso Master.eSettings.MovieExtrathumbsAnyEnabled
                DoMainFanart = tScrapeModifiers.MainFanart AndAlso Master.eSettings.MovieFanartAnyEnabled
                DoMainKeyart = tScrapeModifiers.MainKeyart AndAlso Master.eSettings.MovieKeyartAnyEnabled
                DoMainLandscape = tScrapeModifiers.MainLandscape AndAlso Master.eSettings.MovieLandscapeAnyEnabled
                DoMainPoster = tScrapeModifiers.MainPoster AndAlso Master.eSettings.MoviePosterAnyEnabled
                If DoMainExtrafanarts OrElse DoMainExtrathumbs Then noSubImages = False

            Case Enums.ContentType.MovieSet
                DoMainBanner = tScrapeModifiers.MainBanner AndAlso Master.eSettings.MovieSetBannerAnyEnabled
                DoMainClearArt = tScrapeModifiers.MainClearArt AndAlso Master.eSettings.MovieSetClearArtAnyEnabled
                DoMainClearLogo = tScrapeModifiers.MainClearLogo AndAlso Master.eSettings.MovieSetClearLogoAnyEnabled
                DoMainDiscArt = tScrapeModifiers.MainDiscArt AndAlso Master.eSettings.MovieSetDiscArtAnyEnabled
                DoMainFanart = tScrapeModifiers.MainFanart AndAlso Master.eSettings.MovieSetFanartAnyEnabled
                DoMainKeyart = tScrapeModifiers.MainKeyart AndAlso Master.eSettings.MovieSetKeyartAnyEnabled
                DoMainLandscape = tScrapeModifiers.MainLandscape AndAlso Master.eSettings.MovieSetLandscapeAnyEnabled
                DoMainPoster = tScrapeModifiers.MainPoster AndAlso Master.eSettings.MovieSetPosterAnyEnabled

            Case Enums.ContentType.TV
                ' Full TV mode - includes both show and season images
                DoAllSeasonsBanner = tScrapeModifiers.AllSeasonsBanner AndAlso Master.eSettings.TVAllSeasonsBannerAnyEnabled
                DoAllSeasonsFanart = tScrapeModifiers.AllSeasonsFanart AndAlso Master.eSettings.TVAllSeasonsFanartAnyEnabled
                DoAllSeasonsLandscape = tScrapeModifiers.AllSeasonsLandscape AndAlso Master.eSettings.TVAllSeasonsLandscapeAnyEnabled
                DoAllSeasonsPoster = tScrapeModifiers.AllSeasonsPoster AndAlso Master.eSettings.TVAllSeasonsPosterAnyEnabled
                DoMainBanner = tScrapeModifiers.MainBanner AndAlso Master.eSettings.TVShowBannerAnyEnabled
                DoMainCharacterArt = tScrapeModifiers.MainCharacterArt AndAlso Master.eSettings.TVShowCharacterArtAnyEnabled
                DoMainClearArt = tScrapeModifiers.MainClearArt AndAlso Master.eSettings.TVShowClearArtAnyEnabled
                DoMainClearLogo = tScrapeModifiers.MainClearLogo AndAlso Master.eSettings.TVShowClearLogoAnyEnabled
                DoMainExtrafanarts = tScrapeModifiers.MainExtrafanarts AndAlso Master.eSettings.TVShowExtrafanartsAnyEnabled
                DoMainFanart = tScrapeModifiers.MainFanart AndAlso Master.eSettings.TVShowFanartAnyEnabled
                DoMainKeyart = tScrapeModifiers.MainKeyart AndAlso Master.eSettings.TVShowKeyartAnyEnabled
                DoMainLandscape = tScrapeModifiers.MainLandscape AndAlso Master.eSettings.TVShowLandscapeAnyEnabled
                DoMainPoster = tScrapeModifiers.MainPoster AndAlso Master.eSettings.TVShowPosterAnyEnabled
                DoSeasonBanner = tScrapeModifiers.SeasonBanner AndAlso Master.eSettings.TVSeasonBannerAnyEnabled
                DoSeasonFanart = tScrapeModifiers.SeasonFanart AndAlso Master.eSettings.TVSeasonFanartAnyEnabled
                DoSeasonLandscape = tScrapeModifiers.SeasonLandscape AndAlso Master.eSettings.TVSeasonLandscapeAnyEnabled
                DoSeasonPoster = tScrapeModifiers.SeasonPoster AndAlso Master.eSettings.TVSeasonPosterAnyEnabled
                If DoMainExtrafanarts OrElse DoSeasonBanner OrElse DoSeasonFanart OrElse 
                   DoSeasonLandscape OrElse DoSeasonPoster Then noSubImages = False

            Case Enums.ContentType.TVShow
                ' Show-only mode - no season images
                DoMainBanner = tScrapeModifiers.MainBanner AndAlso Master.eSettings.TVShowBannerAnyEnabled
                DoMainCharacterArt = tScrapeModifiers.MainCharacterArt AndAlso Master.eSettings.TVShowCharacterArtAnyEnabled
                DoMainClearArt = tScrapeModifiers.MainClearArt AndAlso Master.eSettings.TVShowClearArtAnyEnabled
                DoMainClearLogo = tScrapeModifiers.MainClearLogo AndAlso Master.eSettings.TVShowClearLogoAnyEnabled
                DoMainExtrafanarts = tScrapeModifiers.MainExtrafanarts AndAlso Master.eSettings.TVShowExtrafanartsAnyEnabled
                DoMainFanart = tScrapeModifiers.MainFanart AndAlso Master.eSettings.TVShowFanartAnyEnabled
                DoMainKeyart = tScrapeModifiers.MainKeyart AndAlso Master.eSettings.TVShowKeyartAnyEnabled
                DoMainLandscape = tScrapeModifiers.MainLandscape AndAlso Master.eSettings.TVShowLandscapeAnyEnabled
                DoMainPoster = tScrapeModifiers.MainPoster AndAlso Master.eSettings.TVShowPosterAnyEnabled
                If DoMainExtrafanarts Then noSubImages = False

            Case Enums.ContentType.TVEpisode
                DoEpisodeFanart = tScrapeModifiers.EpisodeFanart AndAlso Master.eSettings.TVEpisodeFanartAnyEnabled
                DoEpisodePoster = tScrapeModifiers.EpisodePoster AndAlso Master.eSettings.TVEpisodePosterAnyEnabled

            Case Enums.ContentType.TVSeason
                DoAllSeasonsBanner = tScrapeModifiers.AllSeasonsBanner AndAlso Master.eSettings.TVAllSeasonsBannerAnyEnabled
                DoAllSeasonsFanart = tScrapeModifiers.AllSeasonsFanart AndAlso Master.eSettings.TVAllSeasonsFanartAnyEnabled
                DoAllSeasonsLandscape = tScrapeModifiers.AllSeasonsLandscape AndAlso Master.eSettings.TVAllSeasonsLandscapeAnyEnabled
                DoAllSeasonsPoster = tScrapeModifiers.AllSeasonsPoster AndAlso Master.eSettings.TVAllSeasonsPosterAnyEnabled
                DoSeasonBanner = tScrapeModifiers.SeasonBanner AndAlso Master.eSettings.TVSeasonBannerAnyEnabled
                DoSeasonFanart = tScrapeModifiers.SeasonFanart AndAlso Master.eSettings.TVSeasonFanartAnyEnabled
                DoSeasonLandscape = tScrapeModifiers.SeasonLandscape AndAlso Master.eSettings.TVSeasonLandscapeAnyEnabled
                DoSeasonPoster = tScrapeModifiers.SeasonPoster AndAlso Master.eSettings.TVSeasonPosterAnyEnabled
        End Select

        'If we don't have any SubImage we can hide the panel
        If noSubImages Then
            pnlImgSelectLeft.Visible = False
        End If
    End Sub

### Do* Flags Summary

| Flag | Movie | MovieSet | TV | TVShow | TVSeason | TVEpisode |
|------|:-----:|:--------:|:--:|:------:|:--------:|:---------:|
| `DoMainPoster` | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| `DoMainFanart` | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| `DoMainBanner` | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| `DoMainLandscape` | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| `DoMainClearArt` | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| `DoMainClearLogo` | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| `DoMainKeyart` | ✅ | ✅ | ✅ | ✅ | ❌ | ❌ |
| `DoMainCharacterArt` | ❌ | ❌ | ✅ | ✅ | ❌ | ❌ |
| `DoMainDiscArt` | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| `DoMainExtrafanarts` | ✅ | ❌ | ✅ | ✅ | ❌ | ❌ |
| `DoMainExtrathumbs` | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| `DoSeasonPoster` | ❌ | ❌ | ✅ | ❌ | ✅ | ❌ |
| `DoSeasonFanart` | ❌ | ❌ | ✅ | ❌ | ✅ | ❌ |
| `DoSeasonBanner` | ❌ | ❌ | ✅ | ❌ | ✅ | ❌ |
| `DoSeasonLandscape` | ❌ | ❌ | ✅ | ❌ | ✅ | ❌ |
| `DoAllSeasons*` | ❌ | ❌ | ✅ | ❌ | ✅ | ❌ |
| `DoEpisodePoster` | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |
| `DoEpisodeFanart` | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ |

---

## [↑](#table-of-contents) Image Download Pipeline

**File:** [`dlgImgSelect.vb`](../../dlgImgSelect.vb) — `DownloadAllImages()` method (lines 1545-1990)

The dialog uses parallel downloading for performance. Images are downloaded in this order:

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

### Loaded* Flags

Each category sets a `Loaded*` flag when complete:

    LoadedMainPoster = True
    LoadedMainKeyart = True
    LoadedMainBanner = True
    ' ... etc

These flags are used by `CreateListImages()` to determine when to populate the UI.

### Progress Reporting

Progress is reported via `bwImgDownload.ReportProgress()`:

    bwImgDownload.ReportProgress(iProgress, Enums.ModifierType.MainPoster)

The `bwImgDownload_ProgressChanged` handler then calls `CreateListImages()` if the reported type matches the currently selected type.

---

## [↑](#table-of-contents) UI Components

### Three-Panel Layout

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
    │  [Buttons]   │                                                  │
    │  Season Poster│  Grid of available images for selected type      │
    │  Season Fanart│  Green plus icon = selectable                   │
    │  Season      │                                                  │
    │  Landscape   │                                                  │
    │  Include     │                                                  │
    │  Fanarts     │                                                  │
    └──────────────┴──────────────────────────────────────────────────┘

### Left Panel Buttons

**File:** [`dlgImgSelect.vb`](../../dlgImgSelect.vb) — Button click handlers

| Button | Method | Purpose |
|--------|--------|---------|
| Extrafanarts | `btnExtrafanarts_Click` | Switch to extrafanarts mode |
| Extrathumbs | `btnExtrathumbs_Click` | Switch to extrathumbs mode |
| Season Banner | `btnSeasonBanner_Click` | Show season banners list |
| Season Fanart | `btnSeasonFanart_Click` | Show season fanarts list |
| Season Landscape | `btnSeasonLandscape_Click` | Show season landscapes list |
| Season Poster | `btnSeasonPoster_Click` | Show season posters list |
| **Include Fanarts** | `btnIncludeFanarts_Click` | Add fanarts to landscape grid |

### Dynamic Control Arrays

The dialog creates controls dynamically for each image:

    ' List images (main panel)
    Private pnlListImage_Panel() As Panel
    Private pbListImage_Image() As PictureBox
    Private pbListImage_Select() As PictureBox      ' Green plus icon
    Private lblListImage_Scraper() As Label
    Private lblListImageList_Resolution() As Label
    
    ' Top images (selected images)
    Private pnlTopImage_Panel() As Panel
    Private pbTopImage_Image() As PictureBox
    Private lblTopImage_Title() As Label
    Private lblTopImage_Resolution() As Label
    
    ' Sub images (seasons or extras)
    Private pnlSubImage_Panel() As Panel
    Private pbSubImage_Image() As PictureBox
    Private lblSubImage_Title() As Label
    Private lblSubImage_Resolution() As Label

---

## [↑](#table-of-contents) Include Fanarts Button

**File:** [`dlgImgSelect.vb`](../../dlgImgSelect.vb)

This button allows users to add Fanart images as options when selecting Landscape images. It supports all landscape types across Movies, TV Shows, and TV Seasons.

### Fields

    Friend WithEvents btnIncludeFanarts As New Button
    Private _fanartsIncludedForContext As String = String.Empty

| Field | Purpose |
|-------|---------|
| `btnIncludeFanarts` | The button control with event handling enabled |
| `_fanartsIncludedForContext` | Tracks which landscape type + season has had fanarts added (e.g., "MainLandscape_-2" or "SeasonLandscape_1") |

### Button Initialization — Setup()

    Private Sub Setup()
        ' ... other button text setup ...
        
        'Initialize "Include Fanarts" button for Landscape image selection
        'Add to the table layout right after Season Poster button (row 7)
        btnIncludeFanarts.Name = "btnIncludeFanarts"
        btnIncludeFanarts.Text = Master.eLang.GetString(99996, "Include Fanarts")
        btnIncludeFanarts.Dock = DockStyle.Fill
        btnIncludeFanarts.Enabled = False
        btnIncludeFanarts.Visible = False
        tblImgSelectLeft.SetColumnSpan(btnIncludeFanarts, 2)
        tblImgSelectLeft.Controls.Add(btnIncludeFanarts, 0, 7)
    End Sub

### Button Visibility — DoSelectTopImage()

    Private Sub DoSelectTopImage(ByVal iIndex As Integer, ByVal tTag As iTag)
        ' ... existing code ...
        
        ' Enable and show "Include Fanarts" button only for Landscape image types
        ' Re-enable the button if switching to a different landscape context
        Dim isLandscapeType As Boolean = (tTag.ImageType = Enums.ModifierType.MainLandscape OrElse
                                          tTag.ImageType = Enums.ModifierType.AllSeasonsLandscape OrElse
                                          tTag.ImageType = Enums.ModifierType.SeasonLandscape)
        Dim currentContext As String = String.Format("{0}_{1}", tTag.ImageType.ToString(), tTag.iSeason)
        Dim fanartsAlreadyIncluded As Boolean = (currentContext = _fanartsIncludedForContext)

        btnIncludeFanarts.Visible = isLandscapeType
        btnIncludeFanarts.Enabled = isLandscapeType AndAlso LoadedMainFanart AndAlso Not fanartsAlreadyIncluded
    End Sub

### Button Visibility — SubImageTypeChanged()

The `SeasonLandscape` type requires special handling because it's accessed via the left panel button, which deselects `currTopImage`:

    Private Sub SubImageTypeChanged(ByVal tModifierType As Enums.ModifierType)
        If Not currSubImageSelectedType = tModifierType Then
            currSubImageSelectedType = tModifierType
            ClearListImages()
            CreateSubImages()
            If currSubImageSelectedType = Enums.ModifierType.MainExtrafanarts OrElse
               currSubImageSelectedType = Enums.ModifierType.MainExtrathumbs OrElse
               currSubImageSelectedType = Enums.ModifierType.SeasonBanner OrElse
               currSubImageSelectedType = Enums.ModifierType.SeasonFanart OrElse
               currSubImageSelectedType = Enums.ModifierType.SeasonPoster Then
                currSubImage = New iTag With {.ImageType = currSubImageSelectedType, .iSeason = -2}
                DeselectAllTopImages()
                CreateListImages(currSubImage)
                ' Hide the Include Fanarts button when viewing non-Landscape sub-image types
                btnIncludeFanarts.Visible = False
                btnIncludeFanarts.Enabled = False
            ElseIf currSubImageSelectedType = Enums.ModifierType.SeasonLandscape Then
                ' SeasonLandscape needs special handling - show Include Fanarts button
                currSubImage = New iTag With {.ImageType = currSubImageSelectedType, .iSeason = -2}
                DeselectAllTopImages()
                CreateListImages(currSubImage)
                ' Show the Include Fanarts button for Season Landscape selection
                ' Re-enable if this context hasn't had fanarts included yet
                Dim currentContext As String = String.Format("{0}_{1}", currSubImage.ImageType.ToString(), currSubImage.iSeason)
                Dim fanartsAlreadyIncluded As Boolean = (currentContext = _fanartsIncludedForContext)
                btnIncludeFanarts.Visible = True
                btnIncludeFanarts.Enabled = LoadedMainFanart AndAlso Not fanartsAlreadyIncluded
            End If
        End If
        pnlImgSelectMain.Focus()
    End Sub

### Button Click Handler

    ''' <summary>
    ''' Handles the Include Fanarts button click event to add fanart images as selectable 
    ''' options for Landscape image types.
    ''' </summary>
    ''' <remarks>
    ''' Creates deep copies of MainFanart images to prevent corruption when LoadAndCache 
    ''' is called. Determines context from currTopImage (top panel) or currSubImage 
    ''' (left panel Season Landscape button).
    ''' </remarks>
    Private Sub btnIncludeFanarts_Click(sender As Object, e As EventArgs) Handles btnIncludeFanarts.Click
        If LoadedMainFanart AndAlso pnlListImage_Panel IsNot Nothing Then
            ' Determine the correct image type and season based on current selection context
            ' When viewing Season Landscapes via left panel, currTopImage is deselected so use currSubImage
            Dim targetImageType As Enums.ModifierType
            Dim targetSeason As Integer

            If currTopImage.ImageType = Enums.ModifierType.MainLandscape OrElse
               currTopImage.ImageType = Enums.ModifierType.AllSeasonsLandscape OrElse
               currTopImage.ImageType = Enums.ModifierType.SeasonLandscape Then
                ' Top image is selected (MainLandscape or AllSeasonsLandscape from top panel)
                targetImageType = currTopImage.ImageType
                targetSeason = currTopImage.iSeason
            ElseIf currSubImage.ImageType = Enums.ModifierType.SeasonLandscape OrElse
                   currSubImage.ImageType = Enums.ModifierType.AllSeasonsLandscape Then
                ' Sub-image is selected (SeasonLandscape via left panel button)
                targetImageType = currSubImage.ImageType
                targetSeason = currSubImage.iSeason
            Else
                ' Fallback - should not happen if button visibility is correct
                Exit Sub
            End If

            Dim iCount As Integer = pnlListImage_Panel.Length
            For Each tImage As MediaContainers.Image In tSearchResultsContainer.MainFanarts
                ' Create a deep copy - do NOT share ImageOriginal/ImageThumb references
                ' Only copy the URLs so the image will be re-downloaded independently
                Dim tImageCopy As New MediaContainers.Image With {
                        .Height = tImage.Height,
                        .Width = tImage.Width,
                        .ImageOriginal = New Images(),
                        .ImageThumb = New Images(),
                        .IsDuplicate = tImage.IsDuplicate,
                        .LongLang = tImage.LongLang,
                        .Scraper = tImage.Scraper,
                        .Season = tImage.Season,
                        .ShortLang = tImage.ShortLang,
                        .URLOriginal = tImage.URLOriginal,
                        .URLThumb = tImage.URLThumb
                        }
                AddListImage(tImageCopy, iCount, targetImageType, targetSeason)
                iCount += 1
            Next
            ' Track which context has had fanarts included and disable button
            _fanartsIncludedForContext = String.Format("{0}_{1}", targetImageType.ToString(), targetSeason)
            btnIncludeFanarts.Enabled = False
        End If
    End Sub

### Context Tracking Flow

    User clicks Main Landscape (top panel)
        → DoSelectTopImage sets currTopImage
        → currentContext = "MainLandscape_-2"
        → Button enabled (context is empty)
                    ↓
    User clicks "Include Fanarts"
        → Uses currTopImage for targetImageType/targetSeason
        → _fanartsIncludedForContext = "MainLandscape_-2"
        → Button disabled
                    ↓
    User clicks Season Landscape (left panel button)
        → SubImageTypeChanged sets currSubImage
        → currentContext = "SeasonLandscape_-2"
        → Different from _fanartsIncludedForContext
        → Button re-enabled!
                    ↓
    User clicks a specific season in sub-image panel
        → DoSelectSubImage sets currSubImage.iSeason
        → currentContext = "SeasonLandscape_1" (for Season 1)
        → Button may re-enable depending on context

### Why Deep Copies Are Essential

    Without Deep Copy:
    ┌─────────────────────────────────────────────────────────────┐
    │  tSearchResultsContainer.MainFanarts[0]                     │
    │      └── ImageOriginal ←─┐                                  │
    │      └── ImageThumb    ←─┤                                  │
    │                          │ (shared reference)               │
    │  Landscape List[N]  ─────┘                                  │
    │                                                             │
    │  Problem: LoadAndCache() on Landscape modifies the          │
    │  original Fanart's cached data!                             │
    └─────────────────────────────────────────────────────────────┘

    With Deep Copy:
    ┌─────────────────────────────────────────────────────────────┐
    │  tSearchResultsContainer.MainFanarts[0]                     │
    │      └── ImageOriginal (Fanart's own cache)                 │
    │      └── ImageThumb    (Fanart's own cache)                 │
    │                                                             │
    │  Landscape List[N] (deep copy)                              │
    │      └── ImageOriginal = New Images() (empty, will reload)  │
    │      └── ImageThumb    = New Images() (empty, will reload)  │
    │      └── URLOriginal   = same URL (will re-download)        │
    │                                                             │
    │  Result: Each image instance is independent                 │
    └─────────────────────────────────────────────────────────────┘

---

## [↑](#table-of-contents) Image Selection and Saving

### SetImage Method (Lines 2500-2566)

When a user clicks an image (via `pbSelect_Click`), the `SetImage` method updates the result:

    Private Sub SetImage(ByVal tTag As iTag)
        Select Case tTag.ImageType
            Case Enums.ModifierType.AllSeasonsBanner, Enums.ModifierType.SeasonBanner
                If tContentType = Enums.ContentType.TV Then
                    Result.Seasons.FirstOrDefault(Function(s) s.Season = tTag.iSeason).Banner = tTag.Image
                    RefreshSubImage(tTag)
                ElseIf tContentType = Enums.ContentType.TVSeason Then
                    Result.ImagesContainer.Banner = tTag.Image
                    RefreshTopImage(tTag)
                End If
            Case Enums.ModifierType.AllSeasonsFanart, Enums.ModifierType.SeasonFanart
                ' Similar logic for fanart
            Case Enums.ModifierType.AllSeasonsLandscape, Enums.ModifierType.SeasonLandscape
                ' Similar logic for landscape
            Case Enums.ModifierType.AllSeasonsPoster, Enums.ModifierType.SeasonPoster
                ' Similar logic for poster
            Case Enums.ModifierType.MainBanner
                Result.ImagesContainer.Banner = tTag.Image
                RefreshTopImage(tTag)
            Case Enums.ModifierType.MainFanart
                Result.ImagesContainer.Fanart = tTag.Image
                RefreshTopImage(tTag)
            Case Enums.ModifierType.MainLandscape
                Result.ImagesContainer.Landscape = tTag.Image
                RefreshTopImage(tTag)
            Case Enums.ModifierType.MainPoster
                Result.ImagesContainer.Poster = tTag.Image
                RefreshTopImage(tTag)
            ' ... other cases ...
        End Select
    End Sub

### DoneAndClose Method

When the user clicks OK, full-size images are downloaded:

    Private Sub DoneAndClose()
        btnOK.Enabled = False
        ' ... disable UI ...

        ' Cancel any running background workers
        If bwImgDefaults.IsBusy Then bwImgDefaults.CancelAsync()
        If bwImgDownload.IsBusy Then bwImgDownload.CancelAsync()
        If _cancellationTokenSource IsNot Nothing Then
            _cancellationTokenSource.Cancel()
        End If

        ' Wait for workers to complete
        While bwImgDefaults.IsBusy OrElse bwImgDownload.IsBusy
            Application.DoEvents()
            Threading.Thread.Sleep(50)
        End While

        ' Download full-size images
        lblStatus.Text = Master.eLang.GetString(952, "Downloading Fullsize Image(s)...")
        
        Result.ImagesContainer.Banner.LoadAndCache(tContentType, True)
        Result.ImagesContainer.CharacterArt.LoadAndCache(tContentType, True)
        Result.ImagesContainer.ClearArt.LoadAndCache(tContentType, True)
        Result.ImagesContainer.ClearLogo.LoadAndCache(tContentType, True)
        Result.ImagesContainer.DiscArt.LoadAndCache(tContentType, True)
        Result.ImagesContainer.Fanart.LoadAndCache(tContentType, True)
        Result.ImagesContainer.Keyart.LoadAndCache(tContentType, True)
        Result.ImagesContainer.Landscape.LoadAndCache(tContentType, True)
        Result.ImagesContainer.Poster.LoadAndCache(tContentType, True)
        
        ' Download extrafanarts/extrathumbs
        For Each img In Result.ImagesContainer.Extrafanarts
            img.LoadAndCache(tContentType, True)
        Next
        For Each img In Result.ImagesContainer.Extrathumbs
            img.LoadAndCache(tContentType, True)
        Next

        ' Download season images (for TV content type with seasons)
        ' Season images are stored in Result.Seasons, not Result.ImagesContainer
        If tContentType = Enums.ContentType.TV Then
            For Each tSeason As MediaContainers.EpisodeOrSeasonImagesContainer In Result.Seasons
                tSeason.Banner.LoadAndCache(tContentType, True)
                tSeason.Fanart.LoadAndCache(tContentType, True)
                tSeason.Landscape.LoadAndCache(tContentType, True)
                tSeason.Poster.LoadAndCache(tContentType, True)
            Next
        End If

        DialogResult = DialogResult.OK
    End Sub

**Important:** Season images are stored in `Result.Seasons` collection, not `Result.ImagesContainer`. Without the season download loop, season landscape images (including fanarts selected as landscapes) would only have thumbnails.

---

## [↑](#table-of-contents) Key Methods Reference

### dlgImgSelect.vb Methods

| Method | Purpose |
|--------|---------|
| `ShowDialog()` | Entry point, determines content type |
| `SetParameters()` | Sets Do* flags based on content type |
| `Setup()` | Initializes UI text and buttons |
| `dlgImgSelect_Shown()` | Starts background download workers |
| `bwImgDefaults_DoWork()` | Downloads default/preferred images |
| `bwImgDefaults_RunWorkerCompleted()` | Creates top panel, starts main download |
| `bwImgDownload_DoWork()` | Wrapper for DownloadAllImages |
| `DownloadAllImages()` | Parallel downloads all thumbnails |
| `CreateTopImages()` | Builds top panel with image type buttons |
| `CreateListImages()` | Checks Loaded* flags, calls FillListImages |
| `FillListImages()` | Populates main grid with images |
| `AddListImage()` | Creates panel/controls for one image |
| `DoSelectTopImage()` | Handles top image selection, manages Include Fanarts visibility |
| `DoSelectSubImage()` | Handles sub image (season) selection |
| `SubImageTypeChanged()` | Handles left panel button clicks, manages SeasonLandscape |
| `SetImage()` | Applies selected image to result |
| `DoneAndClose()` | Downloads full-size images (including seasons), returns OK |
| `btnIncludeFanarts_Click()` | Adds fanarts to landscape grid with deep copies |

### State Tracking Variables

| Variable | Type | Purpose |
|----------|------|---------|
| `tContentType` | `Enums.ContentType` | Current dialog mode |
| `tDBElement` | `Database.DBElement` | Source database element |
| `tSearchResultsContainer` | `SearchResultsContainer` | Scraped image URLs |
| `tScrapeModifiers` | `ScrapeModifiers` | What to scrape |
| `currTopImage` | `iTag` | Currently selected top image |
| `currSubImage` | `iTag` | Currently selected sub image |
| `currListImage` | `iTag` | Currently highlighted list image |
| `currListImageSelectedImageType` | `ModifierType` | Type shown in main grid |
| `currListImageSelectedSeason` | `Integer` | Season shown in main grid |
| `currSubImageSelectedType` | `ModifierType` | Type of sub-image view (e.g., SeasonLandscape) |
| `_fanartsIncludedForContext` | `String` | Tracks which landscape context has fanarts added |
| `DoOnlySeason` | `Integer` | Filter for TVSeason mode |
| `Result` | `PreferredImagesContainer` | Selected images to return |

---

## [↑](#table-of-contents) See Also

- [ImageSelectionProcess.md](ImageSelectionProcess.md) — Higher-level process documentation
- [BL-UX-004-FanartsForLandscape.md](../improvements-docs/backlog/BL-UX-004-FanartsForLandscape.md) — Include Fanarts feature implementation

---

*End of file*