# BL-CQ-002: Null Reference Audit

| Field | Value |
|-------|-------|
| **ID** | BL-CQ-002 |
| **Created** | January 2, 2026 |
| **Updated** | January 14, 2026 |
| **Category** | Code Quality (CQ) |
| **Priority** | High |
| **Effort** | 4-6 hours |
| **Status** | ✅ Completed |
| **Completed** | January 14, 2026 |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Summary

Multiple places in the codebase accessed objects or collections without null checks, causing `NullReferenceException` and `ArgumentNullException` crashes. Users reported frequent crashes in the Kodi API integration.

---

## Table of Contents

- [Problem Description](#problem-description)
- [Root Cause Analysis](#root-cause-analysis)
- [Solution Summary](#solution-summary)
- [Implementation Details](#implementation-details)
- [Testing Summary](#testing-summary)
- [Related Files](#related-files)
- [Notes](#notes)
- [Change History](#change-history)

---

## [↑](#table-of-contents) Problem Description

Multiple places in the codebase accessed objects or collections without null checks, causing:
- `NullReferenceException`
- `ArgumentNullException` ("Parameter cannot be null")

Users reported crashes, particularly in the Kodi API integration when communicating with Kodi instances.

---

## [↑](#table-of-contents) Root Cause Analysis

The KodiAPI project contained numerous JSON deserialization patterns that did not check for null before accessing properties:

- `jObject["params"]["sender"]` — accessing nested properties without null checks
- `download.details["path"]` — assuming response always contains expected fields
- JSON converters not handling null input values

In VB.NET code, arrays initialized to `Nothing` were accessed without null guards.

---

## [↑](#table-of-contents) Solution Summary

Used ReSharper's "Code Issues in Solution" to identify and fix all potential null dereference issues in the KodiAPI project. Applied consistent fix patterns:

- Early return guards for null tokens
- Null-conditional operators (`?.`) for optional data
- Descriptive exceptions for required fields
- Safe casts with `as` operator

**Result:** All ReSharper null reference warnings in KodiAPI resolved.

---

## [↑](#table-of-contents) Implementation Details

### January 14, 2026 — KodiAPI Null Reference Fixes

| File | Method | Issue | Fix |
|------|--------|-------|-----|
| `KodiAPI\XBMCRPC\Client.cs` | `GetData<T>` | `responseStream` could be null | Added null check with descriptive exception |
| `KodiAPI\XBMCRPC\Client.cs` | `GetData<T>` | `query["result"]` could be null | Added null check with descriptive exception |
| `KodiAPI\XBMCRPC\Client.cs` | `GetData<T>` | Manual `.Dispose()` calls | Converted to `using` statements |
| `KodiAPI\XBMCRPC\Client.cs` | `ParseNotification` | `jObject["params"]` could be null | Extracted tokens with null guards at method start |
| `KodiAPI\XBMCRPC\Client.cs` | `ParseNotification` | `paramsToken["sender"]` could be null | Added null check with early return |
| `KodiAPI\XBMCRPC\Client.cs` | `ParseNotification` | `paramsToken["data"]` could be null | Used null-conditional `?.ToObject<T>()` for all 30+ cases |
| `KodiAPI\XBMCRPC\XBMCRPC\List\Item\Base.cs` | `ReadJson` | `jObject` could be null after deserialization | Changed to safe cast with `as` operator |
| `KodiAPI\XBMCRPC\XBMCRPC\List\Item\Base.cs` | `ReadJson` | `val` could be null | Added null check with early return |
| `KodiAPI\XBMCRPC\XBMCRPC\List\Filter\Operators.cs` | `WriteJson` | `value` parameter could be null | Added null check with `writer.WriteNull()` |
| `KodiAPI\XBMCRPC\XBMCRPC\List\Filter\Operators.cs` | `ReadJson` | `reader.Value` could be null | Added null check with early return |
| `KodiAPI\Client.cs` | `GetImageUri` | `download.details` and `["path"]` could be null | Added null-conditional operators and descriptive exception |
| `KodiAPI\Client.cs` | `PrepareDownload` | `download.details` and `["path"]` could be null | Added null-conditional operators and descriptive exception |

### January 14, 2026 — EmberMediaManager Fixes

| File | Method | Issue | Fix |
|------|--------|-------|-----|
| `frmMain.vb` | `FillScreenInfoWith_Movieset` | `currMovieset` could be null | Added guard clause at method start |
| `frmMain.vb` | `FillScreenInfoWith_Movieset` | `pnlGenre.Count` without null check | Added `If pnlGenre IsNot Nothing Then` before loop |

### January 2, 2026 — Initial Fix

| File | Method | Issue | Fix |
|------|--------|-------|-----|
| `frmMain.vb` | `MoveGenres` | `pnlGenre.Count` without null check | Added `If pnlGenre Is Nothing OrElse pnlGenre.Count = 0 Then Exit Sub` |

---

## [↑](#table-of-contents) Testing Summary

**Status Legend:** ⬜ Not tested | ✅ Passed | ❌ Failed | ⏸️ Skipped

| Status | Test Scenario |
|:------:|---------------|
| ✅ | ReSharper reports zero null reference warnings in KodiAPI |
| ✅ | Solution builds without errors |
| ⬜ | Kodi integration with connected Kodi instance |
| ⬜ | Kodi integration with disconnected/unreachable Kodi instance |

---

## [↑](#table-of-contents) Related Files

| File | Purpose |
|------|---------|
| `KodiAPI\XBMCRPC\Client.cs` | Main Kodi RPC client — GetData and ParseNotification |
| `KodiAPI\Client.cs` | Image download methods |
| `KodiAPI\XBMCRPC\XBMCRPC\List\Item\Base.cs` | JSON converter for list items |
| `KodiAPI\XBMCRPC\XBMCRPC\List\Filter\Operators.cs` | JSON converter for filter operators |
| `EmberMediaManager\frmMain.vb` | Main form — genre panel handling |

---

## [↑](#table-of-contents) Notes

### Tools Used

**ReSharper** — Used **ReSharper → Inspect → Code Issues in Solution** to find:
- `Possible 'System.NullReferenceException'`
- `Null assignment to non-nullable entity`

Filtered by project to prioritize KodiAPI fixes based on user crash reports.

### Future Considerations

If additional null reference issues are discovered, open a new backlog item rather than reopening this one.

---

## [↑](#table-of-contents) Change History

| Date | Description |
|------|-------------|
| January 2, 2026 | Created — initial fix for `MoveGenres` null check |
| January 14, 2026 | Used ReSharper to audit KodiAPI; fixed all null reference warnings |
| January 14, 2026 | Closed |

---

*End of file*