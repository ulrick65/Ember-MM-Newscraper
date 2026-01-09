# BL-DR-002: Remove Bug Tracker from Help Menu

| Field | Value |
|-------|-------|
| **ID** | BL-DR-002 |
| **Created** | January 8, 2026 |
| **Updated** | January 8, 2026 |
| **Priority** | Low |
| **Effort** | 0.5 hours |
| **Status** | ✅ Completed |
| **Completed** | January 8, 2026 |
| **Category** | Deprecation & Removal (DR) |
| **Related Files** | [`frmMain.vb`](../../../frmMain.vb), [`frmMain.Designer.vb`](../../../frmMain.Designer.vb) |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## [↑](#table-of-contents) Table of Contents

- [Problem Description](#problem-description)
- [Goal](#goal)
- [Implementation Details](#implementation-details)
- [Testing Scenarios](#testing-scenarios)
- [Dependencies](#dependencies)
- [Notes](#notes)

---

## [↑](#table-of-contents) Problem Description

The Help menu contains a "Bug Tracker" menu item (`mnuMainHelpBugTracker`) that links to a bug tracker that is no longer functional or maintained. Clicking this item attempts to open a URL that is no longer active, creating a poor user experience when users try to report bugs.

---

## [↑](#table-of-contents) Goal

Remove the non-functional Bug Tracker menu item from the Help menu to:

1. Eliminate user confusion when clicking a broken link
2. Clean up the Help menu
3. Remove dead code from the codebase

---

## [↑](#table-of-contents) Implementation Details

### [↑](#table-of-contents) Changes Made

**File:** [`frmMain.Designer.vb`](../../../frmMain.Designer.vb)

- Removed `mnuMainHelpBugTracker` ToolStripMenuItem declaration
- Removed menu item from Help menu's `DropDownItems` collection
- Removed property assignments for the menu item

**File:** [`frmMain.vb`](../../../frmMain.vb)

- Removed click event handler for `mnuMainHelpBugTracker`

---

## [↑](#table-of-contents) Testing Scenarios

| Status | Scenario | Expected Result |
|:------:|----------|-----------------|
| ✅ | Open Help menu | Bug Tracker option no longer appears |
| ✅ | Application builds | No build errors or warnings |
| ✅ | Other Help menu items | All remaining items function correctly |

---

## [↑](#table-of-contents) Dependencies

- None

---

## [↑](#table-of-contents) Notes

- The upstream repository bug tracker is no longer maintained
- Users can still submit issues via GitHub if needed
- The Forum links remain available in the Help menu for community support
- This was a simple Designer change made through the Visual Studio Form Designer

---

*End of file*