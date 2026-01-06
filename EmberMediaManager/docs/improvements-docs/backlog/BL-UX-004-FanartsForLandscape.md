# BL-UX-004: Fanarts Available for Landscape Image Selection

| Field | Value |
|-------|-------|
| **ID** | BL-UX-004 |
| **Created** | January 4, 2026 |
| **Priority** | Medium |
| **Effort** | 4-5 hours |
| **Status** | ✅ Completed |
| **Completed** | January 6, 2026 |
| **Category** | UI/UX Improvements (UX) |
| **Related Files** | `dlgImgSelect.vb` |

---

## Problem Description

When selecting a Landscape image for Movies or TV Shows, the image selection dialog (`dlgImgSelect`) only displayed images specifically tagged as "Landscape" from scrapers. This significantly limited available options since:

1. Most scrapers return few dedicated Landscape images
2. Fanart images are often suitable for use as Landscape images (same aspect ratio)
3. Users had no way to use a Fanart as a Landscape without manual file manipulation

---

## Goal

Enhance the Landscape image selection experience by:

1. Providing an "Include Fanarts" button when viewing Landscape image types
2. Adding Fanart images on-demand to the Landscape selection grid
3. Ensuring selected Fanart images save correctly as Landscape files
4. Supporting all content types: Movies, TV Shows, and TV Seasons

---

## Supported Content Types

| Content Type | Landscape Types | Access Method |
|--------------|-----------------|---------------|
| **Movie** | `MainLandscape` | Top panel |
| **MovieSet** | `MainLandscape` | Top panel |
| **TV Show** | `MainLandscape` | Top panel |
| **TV (Full Scrape)** | `MainLandscape`, `AllSeasonsLandscape`, `SeasonLandscape` | Top panel + Left panel "Season Landscape" button |
| **TV Season** | `AllSeasonsLandscape`, `SeasonLandscape` | Top panel |

---

## Solution Summary

Added a button-based approach that allows users to optionally include Fanart images when selecting Landscape images:

1. Created `btnIncludeFanarts` button in the left panel
2. Button appears only when a Landscape image type is selected
3. Clicking the button adds deep copies of MainFanarts to the current list
4. Shared context tracking (`SeasonLandscape_All`) for consistent season behavior
5. Fanarts automatically re-added when switching between seasons
6. Season images are properly downloaded when dialog closes
7. UI protected with loading panel during image addition

---

## Implementation Details

### Component 1: Fields

**File:** `dlgImgSelect.vb` — Fields Region

    Friend WithEvents btnIncludeFanarts As New Button
    Private _fanartsIncludedForContext As String = String.Empty

| Field | Purpose |
|-------|---------|
| `btnIncludeFanarts` | The button control with event handling enabled |
| `_fanartsIncludedForContext` | Tracks which landscape context has had fanarts added. Uses `"SeasonLandscape_All"` for all season landscapes (shared), or `"ImageType_Season"` format for main landscapes (e.g., `"MainLandscape_-2"`) |

---

### Component 2: Button Initialization

**File:** `dlgImgSelect.vb` — `Setup()` method

    'Initialize "Include Fanarts" button for Landscape image selection
    'Add to the table layout right after Season Poster button (row 7)
    btnIncludeFanarts.Name = "btnIncludeFanarts"
    btnIncludeFanarts.Text = Master.eLang.GetString(99996, "Include Fanarts")
    btnIncludeFanarts.Dock = DockStyle.Fill
    btnIncludeFanarts.Enabled = False
    btnIncludeFanarts.Visible = False
    tblImgSelectLeft.SetColumnSpan(btnIncludeFanarts, 2)
    tblImgSelectLeft.Controls.Add(btnIncludeFanarts, 0, 7)

**Key Points:**
- Button starts hidden and disabled
- Added to row 7 of the left panel's table layout
- Spans both columns for consistent width with other buttons
- Uses localization for button text

---

### Component 3: Button Visibility Logic (Top Panel)

