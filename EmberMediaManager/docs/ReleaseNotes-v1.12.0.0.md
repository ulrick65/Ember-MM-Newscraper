# Ember Media Manager v1.12.0.0

## 🎉 Major Update: .NET Framework 4.8 Migration

This release upgrades Ember Media Manager from .NET Framework 4.5 to 4.8, providing better performance, security, and compatibility with modern Windows systems.

---

## 🔧 What's Changed

### Core Updates
- **EmberMediaManager**: `1.11.1.0` → `1.12.0.0`
- **EmberAPI**: `1.11.1.0` → `1.12.0.0`
- Upgraded all projects to .NET Framework 4.8

### Bug Fixes
- 🐛 **Fixed async/await bug in Kodi Interface** (`generic.Interface.Kodi`)
  - Resolved blocking issue in `ConnectAsync` method
  - Improved responsiveness when connecting to Kodi hosts

### Addon Updates
All 28 addons have been recompiled for .NET Framework 4.8 with incremented build numbers:
- `generic.Interface.Kodi`: `1.11.0.0` → `1.11.1.0` (includes async fix)
- `scraper.TMDB.Data`: Updated to `1.12.0.0`
- All other addons: Build numbers incremented

---

## 📦 Requirements

- **Windows**: 7 SP1, 8.1, 10, or 11
- **.NET Framework 4.8**: [Download from Microsoft](https://dotnet.microsoft.com/download/dotnet-framework/net48)

---

## 🔗 Links

- **Original Project**: [Ember-MM-Newscraper by nagten](https://github.com/nagten/Ember-MM-Newscraper)
- **Report Issues**: [GitHub Issues](https://github.com/ulrick65/Ember-MM-Newscraper-nagten/issues)

---

## 💾 Installation

1. Download the release package
2. Extract to your desired location
3. Run `Ember Media Manager.exe`

---

**Full Changelog**: [v1.11.1...v1.12.0.0](https://github.com/ulrick65/Ember-MM-Newscraper-nagten/compare/v1.11.1...v1.12.0.0)