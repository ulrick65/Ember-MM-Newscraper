# BL-CQ-005: Consistent Date Format Handling in NFO

| Field | Value |
|-------|-------|
| **ID** | BL-CQ-005 |
| **Category** | Code Quality (CQ) |
| **Priority** | Low |
| **Effort** | 1-2 hrs |
| **Created** | January 7, 2026 |
| **Status** | Open |
| **Related Files** | `clsAPINFO.vb`, `clsAPINumUtils.vb` |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Overview

Date fields (Premiered, Aired) are normalized to ISO8601 format using `NumUtils.DateToISO8601Date()`. However, error handling for invalid dates could be improved to preserve data and provide better debugging information.

---

## Table of Contents

- [Problem Description](#problem-description)
- [Current Behavior](#current-behavior)
- [Proposed Solution](#proposed-solution)
- [Affected Methods](#affected-methods)
- [Related Files](#related-files)
- [Benefits](#benefits)
- [Risks](#risks)
- [Source Reference](#source-reference)

---

## [↑](#table-of-contents) Problem Description

When date parsing fails:

- The result may be an empty string or unexpected value
- No logging of the original invalid date occurs
- Users have no visibility into date format problems in their NFO files

---

## [↑](#table-of-contents) Current Behavior

Dates are converted during the clean process without error handling:

    result.Premiered = NumUtils.DateToISO8601Date(result.Premiered)

If parsing fails, the result is silently converted to an empty string.

---

## [↑](#table-of-contents) Proposed Solution

Add explicit validation and logging for date parsing failures:

    Private Shared Function NormalizeDate(dateValue As String, 
                                          fieldName As String, 
                                          filePath As String) As String
        If String.IsNullOrWhiteSpace(dateValue) Then
            Return String.Empty
        End If
        
        Dim normalized As String = NumUtils.DateToISO8601Date(dateValue)
        
        If String.IsNullOrEmpty(normalized) AndAlso 
           Not String.IsNullOrEmpty(dateValue) Then
            ' Date parsing failed - log for debugging
            logger.Warn("Invalid date format in {0} field: '{1}' (file: {2})", 
                        fieldName, dateValue, filePath)
            ' Option: Preserve original value instead of returning empty
            Return dateValue
        End If
        
        Return normalized
    End Function

Usage in clean methods:

    result.Premiered = NormalizeDate(result.Premiered, "Premiered", filePath)
    result.Aired = NormalizeDate(result.Aired, "Aired", filePath)

---

## [↑](#table-of-contents) Affected Methods

| Method | Field | File |
|--------|-------|------|
| `CleanNFO_Movies` | Premiered | [clsAPINFO.vb](../../../EmberAPI/clsAPINFO.vb) |
| `CleanNFO_TVEpisodes` | Aired | [clsAPINFO.vb](../../../EmberAPI/clsAPINFO.vb) |
| `CleanNFO_TVShow` | Premiered | [clsAPINFO.vb](../../../EmberAPI/clsAPINFO.vb) |

---

## [↑](#table-of-contents) Related Files

| File | Purpose |
|------|---------|
| [clsAPINFO.vb](../../../EmberAPI/clsAPINFO.vb) | NFO cleaning methods |
| [clsAPINumUtils.vb](../../../EmberAPI/clsAPINumUtils.vb) | Date parsing utility |

---

## [↑](#table-of-contents) Benefits

- Better data preservation (option to keep original if parsing fails)
- Improved debugging for date format issues
- More predictable behavior
- Visibility into date format problems in user NFO files

---

## [↑](#table-of-contents) Risks

- May surface previously hidden date format issues in logs
- Decision needed: preserve original vs. clear invalid dates

---

## [↑](#table-of-contents) Source Reference

[NfoFileImprovements.md](../NfoFileImprovements.md) - Item 5.2

---

*Created: January 7, 2026*