**File:** `dlgImgSelect.vb` — `DoSelectTopImage()` method

    ' Enable and show "Include Fanarts" button only for Landscape image types
    Dim isLandscapeType As Boolean = (tTag.ImageType = Enums.ModifierType.MainLandscape OrElse
                                      tTag.ImageType = Enums.ModifierType.AllSeasonsLandscape OrElse
                                      tTag.ImageType = Enums.ModifierType.SeasonLandscape)
    ' For SeasonLandscape, use a shared context key since fanarts are the same for all seasons
    Dim currentContext As String
    If tTag.ImageType = Enums.ModifierType.SeasonLandscape Then
        currentContext = "SeasonLandscape_All"
    Else
        currentContext = String.Format("{0}_{1}", tTag.ImageType.ToString(), tTag.iSeason)
    End If
    Dim fanartsAlreadyIncluded As Boolean = (currentContext = _fanartsIncludedForContext)

    ' If this Landscape context previously had fanarts included, re-add them
    If isLandscapeType AndAlso fanartsAlreadyIncluded AndAlso LoadedMainFanart AndAlso pnlListImage_Panel IsNot Nothing Then
        pnlLoading.Visible = True
        pnlImgSelectMain.SuspendLayout()
        ' ... re-add fanart copies ...
        pnlImgSelectMain.ResumeLayout()
        pnlLoading.Visible = False
    End If

    btnIncludeFanarts.Visible = isLandscapeType
    btnIncludeFanarts.Enabled = isLandscapeType AndAlso LoadedMainFanart AndAlso Not fanartsAlreadyIncluded

**Logic:**
- Button is visible only for the three Landscape image types
- Button is enabled only when:
  - `LoadedMainFanart = True` (fanarts have been downloaded)
  - Current context hasn't already had fanarts added
- SeasonLandscape uses shared context `"SeasonLandscape_All"` for all seasons
- When switching to a context that previously had fanarts, they are automatically re-added
- UI is protected with loading panel during re-addition
---

### Component 4: Button Visibility Logic (Left Panel - Season Landscape)

**File:** `dlgImgSelect.vb` — `SubImageTypeChanged()` method

    ElseIf currSubImageSelectedType = Enums.ModifierType.SeasonLandscape Then
        ' SeasonLandscape needs special handling - set up context and show Include Fanarts button
        currSubImage = New iTag With {.ImageType = currSubImageSelectedType, .iSeason = -2}
        DeselectAllTopImages()
        CreateListImages(currSubImage)
        ' Show the Include Fanarts button for Season Landscape selection
        ' For SeasonLandscape, use a shared context key since fanarts are the same for all seasons
        Dim currentContext As String = "SeasonLandscape_All"
        Dim fanartsAlreadyIncluded As Boolean = (currentContext = _fanartsIncludedForContext)
        btnIncludeFanarts.Visible = True
        btnIncludeFanarts.Enabled = LoadedMainFanart AndAlso Not fanartsAlreadyIncluded
    End If

**Key Fix:** `SeasonLandscape` was previously not handled in the condition list, causing the button to remain in a stale state when clicking the "Season Landscape" button in the left panel.

---

### Component 4b: Fanart Re-Addition on Season Selection

**File:** `dlgImgSelect.vb` — `DoSelectSubImage()` method

When a user selects a season in the left panel after fanarts were previously included:

    ' For SeasonLandscape or AllSeasonsLandscape, check if fanarts were previously included and re-add them
    If (tTag.ImageType = Enums.ModifierType.SeasonLandscape OrElse tTag.ImageType = Enums.ModifierType.AllSeasonsLandscape) AndAlso
       _fanartsIncludedForContext = "SeasonLandscape_All" AndAlso
       LoadedMainFanart AndAlso pnlListImage_Panel IsNot Nothing Then
        ' Show loading panel and suspend layout to prevent scrambled display while adding images
        pnlLoading.Visible = True
        pnlImgSelectMain.SuspendLayout()
        Application.DoEvents()
        
        ' Re-add fanart deep copies...
        
        pnlImgSelectMain.ResumeLayout()
        pnlLoading.Visible = False
    End If

