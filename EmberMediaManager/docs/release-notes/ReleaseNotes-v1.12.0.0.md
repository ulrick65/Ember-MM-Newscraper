# Ember Media Manager v1.12.0.0

**Release Date:** December 22, 2025  
**Previous Version:** 1.11.1.7  
**Target Framework:** .NET Framework 4.8

---

## 🎉 Major Update: .NET Framework 4.8 Migration

This release upgrades Ember Media Manager from .NET Framework 4.5 to 4.8, providing better performance, security, and compatibility with modern Windows systems.

---

## 🔧 What's Changed

### Framework Upgrade
- Upgraded all 28 projects from .NET Framework 4.5 to 4.8
- Updated all project files (.vbproj, .csproj) with new framework target

### Core Application Updates

| Project | Previous | New |
|---------|----------|-----|
| EmberMediaManager | 1.11.1.0 | 1.12.0.0 |
| EmberAPI | 1.11.1.0 | 1.12.0.0 |
| KodiAPI | 1.10.0 | 1.10.0 (framework only) |

### NuGet Package Updates

| Package | From | To |
|---------|------|-----|
| EntityFramework | 6.4.4 | 6.5.1 |
| NLog | 5.x | 6.0.7 |
| System.Data.SQLite | 1.0.118.0 | 1.0.119.0 |
| HtmlAgilityPack | 1.11.x | 1.12.4 |
| TMDbLib | 1.9.x | 2.3.0 |
| VideoLibrary | 3.2.x | 3.2.9 |
| Newtonsoft.Json | 13.0.3 | 13.0.4 |

### Bug Fixes
- 🐛 **Fixed async/await blocking in Kodi Interface** (`generic.Interface.Kodi`)
  - Resolved blocking issue in `ConnectAsync` method
  - Improved responsiveness when connecting to Kodi hosts

### Binary Updates
- Updated MediaInfo.dll (x64 and x86) to latest version
- Updated ffmpeg.exe (x64 and x86) to latest version

### Project Removals
The following unused/legacy projects were removed to reduce solution complexity:

| Project | Reason |
|---------|--------|
| scraper.EmberCore.XML | Legacy XML scraper, unused |
| scraper.TVDB.Poster | Duplicate functionality |
| EmberAPI_Test | Outdated test project |
| mediainfo-rar utilities | No longer needed |

### Other Changes
- Renamed `sci-fi.jpg` to `science-fiction.jpg` in genre images
- Updated `NLog.config` with improved logging configuration
- Updated VideoSourceMapping default settings

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

---

## ⚠️ Breaking Changes

- Requires .NET Framework 4.8 (previously 4.5)
- Removed projects listed above are no longer available

---

**Full Changelog**: See [ForkChangeLog.md](ForkChangeLog.md) for complete details