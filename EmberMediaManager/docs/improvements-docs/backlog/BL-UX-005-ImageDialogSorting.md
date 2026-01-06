# BL-UI-005: Image Selection Dialog - Sorting Options

| Field | Value |
|-------|-------|
| **ID** | BL-UI-003 |
| **Category** | UI/UX (UI) |
| **Priority** | Low |
| **Effort** | 4-8 hrs |
| **Created** | January 5, 2026 |
| **Status** | Open |

---

## Summary

Add user-controllable sorting options to the image selection dialog (`dlgImgSelect`) to allow users to organize images by resolution, language, scraper source, or other attributes.

---

## Current Behavior

- Images display in the order returned from scrapers (API order)
- For Movies only: Duplicate images are moved to bottom when duplicate filtering is enabled
- Sub-images (seasons, extrathumbs) are sorted by season number or index
- No user control over image sort order in the main image list

---

## Proposed Enhancement

Add a sort dropdown/ComboBox to the image selection dialog with options:
- **Resolution** (largest first)
- **Language** (alphabetical or preferred language first)
- **Scraper Source** (alphabetical or by priority)
- **Original Order** (current behavior - as returned from scrapers)

---

## Technical Analysis

### Current Sort Locations

1. **Duplicate Filtering** (`ShowDialog` method):
   - Sorts `MainPosters` and `MainFanarts` to move duplicates to bottom
   - Only applies to Movies when `GeneralImageFilterImagedialog` is enabled

2. **Sub-Images** (`CreateSubImages` method):
   - Seasons sorted by `.Season` property
   - Extrathumbs sorted by `.Index` property

3. **Main Image List** (`FillListImages` method):
   - No explicit sorting - images added in source order

### Affected Methods
- `FillListImages` - Primary location for implementing sort
- `DoSelectTopImage` - Re-adding fanarts after context switch may need sorting
- `btnIncludeFanarts_Click` - Added fanarts may need to be sorted into existing list

### Considerations
- Sort should preserve the "Include Fanarts" feature functionality
- Performance impact on large image lists (100+ images)
- Whether to persist sort preference across sessions
- Whether sort should apply to all image types or be configurable per type

---

## Related Files

- `EmberMediaManager\dlgImgSelect.vb` - Main dialog file

---

## Notes

- Low priority enhancement - current behavior is functional
- Consider adding a settings option to remember preferred sort order

---

*End of File*