**Key Feature:** Fanarts automatically re-appear when switching between seasons, providing a consistent experience without requiring the user to click "Include Fanarts" for each season.

---

### Component 5: Button Click Handler

**File:** `dlgImgSelect.vb` — `btnIncludeFanarts_Click()` method

    ''' <summary>
    ''' Handles the Include Fanarts button click event to add fanart images as selectable options for Landscape image types.
    ''' </summary>
    ''' <param name="sender">The button that raised the event.</param>
    ''' <param name="e">Event arguments.</param>
    ''' <remarks>
    ''' This method creates deep copies of all MainFanart images and adds them to the current Landscape image list.
    ''' Deep copies are created with new ImageOriginal and ImageThumb instances to prevent corruption when
    ''' LoadAndCache is called on the selected Landscape image during save. Only the URLs are copied so the
    ''' image will be re-downloaded independently when selected.
    ''' 
    ''' The method determines the correct image type and season context based on whether a top image 
    ''' (MainLandscape, AllSeasonsLandscape) or sub-image (SeasonLandscape via left panel) is currently active.
    ''' When viewing Season Landscapes via the left panel button, currTopImage is deselected, so we use
    ''' currSubImage instead to get the correct ModifierType and season number.
    ''' 
    ''' The button is disabled after use to prevent duplicate additions.
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
            ' For SeasonLandscape, track by image type only (not season) since fanarts are shared across all seasons
            If targetImageType = Enums.ModifierType.SeasonLandscape Then
                _fanartsIncludedForContext = "SeasonLandscape_All"
            Else
                _fanartsIncludedForContext = String.Format("{0}_{1}", targetImageType.ToString(), targetSeason)
            End If

            ' Resume layout and hide loading panel
            pnlImgSelectMain.ResumeLayout()
            pnlLoading.Visible = False

            btnIncludeFanarts.Enabled = False
        End If
    End Sub

**Critical Design Decisions:**

1. **Deep Copy Required:** Creates new `MediaContainers.Image` objects with fresh `ImageOriginal` and `ImageThumb` instances
2. **URL-Only Copy:** Only copies the URLs, not the cached image data — the image will be re-downloaded when selected
3. **Prevents Corruption:** Without deep copies, calling `LoadAndCache()` on the selected Landscape would corrupt the original Fanart's cached data
4. **Dual Context Support:** Uses `currTopImage` when available, falls back to `currSubImage` for Season Landscape via left panel
5. **Context Tracking:** Records which context had fanarts added to allow re-enabling for different contexts

---

### Component 6: Season Images Download Fix

**File:** `dlgImgSelect.vb` — `DoneAndClose()` method

    'Season images (for TV content type with seasons)
    If tContentType = Enums.ContentType.TV Then
        For Each tSeason As MediaContainers.EpisodeOrSeasonImagesContainer In Result.Seasons
            tSeason.Banner.LoadAndCache(tContentType, True)
            tSeason.Fanart.LoadAndCache(tContentType, True)
            tSeason.Landscape.LoadAndCache(tContentType, True)
            tSeason.Poster.LoadAndCache(tContentType, True)
        Next
    End If

**Key Fix:** Season images are stored in `Result.Seasons` collection, not `Result.ImagesContainer`. Without this loop, season landscape images (including fanarts selected as landscapes) would only have thumbnails — the full-size images wouldn't be downloaded before the dialog closes.

---

## Technical Architecture

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
        → currentContext = "SeasonLandscape_All" (shared for all seasons)
        → Different from _fanartsIncludedForContext
        → Button enabled!
                    ↓
    User clicks "Include Fanarts"
        → _fanartsIncludedForContext = "SeasonLandscape_All"
        → Button disabled for ALL seasons
                    ↓
    User clicks Season 2 in left panel
        → DoSelectSubImage detects _fanartsIncludedForContext = "SeasonLandscape_All"
        → Fanarts automatically re-added to list
        → Button remains disabled (already included)

### Context Key Summary

| Context Type | Key Format | Shared? |
|--------------|------------|---------|
| MainLandscape | `"MainLandscape_{season}"` | No |
| AllSeasonsLandscape | `"AllSeasonsLandscape_{season}"` | No |
| SeasonLandscape | `"SeasonLandscape_All"` | Yes (all seasons) |

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

## Bug Fixes Applied

### January 5, 2026

### Issue #1: SeasonLandscape Not Handled in SubImageTypeChanged

**Problem:** Clicking "Season Landscape" button in left panel didn't properly set up context. The button visibility logic was skipped, leaving `currSubImage` unset and the Include Fanarts button in a stale state.

**Solution:** Added `ElseIf` branch to handle `SeasonLandscape` with proper context setup, `currSubImage` initialization, and button visibility logic.

---

### Issue #2: Season Images Not Downloaded in DoneAndClose

**Problem:** For TV content type, season images are stored in `Result.Seasons` collection, but `DoneAndClose()` only called `LoadAndCache` on `Result.ImagesContainer`. Season landscapes (including fanarts selected as landscapes) wouldn't have full-size images downloaded.

**Solution:** Added loop in `DoneAndClose()` to download all season images (Banner, Fanart, Landscape, Poster) when `tContentType = Enums.ContentType.TV`.

---

### Issue #3: Button Stayed Disabled When Switching Landscape Types

**Problem:** After clicking "Include Fanarts" for one landscape type (e.g., Main Landscape), the button stayed disabled when switching to a different landscape type (e.g., Season 1 Landscape). Users had to close and reopen the dialog to add fanarts to a different landscape.

**Solution:** Added `_fanartsIncludedForContext` field to track which specific landscape context has had fanarts added. Button re-enables when switching to a different context.

---

### January 6, 2026

### Issue #4: Fanarts Not Re-Added When Switching Between Seasons

**Problem:** After clicking "Include Fanarts" for Season 1 Landscape, switching to Season 2 Landscape would not show the fanart images. The images were only added to the list when the button was clicked, but when switching seasons, the list was rebuilt from scratch without the fanarts.

**Solution:** Added fanart re-addition logic to `DoSelectSubImage()` method. When switching to a SeasonLandscape or AllSeasonsLandscape that previously had fanarts included (tracked via `_fanartsIncludedForContext = "SeasonLandscape_All"`), the fanarts are automatically re-added to the list.

---

### Issue #5: Shared Context for All SeasonLandscape Types

**Problem:** The original context tracking used format `"SeasonLandscape_{season}"` which meant fanarts had to be added separately for each season. This was inconsistent since the same MainFanarts are available for all seasons.

**Solution:** Changed to use a shared context key `"SeasonLandscape_All"` for all SeasonLandscape types. This means:
- Clicking "Include Fanarts" once marks all SeasonLandscape contexts as having fanarts
- Switching between seasons automatically re-adds the fanarts to the list
- The button remains disabled for all seasons after clicking once

---

### Issue #6: UI Scrambling When Re-Adding Fanarts

**Problem:** When switching between seasons after fanarts were included, the images would appear scrambled/overlapping as they were added to the list while the user could scroll.

**Solution:** Added `SuspendLayout()`/`ResumeLayout()` and `pnlLoading.Visible = True` protection to the fanart re-addition logic in both `DoSelectSubImage()` and `DoSelectTopImage()`. This prevents user interaction during the image addition process.

---

## Testing Checklist

### Movies
- [x] Open Movie image selection for Landscape
- [x] Verify "Include Fanarts" button appears
- [x] Click button — Fanart images appear in grid
- [x] Button disables after click
- [x] Select a Fanart image as Landscape
- [x] Verify image saves correctly as Landscape file
- [x] Verify original Fanart selection still works independently

### TV Shows - Main Landscape
- [x] Open TV Show image selection (with seasons)
- [x] Click Main Landscape in top panel
- [x] Verify "Include Fanarts" button appears
- [x] Click button — Fanart images appear in grid
- [x] Select a Fanart image as Main Landscape
- [x] Verify image saves correctly

### TV Shows - Season Landscape (via Left Panel)
- [x] Open TV Show image selection (with seasons)
- [x] Click "Season Landscape" button in left panel
- [x] Verify season list appears in sub-image panel
- [x] Click on a specific season (e.g., Season 1)
- [x] Verify "Include Fanarts" button appears and is enabled
- [x] Click button — Fanart images appear in grid
- [x] Select a Fanart image as Season Landscape
- [x] Verify image saves correctly for that season

### TV Shows - Multiple Landscape Types
- [x] Open TV Show image selection (with seasons)
- [x] Click Main Landscape, add fanarts, button disables
- [x] Click "Season Landscape" button in left panel
- [x] Select a season — button should be disabled (shared context)
- [x] Fanarts automatically appear in season landscape list
- [x] Verify both Main Landscape and Season Landscape have correct images

### TV Shows - Season Switching
- [x] Click "Include Fanarts" for Season 1 Landscape
- [x] Switch to Season 2 — fanarts automatically re-added
- [x] Switch back to Season 1 — fanarts still present
- [x] Button remains disabled for all seasons
- [x] Loading panel displays during re-addition (no UI scrambling)

### TV Seasons (Single Season Edit)
- [x] Open TV Season image selection
- [x] Verify landscape options in top panel
- [x] Test "Include Fanarts" functionality
- [x] Verify image saves correctly

### Edge Cases
- [x] Button hidden when viewing Extrafanarts
- [x] Button hidden when viewing Extrathumbs
- [x] Button hidden when viewing Season Banner/Fanart/Poster
- [x] Button disabled if MainFanarts not yet loaded
- [x] No duplicate images after multiple dialog opens
- [x] Button behavior correct when switching between landscape types
- [x] Season images download correctly when clicking OK
- [x] No UI scrambling when fanarts are re-added on season switch

---

## Future Enhancements

### Potential Improvement: Include SeasonFanarts for Season Landscapes

Currently, only `MainFanarts` are added for all Landscape types. For `SeasonLandscape`, it might be useful to also include `SeasonFanarts`:

    ' In btnIncludeFanarts_Click, add after MainFanarts loop:
    If targetImageType = Enums.ModifierType.SeasonLandscape AndAlso 
       LoadedSeasonFanart Then
        For Each tImage In tSearchResultsContainer.SeasonFanarts.Where(
            Function(f) f.Season = targetSeason)
            ' Create deep copy and add...
        Next
    End If

This would provide season-specific fanart options when editing season landscapes.

---

## Related Files

| File | Purpose |
|------|---------|
| `EmberMediaManager\dlgImgSelect.vb` | Main implementation — button, click handler, visibility logic, season download fix |

---

## Change History

| Date | Description |
|------|-------------|
| January 4, 2026 | Initial implementation for Movies |
| January 5, 2026 | Extended to TV Shows with bug fixes for SeasonLandscape handling, season image downloads, and button re-enable logic |
| January 6, 2026 | Fixed season switching issues — fanarts now auto-reload when switching seasons, shared context for all SeasonLandscape types, UI scrambling prevention |

---

## Completion Summary

This enhancement successfully adds the ability to use Fanart images as Landscape images across all content types:

- **Movies/MovieSets**: Single "Include Fanarts" click adds fanarts to MainLandscape
- **TV Shows**: Fanarts can be added to MainLandscape, AllSeasonsLandscape, and all SeasonLandscape types
- **TV Seasons**: Full support for landscape selection with fanarts

Key technical decisions:
1. **Deep copies** prevent image corruption between Fanart and Landscape contexts
2. **Shared context tracking** (`SeasonLandscape_All`) provides consistent behavior across all seasons
3. **Automatic re-addition** ensures fanarts persist when switching between seasons
4. **UI protection** (SuspendLayout/loading panel) prevents visual glitches during image addition

---

*Created: January 4, 2026*
*Completed: January 6, 2026*