# BL-KI-006: MovieSet Crash When Opening MovieSets Tab

| Field | Value |
|-------|-------|
| **ID** | BL-KI-006 |
| **Created** | January 11, 2026 |
| **Updated** | January 14, 2026 |
| **Category** | Known Issue (KI) |
| **Priority** | Medium |
| **Effort** | 1 hour |
| **Status** | ✅ Complete |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Summary

Opening the MovieSets tab crashed the application with an `ArgumentNullException` in `FillScreenInfoWith_Movieset()`. The crash occurred because `pnlGenre` (a dynamic Panel array) was null when accessed.

---

## Table of Contents

- [Problem Description](#problem-description)
- [Exception Details](#exception-details)
- [Root Cause](#root-cause)
- [Solution Implemented](#solution-implemented)
- [Related Files](#related-files)
- [Testing Notes](#testing-notes)
- [Change History](#change-history)

---

## [↑](#table-of-contents) Problem Description

When selecting a MovieSet in the UI, the application crashed immediately. This happened most reliably when navigating to the MovieSets tab before ever viewing a Movie or TV Show.

---

## [↑](#table-of-contents) Exception Details

    EXCEPTION OCCURRED: System.ArgumentNullException: Value cannot be null.
    Parameter name: source
       at System.Linq.Enumerable.Count[TSource](IEnumerable`1 source)
       at Ember_Media_Manager.frmMain.FillScreenInfoWith_Movieset()
       at Ember_Media_Manager.frmMain.DataGridView_SelectRow_MovieSet(Int32 iRow)
       at Ember_Media_Manager.frmMain.tmrLoad_MovieSet_Tick(Object sender, EventArgs e)

---

## [↑](#table-of-contents) Root Cause

The `pnlGenre` array is declared at the class level as:

    Private pnlGenre() As Panel = Nothing

This array is populated dynamically by `createGenreThumbs()` when displaying Movies or TV Shows (which have genres). **MovieSets don't have genres**, so `createGenreThumbs()` is never called for them.

The `FillScreenInfoWith_Movieset()` method contained this loop (likely copy-pasted from `FillScreenInfoWith_Movie()`):

    For i As Integer = 0 To pnlGenre.Count - 1
        pnlGenre(i).Visible = True
    Next

When the user navigates to MovieSets before ever viewing a Movie:
1. `pnlGenre` is still `Nothing` (never initialized)
2. Calling `.Count` on `Nothing` throws `ArgumentNullException`

**Why the guard clause for `currMovieset` didn't help:** The issue wasn't `currMovieset` being null — the MovieSet data loaded fine. The crash was in the genre panel loop at the end of the method, which shouldn't even be in this method since MovieSets don't have genres.

---

## [↑](#table-of-contents) Solution Implemented

Added a null check before the genre panel loop:

    If pbMPAA.Image IsNot Nothing Then pnlMPAA.Visible = True
    If pnlGenre IsNot Nothing Then
        For i As Integer = 0 To pnlGenre.Count - 1
            pnlGenre(i).Visible = True
        Next
    End If

This defensive fix:
- Prevents the crash when `pnlGenre` hasn't been initialized
- Doesn't change behavior for content types that do have genres
- Keeps the code consistent with other `FillScreenInfoWith_*` methods

**Note:** The genre panel loop probably shouldn't be in this method at all since MovieSets don't have genres. However, the null check is the safe minimal fix.

---

## [↑](#table-of-contents) Related Files

| File | Change |
|------|--------|
| `EmberMediaManager\frmMain.vb` | Added null check for `pnlGenre` in `FillScreenInfoWith_Movieset()` |

---

## [↑](#table-of-contents) Testing Notes

- Open application with existing MovieSets in database
- Click MovieSets tab immediately (before viewing any Movies)
- Select a MovieSet
- **Expected:** MovieSet info displays without crash
- **Verified:** No crash occurs

---

## [↑](#table-of-contents) Change History

| Date | Description |
|------|-------------|
| January 11, 2026 | Created — reported crash when clicking MovieSets tab |
| January 14, 2026 | Fixed — added null check for `pnlGenre` array; root cause was uninitialized genre panel array |

---

*End of file*