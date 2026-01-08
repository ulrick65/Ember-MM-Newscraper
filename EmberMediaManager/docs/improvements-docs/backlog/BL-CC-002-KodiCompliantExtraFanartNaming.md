# BL-CC-002: Kodi-Compliant ExtraFanart Naming

| | Document Info |
|---------------|---|
| **ID** | BL-CC-002 |
| **Category** | Code Cleanup (CC) |
| **Priority** | Medium |
| **Effort** | 4-6 hrs |
| **Created** | January 5, 2026 |
| **Updated** | January 7, 2026 |
| **Status** | ✅ Complete |
| **Related Files** | `clsAPIImages.vb`, `clsAPIFileUtils.vb`, `clsAPIScanner.vb`, `clsAPIDatabase.vb`, `clsAPIMediaContainers.vb`, `frmMain.vb` |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Overview

Kodi is deprecating the `extrafanart/` subfolder approach for additional fanart images. This item updates Ember to use Kodi's current standard: sequential fanart naming in the main media folder.

**What changed:**
- Movies now use `[filename]-fanart1.jpg`, `[filename]-fanart2.jpg`, etc.
- TV Shows now use `fanart1.jpg`, `fanart2.jpg`, etc.
- Legacy `extrafanart/` subfolders are automatically migrated during library scan

