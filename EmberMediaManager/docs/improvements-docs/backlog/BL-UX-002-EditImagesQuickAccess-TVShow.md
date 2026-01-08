# BL-UX-002: Edit Images Quick Access - TV Shows

| Field | Value |
|-------|-------|
| **ID** | BL-UX-002 |
| **Created** | January 3, 2026 |
| **Priority** | Medium |
| **Effort** | 1-2 hours |
| **Status** | ✅ Completed |
| **Category** | UI/UX Improvements (UX) |
| **Related Files** | [`frmMain.vb`](../../../frmMain.vb), [`frmMain.Designer.vb`](../../../frmMain.Designer.vb), [`dlgImgSelect.vb`](../../../dlgImgSelect.vb) |
| **Depends On** | BL-UX-001 (pattern established) |

---

## Problem

Same issue as movies - editing TV Show images requires going through the full Edit dialog. The `dlgImgSelect` dialog provides a better experience but is only accessible during scraping.

---

## Goal

Add an "Edit Images..." option to the TV Show context menu that:
1. Scrapes fresh images from configured sources (TMDB, TVDB, Fanart.tv, etc.)
2. Opens `dlgImgSelect` directly with all image types **including Season images**
3. Saves selected images without modifying NFO or other metadata

---

## Current Status

### What Works
- Menu item appears in TV Show context menu
- Dialog opens with scraped images
- Show-level images (Poster, Fanart, Banner, etc.) can be selected
- "Include Fanarts" button appears for Landscape images

### Bug Found (January 6, 2026)
The dialog opens in **TVShow mode** instead of **TV mode**, which means:
- ❌ Season buttons (Season Banner, Season Fanart, etc.) are not visible
- ❌ Left panel with season list is hidden
- ❌ Cannot select season-specific images
- ✅ Show-level images work correctly

---

## Root Cause Analysis

### How dlgImgSelect Determines Mode

**File:** [`dlgImgSelect.vb`](../../../dlgImgSelect.vb) — `ShowDialog()` method (lines 199-205)

    Select Case DBElement.ContentType
        Case Enums.ContentType.TVShow
            If ScrapeModifiers.withEpisodes OrElse ScrapeModifiers.withSeasons Then
                tContentType = Enums.ContentType.TV      ' Full TV mode with seasons
            Else
                tContentType = Enums.ContentType.TVShow  ' Show-only mode
            End If

### The Problem

The current implementation in [`frmMain.vb`](../../../frmMain.vb) sets:

    Dim nScrapeModifiers As New Structures.ScrapeModifiers With {
        .MainBanner = Master.eSettings.TVShowBannerAnyEnabled,
        .MainFanart = Master.eSettings.TVShowFanartAnyEnabled,
        ' ... other Main* flags
        ' MISSING: .withSeasons = True
        ' MISSING: .SeasonBanner, .SeasonFanart, etc.
    }

Without `.withSeasons = True`, the dialog opens in `TVShow` mode (show images only).

### Content Type Differences

| tContentType | Mode | Season Buttons | Left Panel | Use Case |
|--------------|------|----------------|------------|----------|
| `TV` | Full TV | ✅ Visible | ✅ Shows seasons | Full scrape with seasons |
| `TVShow` | Show Only | ❌ Hidden | ❌ Hidden | Show images only |
| `TVSeason` | Single Season | ❌ Hidden | ❌ Hidden | Season-specific edit |

### SetParameters() Logic

**File:** [`dlgImgSelect.vb`](../../../dlgImgSelect.vb) — `SetParameters()` method (lines 2569-2644)

The `SetParameters()` method uses `tContentType` to determine which `Do*` flags to set:

- **`Enums.ContentType.TV`** (lines 2594-2614): Sets both show-level AND season-level flags
- **`Enums.ContentType.TVShow`** (lines 2615-2625): Sets ONLY show-level flags

This is why season buttons don't appear — the `DoSeasonBanner`, `DoSeasonFanart`, etc. flags are never set to `True`.

---

## Required Fix

### Updated ScrapeModifiers

**File:** [`frmMain.vb`](../../../frmMain.vb) — `cmnuShowEditImages_Click` method

**Current (Broken):**

    Dim nScrapeModifiers As New Structures.ScrapeModifiers With {
        .MainBanner = Master.eSettings.TVShowBannerAnyEnabled,
        .MainCharacterArt = Master.eSettings.TVShowCharacterArtAnyEnabled,
        .MainClearArt = Master.eSettings.TVShowClearArtAnyEnabled,
        .MainClearLogo = Master.eSettings.TVShowClearLogoAnyEnabled,
        .MainExtrafanarts = Master.eSettings.TVShowExtrafanartsAnyEnabled,
        .MainFanart = Master.eSettings.TVShowFanartAnyEnabled,
        .MainKeyart = Master.eSettings.TVShowKeyartAnyEnabled,
        .MainLandscape = Master.eSettings.TVShowLandscapeAnyEnabled,
        .MainPoster = Master.eSettings.TVShowPosterAnyEnabled
    }

