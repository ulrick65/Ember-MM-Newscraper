# BL-KI-004: Extrafanarts/Extrathumbs Dialog Preselect Fix

| Field | Value |
|-------|-------|
| **ID** | BL-KI-004 |
| **Created** | January 8, 2026 |
| **Priority** | Medium |
| **Effort** | 2-3 hours |
| **Status** | ✅ Completed |
| **Completed** | January 8, 2026 |
| **Category** | Known Issues (KI) |
| **Related Files** | `dlgImgSelect.vb` |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Summary

The Image Selection Dialog incorrectly auto-selected new extrafanarts/extrathumbs when `Preselect=False` due to flawed `OrElse` logic and a missing `ContentType.TV` case.

---

## Table of Contents

- [Problem Description](#problem-description)
- [Reproduction Steps](#reproduction-steps)
- [Root Cause Analysis](#root-cause-analysis)
- [Solution Summary](#solution-summary)
- [Implementation Details](#implementation-details)
- [Testing Summary](#testing-summary)
- [Related Files](#related-files)
- [Notes](#notes)
- [Change History](#change-history)

---

## [↑](#table-of-contents) Problem Description

The Image Selection Dialog was incorrectly auto-selecting new extrafanarts/extrathumbs even when the `Preselect` setting was set to `False`. This occurred due to two issues:

### Issue 1: Incorrect OrElse Logic

The dialog's pre-selection logic used an `OrElse` condition that checked both `Preselect` and `KeepExisting` settings, causing the dialog to auto-populate with new images whenever `KeepExisting=True`, regardless of `Preselect`.

### Issue 2: Missing ContentType.TV Case

For TV Shows scraped with seasons, the `tContentType` was set to `Enums.ContentType.TV` (not `TVShow`), causing the extrafanarts logic to be skipped entirely in the `Select Case` statement.

---

## [↑](#table-of-contents) Reproduction Steps

1. Set Movie Extrafanarts settings: `Preselect=False`, `KeepExisting=True`, `Limit=5`
2. Have a movie with 2 existing extrafanarts
3. Open Edit Images dialog for that movie
4. **Observed:** Dialog shows 5 images (2 existing + 3 new auto-selected)
5. **Expected:** Dialog shows only 2 existing images (user manually adds more)

---

## [↑](#table-of-contents) Root Cause Analysis

### Code Flow

    dlgImgSelect.ShowDialog()
        │
        ▼
    dlgImgSelect_Shown()
        │
        ▼
    bwImgDefaults.RunWorkerAsync()
        │
        ▼
    bwImgDefaults_DoWork()
        │
        ├──► GetPreferredImages()        ◄─── Dialog wrapper method
        │         │
        │         ▼
        │    Images.GetPreferredImagesContainer()  ◄─── Auto-selection logic
        │         │
        │         ▼
        │    Returns tPreferredImagesContainer (with auto-selected images)
        │         │
        │         ▼
        │    Dialog applies Preselect filter to Result
        │
        └──► DownloadDefaultImages()

### Why Auto-Scrape Was Unaffected

The auto-scraper calls `Images.GetPreferredImagesContainer()` directly and uses its output without additional filtering. The `GetPreferredImagesContainer()` method correctly uses only `KeepExisting` and `Limit` settings — it never checks `Preselect`.

The bug was isolated to the dialog's `GetPreferredImages()` method, which incorrectly added images when `KeepExisting=True` regardless of `Preselect`.

---

## [↑](#table-of-contents) Solution Summary

### Fix 1: Correct Preselect Logic

Changed the dialog's extrafanarts/extrathumbs pre-selection to check `Preselect` first.

### Before

    If .MovieExtrafanartsPreselect OrElse .MovieExtrafanartsKeepExisting Then
        Result.ImagesContainer.Extrafanarts.AddRange(tPreferredImagesContainer.ImagesContainer.Extrafanarts)
    End If

### After

    If .MovieExtrafanartsPreselect Then
        Result.ImagesContainer.Extrafanarts.AddRange(tPreferredImagesContainer.ImagesContainer.Extrafanarts)
    Else
        Result.ImagesContainer.Extrafanarts.AddRange(tDBElement.ImagesContainer.Extrafanarts)
    End If

### Fix 2: Add ContentType.TV to Select Case

    ' Before:
    Case Enums.ContentType.TVShow

    ' After:
    Case Enums.ContentType.TVShow, Enums.ContentType.TV

---

## [↑](#table-of-contents) Implementation Details

**File:** `dlgImgSelect.vb` — `GetPreferredImages()` method

    ' Dialog pre-selection logic for extrafanarts/extrathumbs:
    ' - Preselect=True: Use full auto-selected set from GetPreferredImagesContainer (respects KeepExisting and Limit)
    ' - Preselect=False: Only show existing images - user manually manages selection
    ' Note: KeepExisting only affects auto-selection behavior when Preselect=True
    ' Note: TVShow with seasons uses ContentType.TV, so we check both TVShow and TV
    Select Case tContentType
        Case Enums.ContentType.Movie
            If .MovieExtrafanartsPreselect Then
                Result.ImagesContainer.Extrafanarts.AddRange(tPreferredImagesContainer.ImagesContainer.Extrafanarts)
            Else
                Result.ImagesContainer.Extrafanarts.AddRange(tDBElement.ImagesContainer.Extrafanarts)
            End If
            If .MovieExtrathumbsPreselect Then
                Result.ImagesContainer.Extrathumbs.AddRange(tPreferredImagesContainer.ImagesContainer.Extrathumbs)
            Else
                Result.ImagesContainer.Extrathumbs.AddRange(tDBElement.ImagesContainer.Extrathumbs)
            End If
        Case Enums.ContentType.TVShow, Enums.ContentType.TV
            If .TVShowExtrafanartsPreselect Then
                Result.ImagesContainer.Extrafanarts.AddRange(tPreferredImagesContainer.ImagesContainer.Extrafanarts)
            Else
                Result.ImagesContainer.Extrafanarts.AddRange(tDBElement.ImagesContainer.Extrafanarts)
            End If
    End Select

---

## [↑](#table-of-contents) Testing Summary

*(Status: ⬜ Not tested, ✅ Passed, ❌ Failed, ⏸️ Skipped)*

### Movies

| Status | Test Scenario |
|:------:|---------------|
| ✅ | Preselect=False, KeepExisting=True: Only existing images shown |
| ✅ | Preselect=True, KeepExisting=True: Existing + new up to limit |
| ✅ | Auto-Scrape unchanged: Respects KeepExisting and Limit |

### TV Shows

| Status | Test Scenario |
|:------:|---------------|
| ✅ | Preselect=False with seasons: Only existing images shown |
| ✅ | Preselect=True with seasons: New images auto-selected |
| ✅ | Auto-Scrape unchanged: Respects KeepExisting and Limit |

---

## [↑](#table-of-contents) Related Files

| File | Purpose |
|------|---------|
| `EmberMediaManager\dlgImgSelect.vb` | Dialog pre-selection logic fix |
| `EmberAPI\clsAPIImages.vb` | Auto-scraper logic (unchanged) |

---

## [↑](#table-of-contents) Notes

### Settings Behavior Matrix

| Setting | Purpose | Where it applies |
|---------|---------|------------------|
| **Preselect** | Pre-populate selections when opening the Image Dialog | Dialog only |
| **KeepExisting** | When auto-selecting, append to existing vs replace | Auto-scrape + Dialog pre-selection |
| **Limit** | Max number of images to auto-select | Auto-scrape + Dialog pre-selection |

**Key Insight:** `Preselect` controls whether the dialog auto-adds NEW images. When `Preselect=False`, the dialog should only show existing images regardless of `KeepExisting`.

---

## [↑](#table-of-contents) Change History

| Date | Description |
|------|-------------|
| January 8, 2026 | Fixed Preselect logic and added ContentType.TV case |

---

*End of file*