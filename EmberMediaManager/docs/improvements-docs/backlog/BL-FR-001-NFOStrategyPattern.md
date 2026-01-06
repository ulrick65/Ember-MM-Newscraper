# BL-FR-001: Strategy Pattern for NFO Formats

| Field | Value |
|-------|-------|
| **ID** | BL-FR-001 |
| **Category** | Feature Requests (FR) |
| **Priority** | Low |
| **Effort** | 8-12 hrs |
| **Created** | January 7, 2026 |
| **Status** | Open |
| **Related Files** | `clsAPINFO.vb` |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Overview

Support for different NFO formats (Kodi, YAMJ, Boxee) is scattered throughout the codebase with conditional checks. Adding new formats requires modifications in multiple locations. A strategy pattern would improve extensibility and maintainability.

---

## Table of Contents

- [Problem Description](#problem-description)
- [Current Behavior](#current-behavior)
- [Proposed Solution](#proposed-solution)
  - [Interface Definition](#interface-definition)
  - [Concrete Implementations](#concrete-implementations)
  - [Factory Selection](#factory-selection)
- [Implementation Steps](#implementation-steps)
- [Affected Files](#affected-files)
- [Benefits](#benefits)
- [Risks](#risks)
- [Related Considerations](#related-considerations)
- [Source Reference](#source-reference)

---

## [↑](#table-of-contents) Problem Description

The current approach has several issues:

- Format-specific logic is scattered across multiple files
- Adding new formats requires modifying multiple locations
- Increases code complexity with nested conditionals
- Reduces testability

---

## [↑](#table-of-contents) Current Behavior

Conditional statements are used throughout save methods:

    If Master.eSettings.MovieUseYAMJ Then
        ' YAMJ-specific logic
    ElseIf Master.eSettings.TVUseBoxee Then
        ' Boxee-specific logic
    Else
        ' Default Kodi format
    End If

---

## [↑](#table-of-contents) Proposed Solution

Implement a strategy pattern with an `INfoWriter` interface and concrete implementations for each format.

### [↑](#table-of-contents) Interface Definition

    Public Interface INfoWriter
        Sub SaveMovie(movie As MediaContainers.Movie, path As String)
        Sub SaveTVShow(show As MediaContainers.TVShow, path As String)
        Sub SaveEpisode(episode As MediaContainers.EpisodeDetails, path As String)
        Sub SaveMovieSet(movieset As MediaContainers.MovieSet, path As String)
    End Interface

---

### [↑](#table-of-contents) Concrete Implementations

| Class | Description |
|-------|-------------|
| `KodiNfoWriter` | Default Kodi/XBMC format |
| `YamjNfoWriter` | YAMJ format (legacy) |
| `BoxeeNfoWriter` | Boxee format (legacy) |

---

### [↑](#table-of-contents) Factory Selection

    Public Class NfoWriterFactory
        Public Shared Function GetWriter() As INfoWriter
            If Master.eSettings.MovieUseYAMJ Then
                Return New YamjNfoWriter()
            ElseIf Master.eSettings.TVUseBoxee Then
                Return New BoxeeNfoWriter()
            Else
                Return New KodiNfoWriter()
            End If
        End Function
    End Class

---

## [↑](#table-of-contents) Implementation Steps

1. Define `INfoWriter` interface with save methods for each media type
2. Create concrete implementations for each format
3. Create factory to select appropriate writer based on settings
4. Refactor existing code to use the strategy pattern
5. Add unit tests for each writer implementation

---

## [↑](#table-of-contents) Affected Files

| File | Purpose |
|------|---------|
| [clsAPINFO.vb](../../../EmberAPI/clsAPINFO.vb) | Existing NFO save methods to refactor |
| New: `EmberAPI\Interfaces\INfoWriter.vb` | Interface definition |
| New: `EmberAPI\NfoWriters\KodiNfoWriter.vb` | Kodi implementation |
| New: `EmberAPI\NfoWriters\YamjNfoWriter.vb` | YAMJ implementation |
| New: `EmberAPI\NfoWriters\BoxeeNfoWriter.vb` | Boxee implementation |
| New: `EmberAPI\NfoWriters\NfoWriterFactory.vb` | Factory class |

---

## [↑](#table-of-contents) Benefits

- Cleaner separation of concerns
- Easier to add new NFO formats
- Improved testability (each writer can be unit tested)
- Reduced conditional complexity in main code

---

## [↑](#table-of-contents) Risks

- Significant refactoring effort
- Risk of introducing bugs during refactoring
- Need comprehensive regression testing

---

## [↑](#table-of-contents) Related Considerations

If legacy formats (YAMJ, Boxee) are removed per Code Cleanup items in [FutureEnhancements.md](../FutureEnhancements.md), this enhancement may become unnecessary. Consider sequencing with format removal decisions.

---

## [↑](#table-of-contents) Source Reference

[NfoFileImprovements.md](../NfoFileImprovements.md) - Item 4.1

---

*Created: January 7, 2026*