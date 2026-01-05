# BL-KI-001: TV Season Images Not Saved After Single Scrape

| Field | Value |
|-------|-------|
| **ID** | BL-KI-001 |
| **Created** | January 4, 2026 |
| **Completed** | January 5, 2026 |
| **Priority** | High |
| **Effort** | 1 hour |
| **Status** | ✅ Completed |
| **Category** | Known Issues (KI) |
| **Related Files** | `frmMain.vb`, `dlgImgSelect.vb`, `clsAPIImages.vb` |

---

## Problem Description

When scraping a single TV Season and selecting images in `dlgImgSelect`, the selected images are not saved to disk. The user selects images, clicks OK, but the images do not persist.

---

## Root Cause Analysis

### Debug Evidence

Debug logging in `dlgImgSelect.DoneAndClose()` confirmed:

    ContentType: TVSeason
    DoOnlySeason: 2
    Result.ImagesContainer.Poster.URLOriginal: http://image.tmdb.org/t/p/original/pQ33MqEUEQGChyknPtvWODUza1q.jpg
    Result.Seasons.Count: 0

**The `dlgImgSelect` dialog correctly stores the selected image in `Result.ImagesContainer`.**

### Bug Location

**File:** `frmMain.vb`  
**Method:** `bwTVSeasonScraper_DoWork` (lines ~3277-3281)

The original code had a condition that skipped the save for `SingleScrape`:

    ' BEFORE (Bug):
    If Not (Args.ScrapeType = Enums.ScrapeType.SingleScrape) Then
        ' Save only happens here - NOT for single scrape!
        ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.ScraperMulti_TVSeason, Nothing, Nothing, False, DBScrapeSeason)
        bwTVSeasonScraper.ReportProgress(-3, String.Concat(Master.eLang.GetString(399, "Downloading and Saving Contents into Database"), ":"))
        DBScrapeSeason = Master.DB.Save_TVSeasonAsync(DBScrapeSeason, False, True, True).GetAwaiter().GetResult()
        bwTVSeasonScraper.ReportProgress(-2, DBScrapeSeason.ID)
    End If

### Design Issue

The original design assumed `SingleScrape` would always be followed by an Edit dialog that handles saving. However:

1. **"Edit Images..." context menu** - No Edit dialog follows
2. **Single Season Scrape** - No Edit dialog follows either

Both scenarios relied on a save that never happened for `SingleScrape` mode.

---

## Implemented Fix

**Solution:** Remove the `If Not SingleScrape` condition entirely. The save now executes for all scrape types.

    ' AFTER (Fixed):
    ' Post-scrape processing and save
    ' BL-KI-001 Fix: This block now executes for ALL scrape types (previously skipped for SingleScrape).
    ' This ensures images selected via dlgImgSelect are persisted for:
    '   - Single season scrape (context menu)
    '   - "Edit Images..." context menu feature
    '   - Bulk/batch season scrapes
    ModulesManager.Instance.RunGeneric(Enums.ModuleEventType.ScraperMulti_TVSeason, Nothing, Nothing, False, DBScrapeSeason)
    bwTVSeasonScraper.ReportProgress(-3, String.Concat(Master.eLang.GetString(399, "Downloading and Saving Contents into Database"), ":"))
    DBScrapeSeason = Master.DB.Save_TVSeasonAsync(DBScrapeSeason, False, True, True).GetAwaiter().GetResult()
    bwTVSeasonScraper.ReportProgress(-2, DBScrapeSeason.ID)

### Why This Works

- The save is now consistent across all scrape types
- Images selected in `dlgImgSelect` are persisted immediately
- Matches the behavior of double-click image handlers (which always saved immediately)
- No negative impact on batch scrapes (they already executed this code)

---

## Affected Scenarios

| Scenario | Before Fix | After Fix |
|----------|------------|-----------|
| Single TV Season scrape via context menu | ❌ Images lost | ✅ Works |
| "Edit Images..." context menu | ❌ Images lost | ✅ Works |
| Bulk TV Season scrape (multiple seasons) | ✅ Worked | ✅ Works |
| Double-click image in main window | ✅ Worked | ✅ Works |
| Edit Season dialog → scrape image | ✅ Worked | ✅ Works |

---

## Testing Completed

- [x] Single season scrape saves selected images
- [x] Bulk season scrape still works
- [x] Double-click image still works
- [x] Edit Season dialog image scrape still works
- [x] All season image types (Poster, Banner, Fanart, Landscape) save correctly

---

## Related Documentation Updates

- Updated XML documentation for `bwTVSeasonScraper_DoWork` method
- Added inline code comments referencing BL-KI-001

---

*Created: January 4, 2026*  
*Completed: January 5, 2026*