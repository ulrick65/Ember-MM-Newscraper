# BL-KI-006: MovieSet Crash After Movie Removal

| Document Info | |
|---------------|---|
| **ID** | BL-KI-006 |
| **Category** | Known Issues (KI) |
| **Priority** | Medium |
| **Status** | Open |
| **Created** | January 11, 2026 |
| **Updated** | January 11, 2026 |
| **Author** | ulrick65 |

##### [← Return to Future Enhancements](../FutureEnhancements.md)

---

## Summary

Removing movies from the database and then clicking on MovieSets crashes the application with an `ArgumentNullException` in `FillScreenInfoWith_Movieset()`.

---

## Problem Description

When movies are removed from the database, the MovieSet records still reference those movies. When the user selects a MovieSet in the UI, the code attempts to call `.Count` on a null collection (likely the list of movies in the set), causing the application to crash.

**Steps to Reproduce:**

1. Have movies that belong to a MovieSet in the database
2. Remove those movies from the database (but not the MovieSet)
3. Click on the MovieSets tab
4. Select a MovieSet that referenced the removed movies
5. Application crashes

---

## Exception Details

    EXCEPTION OCCURRED: System.ArgumentNullException: Value cannot be null.
    Parameter name: source
       at System.Linq.Enumerable.Count[TSource](IEnumerable`1 source)
       at Ember_Media_Manager.frmMain.FillScreenInfoWith_Movieset()
       at Ember_Media_Manager.frmMain.DataGridView_SelectRow_MovieSet(Int32 iRow)
       at Ember_Media_Manager.frmMain.tmrLoad_MovieSet_Tick(Object sender, EventArgs e)
       at System.Windows.Forms.Timer.OnTick(EventArgs e)
       at System.Windows.Forms.Timer.TimerNativeWindow.WndProc(Message& m)
       at System.Windows.Forms.NativeWindow.Callback(IntPtr hWnd, Int32 msg, IntPtr wparam, IntPtr lparam)

---

## Root Cause Analysis

The `FillScreenInfoWith_Movieset()` method calls `.Count` on a collection without first checking if the collection is null. When the movies that belonged to a MovieSet are removed, the collection becomes null or the reference becomes invalid.

**Likely location:** `frmMain.vb` in `FillScreenInfoWith_Movieset()`

---

## Proposed Solution

Add null checks before calling `.Count` on collections in `FillScreenInfoWith_Movieset()`:

    ' Before
    If collection.Count > 0 Then

    ' After  
    If collection IsNot Nothing AndAlso collection.Count > 0 Then

This is similar to the fix applied for `MoveGenres` (completed January 2, 2026).

---

## Related Items

- [BL-CQ-002: Null check audit (.Count calls)](BL-CQ-002-NullCheckAudit.md) — This is another instance of the same pattern

---

## Files Affected

| File | Change Type |
|------|-------------|
| `EmberMediaManager\frmMain.vb` | Add null checks in `FillScreenInfoWith_Movieset()` |

---

## Testing Notes

- Remove movies that belong to a MovieSet
- Verify MovieSets tab doesn't crash
- Verify empty MovieSets display gracefully

---

*End of file*