# BL-UX-007: Auto-Select Extrafanarts View When Only Image Type Enabled

| Field | Value |
|-------|-------|
| **ID** | BL-UX-007 |
| **Created** | January 10, 2026 |
| **Priority** | Low |
| **Effort** | 1 hour |
| **Status** | ✅ Complete |
| **Category** | UI/UX Improvements |
| **Related Files** | [`dlgImgSelect.vb`](../../../dlgImgSelect.vb) |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Summary

When the Edit Images dialog (`dlgImgSelect`) opens with **only** Extrafanarts enabled, it now automatically displays the extrafanarts panel instead of requiring the user to click the "Extrafanarts" button. This improves UX when the user specifically opened the dialog to manage extrafanarts.

---

## Table of Contents

- [Problem Description](#problem-description)
- [User Flow](#user-flow)
- [Solution Summary](#solution-summary)
- [Implementation Details](#implementation-details)
- [Testing Summary](#testing-summary)
- [Related Files](#related-files)
- [Notes](#notes)

---

## [↑](#table-of-contents) Problem Description

When a user opens the image selection dialog specifically to edit Extrafanarts (e.g., from Edit Movie → Images tab → Extrafanarts button), they must still click the "Extrafanarts" button on the left panel to see the extrafanarts list. This is an unnecessary extra step since the only reason they opened the dialog was to manage extrafanarts.

**Previous behavior:**
1. User clicks button to edit Extrafanarts specifically
2. Dialog opens showing empty top images area
3. User must click "Extrafanarts" button on left panel
4. Extrafanarts list finally appears

**New behavior:**
1. User clicks button to edit Extrafanarts specifically
2. Dialog opens and automatically shows Extrafanarts panel ✅

---

## [↑](#table-of-contents) User Flow

### When This Applies

The auto-select happens when:
- `DoMainExtrafanarts = True` AND no other image types are enabled

### When This Does NOT Apply

- Full image selection (Poster, Fanart, Banner, etc. enabled)
- Multiple sub-image types enabled
- TV Show season image selection

---

## [↑](#table-of-contents) Solution Summary

Modified `bwImgDefaults_RunWorkerCompleted` in `dlgImgSelect.vb` to check if only Extrafanarts is enabled and automatically call `SubImageTypeChanged()`.

---

## [↑](#table-of-contents) Implementation Details

### Location

**File:** [`EmberMediaManager\dlgImgSelect.vb`](../../../dlgImgSelect.vb)

### Changes Made

**1. Modified `bwImgDefaults_RunWorkerCompleted`:**

    Private Sub bwImgDefaults_RunWorkerCompleted(...) Handles bwImgDefaults.RunWorkerCompleted
        If Not e.Cancelled Then
            CreateTopImages()

            ' Auto-select Extrafanarts view if it's the only enabled image type
            ' This improves UX when user opens dialog specifically to edit extrafanarts
            If IsOnlyExtrafanartsEnabled() Then
                SubImageTypeChanged(Enums.ModifierType.MainExtrafanarts)
            End If

            lblStatus.Text = Master.eLang.GetString(954, "(Down)Loading New Images...")
            ' ... rest of method
        End If
    End Sub

**2. Added helper method `IsOnlyExtrafanartsEnabled()`:**

    ''' <summary>
    ''' Checks if Extrafanarts is the only enabled image type.
    ''' Used to auto-select Extrafanarts view when dialog opens for extrafanarts-only editing.
    ''' </summary>
    ''' <returns>True if only Extrafanarts is enabled, False otherwise.</returns>
    Private Function IsOnlyExtrafanartsEnabled() As Boolean
        Return DoMainExtrafanarts AndAlso
               Not DoMainBanner AndAlso
               Not DoMainCharacterArt AndAlso
               Not DoMainClearArt AndAlso
               Not DoMainClearLogo AndAlso
               Not DoMainDiscArt AndAlso
               Not DoMainExtrathumbs AndAlso
               Not DoMainFanart AndAlso
               Not DoMainKeyart AndAlso
               Not DoMainLandscape AndAlso
               Not DoMainPoster AndAlso
               Not DoAllSeasonsBanner AndAlso
               Not DoAllSeasonsFanart AndAlso
               Not DoAllSeasonsLandscape AndAlso
               Not DoAllSeasonsPoster AndAlso
               Not DoSeasonBanner AndAlso
               Not DoSeasonFanart AndAlso
               Not DoSeasonLandscape AndAlso
               Not DoSeasonPoster AndAlso
               Not DoEpisodeFanart AndAlso
               Not DoEpisodePoster
    End Function

---

## [↑](#table-of-contents) Testing Summary

| Status | Test Scenario |
|:------:|---------------|
| ✅ | Movie: Open Edit Images with only Extrafanarts → auto-shows extrafanarts |
| ✅ | Movie: Open Edit Images with multiple types → does NOT auto-select |
| ✅ | TV Show: Open Edit Images with only Extrafanarts → auto-shows extrafanarts |
| ✅ | Full scrape image selection → does NOT auto-select extrafanarts |
| ✅ | Dialog still functions normally after auto-select |

---

## [↑](#table-of-contents) Related Files

| File | Purpose |
|------|---------|
| [`EmberMediaManager\dlgImgSelect.vb`](../../../dlgImgSelect.vb) | Dialog modified |
| [`EmberMediaManager\dlgEdit_Movie.vb`](../../../dlgEdit_Movie.vb) | Calls dlgImgSelect for movies |
| [`EmberMediaManager\dlgEdit_TVShow.vb`](../../../dlgEdit_TVShow.vb) | Calls dlgImgSelect for TV shows |

---

## [↑](#table-of-contents) Notes

- The `Do*` flags are set in `SetParameters()` based on `ScrapeModifiers` passed to the dialog
- The `SubImageTypeChanged()` method already handles all the UI updates needed
- This change is backward-compatible — existing workflows are unaffected
- Extrathumbs support not implemented as it will be deprecated in a future release

---

## [↑](#table-of-contents) Change History

| Date | Description |
|------|-------------|
| January 10, 2026 | Created |
| January 10, 2026 | Implemented and tested - Complete |

---

*End of file*