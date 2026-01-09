# BL-DR-001: Remove Theme Scraper Code

| Field | Value |
|-------|-------|
| **Version** | 1.0 |
| **Created** | January 8, 2026 |
| **Updated** | January 8, 2026 |
| **Author** | Development Team |
| **Purpose** | Document full removal of Theme scraper functionality |
| **Status** | 📋 Backlog |
| **Priority** | Low |
| **Effort** | Medium |

##### [← Return to Document Index](../DocumentIndex.md)

---

## [↑](#table-of-contents) Table of Contents

- [Overview](#overview)
- [Quick Fix Applied](#quick-fix-applied)
- [Full Removal Scope](#full-removal-scope)
- [Files Requiring Changes](#files-requiring-changes)
- [Implementation Notes](#implementation-notes)

---

## [↑](#table-of-contents) Overview

The Theme scraper functionality (for downloading theme music from sources like TelevisionTunes and YouTube) has been removed from the solution. However, significant code infrastructure remains that should be cleaned up.

**Current State:** Settings panels hidden via quick fix (commented out in `dlgSettings.vb`)

**Goal:** Complete removal of all Theme scraper infrastructure code

---

## [↑](#table-of-contents) Quick Fix Applied

On January 8, 2026, a quick fix was applied to hide the Theme scraper panels from the Settings dialog:

**File:** `EmberMediaManager\dlgSettings.vb` in `AddPanels()` method

- Commented out `pnlMovieTheme` panel registration (~line 257-262)
- Commented out `pnlTVTheme` panel registration (~line 295-300)
- Added TODO comments referencing this BL document

---

## [↑](#table-of-contents) Full Removal Scope

### API Layer (`EmberAPI`)

| File | Items to Remove |
|------|-----------------|
| `clsAPIModules.vb` | `externalScrapersModules_Theme_Movie` list |
| | `externalScrapersModules_Theme_TV` list |
| | `_externalScraperModuleClass_Theme_Movie` class |
| | `_externalScraperModuleClass_Theme_TV` class |
| | `ScrapeTheme_Movie()` method |
| | `ScrapeTheme_TVShow()` method |
| `clsAPIInterfaces.vb` | `ScraperModule_Theme_Movie` interface |
| | `ScraperModule_Theme_TV` interface |

### Settings Dialog (`EmberMediaManager`)

| File | Items to Remove |
|------|-----------------|
| `dlgSettings.vb` | Theme panel registrations (already commented) |
| | `AddScraperPanels()` - Theme scraper loops |
| | `RemoveScraperPanels()` - Theme scraper loops |
| `dlgSettings.Designer.vb` | `pnlMovieThemes` panel and controls |
| | `pnlTVThemes` panel and controls |

### Documentation

| File | Action |
|------|--------|
| `ScrapingProcessMovies.md` | Update Part 6 to remove Theme Scraping section |
| `ScrapingProcessTvShows.md` | Update to remove Theme Scraping references |

---

## [↑](#table-of-contents) Files Requiring Changes

### Primary Files (Code Removal)

1. **`EmberAPI\clsAPIModules.vb`**
   - Remove Theme scraper module lists and classes
   - Remove `ScrapeTheme_Movie()` and `ScrapeTheme_TVShow()` methods
   - Search for: `Theme_Movie`, `Theme_TV`, `ScrapeTheme`

2. **`EmberAPI\clsAPIInterfaces.vb`**
   - Remove `ScraperModule_Theme_Movie` interface
   - Remove `ScraperModule_Theme_TV` interface

3. **`EmberMediaManager\dlgSettings.vb`**
   - Remove commented panel registrations
   - Remove Theme scraper loops in `AddScraperPanels()`
   - Remove Theme scraper loops in `RemoveScraperPanels()`

4. **`EmberMediaManager\dlgSettings.Designer.vb`**
   - Remove `pnlMovieThemes` panel and all child controls
   - Remove `pnlTVThemes` panel and all child controls

### Secondary Files (May Have References)

- `dlgEdit_Movie.vb` - Check for Theme-related code
- `dlgEdit_TVShow.vb` - Check for Theme-related code
- Various scraper addon files - May have Theme scraper implementations

### Documentation Files

- `EmberMediaManager\docs\process-docs\ScrapingProcessMovies.md`
- `EmberMediaManager\docs\process-docs\ScrapingProcessTvShows.md`

---

## [↑](#table-of-contents) Implementation Notes

### Recommended Approach

1. **Phase 1:** Remove API infrastructure
   - Delete interfaces from `clsAPIInterfaces.vb`
   - Delete classes and methods from `clsAPIModules.vb`

2. **Phase 2:** Clean up Settings dialog
   - Delete Designer controls
   - Remove code from `dlgSettings.vb`

3. **Phase 3:** Search for remaining references
   - Global search for `Theme_Movie`, `Theme_TV`, `ScrapeTheme`
   - Review and remove any orphaned code

4. **Phase 4:** Update documentation
   - Remove Theme scraping sections from process docs

### Testing Considerations

- Verify Settings dialog opens without errors
- Verify Movie and TV Show scraping still works
- Verify no runtime errors from missing Theme modules

---

*End of file*