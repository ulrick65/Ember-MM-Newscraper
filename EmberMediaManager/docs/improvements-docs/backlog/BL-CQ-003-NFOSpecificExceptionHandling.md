# BL-CQ-003: Specific Exception Handling in NFO Load Methods

| Field | Value |
|-------|-------|
| **ID** | BL-CQ-003 |
| **Category** | Code Quality (CQ) |
| **Priority** | Low |
| **Effort** | 2-3 hrs |
| **Created** | January 7, 2026 |
| **Status** | Open |
| **Related Files** | `clsAPINFO.vb` |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Overview

The `LoadFromNFO_*` methods in `clsAPINFO.vb` catch generic `Exception` types. More specific exception handling would improve error diagnosis and allow for targeted recovery actions.

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

All NFO load methods use a generic catch pattern that catches all exceptions uniformly. This makes it difficult to:

- Provide specific error messages to users
- Implement recovery actions for known failure modes
- Distinguish between file system issues and XML parsing problems

---

## [↑](#table-of-contents) Current Behavior

All load methods use a generic catch pattern:

    Try
        ' Deserialization and processing
    Catch ex As Exception
        logger.Error(ex, New StackFrame().GetMethod().Name)
    End Try

---

## [↑](#table-of-contents) Proposed Solution

Catch specific exceptions with appropriate handling for each:

    Try
        ' Deserialization and processing
    Catch ex As XmlException
        logger.Warn("Malformed XML in NFO file: {0}", filePath)
        ' Could attempt to repair or notify user
    Catch ex As IOException
        logger.Error("File access error reading NFO: {0}", filePath)
        ' File locked, network issue, etc.
    Catch ex As UnauthorizedAccessException
        logger.Warn("Permission denied reading NFO: {0}", filePath)
        ' Permission issue - different handling than corrupt file
    Catch ex As Exception
        logger.Error(ex, New StackFrame().GetMethod().Name)
        ' Fallback for unexpected exceptions
    End Try

---

## [↑](#table-of-contents) Affected Methods

| Method | File |
|--------|------|
| `LoadFromNFO_Movie` | [clsAPINFO.vb](../../../EmberAPI/clsAPINFO.vb) |
| `LoadFromNFO_MovieSet` | [clsAPINFO.vb](../../../EmberAPI/clsAPINFO.vb) |
| `LoadFromNFO_TVEpisode` (both overloads) | [clsAPINFO.vb](../../../EmberAPI/clsAPINFO.vb) |
| `LoadFromNFO_TVShow` | [clsAPINFO.vb](../../../EmberAPI/clsAPINFO.vb) |

---

## [↑](#table-of-contents) Related Files

| File | Purpose |
|------|---------|
| [clsAPINFO.vb](../../../EmberAPI/clsAPINFO.vb) | NFO loading methods |

---

## [↑](#table-of-contents) Benefits

- Better error messages for users
- Easier troubleshooting of NFO issues
- Potential for specific recovery actions per exception type
- Improved logging granularity

---

## [↑](#table-of-contents) Risks

- Need to ensure all exception types are covered to prevent unhandled exceptions
- Must maintain existing fail-safe behavior

---

## [↑](#table-of-contents) Source Reference

[NfoFileImprovements.md](../NfoFileImprovements.md) - Item 1.2

---

*Created: January 7, 2026*