**Scope:**
- ✅ Movies — Full support for extrafanarts
- ✅ TV Shows — Full support for extrafanarts (show-level only)
- ❌ TV Seasons — Not supported (Ember's data model does not include season extrafanarts)
- ❌ TV Episodes — Not supported

---

## Table of Contents

- [Problem Description](#problem-description)
- [Current Behavior](#current-behavior)
- [Implemented Solution](#implemented-solution)
- [Files Modified](#files-modified)
- [Migration Behavior](#migration-behavior)
- [Benefits](#benefits)
- [Testing Checklist](#testing-checklist)
- [Implementation Details](#implementation-details)
- [Bugs Fixed](#bugs-fixed)

---

## [↑](#table-of-contents) Problem Description

Kodi is deprecating the `extrafanart/` subfolder method for storing additional fanart images. The previous Ember implementation:

- Saved extrafanarts to a separate `extrafanart/` subfolder
- Used filenames like `fanart1.jpg` inside that subfolder
- Created unnecessary folder complexity
- Did not align with Kodi's current naming standard

---

## [↑](#table-of-contents) Current Behavior

**Before (Legacy):**

    Movie Folder/
    ├── MovieName.nfo
    ├── MovieName-fanart.jpg        (primary fanart)
    ├── MovieName-poster.jpg
    └── extrafanart/                (subfolder - deprecated)
        ├── fanart1.jpg
        ├── fanart2.jpg
        └── ...

**After (Kodi-Compliant):**

    Movie Folder/
    ├── MovieName.nfo
    ├── MovieName-fanart.jpg        (primary fanart - unchanged)
    ├── MovieName-fanart1.jpg       (first extrafanart)
    ├── MovieName-fanart2.jpg       (second extrafanart)
    ├── MovieName-fanart3.jpg       (third extrafanart)
    ├── MovieName-poster.jpg
    └── ...

---

## [↑](#table-of-contents) Implemented Solution

### [↑](#table-of-contents) File Naming Convention

| Media Type | Pattern | Example |
|------------|---------|---------|
| **Movies** | `[filename]-fanart[N].jpg` | `Avatar (2009)-fanart1.jpg` |
| **TV Shows** | `fanart[N].jpg` | `fanart1.jpg` |

**Notes:**
- Primary fanart remains unchanged (`-fanart.jpg` for movies, `fanart.jpg` for TV)
- Extrafanarts are numbered starting from 1
- File extension is preserved from original image
- Order matches user selection order in image dialog
- Regex pattern handles special characters (like `[]`) in filenames

### [↑](#table-of-contents) Regex Pattern Used

For scanning and loading extrafanarts:

    ' Movies: [filename]-fanart[0-9]+\.(jpg|png)
    Dim numberedFanartPattern As New Regex("^" & Regex.Escape(fileNameStack) & "-fanart[0-9]+\.(jpg|png)$", RegexOptions.IgnoreCase)
    
    ' TV Shows: fanart[0-9]+\.(jpg|png)
    Dim numberedFanartPattern As New Regex("^fanart[0-9]+\.(jpg|png)$", RegexOptions.IgnoreCase)

---

## [↑](#table-of-contents) Files Modified

| File | Changes Made |
|------|--------------|
| `clsAPIImages.vb` | Added `MigrateLegacyExtrafanarts_Movie`, `MigrateLegacyExtrafanarts_TVShow`; Updated `SaveMovieExtrafanarts` and `SaveTVShowExtrafanarts` to pre-load images to memory before cleanup; Removed `Clear()` from individual save methods; Added `CleanupNumberedFanarts` helper |
| `clsAPIFileUtils.vb` | Updated `GetFilenameList.Movie` and `GetFilenameList.TVShow` to return media folder path for `MainExtrafanarts` |
| `clsAPIScanner.vb` | Updated `GetFolderContents_Movie` and `GetFolderContents_TVShow` to scan for Kodi-compliant pattern and call migration before scanning |
| `clsAPIDatabase.vb` | Updated `Load_Movie` and `Load_TVShow` to filter extrafanarts with regex pattern |
| `clsAPIMediaContainers.vb` | Fixed `LoadAndCache` to properly load from MemoryStream when LocalFilePath is cleared |
| `frmMain.vb` | Removed duplicate `SaveAllImages` calls from `cmnuMovieEditImages_Click` and `cmnuShowEditImages_Click` handlers |

---

## [↑](#table-of-contents) Migration Behavior

### [↑](#table-of-contents) When Migration Happens

Migration occurs automatically during **library scan** (`Update Library`), not when opening the Edit Images dialog. This ensures:
- Migration happens before images are loaded into the database
- Migrated files are immediately available for scanning
- No user action required

### [↑](#table-of-contents) Migration Flow

**In `clsAPIScanner.vb` → `GetFolderContents_Movie`:**

1. **Before scanning for extrafanarts**, call `Images.MigrateLegacyExtrafanarts_Movie(mediaFolder, fileNameStack)`
2. Migration method checks for `extrafanart/` subfolder
3. If exists:
   - Move all `.jpg` and `.png` files to main folder with new naming
   - Find next available fanart index (in case some new-style files already exist)
   - Delete the legacy `extrafanart/` folder (recursive delete)
4. Scanner then picks up the newly migrated files

**Same pattern for TV Shows in `GetFolderContents_TVShow`.**

### [↑](#table-of-contents) Migration Methods

| Method | Purpose | Called By |
|--------|---------|-----------|
| `MigrateLegacyExtrafanarts_Movie` | Migrate movie `extrafanart/` folders during scan | `GetFolderContents_Movie` |
| `MigrateLegacyExtrafanarts_TVShow` | Migrate TV show `extrafanart/` folders during scan | `GetFolderContents_TVShow` |

### [↑](#table-of-contents) Key Implementation

From `clsAPIImages.vb`:

    Public Shared Sub MigrateLegacyExtrafanarts_Movie(ByVal mediaFolder As String, ByVal fileNameBase As String)
        Dim legacyFolder As String = Path.Combine(mediaFolder, "extrafanart")
        If Not Directory.Exists(legacyFolder) Then Return

        ' Get all image files from legacy folder
        Dim imageFiles As New List(Of String)
        imageFiles.AddRange(Directory.GetFiles(legacyFolder, "*.jpg"))
        imageFiles.AddRange(Directory.GetFiles(legacyFolder, "*.png"))

        ' Find next available fanart index
        Dim maxIndex As Integer = 0
        ' ... check for existing [filename]-fanart[N] files ...

        ' Move files with new naming
        For Each sourceFile In imageFiles
            Dim newFileName As String = String.Format("{0}-fanart{1}{2}", fileNameBase, fanartIndex, extension)
            File.Move(sourceFile, Path.Combine(mediaFolder, newFileName))
        Next

        ' Delete legacy folder (recursive)
        Directory.Delete(legacyFolder, True)
    End Sub

---

## [↑](#table-of-contents) Benefits

- ✅ **Kodi compliance** — Aligns with current Kodi naming standard
- ✅ **Simpler folder structure** — No subfolder clutter
- ✅ **Predictable naming** — Easy to identify fanart order
- ✅ **User-friendly** — Manual file management is straightforward
- ✅ **Future-proof** — Ready for when Kodi removes subfolder support
- ✅ **Automatic migration** — Legacy folders converted seamlessly during scan

---

## [↑](#table-of-contents) Testing Checklist

### [↑](#table-of-contents) Movies

- [x] Save extrafanarts with new naming convention (`[filename]-fanart1.jpg`)
- [x] Verify order matches selection order in dialog
- [x] Re-scrape with fewer extrafanarts — old files cleaned up
- [x] Re-scrape with more extrafanarts — new files added correctly
- [x] Primary `[filename]-fanart.jpg` remains unchanged
- [x] Special characters in filename (e.g., `Movie [2024]`) handled correctly
- [x] Legacy `extrafanart/` folder migrated during library scan
- [x] Legacy folder deleted after migration (including non-image files)

### [↑](#table-of-contents) TV Shows

- [x] Save show-level extrafanarts with new naming (`fanart1.jpg`)
- [x] Verify naming in show folder
- [x] Legacy `extrafanart/` folder migrated during library scan
- [x] Legacy folder deleted after migration

### [↑](#table-of-contents) Edge Cases

- [x] No extrafanarts selected — no numbered fanart files created
- [x] Only primary fanart — no numbered fanart files created
- [x] Existing new-style files — migration appends with next available index

### [↑](#table-of-contents) Database Loading

- [x] Edit movie loads only correct extrafanarts (not banner, poster, etc.)
- [x] Edit TV show loads only correct extrafanarts

---

## [↑](#table-of-contents) Implementation Details

### [↑](#table-of-contents) Save Flow (Movies)

1. **Pre-load all images into memory** — Before any file operations
2. **Clear LocalFilePath** — Prevent LoadAndCache from trying to reload from files that will be deleted
3. **Cleanup existing numbered fanarts** — `CleanupNumberedFanarts(mediaFolder, fileNameStack)`
4. **Save all extrafanarts** — Using sequential numbering (`-fanart1.jpg`, `-fanart2.jpg`, etc.)
5. **Clear image memory** — After all images saved

### [↑](#table-of-contents) Save Flow (TV Shows)

Same as movies, but with simple naming (`fanart1.jpg`, `fanart2.jpg`, etc.)

---

## [↑](#table-of-contents) Bugs Fixed

### [↑](#table-of-contents) Bug #1: Wrong Images Loaded in Edit Dialog

When `ExtrafanartsPath` changed from pointing to `extrafanart/` subfolder to pointing to the **main media folder**, the database loader (`Load_Movie`, `Load_TVShow`) was grabbing ALL `.jpg` files instead of just the numbered fanarts.

**Root Cause:** The old code used:

    For Each ePath As String In Directory.GetFiles(_movieDB.ExtrafanartsPath, "*.jpg")

**Fix:** Added regex filtering to match only Kodi-compliant patterns:

    Dim numberedFanartPattern As New Regex("^" & Regex.Escape(fileNameStack) & "-fanart[0-9]+\.(jpg|png)$", RegexOptions.IgnoreCase)
    For Each filePath In Directory.GetFiles(_movieDB.ExtrafanartsPath, "*.jpg")
        If numberedFanartPattern.IsMatch(Path.GetFileName(filePath)) Then
            efList.Add(filePath)
        End If
    Next

### [↑](#table-of-contents) Bug #2: Images Deleted Before Save (Memory Issue)

**Symptom:** When editing extrafanarts, the save operation would fail because images were being deleted by `CleanupNumberedFanarts` before they could be saved.

**Root Cause:** The cleanup step deleted files that `LocalFilePath` pointed to. When `LoadAndCache` was called later, it couldn't find the files.

**Fix in `clsAPIImages.vb` (`SaveMovieExtrafanarts` and `SaveTVShowExtrafanarts`):**

1. **Pre-load ALL images into memory BEFORE any file operations**
2. **Clear `LocalFilePath`** after loading to prevent `LoadAndCache` from trying to reload from deleted files
3. **Move `Clear()` calls** from individual save methods to batch cleanup after all images saved

### [↑](#table-of-contents) Bug #3: Duplicate SaveAllImages Calls

**Symptom:** Extrafanarts were being saved twice, causing issues with the second save (files already cleaned up).

**Root Cause:** `frmMain.vb` handlers (`cmnuMovieEditImages_Click`, `cmnuShowEditImages_Click`) were calling `SaveAllImages` explicitly after `Save_Movie`/`Save_TVShow`, but those save methods already call `SaveAllImages` internally when `toDisk=True`.

**Fix:** Removed the duplicate `SaveAllImages` calls from the handlers.

### [↑](#table-of-contents) Bug #4: Legacy Folder Not Deleted

**Symptom:** After migration, the `extrafanart/` folder remained if it contained non-image files (like `Thumbs.db`).

**Root Cause:** Original code only deleted the folder if it was completely empty.

**Fix:** Changed to recursive delete (`Directory.Delete(legacyFolder, True)`) to remove the folder and any remaining files.

---

*End of file*