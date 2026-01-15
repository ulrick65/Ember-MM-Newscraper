# BL-CC-001: Replace BinaryFormatter in CloneDeep Methods

| Field | Value |
|-------|-------|
| **ID** | BL-CC-001 |
| **Created** | January 7, 2026 |
| **Updated** | January 14, 2026 |
| **Category** | Code Cleanup (CC) |
| **Priority** | Medium |
| **Effort** | 3 hours |
| **Status** | ✅ Complete |
| **Completed** | January 14, 2026 |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Summary

The `CloneDeep()` methods in media container classes use `BinaryFormatter`, which is deprecated in .NET 5+ and has known security vulnerabilities. This cleanup replaces all instances with JSON serialization using Newtonsoft.Json.

---

## Table of Contents

- [Problem Description](#problem-description)
- [Affected Methods](#affected-methods)
- [Chosen Solution](#chosen-solution)
- [Implementation Reference](#implementation-reference)
- [Implementation Progress](#implementation-progress)
- [Testing Summary](#testing-summary)
- [Proposed Solutions (Reference)](#proposed-solutions-reference)
- [Related Files](#related-files)
- [Benefits](#benefits)
- [Risks](#risks)
- [Notes](#notes)
- [Change History](#change-history)

---

## [↑](#table-of-contents) Problem Description

`BinaryFormatter` is deprecated in .NET 5+ due to:

- Known security vulnerabilities (deserialization attacks)
- Poor cross-platform compatibility
- No support in future .NET versions

While Ember currently targets .NET Framework 4.8, addressing this now will ease future migration.

---

## [↑](#table-of-contents) Affected Methods

### Phase 1: clsAPIMediaContainers.vb (Completed ✅)

| Method | Class | File | Status |
|--------|-------|------|:------:|
| `CloneDeep()` | `MediaContainers.Movie` | clsAPIMediaContainers.vb | ✅ |
| `CloneDeep()` | `MediaContainers.EpisodeDetails` | clsAPIMediaContainers.vb | ✅ |
| `CloneDeep()` | `MediaContainers.TVShow` | clsAPIMediaContainers.vb | ✅ |
| `CloneDeep()` | `MediaContainers.Fileinfo` | clsAPIMediaContainers.vb | ✅ |
| `CloneDeep()` | `MediaContainers.Movieset` | clsAPIMediaContainers.vb | ✅ |

### Phase 2: Additional Discovered Items (Completed ✅)

| Method | Class | File | Status |
|--------|-------|------|:------:|
| `CloneDeep()` | `clsXMLSimpleMapping` | XML Serialization\clsXMLSimpleMapping.vb | ✅ |
| `CloneDeep()` | `clsXMLRegexMapping` | XML Serialization\clsXMLRegexMapping.vb | ✅ |
| `CloneDeep()` | `clsXMLGenreMapping` | XML Serialization\clsXMLGenreMapping.vb | ✅ |
| `CloneDeep()` | `Database.DBElement` | clsAPIDatabase.vb | ✅ |

---

## [↑](#table-of-contents) Chosen Solution

**Option 1: JSON Serialization** using Newtonsoft.Json (already referenced in project).

This approach was chosen because:
- Simple implementation (3 lines of code)
- Well-tested library already in use throughout the project
- No caller changes required — method signature remains identical
- Handles complex object graphs correctly

---

## [↑](#table-of-contents) Implementation Reference

This section provides a complete before/after example for reference when implementing additional classes.

### [↑](#table-of-contents) Prerequisites

Ensure `Newtonsoft.Json` import exists at top of file:

    Imports Newtonsoft.Json

If the file doesn't have this import, use fully qualified names instead:

    Newtonsoft.Json.JsonConvert.SerializeObject(Me)
    Newtonsoft.Json.JsonConvert.DeserializeObject(Of [ClassName])(json)

### [↑](#table-of-contents) Before (BinaryFormatter — deprecated)

    Public Function CloneDeep() As Object Implements ICloneable.Clone
        Dim Stream As New MemoryStream(50000)
        Dim Formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Formatter.Serialize(Stream, Me)
        Stream.Seek(0, SeekOrigin.Begin)
        CloneDeep = Formatter.Deserialize(Stream)
        Stream.Close()
    End Function

### [↑](#table-of-contents) After (JSON Serialization — recommended)

    ''' <summary>
    ''' Creates a deep copy of this [ClassName] instance.
    ''' </summary>
    ''' <returns>A new [ClassName] object with all properties copied.</returns>
    ''' <remarks>
    ''' Uses JSON serialization via Newtonsoft.Json to perform the deep clone.
    ''' This replaces the deprecated BinaryFormatter approach for security and 
    ''' future .NET compatibility.
    ''' </remarks>
    Public Function CloneDeep() As Object Implements ICloneable.Clone
        'Use JSON serialization for deep cloning (replaces deprecated BinaryFormatter)
        Dim json As String = JsonConvert.SerializeObject(Me)
        Return JsonConvert.DeserializeObject(Of [ClassName])(json)
    End Function

**Note:** Replace `[ClassName]` with the actual class name (e.g., `Movie`, `TVShow`, `clsXMLSimpleMapping`, `DBElement`).

---

## [↑](#table-of-contents) Implementation Progress

### Phase 1 Complete — January 15, 2026

All five `CloneDeep()` methods in `clsAPIMediaContainers.vb` have been updated using the pattern shown in [Implementation Reference](#implementation-reference).

**Classes Updated:**
- `Movie` 
- `EpisodeDetails`
- `TVShow`
- `Fileinfo`
- `Movieset` (added `Implements ICloneable` and `CloneDeep()` method)

### Phase 2 Complete — January 14, 2026

Four additional `CloneDeep()` methods updated:

| Class | File | Notes |
|-------|------|-------|
| `clsXMLSimpleMapping` | [`clsXMLSimpleMapping.vb`](../../../../EmberAPI/XML%20Serialization/clsXMLSimpleMapping.vb) | Added `Imports Newtonsoft.Json` |
| `clsXMLRegexMapping` | [`clsXMLRegexMapping.vb`](../../../../EmberAPI/XML%20Serialization/clsXMLRegexMapping.vb) | Added `Imports Newtonsoft.Json` |
| `clsXMLGenreMapping` | [`clsXMLGenreMapping.vb`](../../../../EmberAPI/XML%20Serialization/clsXMLGenreMapping.vb) | Added `Imports Newtonsoft.Json` |
| `Database.DBElement` | [`clsAPIDatabase.vb`](../../../../EmberAPI/clsAPIDatabase.vb) | Nested class within `Database` class |

---

## [↑](#table-of-contents) Testing Summary

**Status Legend:** ⬜ Not tested | ✅ Passed | ❌ Failed | ⏸️ Skipped

### Phase 1: clsAPIMediaContainers.vb

#### Movie

| Status | Test Scenario |
|:------:|---------------|
| ✅ | Edit movie → Cancel → Original unchanged |
| ✅ | Edit movie → Save → Changes persisted |
| ✅ | Scrape movie → Data correct |

#### EpisodeDetails

| Status | Test Scenario |
|:------:|---------------|
| ✅ | Edit episode → Cancel → Original unchanged |
| ✅ | Edit episode → Save → Changes persisted |
| ✅ | Scrape episode → Data correct |

#### TVShow

| Status | Test Scenario |
|:------:|---------------|
| ✅ | Edit TV show → Cancel → Original unchanged |
| ✅ | Edit TV show → Save → Changes persisted |
| ✅ | Scrape TV show → Data correct |

#### Fileinfo

| Status | Test Scenario |
|:------:|---------------|
| ✅ | Edit file info → Cancel → Original unchanged |
| ✅ | Edit file info → Save → Changes persisted |

#### Movieset

| Status | Test Scenario |
|:------:|---------------|
| ✅ | Search movieset → Select result → Details displayed correctly |
| ✅ | Search movieset → Select different result → Cache works correctly |
| ✅ | Edit movieset → Cancel → Original unchanged |

### Phase 2: Additional Classes

#### clsXMLSimpleMapping (Certification, Country, Status, Studio mappings)

| Status | Test Scenario |
|:------:|---------------|
| ✅ | Settings → Certification Mapping → Edit → Cancel → No changes saved |
| ✅ | Settings → Country Mapping → Add/modify entry → Save → Changes persisted |

#### clsXMLRegexMapping

| Status | Test Scenario |
|:------:|---------------|
| ✅ | Build completes without errors (class may be unused/legacy) |

#### clsXMLGenreMapping

| Status | Test Scenario |
|:------:|---------------|
| ✅ | Settings → Genre Mapping → Edit genre → Cancel → No changes saved |
| ✅ | Settings → Genre Mapping → Add mapping → Save → Changes persisted |

#### Database.DBElement

| Status | Test Scenario |
|:------:|---------------|
| ✅ | Scan TV show with multi-episode file (e.g., S01E01E02) → Both episodes created with correct distinct metadata |
| ✅ | Edit Episode dialog → Change episode number → Save → Verify each episode in multi-episode file retains correct data |

---

## [↑](#table-of-contents) Proposed Solutions (Reference)

### Option 1: JSON Serialization ← CHOSEN

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

### Option 2: Manual Copy Constructor

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

### Option 3: Expression-based Cloning

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
| [`EmberAPI\clsAPIMediaContainers.vb`](../../../../EmberAPI/clsAPIMediaContainers.vb) | Phase 1: Media container classes (completed) |
| [`EmberAPI\XML Serialization\clsXMLSimpleMapping.vb`](../../../../EmberAPI/XML%20Serialization/clsXMLSimpleMapping.vb) | Phase 2: Simple mapping class |
| [`EmberAPI\XML Serialization\clsXMLRegexMapping.vb`](../../../../EmberAPI/XML%20Serialization/clsXMLRegexMapping.vb) | Phase 2: Regex mapping class |
| [`EmberAPI\XML Serialization\clsXMLGenreMapping.vb`](../../../../EmberAPI/XML%20Serialization/clsXMLGenreMapping.vb) | Phase 2: Genre mapping class |
| [`EmberAPI\clsAPIDatabase.vb`](../../../../EmberAPI/clsAPIDatabase.vb) | Phase 2: DBElement nested class |

---

## [↑](#table-of-contents) Benefits

- Future .NET compatibility (.NET 5/6/7+)
- Improved security (eliminates BinaryFormatter vulnerabilities)
- Consistent implementation pattern across all classes

---

## [↑](#table-of-contents) Risks

- JSON serialization may handle some edge cases differently
- Need thorough testing to ensure deep copy completeness
- Properties with `<XmlIgnore>` attributes are still serialized by JSON (verified working)
- `Database.DBElement` is a complex nested class — may require additional testing

---

## [↑](#table-of-contents) Notes

**Source Reference:** [NfoFileImprovements.md](../NfoFileImprovements.md) - Item 3.1

**Phase 1 Implementation Notes:**
- `Movie` class required `<JsonIgnore>` attributes on `Set_Kodi` and `Sets_YAMJ` properties because these are computed properties that return `XmlDocument`/`SetContainer` for NFO serialization — JSON cannot properly serialize/deserialize these types. The actual data is stored in the `Sets` property which JSON handles correctly.
- `Movieset` class did not previously implement `ICloneable` — added `Implements ICloneable` and `CloneDeep()` method to enable cloning in search result dialogs.

**Phase 2 Implementation Notes:**
- XML Serialization classes don't have `Imports Newtonsoft.Json` — use fully qualified names
- `Database.DBElement` is a nested class inside `Database` — ensure proper class reference in `DeserializeObject(Of Database.DBElement)`

---

## [↑](#table-of-contents) Change History

| Date | Description |
|------|-------------|
| January 7, 2026 | Created |
| January 14, 2026 | Completed Phase 1: All four CloneDeep methods in clsAPIMediaContainers.vb updated and tested |
| January 14, 2026 | Added Phase 2: Discovered four additional BinaryFormatter usages in XML Serialization classes and clsAPIDatabase.vb |
| January 14, 2026 | Added `<JsonIgnore>` to `Movie.Set_Kodi` and `Movie.Sets_YAMJ` to fix JSON serialization error for movies in sets |
| January 14, 2026 | Phase 2 testing complete — all classes verified ✅ |
| January 15, 2026 | Added `Movieset` class: Implemented `ICloneable` and `CloneDeep()` method (class previously lacked cloning support) |
| January 15, 2026 | All implementation complete — BL-CC-001 fully closed ✅ |

---

*End of file*