**Fixed:**

    Dim nScrapeModifiers As New Structures.ScrapeModifiers With {
        ' Show-level images
        .MainBanner = Master.eSettings.TVShowBannerAnyEnabled,
        .MainCharacterArt = Master.eSettings.TVShowCharacterArtAnyEnabled,
        .MainClearArt = Master.eSettings.TVShowClearArtAnyEnabled,
        .MainClearLogo = Master.eSettings.TVShowClearLogoAnyEnabled,
        .MainExtrafanarts = Master.eSettings.TVShowExtrafanartsAnyEnabled,
        .MainFanart = Master.eSettings.TVShowFanartAnyEnabled,
        .MainKeyart = Master.eSettings.TVShowKeyartAnyEnabled,
        .MainLandscape = Master.eSettings.TVShowLandscapeAnyEnabled,
        .MainPoster = Master.eSettings.TVShowPosterAnyEnabled,
        ' Season-level images
        .AllSeasonsBanner = Master.eSettings.TVAllSeasonsBannerAnyEnabled,
        .AllSeasonsFanart = Master.eSettings.TVAllSeasonsFanartAnyEnabled,
        .AllSeasonsLandscape = Master.eSettings.TVAllSeasonsLandscapeAnyEnabled,
        .AllSeasonsPoster = Master.eSettings.TVAllSeasonsPosterAnyEnabled,
        .SeasonBanner = Master.eSettings.TVSeasonBannerAnyEnabled,
        .SeasonFanart = Master.eSettings.TVSeasonFanartAnyEnabled,
        .SeasonLandscape = Master.eSettings.TVSeasonLandscapeAnyEnabled,
        .SeasonPoster = Master.eSettings.TVSeasonPosterAnyEnabled,
        ' Critical flag to enable TV mode
        .withSeasons = True
    }

### Updated Scraper Call

The scraper call may also need to change to fetch season images. Need to verify if `ScrapeImage_TVShow` in [`clsAPIModules.vb`](../../../EmberAPI/clsAPIModules.vb) fetches season images when `withSeasons = True`, or if a different method is needed.

**Current:**

    If Not ModulesManager.Instance.ScrapeImage_TVShow(tmpDBElement, nSearchResultsContainer, nScrapeModifiers, True) Then

### Updated Images Found Check

    Dim bImagesFound As Boolean = nSearchResultsContainer.MainBanners.Count > 0 OrElse
                                  nSearchResultsContainer.MainCharacterArts.Count > 0 OrElse
                                  nSearchResultsContainer.MainClearArts.Count > 0 OrElse
                                  nSearchResultsContainer.MainClearLogos.Count > 0 OrElse
                                  nSearchResultsContainer.MainFanarts.Count > 0 OrElse
                                  nSearchResultsContainer.MainKeyarts.Count > 0 OrElse
                                  nSearchResultsContainer.MainLandscapes.Count > 0 OrElse
                                  nSearchResultsContainer.MainPosters.Count > 0 OrElse
                                  nSearchResultsContainer.SeasonBanners.Count > 0 OrElse
                                  nSearchResultsContainer.SeasonFanarts.Count > 0 OrElse
                                  nSearchResultsContainer.SeasonLandscapes.Count > 0 OrElse
                                  nSearchResultsContainer.SeasonPosters.Count > 0

### Updated Save Logic

After dialog closes, need to save season images as well:

    If dlgImgS.ShowDialog(tmpDBElement, nSearchResultsContainer, nScrapeModifiers) = DialogResult.OK Then
        ' Apply selected images back to the TV show
        tmpDBElement.ImagesContainer = dlgImgS.Result.ImagesContainer
        
        ' Apply season images
        For Each seasonResult In dlgImgS.Result.Seasons
            Dim dbSeason = tmpDBElement.Seasons.FirstOrDefault(Function(s) s.TVSeason.Season = seasonResult.Season)
            If dbSeason IsNot Nothing Then
                dbSeason.ImagesContainer.Banner = seasonResult.Banner
                dbSeason.ImagesContainer.Fanart = seasonResult.Fanart
                dbSeason.ImagesContainer.Landscape = seasonResult.Landscape
                dbSeason.ImagesContainer.Poster = seasonResult.Poster
            End If
        Next
        
        ' Save to database
        Master.DB.Save_TVShow(tmpDBElement, False, False, True, False, False)
        
        ' Save images to disk (show images)
        tmpDBElement.ImagesContainer.SaveAllImages(tmpDBElement, False)
        
        ' Save season images to disk
        For Each dbSeason In tmpDBElement.Seasons
            dbSeason.ImagesContainer.SaveAllImages(dbSeason, False)
        Next
        
        ' Refresh the display
        RefreshRow_TVShow(tmpDBElement.ID)
        LoadInfo_TVShow(tmpDBElement.ID)
    End If

---

## Original Implementation (Reference)

### Step 1: Add Menu Item to Designer

