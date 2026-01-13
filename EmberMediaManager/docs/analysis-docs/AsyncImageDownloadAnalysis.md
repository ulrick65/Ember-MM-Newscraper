# Async Image Download Regression Analysis

| Document Info | |
|---------------|---|
| **Version** | 1.0 |
| **Created** | December 29, 2025 |
| **Author** | ulrick65 |
| **Status** | Analysis Complete |
| **Reference** | [PerformanceImprovements-Phase1.md](PerformanceImprovements-Phase1.md) Item 5 |

---

## Executive Summary

The parallel image download implementation (Item 5) achieved a **59% improvement** in download time but introduced an **1,850% regression** in the "SaveToDisk" phase. Investigation reveals the regression is caused by **`LoadAndCache()` being called inside the SaveToDisk timing scope**, which triggers additional synchronous web downloads for images that weren't properly loaded during the parallel download phase.

---

## The Problem in Detail

### What We Expected

| Phase | Expected Time | Actual Time |
|-------|---------------|-------------|
| Parallel Download | ~335 ms | 335 ms ✅ |
| Save to Disk | ~46 ms | 896 ms ⚠️ |
| **Total** | ~381 ms | 1,231 ms |

### What's Actually Happening

The `SaveAllImagesAsync` method has three phases:

    Phase 1: Collect images that need downloading (NeedsDownload() check)
    Phase 2: Download images in parallel (DownloadImagesParallelAsync)
    Phase 3: Save images to disk (inside SaveToDisk timing scope)

**The issue:** Phase 3 calls `LoadAndCache()` for EVERY image type, and `LoadAndCache()` can trigger **synchronous web downloads** if the image isn't already in memory.

---

## Code Flow Analysis

### Phase 1: Image Collection (Lines 4730-4748)

    ' Phase 1: Collect all images that need downloading
    Dim imagesToDownload As New List(Of Image)

    If Banner.NeedsDownload() Then imagesToDownload.Add(Banner)
    If ClearArt.NeedsDownload() Then imagesToDownload.Add(ClearArt)
    ' ... etc for all image types

**`NeedsDownload()` returns `False` when:**
- Image already has `MemoryStream` data
- Image has existing `LocalFilePath` that exists on disk
- Image is in cache (`CacheOriginalPath` or `CacheThumbPath` exists)

### Phase 2: Parallel Download (Lines 4750-4761)

    If imagesToDownload.Count > 0 Then
        Using downloadScope = PerformanceTracker.StartOperation("...ParallelDownload")
            Await Images.DownloadImagesParallelAsync(
                imagesToDownload,
                tContentType,
                maxConcurrency:=5,
                loadBitmap:=False,    ' <-- KEY: Only loads MemoryStream, not Image
                cancellationToken:=Nothing,
                progressCallback:=Nothing
            ).ConfigureAwait(False)
        End Using
    End If

**What this does:**
- Calls `LoadAndCacheAsync()` on each image in parallel
- Downloads image bytes into `MemoryStream`
- Does **NOT** load the `Image` bitmap (to save memory)

### Phase 3: Save to Disk (Lines 4764-4850) - THE PROBLEM

    Using saveScope = PerformanceTracker.StartOperation("...SaveToDisk")
        'Movie Banner
        If Banner.LoadAndCache(tContentType, True) Then    ' <-- PROBLEM!
            If ForceFileCleanup Then Images.Delete_Movie(...)
            Banner.LocalFilePath = Banner.ImageOriginal.Save_Movie(...)
        Else
            Images.Delete_Movie(...)
            Banner = New Image
        End If
        ' ... repeated for ClearArt, ClearLogo, DiscArt, Fanart, etc.
    End Using

**The `LoadAndCache()` call inside the SaveToDisk scope:**

1. Checks if image is already loaded (memory stream or bitmap)
2. Checks if local file exists
3. Checks if cached file exists
4. **If none of the above: Downloads from web using synchronous `LoadFromWeb()`!**

---

## Why Images Get Re-Downloaded

### Scenario 1: NeedsDownload() vs LoadAndCache() Mismatch

