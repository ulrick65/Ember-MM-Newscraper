# NFO File Processing - Improvement Plan

## Overview

This document outlines potential improvements identified during the NFO file processing analysis. Each item includes a description of the issue, proposed solution, affected files, and estimated complexity. Items can be discussed and approved before implementation.

---

## Improvement Tracking Legend

- [ ] **Not Started** - Item has not been reviewed
- [x] **Approved** - Item approved for implementation
- [~] **Declined** - Item reviewed but declined
- [!] **In Progress** - Currently being implemented
- [✓] **Completed** - Implementation finished

---

## Category 1: Exception Handling

### 1.1 Empty Catch Blocks in Directory Operations

- [ ] **Status:** Not Started

**Current Issue:**
In `clsAPINFO.vb` (Lines 1585-1614), directory file searches use empty catch blocks that silently swallow errors, making debugging difficult.

**Current Code Pattern:**
The code uses `Try` with `lFiles.AddRange(Directory.GetFiles(dirPath, "*.nfo"))` followed by an empty `Catch` and `End Try` block.

**Proposed Solution:**
Add proper logging for exceptions while maintaining the fail-safe behavior. Log at Debug or Info level to avoid cluttering logs during normal operation.

**Affected Files:**
- `EmberAPI\clsAPINFO.vb`

**Complexity:** Low

**Benefits:**
- Improved debugging capability
- Better visibility into file system issues
- Maintains current fail-safe behavior

**Risks:**
- Minimal - logging only, no behavior change

---

### 1.2 Generic Exception Handling in Load Methods

- [ ] **Status:** Not Started

**Current Issue:**
The `LoadFromNFO_*` methods catch generic `Exception` types. More specific exception handling would improve error diagnosis.

**Affected Methods:**
- `LoadFromNFO_Movie` (Lines 1838-1902)
- `LoadFromNFO_MovieSet` (Lines 1905-1929)
- `LoadFromNFO_TVEpisode` (Lines 1932-1994)
- `LoadFromNFO_TVShow` (Lines 2062-2104)

**Proposed Solution:**
Catch specific exceptions (`XmlException`, `IOException`, `UnauthorizedAccessException`) with appropriate handling for each.

**Affected Files:**
- `EmberAPI\clsAPINFO.vb`

**Complexity:** Medium

**Benefits:**
- Better error messages for users
- Easier troubleshooting
- Potential for specific recovery actions per exception type

**Risks:**
- Need to ensure all exception types are covered to prevent unhandled exceptions

---

## Category 2: Performance Optimizations

### 2.1 Cache XmlSerializer Instances

- [ ] **Status:** Not Started

**Current Issue:**
Each NFO load/save operation creates a new `XmlSerializer` instance. XmlSerializer construction is expensive as it generates and compiles code at runtime.

**Current Code Pattern:**
Each method creates a new serializer with `Dim xmlSer As XmlSerializer = New XmlSerializer(GetType(MediaContainers.Movie))` pattern.

**Proposed Solution:**
Create static, cached XmlSerializer instances for each media type. Use a dedicated helper class or Lazy initialization.

**Implementation Approach:**
Create a new helper class `NFOSerializers` with static lazy-initialized serializers for each type: Movie, Movieset, TVShow, EpisodeDetails.

**Affected Files:**
- `EmberAPI\clsAPINFO.vb`
- New file: `EmberAPI\clsAPINFOSerializers.vb` (optional, could be nested class)

**Complexity:** Medium

**Benefits:**
- Significant performance improvement for large libraries
- Reduced memory allocations
- Faster scanning operations

**Risks:**
- Thread safety considerations (XmlSerializer is thread-safe for Serialize/Deserialize)
- Need to handle assembly load context issues

---

### 2.2 Lightweight NFO Validation

- [ ] **Status:** Not Started

**Current Issue:**
The `IsConformingNFO_*` methods perform full deserialization just to check if an NFO is valid. This is expensive for large libraries.

**Current Approach:**
Full deserialization attempt, catch exception if invalid.

**Proposed Solution:**
Implement a lightweight validation that checks for the presence of the root XML element without full deserialization. Use `XmlReader` for quick validation.

**Implementation Approach:**
Create a method that uses XmlReader to verify the root element matches expected type (movie, tvshow, episodedetails, movieset) without parsing the entire document.

