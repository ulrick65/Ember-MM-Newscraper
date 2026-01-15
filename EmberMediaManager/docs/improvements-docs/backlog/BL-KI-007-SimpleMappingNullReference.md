# BL-KI-007: NullReferenceException in Mapping Dialog SaveChanges

| Field | Value |
|-------|-------|
| **ID** | BL-KI-007 |
| **Created** | January 14, 2026 |
| **Updated** | January 14, 2026 |
| **Category** | Known Issue (KI) |
| **Priority** | Low |
| **Effort** | 1-2 hours |
| **Status** | ✅ Complete |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Summary

The Simple Mapping and Regex Mapping dialogs threw a `NullReferenceException` when saving if a row had been "deleted" by clearing its contents rather than removing the row entirely. Fixed by adding null checks to skip empty rows during save.

---

## Table of Contents

- [Problem Description](#problem-description)
- [Steps to Reproduce](#steps-to-reproduce)
- [Exception Details](#exception-details)
- [Root Cause](#root-cause)
- [Solution Implemented](#solution-implemented)
- [Related Files](#related-files)
- [Notes](#notes)
- [Change History](#change-history)

---

## [↑](#table-of-contents) Problem Description

When a user added a mapping entry and then tried to delete it by clearing the cell contents (rather than using a delete row function), the `SaveChanges()` method encountered a null reference when iterating through the grid rows.

The dialogs do not provide a way to delete rows — users can only clear cell contents, which left empty rows that caused the crash.

---

## [↑](#table-of-contents) Steps to Reproduce

1. Open Settings → Certification Mapping (or any Simple/Regex Mapping dialog)
2. Add a new mapping entry
3. Try to delete the entry by clearing the cell contents (there is no delete row button)
4. Click OK to save
5. **Result:** NullReferenceException (before fix)

---

## [↑](#table-of-contents) Exception Details

    System.NullReferenceException: Object reference not set to an instance of an object.
       at generic.EmberCore.Mapping.dlgSimpleMapping.SaveChanges()
       at generic.EmberCore.Mapping.dlgSimpleMapping.btnOK_Click(Object sender, EventArgs e)

---

## [↑](#table-of-contents) Root Cause

The `SaveChanges()` method called `.ToString` on potentially null cell values without checking for null first. When a row was "cleared" but not removed, the cell values were null, causing the exception.

---

## [↑](#table-of-contents) Solution Implemented

Added null/empty checks in `SaveChanges()` to skip rows with empty values:

    For Each aRow As DataGridViewRow In dgvMappings.Rows
        'Skip new row placeholder and rows with null/empty input values
        If aRow.IsNewRow Then Continue For
        If aRow.Cells(0).Value Is Nothing OrElse String.IsNullOrEmpty(aRow.Cells(0).Value.ToString.Trim) Then Continue For
        If aRow.Cells(1).Value Is Nothing Then Continue For
        
        ' ... save logic
    Next

This approach:
- Prevents the crash
- Prevents blank entries from being saved to XML
- Doesn't change user workflow

---

## [↑](#table-of-contents) Related Files

| File | Status |
|------|--------|
| `Addons\generic.EmberCore.Mapping\dlgSimpleMapping.vb` | ✅ Fixed — added null checks |
| `Addons\generic.EmberCore.Mapping\dlgRegexMapping.vb` | ✅ Fixed — added null checks |
| `Addons\generic.EmberCore.Mapping\dlgGenreMapping.vb` | ✅ Safe — uses different save pattern |

---

## [↑](#table-of-contents) Notes

- Discovered while testing BL-CC-001 (Replace BinaryFormatter)
- This was a pre-existing bug, not caused by recent changes
- Affects: Certification, Country, Status, Studio, and Edition mapping dialogs

---

## [↑](#table-of-contents) Change History

| Date | Description |
|------|-------------|
| January 14, 2026 | Created — discovered during BL-CC-001 testing |
| January 14, 2026 | Fixed — added null checks to `dlgSimpleMapping.vb` and `dlgRegexMapping.vb` |

---

*End of file*