**`NeedsDownload()` logic (simplified):**

    ' Returns False if ANY of these are true:
    If ImageOriginal.HasMemoryStream Then Return False
    If LocalFilePath exists Then Return False  
    If CacheOriginalPath exists Then Return False
    ' Otherwise, has URL to download:
    Return Not String.IsNullOrEmpty(URLOriginal)

**`LoadAndCache()` logic (simplified):**

    ' Tries to load from multiple sources:
    If File.Exists(LocalFilePath) Then LoadFromFile(...)
    ElseIf ImageOriginal.HasMemoryStream Then LoadFromMemoryStream()
    ElseIf File.Exists(CacheOriginalPath) Then LoadFromFile(...)
    ElseIf Not String.IsNullOrEmpty(URLOriginal) Then
        LoadFromWeb(URLOriginal)  ' <-- SYNCHRONOUS DOWNLOAD!
    End If

**The mismatch:** An image with `LocalFilePath` set but file doesn't exist yet will:
- Pass `NeedsDownload()` check (returns False because LocalFilePath is set)
- Skip parallel download phase
- Then `LoadAndCache()` tries to load it and finds no file → downloads from web

### Scenario 2: Images Not in Parallel Download List

Some images may not be in the `imagesToDownload` list:
- Images that already have `MemoryStream` from earlier operations
- Images that have existing local files
- Images in cache

But if the `MemoryStream` gets disposed or the cached file is stale, `LoadAndCache()` will re-download.

### Scenario 3: Extrafanarts/Extrathumbs Special Handling

    'Movie Extrafanarts
    If Extrafanarts.Count > 0 Then
        DBElement.ExtrafanartsPath = Images.SaveMovieExtrafanarts(DBElement)

`SaveMovieExtrafanarts()` internally calls `LoadAndCache()` for each extrafanart:

    For Each eImg In mMovie.ImagesContainer.Extrafanarts
        If eImg.LoadAndCache(mMovie.ContentType, True) Then  ' <-- Downloads!
            efPath = eImg.ImageOriginal.SaveAsMovieExtrafanart(mMovie)
        End If
    Next

---

## Timing Breakdown

What the `SaveToDisk` scope actually measures:

| Operation | Expected | Actually Included |
|-----------|----------|-------------------|
| File.Exists() checks | ✅ | ✅ |
| Directory.CreateDirectory() | ✅ | ✅ |
| Image.Save() to disk | ✅ | ✅ |
| LoadAndCache() calls | ❌ | ✅ ← Problem! |
| Synchronous web downloads | ❌ | ✅ ← Major Problem! |
| Image resizing (if enabled) | ❌ | ✅ |
| MemoryStream operations | ❌ | ✅ |

---

## Why Baseline Was Fast

In the baseline (synchronous `SaveAllImages`), there's no separate "SaveToDisk" scope. The timing was:

    ' Baseline: Each LoadAndCache is timed separately
    If Banner.LoadAndCache(tContentType, True) Then  ' Includes download in its own timing
        Banner.LocalFilePath = Banner.ImageOriginal.Save_Movie(...)  ' Just disk write
    End If

The download time was captured in `Image.LoadFromWeb` metrics, not in "DiskWrite".

In the async version, we moved the parallel downloads to Phase 2, but **forgot to remove `LoadAndCache()` from Phase 3**. Now Phase 3 captures both:
- Legitimate disk writes (~46ms)
- Fallback downloads for images that weren't properly loaded (~850ms)

---

## Solutions

### Option 1: Remove LoadAndCache() from Phase 3

**Before:**

    Using saveScope = PerformanceTracker.StartOperation("...SaveToDisk")
        If Banner.LoadAndCache(tContentType, True) Then
            Banner.LocalFilePath = Banner.ImageOriginal.Save_Movie(...)

**After:**

    Using saveScope = PerformanceTracker.StartOperation("...SaveToDisk")
        ' Images were already downloaded in Phase 2 - just check if we have data
        If Banner.ImageOriginal.HasMemoryStream Then
            Banner.LocalFilePath = Banner.ImageOriginal.Save_Movie(...)

**Pros:**
- Clean separation of download and save phases
- SaveToDisk scope measures only disk I/O
- Expected ~46ms performance