**Affected Files:**
- `EmberAPI\clsAPINFO.vb`

**Complexity:** Medium

**Benefits:**
- Faster library scanning
- Reduced memory usage during validation
- Quick rejection of non-conforming files

**Risks:**
- May miss some edge cases that full deserialization would catch
- Need comprehensive testing

---

## Category 3: Code Quality and Maintainability

### 3.1 Replace BinaryFormatter in CloneDeep Methods

- [ ] **Status:** Not Started

**Current Issue:**
The `CloneDeep()` methods in media container classes use `BinaryFormatter`, which is deprecated in .NET 5+ and has known security vulnerabilities. While the current project targets .NET Framework 4.8, this should be addressed for future compatibility.

**Current Code Location:**
- `MediaContainers.Movie.CloneDeep()` 
- `MediaContainers.EpisodeDetails.CloneDeep()`
- `MediaContainers.TVShow.CloneDeep()`
- `MediaContainers.Fileinfo.CloneDeep()`

**Proposed Solutions (in order of preference):**

1. **JSON Serialization** - Use System.Text.Json or Newtonsoft.Json
2. **Manual Copy Constructor** - Implement explicit property copying
3. **Expression-based Cloning** - Use libraries like FastDeepCloner

**Affected Files:**
- `EmberAPI\clsAPIMediaContainers.vb`

**Complexity:** Medium-High

**Benefits:**
- Future .NET compatibility
- Improved security
- Potentially better performance (depending on approach)

**Risks:**
- JSON serialization may handle some edge cases differently
- Manual copying requires maintenance when properties change
- Need thorough testing to ensure deep copy completeness

---

### 3.2 Fragile Multi-Episode Regex Pattern

- [ ] **Status:** Not Started

**Current Issue:**
The regex pattern used to extract multiple episode details from a single NFO file could fail with malformed XML or edge cases.

**Current Pattern:**
The pattern `<episodedetails.*?>.*?</episodedetails>` is used with RegexOptions for case insensitivity and singleline matching.

**Location:** `LoadFromNFO_TVEpisode` (Line 1942)

**Proposed Solution:**
Use XmlReader with fragment parsing capability, or pre-process the file to properly split at episode boundaries. Consider using XDocument with proper XML parsing.

**Affected Files:**
- `EmberAPI\clsAPINFO.vb`

**Complexity:** Medium

**Benefits:**
- More robust parsing
- Better handling of edge cases (CDATA, comments, attributes with angle brackets)
- Proper XML handling

**Risks:**
- May change behavior for some existing NFO files
- Need backward compatibility testing

---

## Category 4: Architecture Improvements

### 4.1 Strategy Pattern for NFO Formats

- [ ] **Status:** Not Started

**Current Issue:**
Support for different NFO formats (Kodi, YAMJ, Boxee) is scattered throughout the codebase with conditional checks. Adding new formats requires modifications in multiple locations.

**Current Approach:**
Conditional statements like `If Master.eSettings.MovieUseYAMJ Then` and `If Master.eSettings.TVUseBoxee Then` are used throughout save methods.

**Proposed Solution:**
Implement a strategy pattern with an `INfoWriter` interface and concrete implementations for each format (KodiNfoWriter, YamjNfoWriter, BoxeeNfoWriter).

**Implementation Approach:**
1. Define INfoWriter interface with Save methods for each media type
2. Create concrete implementations for each format
3. Use factory to select appropriate writer based on settings
4. Refactor existing code to use the strategy pattern

**Affected Files:**
- `EmberAPI\clsAPINFO.vb`
- New file: `EmberAPI\Interfaces\INfoWriter.vb`
- New files: `EmberAPI\NfoWriters\KodiNfoWriter.vb`, etc.

**Complexity:** High

**Benefits:**
- Cleaner separation of concerns
- Easier to add new NFO formats
- Improved testability
- Reduced conditional complexity

**Risks:**
- Significant refactoring effort
- Risk of introducing bugs during refactoring
- Need comprehensive regression testing

---

### 4.2 Separate Reading and Validation Concerns

- [ ] **Status:** Not Started

**Current Issue:**
The `LoadFromNFO_*` methods handle validation, normalization, and loading together. This makes the code harder to test and maintain.

**Current Behavior:**
Each load method performs: file existence check, deserialization, data cleaning/normalization, non-conforming file handling, and language resolution all in one method.

