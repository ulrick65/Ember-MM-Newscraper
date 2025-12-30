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

- [✓] **Status:** Completed (2025-12-26)

**Current Issue:**
In `clsAPINFO.vb`, directory file searches use empty catch blocks that silently swallow errors, making debugging difficult.

**Current Code Pattern:**
The code uses `Try` with `lFiles.AddRange(Directory.GetFiles(dirPath, "*.nfo"))` followed by an empty `Catch` and `End Try` block.

**Implemented Solution:**
Added proper logging for exceptions using `logger.Debug()` with descriptive messages including the directory path and search pattern. Maintains fail-safe behavior while providing debugging visibility.

**Affected Files:**
- `EmberAPI\clsAPINFO.vb` - `GetIMDBFromNonConf` method

**Complexity:** Low

**Benefits:**
- Improved debugging capability
- Better visibility into file system issues
- Maintains current fail-safe behavior

---

### 1.2 Generic Exception Handling in Load Methods

- [ ] **Status:** Not Started

**Current Issue:**
The `LoadFromNFO_*` methods catch generic `Exception` types. More specific exception handling would improve error diagnosis.

**Affected Methods:**
- `LoadFromNFO_Movie`
- `LoadFromNFO_MovieSet`
- `LoadFromNFO_TVEpisode`
- `LoadFromNFO_TVShow`

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

- [✓] **Status:** Completed (2025-12-26)

**Current Issue:**
Each NFO load/save operation creates a new `XmlSerializer` instance. XmlSerializer construction is expensive as it generates and compiles code at runtime.

**Implemented Solution:**
Created a new helper class `NFOSerializers` with static, thread-safe cached XmlSerializer instances for each media type using `Lazy(Of T)` initialization.

**New File Created:**
- `EmberAPI\clsAPINFOSerializers.vb`

**Updated Methods in `EmberAPI\clsAPINFO.vb`:**
- `IsConformingNFO_Movie`, `IsConformingNFO_TVEpisode`, `IsConformingNFO_TVShow`
- `LoadFromNFO_Movie`, `LoadFromNFO_MovieSet`, `LoadFromNFO_TVEpisode` (2 overloads), `LoadFromNFO_TVShow`
- `SaveToNFO_Movie`, `SaveToNFO_MovieSet`, `SaveToNFO_TVEpisode`, `SaveToNFO_TVShow`

**Complexity:** Medium

**Benefits:**
- Significant performance improvement for large libraries
- Reduced memory allocations
- Faster scanning operations
- Thread-safe implementation

---

### 2.2 Lightweight NFO Validation

- [~] **Status:** Declined (2025-12-26)

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

**Decision:** Declined - Full deserialization is necessary to properly validate NFO conformance. Lightweight validation would miss data type errors, encoding issues, and structural problems that full deserialization catches. With cached XmlSerializers (Item 2.1), performance is already significantly improved. Future serializer optimizations will further reduce any scanning overhead.

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

**Location:** `LoadFromNFO_TVEpisode`

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

- [✓] **Status:** Completed (2025-12-26)

**Current Issue:**
The `CleanNFO_*` methods normalize line endings with a double replace pattern: `Replace(vbCrLf, vbLf).Replace(vbLf, vbCrLf)`. This works but is not intuitive and doesn't handle standalone CR characters.

**Implemented Solution:**
Created a dedicated `NormalizeLineEndings` helper method with clear XML documentation explaining the purpose. The method handles all line ending formats (CR, LF, CRLF) and normalizes to Windows format (CRLF).

**Implementation Details:**
- Added `NormalizeLineEndings(text As String)` private shared function
- Handles null/empty input gracefully
- Converts CR → LF → CRLF to ensure consistent output
- Applied to Plot and Outline fields in all CleanNFO methods
- Also applied to `LoadFromNFO_MovieSet` for Plot field

**Affected Files:**
- `EmberAPI\clsAPINFO.vb`
  - New helper method: `NormalizeLineEndings`
  - Updated: `CleanNFO_Movies` (Outline, Plot)
  - Updated: `CleanNFO_TVEpisodes` (Plot)
  - Updated: `CleanNFO_TVShow` (Plot)
  - Updated: `LoadFromNFO_MovieSet` (Plot)

**Complexity:** Low

**Benefits:**
- Clearer code intent with descriptive method name
- Centralized line ending handling
- Handles all line ending formats (CR, LF, CRLF)
- Easier to maintain and modify

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
1. ~~**1.1** - Empty Catch Blocks (adds debugging without behavior change)~~ ✓ COMPLETED
2. ~~**5.1** - Line Ending Normalization (simple cleanup)~~ ✓ COMPLETED
3. ~~**2.1** - Cache XmlSerializer (good performance gain)~~ ✓ COMPLETED

### Medium Priority (Moderate effort, High value)
4. ~~**2.2** - Lightweight NFO Validation (performance improvement)~~ **DECLINED**
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
| 2025-12-26 | Developer | Quick Wins (1.1, 2.1, 5.1) | Approved and implemented all three quick win items |
| 2.2 | ~ Declined | - | - | 2025-12-26 |

### Additional Considerations

- All changes should include appropriate unit tests
- Performance improvements should be measured before/after
- Breaking changes should be avoided where possible
- Documentation should be updated as changes are implemented

---

## Implementation Progress

| Item | Status | Assigned To | Target Date | Completed Date |
|------|--------|-------------|-------------|----------------|
| 1.1 | ✓ Completed | - | - | 2025-12-26 |
| 1.2 | Not Started | | | |
| 2.1 | ✓ Completed | - | - | 2025-12-26 |
| 2.2 | ~ Declined | - | - | 2025-12-26 |
| 3.1 | Not Started | | | |
| 3.2 | Not Started | | | |
| 4.1 | Not Started | | | |
| 4.2 | Not Started | | | |
| 5.1 | ✓ Completed | - | - | 2025-12-26 |
| 5.2 | Not Started | | | |