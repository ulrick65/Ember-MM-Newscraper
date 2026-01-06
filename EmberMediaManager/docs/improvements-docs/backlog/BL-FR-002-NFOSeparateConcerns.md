# BL-FR-002: Separate NFO Reading and Validation Concerns

| Field | Value |
|-------|-------|
| **ID** | BL-FR-002 |
| **Category** | Feature Requests (FR) |
| **Priority** | Low |
| **Effort** | 6-10 hrs |
| **Created** | January 7, 2026 |
| **Status** | Open |
| **Related Files** | `clsAPINFO.vb` |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Overview

The `LoadFromNFO_*` methods handle validation, normalization, and loading together. This makes the code harder to test and maintain. Separating these concerns would improve code quality and testability.

---

## Table of Contents

- [Problem Description](#problem-description)
- [Current Behavior](#current-behavior)
- [Proposed Solution](#proposed-solution)
  - [NFOValidator Class](#nfovalidator-class)
  - [NFOReader Class](#nforeader-class)
  - [NFONormalizer Class](#nfonormalizer-class)
  - [NFOLoader Orchestrator](#nfoloader-orchestrator)
- [Implementation Steps](#implementation-steps)
- [Affected Files](#affected-files)
- [Benefits](#benefits)
- [Risks](#risks)
- [Source Reference](#source-reference)

---

## [↑](#table-of-contents) Problem Description

Each load method currently performs multiple responsibilities:

1. File existence check
2. Deserialization
3. Data cleaning/normalization
4. Non-conforming file handling
5. Language resolution

This violates the Single Responsibility Principle and makes the code difficult to test and maintain.

---

## [↑](#table-of-contents) Current Behavior

All responsibilities are handled in one large method:

    Public Shared Function LoadFromNFO_Movie(path As String) As MediaContainers.Movie
        If Not File.Exists(path) Then Return Nothing
        
        Try
            ' Deserialization
            Using reader As New StreamReader(path)
                result = serializer.Deserialize(reader)
            End Using
            
            ' Data cleaning
            result.Title = result.Title?.Trim()
            result.Plot = NormalizeLineEndings(result.Plot)
            
            ' Non-conforming handling
            If Not IsConformingNFO_Movie(path) Then
                ' Handle legacy format
            End If
            
            ' Language resolution
            result.Language = ResolveLanguage(result.Language)
            
            Return result
        Catch ex As Exception
            logger.Error(ex, ...)
            Return Nothing
        End Try
    End Function

---

## [↑](#table-of-contents) Proposed Solution

Extract concerns into separate classes with single responsibilities.

### [↑](#table-of-contents) NFOValidator Class

Handles file validation and conformance checking:

    Public Class NFOValidator
        Public Function IsValid(path As String, mediaType As MediaType) As Boolean
        Public Function GetValidationErrors(path As String) As List(Of String)
        Public Function IsConforming(path As String, mediaType As MediaType) As Boolean
    End Class

---

### [↑](#table-of-contents) NFOReader Class

Handles pure deserialization without any cleaning:

    Public Class NFOReader
        Public Function ReadMovie(path As String) As MediaContainers.Movie
        Public Function ReadTVShow(path As String) As MediaContainers.TVShow
        Public Function ReadEpisode(path As String) As MediaContainers.EpisodeDetails
        Public Function ReadMovieSet(path As String) As MediaContainers.MovieSet
    End Class

---

### [↑](#table-of-contents) NFONormalizer Class

Handles data cleaning and normalization:

    Public Class NFONormalizer
        Public Sub NormalizeMovie(movie As MediaContainers.Movie)
        Public Sub NormalizeTVShow(show As MediaContainers.TVShow)
        Public Sub NormalizeEpisode(episode As MediaContainers.EpisodeDetails)
        Public Sub NormalizeMovieSet(movieset As MediaContainers.MovieSet)
    End Class

---

### [↑](#table-of-contents) NFOLoader Orchestrator

Coordinates the other classes:

    Public Class NFOLoader
        Private _validator As NFOValidator
        Private _reader As NFOReader
        Private _normalizer As NFONormalizer
        
        Public Function LoadMovie(path As String) As MediaContainers.Movie
            If Not _validator.IsValid(path, MediaType.Movie) Then
                Return Nothing
            End If
            
            Dim movie = _reader.ReadMovie(path)
            _normalizer.NormalizeMovie(movie)
            Return movie
        End Function
    End Class

---

## [↑](#table-of-contents) Implementation Steps

1. Create `NFOValidator` class with validation methods
2. Create `NFOReader` class with pure deserialization
3. Create `NFONormalizer` class with cleaning logic
4. Create `NFOLoader` orchestrator class
5. Refactor existing methods to use new classes
6. Add unit tests for each class
7. Update callers to use new `NFOLoader` class

---

## [↑](#table-of-contents) Affected Files

| File | Purpose |
|------|---------|
| [clsAPINFO.vb](../../../EmberAPI/clsAPINFO.vb) | Existing methods to refactor |
| New: `EmberAPI\NFO\NFOValidator.vb` | Validation logic |
| New: `EmberAPI\NFO\NFOReader.vb` | Deserialization logic |
| New: `EmberAPI\NFO\NFONormalizer.vb` | Cleaning logic |
| New: `EmberAPI\NFO\NFOLoader.vb` | Orchestration |

---

## [↑](#table-of-contents) Benefits

- Improved testability (each concern can be unit tested independently)
- Cleaner code with single responsibility principle
- Easier to modify one aspect without affecting others
- Better code reuse

---

## [↑](#table-of-contents) Risks

- Significant refactoring effort
- Need to maintain backward compatibility
- Potential performance impact if not implemented carefully (multiple passes over data)

---

## [↑](#table-of-contents) Source Reference

[NfoFileImprovements.md](../NfoFileImprovements.md) - Item 4.2

---

*Created: January 7, 2026*