# BL-CC-002: Kodi-Compliant Fanart Naming

| Field | Value |
|-------|-------|
| **ID** | BL-CC-002 |
| **Category** | Code Cleanup (CC) |
| **Priority** | Medium |
| **Effort** | 4-6 hrs |
| **Created** | January 7, 2026 |
| **Status** | Open |
| **Related Files** | `clsAPIImages.vb`, `clsAPIFileUtils.vb` |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Overview

Kodi is deprecating the `extrafanart/` subfolder approach for additional fanart images. This item updates Ember to use Kodi's current standard: sequential fanart naming (`fanart1.jpg`, `fanart2.jpg`, etc.) in the main media folder.

---

## Table of Contents

- [Problem Description](#problem-description)
- [Current Behavior](#current-behavior)
- [Proposed Solution](#proposed-solution)
  - [File Naming Convention](#file-naming-convention)
  - [Order Preservation](#order-preservation)
  - [Cleanup Logic](#cleanup-logic)
- [Implementation Steps](#implementation-steps)
- [Affected Files](#affected-files)
- [Benefits](#benefits)
- [Risks](#risks)
- [Testing Checklist](#testing-checklist)
- [Source Reference](#source-reference)

---

## [↑](#table-of-contents) Problem Description

Kodi is deprecating the `extrafanart/` subfolder method for storing additional fanart images. The current Ember implementation:

- Saves extrafanarts to a separate `extrafanart/` subfolder
- Uses random numeric filenames (e.g., `12345678.jpg`)
- Creates unnecessary folder complexity
- Does not align with Kodi's current naming standard

---

## [↑](#table-of-contents) Current Behavior

Extrafanarts are saved to a subfolder structure with random names:

    Movie Folder/
    ├── movie.nfo
    ├── fanart.jpg              (primary fanart)
    ├── poster.jpg
    └── extrafanart/            (subfolder - being deprecated)
        ├── 12345678.jpg        (random names)
        ├── 87654321.jpg
        └── ...

---

## [↑](#table-of-contents) Proposed Solution

Save all fanart images in the main folder with sequential naming per Kodi standard:

    Movie Folder/
    ├── movie.nfo
    ├── fanart.jpg              (primary fanart - unchanged)
    ├── fanart1.jpg             (first extrafanart)
    ├── fanart2.jpg             (second extrafanart)
    ├── fanart3.jpg             (third extrafanart)
    ├── poster.jpg
    └── ...

### [↑](#table-of-contents) File Naming Convention

| Image Type | Current Name | New Name |
|------------|--------------|----------|
| Primary Fanart | `fanart.jpg` | `fanart.jpg` (unchanged) |
| 1st Extrafanart | `extrafanart/random.jpg` | `fanart1.jpg` |
| 2nd Extrafanart | `extrafanart/random.jpg` | `fanart2.jpg` |
| Nth Extrafanart | `extrafanart/random.jpg` | `fanartN.jpg` |

**Notes:**
- Primary fanart remains `fanart.jpg` (or `fanart.png`)
- Extrafanarts are numbered starting from 1
- File extension is preserved from original image
- Order matches user selection order in image dialog

---

### [↑](#table-of-contents) Order Preservation

The order of extrafanarts must be preserved from user selection:

1. User selects images in `dlgImgSelect`
2. Images are stored in `ImagesContainer.Extrafanarts` list
3. List order determines file numbering (index 0 → `fanart1.jpg`, etc.)
4. Save logic iterates list in order, assigning sequential numbers

Pseudo-code:

    For i As Integer = 0 To Extrafanarts.Count - 1
        Dim filename As String = String.Format("fanart{0}.{1}", i + 1, extension)
        SaveImage(Extrafanarts(i), Path.Combine(mediaFolder, filename))
    Next

---

### [↑](#table-of-contents) Cleanup Logic

When saving, existing numbered fanarts must be cleaned up to prevent orphaned files:

1. Before saving new extrafanarts, delete existing `fanart#.*` files
2. Use pattern matching: `fanart[0-9]+\.(jpg|png)`
3. Preserve primary `fanart.jpg` (no number)

Cleanup pseudo-code:

    ' Find and delete existing numbered fanarts
    Dim existingFiles = Directory.GetFiles(mediaFolder, "fanart*.jpg")
                        .Concat(Directory.GetFiles(mediaFolder, "fanart*.png"))
    
    For Each file In existingFiles
        Dim filename = Path.GetFileNameWithoutExtension(file)
        ' Only delete numbered fanarts, not the primary fanart
        If Regex.IsMatch(filename, "^fanart[0-9]+$") Then
            File.Delete(file)
        End If
    Next

---

## [↑](#table-of-contents) Implementation Steps

1. **Identify save methods** — Locate where extrafanarts are currently saved
2. **Update path generation** — Modify `FileUtils` to generate sequential names
3. **Add cleanup logic** — Remove old numbered fanarts before saving new ones
4. **Update save logic** — Iterate extrafanarts list in order with sequential numbering
5. **Test with Movies** — Verify correct naming and cleanup
6. **Test with TV Shows** — Verify show-level extrafanarts work correctly
7. **Test re-scraping** — Ensure old files are cleaned up properly

---

## [↑](#table-of-contents) Affected Files

| File | Changes Needed |
|------|----------------|
| [clsAPIImages.vb](../../../EmberAPI/clsAPIImages.vb) | Modify extrafanart save logic |
| [clsAPIFileUtils.vb](../../../EmberAPI/clsAPIFileUtils.vb) | Update path generation for extrafanarts |
| [dlgImgSelect.vb](../../../EmberMediaManager/dlgImgSelect.vb) | Ensure order is preserved in result |

---

## [↑](#table-of-contents) Benefits

- **Kodi compliance** — Aligns with current Kodi naming standard
- **Simpler folder structure** — No subfolder clutter
- **Predictable naming** — Easy to identify fanart order
- **User-friendly** — Manual file management is straightforward
- **Future-proof** — Ready for when Kodi removes subfolder support

---

## [↑](#table-of-contents) Risks

| Risk | Mitigation |
|------|------------|
| Existing libraries have subfolder structure | Old `extrafanart/` folders remain untouched; migration is optional |
| Filename collisions | Cleanup logic removes old numbered files before saving |
| Kodi version compatibility | Verify support in target Kodi versions |

---

## [↑](#table-of-contents) Testing Checklist

### Movies

- [ ] Save extrafanarts with new naming convention
- [ ] Verify order matches selection order in dialog
- [ ] Re-scrape with fewer extrafanarts — old files cleaned up
- [ ] Re-scrape with more extrafanarts — new files added correctly
- [ ] Primary `fanart.jpg` remains unchanged
- [ ] Mix of JPG and PNG extrafanarts handled correctly

### TV Shows

- [ ] Save show-level extrafanarts with new naming
- [ ] Verify naming in show folder

### Edge Cases

- [ ] No extrafanarts selected — no `fanart#` files created
- [ ] Only primary fanart — no `fanart#` files created
- [ ] Special characters in folder path
- [ ] Read-only folder handling

### Kodi Verification

- [ ] Test with Kodi — verify all fanarts display
- [ ] Verify Kodi respects fanart order

---

## [↑](#table-of-contents) Source Reference

Feature request from Eric — January 7, 2026

Kodi is deprecating the `extrafanart/` subfolder approach in favor of sequential naming in the main folder.

---

*Created: January 7, 2026**