**Proposed Solution:**
Extract these concerns into separate methods or classes:
1. `NFOValidator` - Handles file validation and conformance checking
2. `NFOReader` - Handles pure deserialization
3. `NFONormalizer` - Handles data cleaning and normalization

**Affected Files:**
- `EmberAPI\clsAPINFO.vb`
- Potential new files for extracted classes

**Complexity:** High

**Benefits:**
- Improved testability (each concern can be unit tested independently)
- Cleaner code with single responsibility
- Easier to modify one aspect without affecting others
- Better code reuse

**Risks:**
- Significant refactoring effort
- Need to maintain backward compatibility
- Potential performance impact if not implemented carefully

---

## Category 5: Data Integrity

### 5.1 Improve Line Ending Normalization

- [ ] **Status:** Not Started

**Current Issue:**
The `CleanNFO_*` methods normalize line endings with a double replace pattern: `Replace(vbCrLf, vbLf).Replace(vbLf, vbCrLf)`. This works but is not intuitive.

**Current Pattern:**
Used in `CleanNFO_Movies`, `CleanNFO_TVShow`, and `CleanNFO_TVEpisodes` for Plot and Outline fields.

**Proposed Solution:**
Create a dedicated `NormalizeLineEndings` helper method with clear documentation explaining the purpose. Consider using `Environment.NewLine` for consistency.

**Affected Files:**
- `EmberAPI\clsAPINFO.vb`

**Complexity:** Low

**Benefits:**
- Clearer code intent
- Centralized line ending handling
- Easier to maintain

**Risks:**
- Minimal - simple refactoring

---

### 5.2 Consistent Date Format Handling

- [ ] **Status:** Not Started

**Current Issue:**
Date fields (Premiered, Aired) are normalized to ISO8601 format using `NumUtils.DateToISO8601Date()`. However, error handling for invalid dates could be improved.

**Current Behavior:**
Dates are converted during the clean process, but invalid dates may result in empty strings or unexpected behavior.

**Proposed Solution:**
Add explicit validation and logging for date parsing failures. Consider preserving original value if parsing fails with a warning.

**Affected Files:**
- `EmberAPI\clsAPINFO.vb`
- `EmberAPI\clsAPINumUtils.vb` (if date handling needs improvement)

**Complexity:** Low-Medium

**Benefits:**
- Better data preservation
- Improved debugging for date issues
- More predictable behavior

**Risks:**
- May surface previously hidden date format issues

---

## Priority Recommendations

Based on impact and effort, here is a suggested implementation order:

### Quick Wins (Low effort, Good value)
1. **1.1** - Empty Catch Blocks (adds debugging without behavior change)
2. **5.1** - Line Ending Normalization (simple cleanup)
3. **2.1** - Cache XmlSerializer (good performance gain)

### Medium Priority (Moderate effort, High value)
4. **2.2** - Lightweight NFO Validation (performance improvement)
5. **1.2** - Specific Exception Handling (better error diagnosis)
6. **3.2** - Multi-Episode Regex (robustness improvement)

### Long-term Improvements (High effort, Strategic value)
7. **3.1** - Replace BinaryFormatter (future compatibility)
8. **4.2** - Separate Reading/Validation Concerns (maintainability)
9. **4.1** - Strategy Pattern for NFO Formats (extensibility)

---

## Discussion Notes

Use this section to document decisions and discussions about each improvement item.

### Meeting Notes

| Date | Participants | Items Discussed | Decisions |
|------|--------------|-----------------|-----------|
| | | | |

### Additional Considerations

- All changes should include appropriate unit tests
- Performance improvements should be measured before/after
- Breaking changes should be avoided where possible
- Documentation should be updated as changes are implemented

---

## Implementation Progress

| Item | Status | Assigned To | Target Date | Completed Date |
|------|--------|-------------|-------------|----------------|
| 1.1 | Not Started | | | |
| 1.2 | Not Started | | | |
| 2.1 | Not Started | | | |
| 2.2 | Not Started | | | |
| 3.1 | Not Started | | | |
| 3.2 | Not Started | | | |
| 4.1 | Not Started | | | |
| 4.2 | Not Started | | | |
| 5.1 | Not Started | | | |
| 5.2 | Not Started | | | |