**Cons:**
- If Phase 2 failed to download, image will be skipped (no fallback)
- Need to handle edge cases where MemoryStream isn't populated

### Option 2: Move LoadAndCache() Before SaveToDisk Scope (Recommended)

**Add a Phase 2.5 for any images that need loading:**

    ' Phase 2.5: Ensure all images are loaded (fallback for any missed)
    Banner.LoadAndCache(tContentType, True)
    ClearArt.LoadAndCache(tContentType, True)
    ' ... etc

    ' Phase 3: Save images to disk (now truly just disk I/O)
    Using saveScope = PerformanceTracker.StartOperation("...SaveToDisk")
        If Banner.ImageOriginal.HasMemoryStream Then
            Banner.LocalFilePath = Banner.ImageOriginal.Save_Movie(...)

**Pros:**
- Maintains fallback behavior
- Clean separation of timing scopes
- More accurate metrics

**Cons:**
- Fallback downloads are still synchronous
- Slightly more code complexity

### Option 3: Fix NeedsDownload() to Catch All Cases

Ensure `NeedsDownload()` returns `True` for any image that will need downloading:

    Public Function NeedsDownload() As Boolean
        ' Already has data - no download needed
        If ImageOriginal.HasMemoryStream OrElse ImageThumb.HasMemoryStream Then
            Return False
        End If
        
        ' Has local file that EXISTS - no download needed
        If Not String.IsNullOrEmpty(LocalFilePath) AndAlso File.Exists(LocalFilePath) Then
            Return False
        End If
        
        ' Has cached file that EXISTS - no download needed
        If Not String.IsNullOrEmpty(CacheOriginalPath) AndAlso File.Exists(CacheOriginalPath) Then
            Return False
        End If
        
        ' Has URL - needs download
        Return Not String.IsNullOrEmpty(URLOriginal) OrElse Not String.IsNullOrEmpty(URLThumb)
    End Function

**Note:** The current implementation already has `File.Exists()` checks. The issue is likely:

1. Images with URLs but also LocalFilePath set (file doesn't exist yet)
2. Extrafanarts/extrathumbs not being added to the parallel download list

---

## Recommendation

**Option 2 (Move LoadAndCache before SaveToDisk scope)** is the safest approach because:

1. Maintains backward compatibility and fallback behavior
2. Provides accurate timing metrics
3. Minimal risk of breaking image saving
4. Easy to implement and test

---

## Test Results Reference

### Item 5 Metrics (49 movies scraped)

| Phase | Baseline (Sync) | Item 5 (Async) | Change |
|-------|-----------------|----------------|--------|
| **Parallel Download (avg ms)** | 815 | 335.18 | **-59%** ✅ |
| **Save to Disk (avg ms)** | 46 | 896.19 | +1,850% ⚠️ |
| **Total SaveAllImagesAsync (avg ms)** | 868 | 1,231.54 | +42% ⚠️ |

### Raw Metrics

| Operation | Count | AvgMs | TotalMs |
|-----------|-------|-------|---------|
| ImagesContainer.SaveAllImagesAsync | 49 | 1,231.54 | 60,345.34 |
| ImagesContainer.SaveAllImagesAsync.ParallelDownload | 49 | 335.18 | 16,423.61 |
| ImagesContainer.SaveAllImagesAsync.SaveToDisk | 49 | 896.19 | 43,913.48 |
| Image.LoadFromWebAsync | 312 | 107.99 | 33,692.32 |
| Database.Save_MovieAsync | 49 | 1,441.26 | 70,621.82 |

---

## Files Involved

| File | Location | Purpose |
|------|----------|---------|
| `clsAPIMediaContainers.vb` | `EmberAPI\` | Contains `SaveAllImagesAsync`, `LoadAndCache`, `NeedsDownload` |
| `clsAPIImages.vb` | `EmberAPI\` | Contains `DownloadImagesParallelAsync`, `LoadFromWeb`, `LoadFromWebAsync` |
| `clsAPIDatabase.vb` | `EmberAPI\` | Contains `Save_MovieAsync` |
| `frmMain.vb` | `EmberMediaManager\` | Bulk scraping integration point (line 1640) |