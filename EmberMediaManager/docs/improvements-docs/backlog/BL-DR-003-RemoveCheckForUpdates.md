# BL-DR-003: Remove Check for Updates and Donate Features

| Field | Value |
|-------|-------|
| **ID** | BL-DR-003 |
| **Created** | January 8, 2026 |
| **Updated** | January 8, 2026 |
| **Priority** | Low |
| **Effort** | 1-2 hours |
| **Status** | ✅ Completed |
| **Completed** | January 8, 2026 |
| **Category** | Deprecation & Removal (DR) |
| **Related Files** | [`clsAPICommon.vb`](../../../../EmberAPI/clsAPICommon.vb), [`frmMain.vb`](../../../frmMain.vb), [`frmMain.Designer.vb`](../../../frmMain.Designer.vb) |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## [↑](#table-of-contents) Table of Contents

- [Problem Description](#problem-description)
- [Goal](#goal)
- [Investigation Findings](#investigation-findings)
- [Implementation Details](#implementation-details)
- [Testing Scenarios](#testing-scenarios)
- [Dependencies](#dependencies)
- [Notes](#notes)

---

## [↑](#table-of-contents) Problem Description

The application had two features that needed to be removed:

### [↑](#table-of-contents) Check for Updates

1. **The feature was never implemented** — `CheckNeedUpdate()` was a stub that always returned `False`
2. **The server endpoint was defunct** — The commented code referenced `http://pcjco.dommel.be/emm-r/` which is from the original upstream project and is no longer maintained
3. **The settings checkbox was misleading** — Users could enable/disable a feature that didn't work
4. **Dead code cluttered the codebase** — Multiple files contained unused update-related code

### [↑](#table-of-contents) Donate Button

1. **Links to upstream project** — PayPal and Patreon links were for the original project maintainers, not the current fork
2. **Inappropriate for fork** — Donations would go to upstream, not the fork maintainer

---

## [↑](#table-of-contents) Goal

Remove all update-checking and donate functionality to:

1. Eliminate user confusion from non-functional features
2. Remove links to upstream project's donation pages
3. Remove dead/stub code from the codebase
4. Clean up the Help menu
5. Reduce maintenance burden

---

## [↑](#table-of-contents) Investigation Findings

### [↑](#table-of-contents) Code Analysis

**`CheckNeedUpdate()` in `clsAPICommon.vb`:**

    Public Shared Function CheckNeedUpdate() As Boolean
        'TODO STUB - Not implemented yet
        Dim needUpdate As Boolean = False
        ' ... all actual code was commented out ...
        Return needUpdate  ' Always returned False
    End Function

**`GetChangelog()` in `clsAPICommon.vb`:**

    Public Shared Function GetChangelog() As String
        'TODO STUB - Not implemented yet
        ' ... all actual code was commented out ...
        Return "Unavailable"
    End Function

---

## [↑](#table-of-contents) Implementation Details

### [↑](#table-of-contents) Changes Made

**File:** [`dlgSettings.Designer.vb`](../../../dlgSettings.Designer.vb)

- Removed `chkGeneralCheckForUpdates` checkbox from General Settings panel

**File:** [`dlgSettings.vb`](../../../dlgSettings.vb)

- Removed load/save code for the "Check for Updates" setting

**File:** [`frmMain.Designer.vb`](../../../frmMain.Designer.vb)

- Removed `mnuMainHelpUpdate` menu item (Check for Updates)
- Removed `mnuMainHelpDonate` menu item (Donate)
- Removed related separators

**File:** [`frmMain.vb`](../../../frmMain.vb)

- Removed `CheckUpdatesToolStripMenuItem_Click` handler
- Removed Donate click handler (PayPal/Patreon URL launch)

**File:** [`clsAPICommon.vb`](../../../../EmberAPI/clsAPICommon.vb)

- Removed `CheckNeedUpdate()` stub function
- Removed `GetChangelog()` stub function

---

## [↑](#table-of-contents) Testing Scenarios

| Status | Scenario | Expected Result |
|:------:|----------|-----------------|
| ✅ | Help menu opens | No "Check for Updates" option |
| ✅ | Help menu opens | No "Donate" option |
| ✅ | Application startup | No update check attempted |
| ✅ | Solution builds | No errors or warnings |

---

## [↑](#table-of-contents) Dependencies

- None — This removed unused functionality

---

## [↑](#table-of-contents) Notes

- The server URL `http://pcjco.dommel.be/emm-r/` was from the original Ember Media Manager project
- The donate links pointed to the upstream project maintainers' PayPal/Patreon accounts
- If auto-update functionality is desired in the future, it should be implemented fresh with a working endpoint
- GitHub Releases could be used for future update notifications if implemented
- If donation links are desired for the fork, they should be added separately with appropriate accounts
- `dlgNewVersion` dialog was never shown since `CheckNeedUpdate()` always returned `False`

---

*End of file*