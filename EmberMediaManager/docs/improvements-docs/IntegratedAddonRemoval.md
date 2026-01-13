# Integrated Addon Removal Plan

| Document Info | |
|---------------|---|
| **Version** | 1.0 |
| **Created** | December 31, 2025 |
| **Author** | ulrick65 |
| **Status** | 📋 Planning |
| **Branch** | TBD (new branch required) |

---

## Table of Contents

- [Overview](#overview)
- [Removal Packages](#removal-packages)
- [Package 1: NMT and Boxee Removal](#package-1-nmt-and-boxee-removal)
- [Package 2: Kodi Addons Tab Removal](#package-2-kodi-addons-tab-removal)
- [Implementation Strategy](#implementation-strategy)
- [Risk Assessment](#risk-assessment)

---

## Overview

### Purpose

Remove legacy integrated features that are no longer functional or relevant:

1. **NMT (Networked Media Tank)** - Defunct media player platform
2. **Boxee** - Discontinued in 2015
3. **Kodi Addons** - Legacy file naming scheme no longer needed with modern Kodi

### Key Difference from Addon Removal

Unlike the scraper addons (which were self-contained DLLs), these features are **deeply integrated** into the core codebase:

| Aspect | Scraper Addons | Integrated Features |
|--------|----------------|---------------------|
| Location | Separate projects | Core EmberAPI/EmberMediaManager |
| Removal Method | Delete folder + solution reference | Code surgery across multiple files |
| Risk Level | 🟢 Very Low | 🟡 Medium |
| Rollback | Easy (restore files) | Complex (code changes) |

### Recommended Approach

Create a **new feature branch** for this work to isolate changes and allow easy rollback if issues arise.

---

## Removal Packages

### Package Summary

| Package | Components | Complexity | Can Be Done Separately |
|---------|------------|------------|------------------------|
| **Package 1** | NMT + Boxee | 🟡 Medium-High | ❌ No - Do together |
| **Package 2** | Kodi Addons | 🟢 Low-Medium | ✅ Yes |

---

## Package 1: NMT and Boxee Removal

### Components to Remove

#### NMT (Networked Media Tank) Settings

| Property | File |
|----------|------|
| `MovieUseNMJ` | clsAPISettings.vb |
| `MovieBannerNMJ` | clsAPISettings.vb |
| `MovieFanartNMJ` | clsAPISettings.vb |
| `MovieNFONMJ` | clsAPISettings.vb |
| `MoviePosterNMJ` | clsAPISettings.vb |
| `MovieTrailerNMJ` | clsAPISettings.vb |
| `MovieYAMJWatchedFile` | clsAPISettings.vb |
| `MovieYAMJWatchedFolder` | clsAPISettings.vb |

#### Boxee Settings - Movies

| Property | File |
|----------|------|
| `MovieUseBoxee` | clsAPISettings.vb |
| `MovieFanartBoxee` | clsAPISettings.vb |
| `MovieNFOBoxee` | clsAPISettings.vb |
| `MoviePosterBoxee` | clsAPISettings.vb |

#### Boxee Settings - TV

| Property | File |
|----------|------|
| `TVUseBoxee` | clsAPISettings.vb |
| `TVEpisodeNFOBoxee` | clsAPISettings.vb |
| `TVEpisodePosterBoxee` | clsAPISettings.vb |
| `TVSeasonPosterBoxee` | clsAPISettings.vb |
| `TVShowBannerBoxee` | clsAPISettings.vb |
| `TVShowFanartBoxee` | clsAPISettings.vb |
| `TVShowNFOBoxee` | clsAPISettings.vb |
| `TVShowPosterBoxee` | clsAPISettings.vb |

### Files Requiring Changes

#### EmberAPI

| File | Changes Required |
|------|------------------|
| `clsAPISettings.vb` | Remove ~20 properties |
| `clsAPIFileUtils.vb` | Remove NMT/Boxee file naming logic |
| `clsAPINFO.vb` | Remove Boxee-specific NFO handling |
| `clsAPIMediaContainers.vb` | Remove BoxeeTvDb properties |

#### EmberMediaManager

| File | Changes Required |
|------|------------------|
| `dlgSettings.vb` | Remove tab event handlers and settings load/save |
| `dlgSettings.Designer.vb` | Remove NMT and Boxee TabPage controls |
| `dlgSettings.resx` | Remove associated resources |

#### Potentially Affected

| File | Needs Review |
|------|--------------|
| `dlgSourceMovie.vb` | File naming references |
| `dlgSourceTVShow.vb` | File naming references |
| Various NFO-related files | Boxee ID handling |

### UI Elements to Remove

#### Movie Settings - Files and Sources

| Tab | Status |
|-----|--------|
| Kodi | ✅ Keep |
| Kodi Addons | See Package 2 |
| **NMT** | ❌ Remove |
| **Boxee** | ❌ Remove |
| Expert | ✅ Keep |

#### TV Settings - Files and Sources

| Tab | Status |
|-----|--------|
| Kodi | ✅ Keep |
| **Boxee** | ❌ Remove |
| Expert | ✅ Keep |

### Estimated Effort

| Task | Time |
|------|------|
| Settings properties removal | 30 min |
| File naming logic cleanup | 1-2 hours |
| NFO logic cleanup | 30 min |
| UI removal (Designer) | 30 min |
| Testing | 1 hour |
| **Total** | **3-4 hours** |

---

## Package 2: Kodi Addons Tab Removal

### Background

The "Kodi Addons" tab provided alternative file naming for older Kodi addon compatibility. Modern Kodi versions no longer require this separate naming scheme.

### Components to Remove

#### Settings Properties

| Property | File |
|----------|------|
| `MovieUseAD` | clsAPISettings.vb |
| `MovieBannerAD` | clsAPISettings.vb |
| `MovieFanartAD` | clsAPISettings.vb |
| `MovieNFOAD` | clsAPISettings.vb |
| `MoviePosterAD` | clsAPISettings.vb |
| `MovieTrailerAD` | clsAPISettings.vb |
| (Additional AD properties) | clsAPISettings.vb |

*Note: Need to verify exact property names - may be "AD" suffix or similar*

### Files Requiring Changes

| File | Changes Required |
|------|------------------|
| `clsAPISettings.vb` | Remove Kodi Addons properties |
| `clsAPIFileUtils.vb` | Remove Kodi Addons file naming logic |
| `dlgSettings.vb` | Remove tab handlers |
| `dlgSettings.Designer.vb` | Remove Kodi Addons TabPage |

### UI Elements to Remove

#### Movie Settings - Files and Sources

| Tab | Status |
|-----|--------|
| Kodi | ✅ Keep |
| **Kodi Addons** | ❌ Remove |
| NMT | See Package 1 |
| Boxee | See Package 1 |
| Expert | ✅ Keep |

### Estimated Effort

| Task | Time |
|------|------|
| Settings properties removal | 15 min |
| File naming logic cleanup | 30-45 min |
| UI removal | 15 min |
| Testing | 30 min |
| **Total** | **1.5-2 hours** |

---

## Implementation Strategy

### Recommended Order

1. **Create new branch** from `feature/performance-improvements-phase4` or `develop`
2. **Package 1 first** (NMT + Boxee) - larger, more complex
3. **Package 2 second** (Kodi Addons) - can be separate commit or separate PR
4. **Thorough testing** after each package

### Branch Naming Suggestion

    feature/remove-legacy-file-naming

or

    feature/remove-nmt-boxee-kodiaddons

### Implementation Steps

#### Step 1: Preparation

- [ ] Create new branch
- [ ] Backup current working state
- [ ] Document current file naming behavior for regression testing

#### Step 2: Package 1 - NMT/Boxee

- [ ] Remove UI tabs from dlgSettings.Designer.vb
- [ ] Remove event handlers from dlgSettings.vb
- [ ] Remove settings properties from clsAPISettings.vb
- [ ] Remove file naming logic from clsAPIFileUtils.vb
- [ ] Remove NFO logic from clsAPINFO.vb
- [ ] Remove container properties from clsAPIMediaContainers.vb
- [ ] Build and fix any remaining references
- [ ] Test Movie file naming
- [ ] Test TV file naming
- [ ] Commit

#### Step 3: Package 2 - Kodi Addons (Optional/Separate)

- [ ] Remove UI tab from dlgSettings.Designer.vb
- [ ] Remove event handlers from dlgSettings.vb
- [ ] Remove settings properties from clsAPISettings.vb
- [ ] Remove file naming logic from clsAPIFileUtils.vb
- [ ] Build and fix any remaining references
- [ ] Test Movie file naming
- [ ] Commit

#### Step 4: Final Verification

- [ ] Full solution build
- [ ] Application startup test
- [ ] Settings dialog opens correctly
- [ ] Movie scraping with file saving
- [ ] TV scraping with file saving
- [ ] No orphaned settings in XML

---

## Risk Assessment

### Package 1 (NMT/Boxee)

| Risk | Level | Mitigation |
|------|-------|------------|
| Breaking Kodi file naming | 🟡 Medium | Careful code review, don't touch Frodo/Eden settings |
| Missing references | 🟡 Medium | Build errors will reveal; grep for property names |
| Settings file corruption | 🟢 Low | Old settings simply ignored |
| Regression in Expert mode | 🟢 Low | Expert mode is separate code path |

### Package 2 (Kodi Addons)

| Risk | Level | Mitigation |
|------|-------|------------|
| Breaking Kodi file naming | 🟢 Low | Main Kodi tab unaffected |
| Missing references | 🟢 Low | Smaller scope |
| User disruption | 🟢 Low | Few users likely using this |

---

## Testing Checklist

### Pre-Implementation Baseline

- [ ] Document current behavior for each file type
- [ ] Screenshot current Settings tabs
- [ ] Note any current NMT/Boxee/Kodi Addons enabled settings

### Post-Implementation

#### Build Verification

- [ ] Solution builds without errors
- [ ] Solution builds without warnings (related to removed code)

#### UI Verification

- [ ] Settings dialog opens
- [ ] Movie Sources panel loads
- [ ] TV Sources panel loads
- [ ] File Naming tabs show only: Kodi, Expert (Movies) / Kodi, Expert (TV)
- [ ] No visual artifacts or layout issues

#### Functional Verification

- [ ] Add new movie source
- [ ] Scrape movie - verify NFO created correctly
- [ ] Scrape movie - verify poster saved correctly
- [ ] Scrape movie - verify fanart saved correctly
- [ ] Add new TV source
- [ ] Scrape TV show - verify NFO created correctly
- [ ] Scrape TV show - verify images saved correctly

#### Regression Verification

- [ ] Expert mode still works for movies
- [ ] Expert mode still works for TV
- [ ] Kodi naming conventions unchanged
- [ ] Existing library still recognized

---

## Notes

### Why Remove Together (NMT + Boxee)

1. **Similar code patterns** - both use same file naming logic structure
2. **Same files affected** - changes overlap significantly
3. **Testing efficiency** - test file naming once for both removals
4. **Cleaner commits** - single "remove legacy platforms" commit

### Why Kodi Addons Can Be Separate

1. **Smaller scope** - fewer properties and code paths
2. **Independent functionality** - doesn't share logic with NMT/Boxee
3. **Lower risk** - less intertwined with core functionality
4. **May want to keep** - some users might still need it (evaluate first)

### Historical Context

- **Boxee**: Discontinued 2015, company acquired by Samsung
- **NMT (Networked Media Tank)**: Popcorn Hour devices, largely obsolete
- **Kodi Addons**: Legacy naming for pre-Frodo addon compatibility

---

## Decision Points

Before implementation, decide:

1. **Branch strategy**: New branch from develop or from current feature branch?
2. **Kodi Addons**: Include in this removal or evaluate separately?
3. **Timeline**: Do immediately or defer to later phase?

---

*Document Version: 1.0*
*Created: December 31, 2025*
*Author: ulrick65*
*Status: 📋 Planning*