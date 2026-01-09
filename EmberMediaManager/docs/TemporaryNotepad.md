# Temporary Notepad

*Staging area for potential backlog items. Items here need further investigation before being added to FutureEnhancements.md.*

---

## Pending Items for Review

### UI/UX Improvements

| # | Item | Category | Notes |
|---|------|----------|-------|
| 1 | Edit Image Dialog: Add image counter to sub-image sections | UX | Display count of images in Extrafanarts, SeasonFanarts, etc. Add counter left of "Insert Fanart" button on OK/Cancel bar. Helps users without scrolling and counting. |
| 4 | Edit Movie → Extrafanarts should auto-scan on open | UX | Currently requires clicking Extrafanarts button to load. Should auto-load like other image types since that's the only reason user opened dialog. |
| 5 | Main window: Resizable columns in movie/tvshow list | UX | Allow sizing columns in left panel list. Remember sizes in user profile. |
| 7 | Edit Image dialog: Progress bar inconsistency | UX/KI? | Progress bar for downloading images appears sometimes, not others. Need to discuss handling during background worker thumbnail downloads. Can't act on image type until complete, but UI does nothing when clicking incomplete Image Type. |
| 14 | Cancel Scrape: Show cancellation feedback | UX | Display message in progress area when cancelling. Add scrolling progress bar or indicator. Currently user has no clue what's happening during cancel. |
| 18 | Edit Movie Images slow to open vs TV Shows | UX/PE? | Seems to download before showing panel. Should show panel first, then progress during download. |

---

### Deprecation & Removal (DR)

| # | Item | Category | Notes |
|---|------|----------|-------|
| 19 | Movies - Files and Sources Settings page  | DR | Remove all the references and associated code the backdrops folder support, deprecated in KODI |
| 20 | Movies - Scrapers - Data settings page  | DR | Collections Box - Remove "Save YAMJ Compatible Sets to NFO" and associated code for it. |
| 21 | Movies - Scrapers - Data settings page  | DR | Certifcations Box - Remove "MPAA: Save only number (YAMJ)" and all associated code  |
| 22 | TVShows - Scrapers - Data settings page  | DR | Collections Box - Remove "Save YAMJ Compatible Sets to NFO" and associated code for it. |
| 23 | Miscellaneous - Context Menu settings page  | DR | Remove all settings and panel and remove associated code for Context Menu support we are deprecating it |

### Feature Requests

| # | Item | Category | Notes |
|---|------|----------|-------|
| 3 | Keyboard Shortcuts settings page | FR | Create settings area to display and edit shortcuts. Find all current shortcuts, determine tasks needing shortcuts, allow modification. |
| 15 | Implement yt-dlp for trailer downloads | FR | Auto-download trailers using yt-dlp. GitHub: https://github.com/yt-dlp/yt-dlp |

---

### Code Quality / Logging

| # | Item | Category | Notes |
|---|------|----------|-------|
| 2 | Improve logging capabilities | CQ | Better logging levels (Debug, Info, Trace, etc.). Add settings to configure log levels. Clean up current logs. NOT for fixing errors — those are separate BL items. |
| 13 | Cleanup certification language log messages | CQ | "Unhandled certification language encountered: ua" and similar. |
| 16 | Understand log entries | CQ | 16a: `[Scanner] [IsValidDir] [NotValidDirIs] Path "...\extrafanart" has been skipped` — Need to confirm understanding. 16b: `[ModulesManager] [RunGeneric] Run generic module <generic.EmberCore.BulkRename>` — What does this mean? |
| 17 | Understand Runtime retrieval process | CQ | Runtime not always correct. Is it using media file or something else? Need to understand full process and settings that affect it. |

---

### Known Issues / Bugs

| # | Item | Category | Notes |
|---|------|----------|-------|
| 11 | Verify if Profiles feature works | KI | App designed for multiple profiles with switching. May not be working. Worth fixing if broken. |

---

## Next Steps

For each item:
1. Investigate/discuss to understand scope
2. Determine appropriate category (may change after investigation)
3. Estimate effort
4. Create BL document if needed
5. Add to FutureEnhancements.md

---

*Last updated: January 8, 2026*