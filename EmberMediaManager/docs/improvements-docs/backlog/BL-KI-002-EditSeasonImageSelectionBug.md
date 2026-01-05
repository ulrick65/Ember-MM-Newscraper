# BL-KI-002: Edit Season Dialog - Most Images Not Selectable

| Field | Value |
|-------|-------|
| **ID** | BL-KI-002 |
| **Created** | January 4, 2026 |
| **Priority** | High |
| **Effort** | 2-3 |
| **Status** | ✅ Completed |
| **Category** | Known Issues (KI) |
| **Related Files** | `EmberMediaManager\dlgImgSelect.vb` |

---

## Problem Description

When editing a TV Season via **Edit Season dialog → Images tab**, clicking to select images from the image picker shows that most images cannot be selected. The green plus icon (indicating the image can be selected) only appears on a few images, while the majority of images are not selectable.

**Additional Symptom Discovered:** When selecting Fanart for a TVSeason, the images load twice (duplicating all images in the list), and the dialog may hang due to the excessive number of UI controls being created.

---

## Resolution Summary

**Fixed on:** January 4, 2026

**Root Cause:** In `DownloadAllImages()`, after MainFanarts download completed, the code reported progress for `SeasonFanart` and `AllSeasonsFanart` types. Then later, after SeasonFanarts download completed, it reported progress for those same types again. This caused `CreateListImages()` to be called twice, duplicating all images in the picker.

**Fix Applied:** Added conditional checks before reporting progress for season/episode fanart types after MainFanarts download. Progress is now only reported for types that won't have dedicated downloads later:

    ' In DownloadAllImages(), after MainFanarts download (around line 1680):
    If Not (DoSeasonFanart OrElse DoAllSeasonsFanart) Then
        bwImgDownload.ReportProgress(iProgress, Enums.ModifierType.AllSeasonsFanart)
        bwImgDownload.ReportProgress(iProgress, Enums.ModifierType.SeasonFanart)
    End If
    If Not DoEpisodeFanart Then
        bwImgDownload.ReportProgress(iProgress, Enums.ModifierType.EpisodeFanart)
    End If

---

## Steps to Reproduce (Before Fix)

1. Right-click on a TV Season
2. Select "Edit Season..." (or double-click to open Edit dialog)
3. Navigate to the Images tab
4. Click on Fanart to open the image picker
5. **Observed:** 
   - Images appeared to load twice, causing duplicates
   - Only a few images showed the green plus icon
   - Dialog became unresponsive with large image sets

---

## Verification Steps (After Fix)

1. Right-click on a TV Season
2. Select "Edit Season..." 
3. Navigate to the Images tab
4. Click on Fanart to open the image picker
5. **Expected:**
   - Images load exactly once
   - All images show the green plus icon
   - Dialog remains responsive
   - Image can be selected and applied

---

## Root Cause Analysis

### Code Flow

    dlgEdit_TVSeason.Image_Scrape_Click (line 477)
        ↓
    ModulesManager.Instance.ScrapeImage_TV() [clsAPIModules.vb line 1476]
        ↓
    dlgImgSelect.ShowDialog() [dlgImgSelect.vb line 193]
        ↓
    dlgImgSelect_Shown → bwImgDefaults.RunWorkerAsync()
        ↓
    bwImgDefaults_RunWorkerCompleted → CreateTopImages(), bwImgDownload.RunWorkerAsync()
        ↓
    bwImgDownload_DoWork → DownloadAllImages()
        ↓
    bwImgDownload_ProgressChanged → CreateListImages() [BUG: Was called multiple times for Fanart]

### The Problem in Detail

In `DownloadAllImages()`, after MainFanarts download:

    bwImgDownload.ReportProgress(iProgress, Enums.ModifierType.AllSeasonsFanart)  ' First report
    bwImgDownload.ReportProgress(iProgress, Enums.ModifierType.SeasonFanart)      ' First report

Then after SeasonFanarts download:

    bwImgDownload.ReportProgress(iProgress, Enums.ModifierType.AllSeasonsFanart)  ' Second report - DUPLICATE!
    bwImgDownload.ReportProgress(iProgress, Enums.ModifierType.SeasonFanart)      ' Second report - DUPLICATE!

In `bwImgDownload_ProgressChanged`:

    ElseIf DirectCast(e.UserState, Enums.ModifierType) = currTopImage.ImageType Then
        CreateListImages(currTopImage)  ' Called twice for SeasonFanart!

For TVSeason with `SeasonFanart` selected as `currTopImage.ImageType`, `CreateListImages` was called twice, causing all images to be added to the UI twice.

### Why TVShow Worked But TVSeason Didn't

- **TVShow:** `currTopImage.ImageType` = `MainFanart` → Only one progress report matched
- **TVSeason:** `currTopImage.ImageType` = `SeasonFanart` → Two progress reports matched (from MainFanarts AND SeasonFanarts)

---

## Files Changed

| File | Change |
|------|--------|
| `EmberMediaManager\dlgImgSelect.vb` | Modified `DownloadAllImages()` method (~line 1680) to conditionally report progress |

---

## Testing Performed

| Test Case | Result |
|-----------|--------|
| Edit Season → Fanart Selection | ✅ Pass - Images load once, all selectable |
| Edit Season → Poster Selection | ✅ Pass - No regression |
| Edit Season → Banner Selection | ✅ Pass - No regression |
| Edit Season → Landscape Selection | ✅ Pass - No regression |

---

## Change History

| Date | Author | Change |
|------|--------|--------|
| January 4, 2026 | Initial | Created issue documentation |
| January 4, 2026 | Investigation | Traced code flow from `dlgEdit_TVSeason` through `dlgImgSelect` |
| January 4, 2026 | Investigation | Identified root cause: duplicate progress reporting in `DownloadAllImages()` |
| January 4, 2026 | Fix | Added conditional checks before reporting progress for season/episode fanart types |
| January 4, 2026 | Verified | Fix confirmed working - images load once and are selectable |

---

*Created: January 4, 2026*
*Completed: January 4, 2026*