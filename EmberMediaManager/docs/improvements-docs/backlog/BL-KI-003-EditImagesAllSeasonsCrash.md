# BL-KI-003: Edit Images Crash with All Seasons Selected

| Field | Value |
|-------|-------|
| **ID** | BL-KI-003 |
| **Category** | Known Issues (KI) |
| **Priority** | Low |
| **Effort** | 2-4 hrs |
| **Created** | January 5, 2026 |
| **Status** | Open |

---

## Summary

Application crashes with `ArgumentNullException` when clicking "Edit Images" on a TV Show that has "All Seasons" selected in the seasons grid and the show has not been scraped (no metadata).

---

## Reproduction Steps

1. Add a new TV Show source with unscraped shows
2. Select an unscraped TV Show in the main list
3. Select "All Seasons" in the seasons grid (or any season with no data)
4. Right-click on the TV Show → Edit Images
5. **Result**: Application crashes

---

## Error Details

    System.ArgumentNullException: Value cannot be null.
    Parameter name: source
       at System.Linq.Enumerable.Count[TSource](IEnumerable`1 source)
       at EmberMediaManager.frmMain.FillScreenInfoWith_TVSeason()
       at EmberMediaManager.frmMain.LoadInfo_TVSeason(Int64 ID)
       at EmberMediaManager.frmMain.DataGridView_SelectRow_TVSeason(Int32 iRow)
       at EmberMediaManager.frmMain.dgvTVSeasons_SelectionChanged(Object sender, EventArgs e)

---

## Root Cause Analysis

The crash occurs in the call chain when `dgvTVSeasons_SelectionChanged` fires during/after the Edit Images dialog interaction. The `.Count()` LINQ extension method is being called on a null collection - likely `Episodes` or `Seasons` - when loading season data for a show without metadata.

**Suspected Location**: `Master.DB.Load_TVSeason()` in `clsAPIDatabase.vb`

The UI-layer methods (`FillScreenInfoWith_TVSeason`, `LoadInfo_TVSeason`) don't contain LINQ `.Count()` calls - they use `.Count` property. The data layer likely returns a `DBElement` with null collection properties when the show has no scraped data.

---

## Proposed Fix

Add null checks in `Load_TVSeason` to initialize empty collections:

    If dbElement.Episodes Is Nothing Then
        dbElement.Episodes = New List(Of Database.DBElement)
    End If
    If dbElement.Seasons Is Nothing Then
        dbElement.Seasons = New List(Of Database.DBElement)
    End If

Or add defensive null check in `FillScreenInfoWith_TVSeason` before any collection access.

---

## Workaround

Scrape the TV Show before attempting to edit images.

---

## Related Files

- `EmberMediaManager\frmMain.vb` - UI event handlers
- `EmberAPI\clsAPIDatabase.vb` - `Load_TVSeason` method (suspected)

---

## Notes

- Low impact: Only affects edge case with unscraped shows
- Quick workaround available (scrape first)
- Full fix requires tracing through database layer

---

*End of File*