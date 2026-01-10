# Temporary Notepad

*Staging area for potential backlog items. Items here need further investigation before being added to FutureEnhancements.md.*

---

## Pending Items for Review

### UI/UX Improvements

| # | Item | Category | Notes |
|---|------|----------|-------|
| 1 | Edit Image Dialog: Add image counter to sub-image sections | UX | Display count of images in Extrafanarts, SeasonFanarts, etc. Add counter left of "Insert Fanart" button on OK/Cancel bar. Helps users without scrolling and counting. |
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

---

### Known Issues / Bugs

| # | Item | Category | Notes |
|---|------|----------|-------|
| 11 | Verify if Profiles feature works | KI | App designed for multiple profiles with switching. May not be working. Worth fixing if broken. |

---

*Last updated: January 8, 2026*