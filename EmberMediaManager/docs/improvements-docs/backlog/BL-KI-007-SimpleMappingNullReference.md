# BL-KI-003: NullReferenceException in dlgSimpleMapping.SaveChanges

| Field | Value |
|-------|-------|
| **ID** | BL-KI-003 |
| **Created** | January 14, 2026 |
| **Updated** | January 14, 2026 |
| **Category** | Known Issue (KI) |
| **Priority** | Low |
| **Effort** | 1-2 hours |
| **Status** | 📋 Backlog |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Summary

The Simple Mapping dialog (Certification, Country, Status, Studio mappings) throws a `NullReferenceException` when saving if a row has been "deleted" by clearing its contents rather than removing the row entirely.

---

## Table of Contents

- [Problem Description](#problem-description)
- [Steps to Reproduce](#steps-to-reproduce)
- [Exception Details](#exception-details)
- [Root Cause](#root-cause)
- [Proposed Solution](#proposed-solution)
- [Related Files](#related-files)
- [Notes](#notes)
- [Change History](#change-history)

---

## [↑](#table-of-contents) Problem Description

When a user adds a mapping entry and then tries to delete it by clearing the cell contents (rather than using a delete row function), the `SaveChanges()` method encounters a null reference when iterating through the grid rows.

The dialog does not provide a way to delete rows — users can only clear cell contents, which leaves empty rows that cause the crash.

---

## [↑](#table-of-contents) Steps to Reproduce

1. Open Settings → Certification Mapping (or any Simple Mapping dialog)
2. Add a new mapping entry
3. Try to delete the entry by clearing the cell contents (there is no delete row button)
4. Click OK to save
5. **Result:** NullReferenceException

---

## [↑](#table-of-contents) Exception Details

    System.NullReferenceException: Object reference not set to an instance of an object.
       at generic.EmberCore.Mapping.dlgSimpleMapping.SaveChanges()
       at generic.EmberCore.Mapping.dlgSimpleMapping.btnOK_Click(Object sender, EventArgs e)

---

## [↑](#table-of-contents) Root Cause

The `SaveChanges()` method in `dlgSimpleMapping.vb` iterates through DataGridView rows and attempts to access cell values without null checking. When a row has been "cleared" but not removed, the cell values are null/empty, causing the exception.

---

## [↑](#table-of-contents) Proposed Solution

Two options:

### Option 1: Add Null Checking (Quick Fix)

Add null/empty checks in `SaveChanges()` to skip rows with empty values:

    For Each row As DataGridViewRow In dgvMappings.Rows
        If row.IsNewRow Then Continue For
        If row.Cells(0).Value Is Nothing OrElse String.IsNullOrEmpty(row.Cells(0).Value.ToString()) Then
            Continue For
        End If
        ' ... existing save logic
    Next

### Option 2: Add Delete Row Functionality (Better UX)

Add a context menu or button to properly delete rows from the grid, preventing empty rows from existing.

---

## [↑](#table-of-contents) Related Files

| File | Purpose |
|------|---------|
| `Addons\generic.EmberCore.Mapping\dlgSimpleMapping.vb` | Dialog with the bug |

---

## [↑](#table-of-contents) Notes

- Discovered while testing BL-CC-001 (Replace BinaryFormatter)
- This is a pre-existing bug, not caused by recent changes
- Affects all Simple Mapping dialogs (Certification, Country, Status, Studio)

---

## [↑](#table-of-contents) Change History

| Date | Description |
|------|-------------|
| January 14, 2026 | Created — discovered during BL-CC-001 testing |

---

*End of file*