**File:** [`frmMain.Designer.vb`](../../../frmMain.Designer.vb)

Find the `cmnuShow` ContextMenuStrip definition and add a new item after `cmnuShowEdit`:

    'cmnuShowEditImages
    '
    Me.cmnuShowEditImages = New System.Windows.Forms.ToolStripMenuItem()
    Me.cmnuShowEditImages.Name = "cmnuShowEditImages"
    Me.cmnuShowEditImages.Size = New System.Drawing.Size(200, 22)
    Me.cmnuShowEditImages.Text = "Edit Images..."

Add to the `cmnuShow.Items.AddRange` array, after `cmnuShowEdit`:

    Me.cmnuShowEdit,
    Me.cmnuShowEditImages,  ' <-- Add this line

Add the field declaration in the fields region:

    Friend WithEvents cmnuShowEditImages As System.Windows.Forms.ToolStripMenuItem

---

### Step 2: Enable/Disable Logic

**File:** [`frmMain.vb`](../../../frmMain.vb)

In `cmnuShow_Opened`, add visibility/enabled logic:

    cmnuShowEditImages.Enabled = (dgvTVShows.SelectedRows.Count = 1)

This ensures the menu item is only enabled when exactly one TV show is selected.

---

## Key Differences from Movie Implementation

| Aspect | Movie | TV Show |
|--------|-------|---------|
| DataGridView | `dgvMovies` | `dgvTVShows` |
| ID Column | `idMovie` | `idShow` |
| Load Method | `Master.DB.Load_Movie()` | `Master.DB.Load_TVShow()` |
| Online Check | `CheckOnlineStatus_Movie()` | `CheckOnlineStatus_TVShow()` |
| Scrape Method | `ScrapeImage_Movie()` | `ScrapeImage_TVShow()` |
| Save Method | `Master.DB.Save_Movie()` | `Master.DB.Save_TVShow()` |
| Refresh Method | `RefreshRow_Movie()` | `RefreshRow_TVShow()` |
| Load Info Method | `LoadInfo_Movie()` | `LoadInfo_TVShow()` |
| Has CharacterArt | No | Yes |
| Has DiscArt | Yes | No |
| Has Extrathumbs | Yes | No |
| **Has Seasons** | **No** | **Yes - requires `.withSeasons = True`** |

---

## Menu Structure After Change

    Right-click on TV show
            ↓
    ┌─────────────────────────┐
    │ Edit...                 │
    │ Edit Images...          │  ← Opens full TV mode with seasons
    │ ─────────────────────── │
    │ Scrape                  │
    │ ...                     │
    └─────────────────────────┘

---

## Testing Checklist

### Menu Functionality
- [x] Menu item appears in TV show context menu
- [x] Menu item disabled when no show selected
- [x] Menu item disabled when multiple shows selected

### Show-Level Images (Currently Working)
- [x] Clicking opens `dlgImgSelect` with scraped images
- [x] Show-level image types appear in dialog (Poster, Fanart, Banner, etc.)
- [x] CharacterArt appears (TV-specific)
- [x] "Include Fanarts" button works for Landscape

### Season Images
- [x] Season buttons visible in left panel (Season Banner, Season Fanart, etc.)
- [x] Season list appears in sub-images panel
- [x] Can select season-specific images
- [x] Season images save correctly to disk

### Save Functionality
- [x] Selecting show images and clicking OK saves them
- [x] Show images appear on disk in correct locations
- [x] Season images save to correct locations
- [x] Main window refreshes to show new images
- [x] Canceling dialog makes no changes

### Edge Cases
- [x] Works for shows with no existing images
- [x] Works for shows with existing images (replace)
- [x] Error handling for offline shows

---

## Related Files

| File | Purpose |
|------|---------|
| [`frmMain.vb`](../../../frmMain.vb) | Contains `cmnuShowEditImages_Click` handler |
| [`frmMain.Designer.vb`](../../../frmMain.Designer.vb) | Menu item definition |
| [`dlgImgSelect.vb`](../../../dlgImgSelect.vb) | Image selection dialog — mode determined by `ScrapeModifiers.withSeasons` |
| [`clsAPIModules.vb`](../../../EmberAPI/clsAPIModules.vb) | Contains `ScrapeImage_TVShow()` method |
| [`Structures.vb`](../../../EmberAPI/Structures.vb) | Contains `ScrapeModifiers` structure definition |

---

## See Also

- [BL-UX-001-EditImagesQuickAccess.md](BL-UX-001-EditImagesQuickAccess.md) — Movie implementation (reference)
- [BL-UX-003-EditImagesQuickAccess-TVSeason.md](BL-UX-003-EditImagesQuickAccess-TVSeason.md) — TV Season implementation
- [ImageSelectionProcess.md](../../process-docs/ImageSelectionProcess.md) — Image selection architecture

---

*Created: January 2, 2026*
*Initial Implementation: January 4, 2026*
*Bug Found: January 5, 2026 — Season buttons not visible*