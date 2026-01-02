# Ember Media Manager v1.12.2.0

**Release Date:** January 2, 2026  
**Previous Version:** 1.12.1.0  
**Target Framework:** .NET Framework 4.8

---

## 📋 Summary

Version standardization release. All 24 projects aligned to version 1.12.2.0, configuration cleanup, build script enhancements, and image save fix.

---

## 🔧 What's Changed

### Bug Fixes

| Fix | Description | Files |
|-----|-------------|-------|
| Image Save | Reverted `SaveToFile` race condition fix that prevented image overwrites during editing | `clsAPIImages.vb` |

### Version Standardization

All 24 projects have been aligned to version 1.12.2.0:

| From Version | Count | Projects |
|--------------|:-----:|----------|
| 1.12.1.0 | 1 | EmberMediaManager |
| 1.12.1.1 | 1 | EmberAPI |
| 1.12.0.0 | 18 | All addons and scrapers at baseline |
| 1.11.2.7 | 1 | scraper.Data.IMDB |
| 1.11.1.0 | 2 | generic.Interface.Kodi, KodiAPI |

### Build Script Enhancements

| Enhancement | Description |
|-------------|-------------|
| `-Rebuild` parameter | BuildCleanup.ps1 now supports automatic solution rebuild after cleanup |
| `-Configuration` parameter | Specify Debug or Release build |
| `-Platform` parameter | Specify x86, x64, or Any CPU |
| VS-style summary | Build output now shows project counts and timing |
| VS Preview support | Added `vswhere.exe` support for VS Preview/Insiders MSBuild discovery |

### Configuration Cleanup

| Change | Description |
|--------|-------------|
| VersionConfig.json | Removed 8 deprecated/removed projects |
| VersionConfig.json | Removed `themeScrapers` category (all projects removed) |
| VersionConfig.json | Fixed project paths to match standardized folder names |
| UpdateAssemblyVersions.ps1 | Removed `ThemeScrapers` category |

### Script Versions

| Script | Version |
|--------|---------|
| BuildCleanup.ps1 | v1.2.1 |
| UpdateAssemblyVersions.ps1 | v2.2.0 |

---

## 📊 Fork Statistics (vs Upstream)

| Metric | Value |
|--------|-------|
| **Files Changed** | 408 |
| **Lines Added** | +17,256 |
| **Lines Removed** | -39,617 |
| **Net Change** | -22,361 (leaner codebase) |

### By File Type

| Category | Files | Insertions | Deletions | Net |
|----------|------:|----------:|----------:|----:|
| Code (.vb/.cs) | 227 | +5,820 | -34,918 | -29,098 |
| Documentation (.md) | 57 | +9,906 | 0 | +9,906 |

### What This Means

- **~29,000 lines of legacy code removed** — 10 deprecated/broken projects deleted
- **~10,000 lines of documentation added** — Comprehensive process docs, performance analysis, changelogs
- **Net reduction of ~22,000 lines** — Cleaner, more maintainable codebase
- **50-60% faster scraping** — Parallel processing for movies and TV shows
- **Framework modernized** — .NET 4.5 → 4.8 with updated packages

---

## 📦 Requirements

- **Windows**: 7 SP1, 8.1, 10, or 11
- **.NET Framework 4.8**: [Download from Microsoft](https://dotnet.microsoft.com/download/dotnet-framework/net48)

---

## 🔗 Links

- **Fork Repository**: [ulrick65/Ember-MM-Newscraper](https://github.com/ulrick65/Ember-MM-Newscraper)
- **Original Project**: [nagten/Ember-MM-Newscraper](https://github.com/nagten/Ember-MM-Newscraper)
- **Report Issues**: [GitHub Issues](https://github.com/ulrick65/Ember-MM-Newscraper/issues)

---

## 💾 Installation

1. Download the release package
2. Extract to your desired location
3. Ensure .NET Framework 4.8 is installed
4. Run `Ember Media Manager.exe`

**Upgrading from v1.12.1.0:** Simply replace all files. No database changes required.

---

## 📝 Notes

- This release establishes all 24 projects at version 1.12.2.0
- Future changes will increment from this unified baseline
- See [ForkChangeLog.md](../ForkChangeLog.md) for complete version history

---

**Full Changelog**: See [ForkChangeLog.md](../ForkChangeLog.md) for complete details