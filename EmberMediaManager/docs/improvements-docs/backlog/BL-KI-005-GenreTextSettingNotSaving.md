# BL-KI-005: "Always Show Genre Text" Setting Not Saving

| Field | Value |
|-------|-------|
| **ID** | BL-KI-005 |
| **Created** | January 9, 2026 |
| **Updated** | January 9, 2026 |
| **Priority** | Low |
| **Effort** | 1-2 hours |
| **Status** | ✅ Completed |
| **Category** | Known Issues |
| **Related Files** | [`dlgSettings.vb`](../../../dlgSettings.vb), [`dlgSettings.Designer.vb`](../../../dlgSettings.Designer.vb) |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## [↑](#table-of-contents) Table of Contents

- [Summary](#summary)
- [Problem Description](#problem-description)
- [Root Cause](#root-cause)
- [Resolution](#resolution)
- [Testing](#testing)

---

## [↑](#table-of-contents) Summary

The "Always Show Genre Text" checkbox in General Options was not saving its state. Investigation revealed the setting and associated code had been removed as part of prior cleanup, but the checkbox control remained in the UI. The checkbox has been removed from the Settings dialog.

---

## [↑](#table-of-contents) Problem Description

**Symptom:** The "Always Show Genre Text" setting in General Options would uncheck itself after saving settings.

**Additional Question:** The purpose of this setting was unclear — what did it actually do?

---

## [↑](#table-of-contents) Root Cause

Investigation found that:

1. The checkbox `chkGeneralDisplayGenresText` existed in the Settings dialog Designer
2. The corresponding setting `GeneralShowGenresText` and all related load/save code had been previously removed
3. Only the UI control remained as an orphan — it was never connected to anything

The setting was likely removed intentionally during a prior cleanup because the feature it controlled was deprecated or no longer functional.

---

## [↑](#table-of-contents) Resolution

**Action Taken:** Removed the orphaned checkbox from the Settings dialog.

**File:** [`dlgSettings.Designer.vb`](../../../dlgSettings.Designer.vb)

- Removed `chkGeneralDisplayGenresText` checkbox control from the General Options panel

**Method:** Used Visual Studio Designer to remove the control, ensuring proper cleanup of all Designer-generated code.

---

## [↑](#table-of-contents) Testing

| Status | Scenario |
|:------:|----------|
| ✅ | Settings dialog opens without errors |
| ✅ | General Options panel displays correctly without the removed checkbox |
| ✅ | Other settings in General Options save and load correctly |

---

*End of file*