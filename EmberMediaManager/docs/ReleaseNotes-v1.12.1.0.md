# Ember Media Manager v1.12.1.0

**Release Date:** December 25, 2025  
**Previous Version:** 1.12.0.0  
**Target Framework:** .NET Framework 4.8

---

## 📋 Summary

This release focuses on bug fixes, version standardization across all addons, and comprehensive documentation improvements.

---

## 🔧 What's Changed

### Bug Fixes

| Fix | Description | Files |
|-----|-------------|-------|
| Genre Mapping | Fixed genre mapping functionality that was not working correctly | `clsXMLGenreMapping.vb` |
| Database Genres | Fixed database genre handling issues | `clsAPIDatabase.vb` |
| XML Processing | Improved XML processing reliability | `clsAPIXML.vb` |

### Code Cleanup
- Removed unused YouTube-related code from `clsAPIYouTube.vb`

### Version Standardization

All addons have been updated to version 1.12.0.0 to reflect the .NET 4.8 framework upgrade:

| Category | Projects Updated |
|----------|------------------|
| Generic Addons | 10 projects |
| Data Scrapers | 7 projects |
| Image Scrapers | 3 projects |
| Trailer Scrapers | 5 projects |
| Theme Scrapers | 2 projects |
| **Total** | **27 projects** |

Additionally:
- KodiAPI updated to 1.10.1.0

### Core Application Versions

| Project | Version |
|---------|---------|
| EmberMediaManager | 1.12.1.0 |
| EmberAPI | 1.12.1.0 |
| generic.Interface.Kodi | 1.11.1.0 |
| scraper.IMDB.Data | 1.11.2.7 |
| All other addons | 1.12.0.0 |

### New Documentation

| Document | Description |
|----------|-------------|
| ForkChangeLog.md | Complete fork history and version tracking |
| BuildProcess.md | Build configuration documentation |
| FileCleanUpPlan.md | Solution cleanup tracking |
| PackageUpdatePlan.md | NuGet package update tracking |
| NfoFileProcess.md | NFO file processing analysis |
| NfoFileImprovements.md | Planned NFO improvements |
| GenreMappingProcess.md | Genre mapping documentation |
| SolutionCleanupAnalysis.md | Solution cleanup analysis |
| + 5 other mapping docs | Various mapping process documentation |

### New Build Tools

| Script | Purpose |
|--------|---------|
| Update-AssemblyVersions.ps1 | Automated version management (run with -Help) |
| VersionConfig.json | Version configuration for all projects |
| BuildCleanup.ps1 | Build artifact cleanup |
| Directory.Build.targets | Centralized build configuration |

### Configuration
- Created `.github/copilot-instructions.md` for AI-assisted development standards

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

**Upgrading from v1.12.0.0:** Simply replace all files. No database changes required.

---

## 📝 Notes

- This release establishes a version baseline for all addons at 1.12.0.0
- Future addon changes will increment from this baseline
- See [ForkChangeLog.md](ForkChangeLog.md) for complete version history

---

**Full Changelog**: See [ForkChangeLog.md](ForkChangeLog.md) for complete details