<a href="http://flattr.com/thing/1321788/" target="_blank"><img src="http://api.flattr.com/button/flattr-badge-large.png" alt="Flattr this" title="Flattr this" border="0" /></a>

# Ember Media Manager

We decided that was time to give Ember a new home. We've taken it upon ourselves not only to pick up the code where it was left off but to attempt to continue its development.

If you found our work useful feel free to [donate](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=VWVJCUV3KAUX2&lc=CH&item_name=Ember%2dTeam%3a%20DanCooper%2c%20m%2esavazzi%20%26%20Cocotus&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted) us a beer!

[![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_SM.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=VWVJCUV3KAUX2&lc=CH&item_name=Ember%2dTeam%3a%20DanCooper%2c%20m%2esavazzi%20%26%20Cocotus&currency_code=USD&bn=PP%2dDonationsBF%3abtn_donateCC_LG%2egif%3aNonHosted)

## Goals
To continue development of EmberMM, because its a great product, that in my opinion is the most stable and useful media manager available, I've tried all others, but yet still come back to Ember.

## Links
- Main discussion : http://forum.xbmc.org/forumdisplay.php?fid=195
- GitHub : https://github.com/DanCooper/Ember-MM-Newscraper (DanCooper is mainaining the most aligned version)

## Helping the development
Any help is more than welcome. We do suggest everyone to participate in the forum to be aligned and updated.

As the codebase is managed by several people we tried to make it easier to maintain and review. We ask everyone to try to adhere to some simple guidelines as much as you can:
- keep it simple, if complexity is needed add a comment to explain why
- avoid duplication of code, if mandatory or needed please comment
- read all the code before changing it, avoid duplication of almost identical functionalities/classes/data. In case of doubt, please ask

_(We know everyone knows and agrees on them but the more we work on the code the more we discover how those simple principles has not been applied even from us... )_

We made a major effort in reviewing the core of Ember Media Manager, the scraping process and part, to bring it to the next level. Here are major points to consider:
- IMDB id is the unique identifier for movies.
- It is INTENTIONAL to separate the scrapers in three groups (Data, Poster, Trailer). We decided that the small overhead of code in the modules manager and some duplication of code was a far minor issue than the complexity (or mess) that had evolved in the multipurpose scrapers, making it complex to fix and almost impossible to add new ones quickly enough.
- Data scrapers will be executed one after the other and will fill ONLY selected & empty fields if not locked from global properites
- Each Data scraper will have the search dialog (is a known and accepted code duplication) because there are TOO many differences between IMDB, TMDB and other so having only one dialog in main would lead to a mess.
- Image scrapers will work in parallel and will return a list of images. The image selection dialog will merge all lists and show them. The dialog will be moved at main program level as is useless to have it replicated in the scrapers
- Order in Image scrapers will only be used for automated scraping where only the first one will be invoked (to be quicker)
- All the file save-handling logic with the names etc... will be put at main program level and will happen only once.
- All image Handling (load-save-fromWEb, etc) MUST be in only in the Images class and must use the memorystream as source (already almost there in 1.3.0.12)
- Trailers should behave as images


## Contact
Please use the forum as main contact point.

# Ember Media Manager (Fork)

[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-blue.svg)](https://dotnet.microsoft.com/download/dotnet-framework/net48)
[![Version](https://img.shields.io/badge/version-1.12.2.0-green.svg)](EmberMediaManager/docs/release-notes/ReleaseNotes-v1.12.2.0.md)
[![License](https://img.shields.io/badge/license-GPL--3.0-orange.svg)](LICENSE)

A powerful media manager for organizing movies and TV shows with Kodi-compatible NFO files.

---

## 🚀 What's Changed in This Fork

This fork includes significant improvements over the [upstream project](https://github.com/nagten/Ember-MM-Newscraper):

| Improvement | Details |
|-------------|---------|
| ⚡ **50-60% Faster Scraping** | Parallel processing for movies and TV shows |
| 🔧 **.NET Framework 4.8** | Upgraded from 4.5 with modern packages |
| 🧹 **Cleaner Codebase** | ~29,000 lines of legacy code removed (10 deprecated projects) |
| 📚 **Comprehensive Docs** | ~10,000 lines of documentation added |
| 🐛 **Bug Fixes** | Genre mapping, image filtering, TVDB file contention, and more |

### Fork Statistics (vs Upstream)

| Metric | Value |
|--------|-------|
| Files Changed | 408 |
| Lines Added | +17,256 |
| Lines Removed | -39,617 |
| Net Change | -22,361 (leaner!) |

👉 **[View Full Changelog](EmberMediaManager/docs/ForkChangeLog.md)** | **[Latest Release Notes](EmberMediaManager/docs/release-notes/ReleaseNotes-v1.12.2.0.md)**

---

## 📦 Requirements

- **Windows** 7 SP1, 8.1, 10, or 11
- **[.NET Framework 4.8](https://dotnet.microsoft.com/download/dotnet-framework/net48)**

---

## 💾 Installation

1. Download the latest release from [Releases](https://github.com/ulrick65/Ember-MM-Newscraper/releases)
2. Extract to your desired location
3. Run `Ember Media Manager.exe`

---

## 📖 Documentation

| Document | Description |
|----------|-------------|
| [ForkChangeLog.md](EmberMediaManager/docs/ForkChangeLog.md) | Complete history of all changes |
| [Document Index](EmberMediaManager/docs/DocumentIndex.md) | Index of all documentation |
| [Build Process](EmberMediaManager/docs/process-docs/BuildProcess.md) | How to build the solution |
| [Future Enhancements](EmberMediaManager/docs/improvements-docs/FutureEnhancements.md) | Planned improvements |

---

## 🔗 Links

| Link | Description |
|------|-------------|
| [This Fork](https://github.com/ulrick65/Ember-MM-Newscraper) | ulrick65/Ember-MM-Newscraper |
| [Upstream](https://github.com/nagten/Ember-MM-Newscraper) | Original nagten project |
| [Issues](https://github.com/ulrick65/Ember-MM-Newscraper/issues) | Report bugs or request features |
| [Releases](https://github.com/ulrick65/Ember-MM-Newscraper/releases) | Download releases |

---

## 🤝 Contributing

Contributions are welcome! Please:
1. Fork this repository
2. Create a feature branch
3. Submit a Pull Request

See [CONTRIBUTING.md](.github/CONTRIBUTING.md) for guidelines (if available).

---

## 📜 License

This project is licensed under the GPL-3.0 License - see the [LICENSE](LICENSE) file for details.

---

*This is a fork of [nagten/Ember-MM-Newscraper](https://github.com/nagten/Ember-MM-Newscraper). All credit to the original authors.*
