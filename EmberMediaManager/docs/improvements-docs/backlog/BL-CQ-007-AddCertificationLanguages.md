# BL-CQ-007: Add Missing Certification Languages

| Field | Value |
|-------|-------|
| **ID** | BL-CQ-007 |
| **Created** | January 10, 2026 |
| **Priority** | Low |
| **Effort** | 2 hours |
| **Status** | ✅ Complete |
| **Completed** | January 10, 2026 |
| **Category** | Code Quality |
| **Related Files** | [`Core.Languages.Certifications.xml`](../../../../EmberAPI/Defaults/Core.Languages.Certifications.xml), [`clsScrapeTMDB.vb`](../../../../Addons/scraper.Data.TMDB/Scraper/clsScrapeTMDB.vb) |

##### [← Return to FutureEnhancements](../FutureEnhancements.md)

---

## Summary

When scraping movies or TV shows, TMDB returns certification ratings with ISO 3166-1 country codes. If a country code isn't in the certification languages XML file, a warning is logged and that certification is skipped. This document originally provided instructions for adding missing countries as encountered, but the XML file has now been expanded to include all 249 ISO 3166-1 country codes.

---

## Table of Contents

- [Problem Description](#problem-description)
- [Solution Summary](#solution-summary)
- [Implementation Details](#implementation-details)
- [Related Files](#related-files)
- [Notes](#notes)

---

## [↑](#table-of-contents) Problem Description

When scraping, the following warning appeared in the log:

    Unhandled certification language encountered: ua

This meant:
1. TMDB returned a certification for country code "ua" (Ukraine)
2. The code "ua" was not in `Core.Languages.Certifications.xml`
3. The certification was skipped and not saved to the movie/show

The original XML file only contained 62 countries, causing frequent log warnings for common countries like Ukraine, India, China, and many others.

---

## [↑](#table-of-contents) Solution Summary

Instead of adding countries one-by-one as encountered, we expanded the XML file to include the **complete ISO 3166-1 alpha-2 country code list** — all 249 countries and territories.

**Before:** 62 countries
**After:** 249 countries (complete coverage)

This eliminates all "Unhandled certification language" warnings for any valid ISO country code.

---

## [↑](#table-of-contents) Implementation Details

### File Modified

**File:** [`EmberAPI\Defaults\Core.Languages.Certifications.xml`](../../../../EmberAPI/Defaults/Core.Languages.Certifications.xml)

### Changes Made

1. **Added 187 new countries** to complete the ISO 3166-1 list
2. **Simplified country names** — removed formal suffixes like "Plurinational State of", "Islamic Republic of", etc.
3. **Fixed incorrect codes:**
   - Malaysia: `pg` → `my`
   - Iceland: `ic` → `is`
4. **Sorted alphabetically** by country name
5. **Kept existing names** for backward compatibility (e.g., "UK" instead of "United Kingdom", "USA" instead of "United States")

### Countries Added (187 new entries)

Major additions include:
- Afghanistan, Albania, Algeria, Angola, Bangladesh, Belgium, China, Cuba, Ghana, Iran, Iraq, Jamaica, Kenya, Luxembourg, Monaco, Nepal, Nigeria, Pakistan, Qatar, Saudi Arabia, Sri Lanka, Sudan, Uganda, Uruguay, Uzbekistan, Yemen, Zambia, Zimbabwe, and 159 more.

---

## [↑](#table-of-contents) Related Files

| File | Purpose |
|------|---------|
| [`EmberAPI\Defaults\Core.Languages.Certifications.xml`](../../../../EmberAPI/Defaults/Core.Languages.Certifications.xml) | Master list of certification country codes (now complete) |
| [`Addons\scraper.Data.TMDB\Scraper\clsScrapeTMDB.vb`](../../../../Addons/scraper.Data.TMDB/Scraper/clsScrapeTMDB.vb) | TMDB scraper that logs the warning |
| [`EmberAPI\XML Serialization\clsXMLCertificationLanguages.vb`](../../../../EmberAPI/XML%20Serialization/clsXMLCertificationLanguages.vb) | Class that loads the XML file |
| [`EmberMediaManager\docs\process-docs\CertificationMappingProcess.md`](../../process-docs/CertificationMappingProcess.md) | Full certification mapping documentation |

---

## [↑](#table-of-contents) Notes

- The XML file in `EmberAPI\Defaults\` is the source; it gets copied to the user's settings folder on first run
- If a user already has a settings file, they need to manually add entries or delete their local copy to get updates
- If any "Unhandled certification language" warnings appear in the future, it would indicate TMDB is returning a non-standard country code worth investigating

---

## [↑](#table-of-contents) Change History

| Date | Description |
|------|-------------|
| January 10, 2026 | Created |
| January 10, 2026 | Expanded XML from 62 to 249 countries (complete ISO 3166-1 list) - Complete |

---

*End of file*