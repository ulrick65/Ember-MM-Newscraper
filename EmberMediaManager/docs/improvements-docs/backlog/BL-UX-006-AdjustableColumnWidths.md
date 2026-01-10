# BL-UX-006: Adjustable Column Widths in Main Media List

| Field | Value |
|-------|-------|
| **ID** | BL-UX-006 |
| **Created** | January 10, 2026 |
| **Priority** | Low |
| **Effort** | 2-3 hours |
| **Status** | 📋 Open |
| **Category** | UI/UX Improvements |
| **Related Files** | [`frmMain.vb`](../../../EmberMediaManager/frmMain.vb), [`frmMain.Designer.vb`](../../../EmberMediaManager/frmMain.Designer.vb) |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Summary

Allow users to adjust column widths in the main form's media list views (Movies, MovieSets, TV Shows, Seasons, Episodes) for better usability and customization.

---

## Table of Contents

- [Problem Description](#problem-description)
- [Solution Summary](#solution-summary)
- [Implementation Details](#implementation-details)
- [Testing Summary](#testing-summary)
- [Related Files](#related-files)
- [Notes](#notes)

---

## [↑](#table-of-contents) Problem Description

The main form displays media items in list views with fixed or auto-sized columns. Users cannot resize columns to:

- See longer titles without truncation
- Hide columns they don't need by shrinking them
- Adjust column widths to their preferences

This limits usability, especially for users with large libraries or specific viewing preferences.

---

## [↑](#table-of-contents) Solution Summary

Enable column resizing in the main media list controls:

1. Set `AllowUserToResizeColumns = True` on DataGridView controls
2. Optionally persist column widths to settings so they survive application restarts
3. Ensure column headers display resize cursors on hover

---

## [↑](#table-of-contents) Implementation Details

**Phase 1: Enable Resizing**

For DataGridView controls in `frmMain.vb`:

    ' In designer or Form_Load
    dgvMovies.AllowUserToResizeColumns = True

**Phase 2: Persist Column Widths (Optional)**

Save column widths to `Master.eSettings` on form close:

    ' Save widths
    For Each col As DataGridViewColumn In dgvMovies.Columns
        ' Save col.Name and col.Width to settings
    Next

Restore on form load:

    ' Restore widths from settings
    For Each col As DataGridViewColumn In dgvMovies.Columns
        ' Load saved width if exists
    Next

---

## [↑](#table-of-contents) Testing Summary

| Status | Test Scenario |
|:------:|---------------|
| ⬜ | Columns can be resized by dragging column borders |
| ⬜ | Resize cursor appears when hovering over column borders |
| ⬜ | Column widths persist after application restart (if implemented) |
| ⬜ | Resizing works for all media lists (Movies, TV Shows, etc.) |
| ⬜ | Double-click column border auto-fits to content |

---

## [↑](#table-of-contents) Related Files

| File | Purpose |
|------|---------|
| [`EmberMediaManager\frmMain.vb`](../../../EmberMediaManager/frmMain.vb) | Main form code-behind |
| [`EmberMediaManager\frmMain.Designer.vb`](../../../EmberMediaManager/frmMain.Designer.vb) | Designer-generated control definitions |

---

## [↑](#table-of-contents) Notes

- The list sorting feature already exists (`MovieGeneralMediaListSorting`, etc. in `dlgSettings.vb`)
- Column visibility is controlled via settings; this enhancement focuses on width adjustment
- Consider adding a "Reset Column Widths" option in the context menu

---

## [↑](#table-of-contents) Change History

| Date | Description |
|------|-------------|
| January 10, 2026 | Created |

---

*End of file*