# BL-CC-001: Replace BinaryFormatter in CloneDeep Methods

| Field | Value |
|-------|-------|
| **ID** | BL-CC-001 |
| **Category** | Code Cleanup (CC) |
| **Priority** | Medium |
| **Effort** | 4-6 hrs |
| **Created** | January 7, 2026 |
| **Status** | Open |
| **Related Files** | `clsAPIMediaContainers.vb` |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Overview

The `CloneDeep()` methods in media container classes use `BinaryFormatter`, which is deprecated in .NET 5+ and has known security vulnerabilities. While the current project targets .NET Framework 4.8, this should be addressed for future compatibility and security.

---

## Table of Contents

- [Problem Description](#problem-description)
- [Current Behavior](#current-behavior)
- [Affected Methods](#affected-methods)
- [Proposed Solutions](#proposed-solutions)
  - [Option 1: JSON Serialization](#option-1-json-serialization)
  - [Option 2: Manual Copy Constructor](#option-2-manual-copy-constructor)
  - [Option 3: Expression-based Cloning](#option-3-expression-based-cloning)
- [Related Files](#related-files)
- [Benefits](#benefits)
- [Risks](#risks)
- [Source Reference](#source-reference)

---

## [↑](#table-of-contents) Problem Description

`BinaryFormatter` is deprecated in .NET 5+ due to:

- Known security vulnerabilities (deserialization attacks)
- Poor cross-platform compatibility
- No support in future .NET versions

While Ember currently targets .NET Framework 4.8, addressing this now will ease future migration.

---

## [↑](#table-of-contents) Current Behavior

CloneDeep methods use BinaryFormatter for deep copying objects:

    Public Function CloneDeep() As Movie
        Using ms As New MemoryStream()
            Dim bf As New BinaryFormatter()
            bf.Serialize(ms, Me)
            ms.Position = 0
            Return DirectCast(bf.Deserialize(ms), Movie)
        End Using
    End Function

---

## [↑](#table-of-contents) Affected Methods

| Method | Class | File |
|--------|-------|------|
| `CloneDeep()` | `MediaContainers.Movie` | [clsAPIMediaContainers.vb](../../../EmberAPI/clsAPIMediaContainers.vb) |
| `CloneDeep()` | `MediaContainers.EpisodeDetails` | [clsAPIMediaContainers.vb](../../../EmberAPI/clsAPIMediaContainers.vb) |
| `CloneDeep()` | `MediaContainers.TVShow` | [clsAPIMediaContainers.vb](../../../EmberAPI/clsAPIMediaContainers.vb) |
| `CloneDeep()` | `MediaContainers.Fileinfo` | [clsAPIMediaContainers.vb](../../../EmberAPI/clsAPIMediaContainers.vb) |

---

## [↑](#table-of-contents) Proposed Solutions

### [↑](#table-of-contents) Option 1: JSON Serialization

Use Newtonsoft.Json (already referenced in project):

    Public Function CloneDeep() As Movie
        Dim json As String = JsonConvert.SerializeObject(Me)
        Return JsonConvert.DeserializeObject(Of Movie)(json)
    End Function

**Pros:**
- Simple implementation
- Well-tested library
- Handles most cases correctly

**Cons:**
- May handle some edge cases differently than BinaryFormatter
- Slightly slower for large objects

---

### [↑](#table-of-contents) Option 2: Manual Copy Constructor

Implement explicit property copying:

    Public Function CloneDeep() As Movie
        Dim clone As New Movie()
        clone.Title = Me.Title
        clone.Year = Me.Year
        clone.Plot = Me.Plot
        ' ... all properties
        Return clone
    End Function

**Pros:**
- Full control over copying behavior
- No serialization overhead
- Fastest option

**Cons:**
- Requires maintenance when properties change
- More code to write and test

---

### [↑](#table-of-contents) Option 3: Expression-based Cloning

Use libraries like FastDeepCloner:

    Public Function CloneDeep() As Movie
        Return DeepCloner.Clone(Me)
    End Function

**Pros:**
- Fast performance
- Handles complex object graphs automatically

**Cons:**
- Adds external dependency
- Less control over cloning behavior

---

## [↑](#table-of-contents) Related Files

| File | Purpose |
|------|---------|
| [clsAPIMediaContainers.vb](../../../EmberAPI/clsAPIMediaContainers.vb) | Media container classes with CloneDeep methods |

---

## [↑](#table-of-contents) Benefits

- Future .NET compatibility (.NET 5/6/7+)
- Improved security (eliminates BinaryFormatter vulnerabilities)
- Potentially better performance (depending on approach chosen)

---

## [↑](#table-of-contents) Risks

- JSON serialization may handle some edge cases differently
- Manual copying requires ongoing maintenance when properties change
- Need thorough testing to ensure deep copy completeness

---

## [↑](#table-of-contents) Source Reference

[NfoFileImprovements.md](../NfoFileImprovements.md) - Item 3.1

---

*Created: January 7, 2026*